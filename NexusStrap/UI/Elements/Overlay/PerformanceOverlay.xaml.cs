using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NexusStrap.UI.Elements.Overlay
{
    public partial class PerformanceOverlay : Wpf.Ui.Controls.UiWindow
    {
        private readonly DispatcherTimer _updateTimer;
        private PerformanceCounter? _cpuCounter;
        private Process? _robloxProcess;

        public PerformanceOverlay()
        {
            InitializeComponent();

            var workingArea = SystemParameters.WorkArea;
            Left = workingArea.Right - Width - 20;
            Top = workingArea.Top + 20;

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _updateTimer.Tick += UpdateStats;
            _updateTimer.Start();

            MouseLeftButtonDown += (_, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            };

            Loaded += (_, _) => FindRobloxProcess();
        }

        private void FindRobloxProcess()
        {
            try
            {
                var processes = Process.GetProcessesByName("RobloxPlayerBeta");
                if (processes.Length > 0)
                {
                    _robloxProcess = processes[0];
                    StatusText.Text = $"Tracking: {_robloxProcess.Id}";
                }
                else
                {
                    StatusText.Text = "Waiting for Roblox...";
                }
            }
            catch
            {
                StatusText.Text = "Process not found";
            }
        }

        private void UpdateStats(object? sender, EventArgs e)
        {
            try
            {
                if (_robloxProcess == null || _robloxProcess.HasExited)
                {
                    FindRobloxProcess();
                    CpuText.Text = "0%";
                    MemText.Text = "0 MB";
                    FpsText.Text = "--";
                    return;
                }

                using var proc = Process.GetProcessById(_robloxProcess.Id);

                long memBytes = proc.WorkingSet64;
                double memMB = memBytes / (1024.0 * 1024.0);
                MemText.Text = $"{memMB:F0} MB";

                try
                {
                    using var searcher = new ManagementObjectSearcher(
                        $"SELECT PercentProcessorTime FROM Win32_PerfFormattedData_PerfProc_Process WHERE IDProcess = '{proc.Id}'");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        double cpu = Convert.ToDouble(obj["PercentProcessorTime"]);
                        CpuText.Text = $"{cpu:F0}%";
                        break;
                    }
                }
                catch
                {
                    CpuText.Text = "?";
                }

                try
                {
                    using var logSearcher = new ManagementObjectSearcher(
                        $"SELECT ElapsedTime FROM Win32_PerfFormattedData_PerfProc_Process WHERE IDProcess = '{proc.Id}'");
                }
                catch { }

                FpsText.Text = "--";
            }
            catch
            {
                CpuText.Text = "0%";
                MemText.Text = "0 MB";
                FpsText.Text = "--";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _updateTimer.Stop();
            base.OnClosed(e);
        }
    }
}
