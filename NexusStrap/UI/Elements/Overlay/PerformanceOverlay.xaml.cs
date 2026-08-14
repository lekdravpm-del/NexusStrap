using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NexusStrap.UI.Elements.Overlay
{
    public partial class PerformanceOverlay : Wpf.Ui.Controls.UiWindow
    {
        private readonly DispatcherTimer _updateTimer;
        private readonly Stopwatch _sampleStopwatch = Stopwatch.StartNew();
        private Process? _robloxProcess;
        private TimeSpan _lastProcessorTime;
        private TimeSpan _lastSampleElapsed;
        private bool _hasCpuSample;

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
                    _robloxProcess?.Dispose();
                    _robloxProcess = processes[0];

                    // GetProcessesByName creates a Process object for every match. Keep only
                    // the process we are tracking so repeated rediscovery does not leak handles.
                    foreach (var process in processes.Skip(1))
                        process.Dispose();

                    ResetCpuSampling();
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

                _robloxProcess.Refresh();

                long memBytes = _robloxProcess.WorkingSet64;
                double memMB = memBytes / (1024.0 * 1024.0);
                MemText.Text = $"{memMB:F0} MB";

                var elapsed = _sampleStopwatch.Elapsed;
                var processorTime = _robloxProcess.TotalProcessorTime;

                if (_hasCpuSample)
                {
                    var elapsedMilliseconds = (elapsed - _lastSampleElapsed).TotalMilliseconds;
                    var cpuMilliseconds = (processorTime - _lastProcessorTime).TotalMilliseconds;

                    if (elapsedMilliseconds > 0)
                    {
                        double cpu = cpuMilliseconds / (elapsedMilliseconds * Environment.ProcessorCount) * 100;
                        CpuText.Text = $"{Math.Clamp(cpu, 0, 100):F0}%";
                    }
                }
                else
                {
                    CpuText.Text = "...";
                    _hasCpuSample = true;
                }

                _lastProcessorTime = processorTime;
                _lastSampleElapsed = elapsed;

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
            _robloxProcess?.Dispose();
            base.OnClosed(e);
        }

        private void ResetCpuSampling()
        {
            _hasCpuSample = false;
            _lastProcessorTime = TimeSpan.Zero;
            _lastSampleElapsed = _sampleStopwatch.Elapsed;
        }
    }
}
