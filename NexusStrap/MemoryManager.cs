using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;

namespace NexusStrap
{
    public static class MemoryManager
    {
        private static readonly object _lock = new();
        private static System.Timers.Timer? _timer;
        private static DateTime _lastCleanTime = DateTime.MinValue;

        private const string LOG_IDENT = "MemoryManager";

        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool EmptyWorkingSet(IntPtr hProcess);

        public static void Start()
        {
            lock (_lock)
            {
                if (_timer != null) return;

                _timer = new System.Timers.Timer(5000)
                {
                    AutoReset = true
                };
                _timer.Elapsed += (_, _) => Tick();
                _timer.Start();

                App.Logger.WriteLine(LOG_IDENT, "Memory manager started");
            }
        }

        public static void Stop()
        {
            lock (_lock)
            {
                if (_timer == null) return;

                _timer.Stop();
                _timer.Dispose();
                _timer = null;

                App.Logger.WriteLine(LOG_IDENT, "Memory manager stopped");
            }
        }

        public static void CleanNow()
        {
            var processes = GetRobloxProcesses();
            if (processes.Count == 0)
            {
                App.Logger.WriteLine(LOG_IDENT, "Clean requested, but no Roblox process is running");
                return;
            }

            foreach (var process in processes)
            {
                using (process)
                {
                    long beforeMB = process.WorkingSet64 / (1024 * 1024);
                    TrimProcess(process);
                    process.Refresh();
                    long afterMB = process.WorkingSet64 / (1024 * 1024);
                    App.Logger.WriteLine(LOG_IDENT, $"Cleaned Roblox PID {process.Id}: {beforeMB} MB -> {afterMB} MB");
                }
            }
        }

        private static void Tick()
        {
            try
            {
                var settings = App.Settings.Prop;

                bool limiterEnabled = settings.EnableMemoryLimiter && settings.MemoryLimitMB > 0;
                bool cleanerEnabled = settings.EnableRamCleaner;

                if (!limiterEnabled && !cleanerEnabled)
                    return;

                var processes = GetRobloxProcesses();
                if (processes.Count == 0)
                    return;

                bool shouldClean = false;

                if (limiterEnabled)
                {
                    double limitBytes = settings.MemoryLimitMB * 1024.0 * 1024.0;

                    foreach (var process in processes)
                    {
                        using (process)
                        {
                            process.Refresh();
                            long usedBytes = process.WorkingSet64;

                            if (usedBytes > limitBytes)
                            {
                                App.Logger.WriteLine(LOG_IDENT, $"Roblox PID {process.Id} over limit ({usedBytes / (1024 * 1024)} MB > {settings.MemoryLimitMB} MB), trimming");
                                shouldClean = true;
                            }
                        }
                    }
                }

                if (cleanerEnabled)
                {
                    double intervalMinutes = settings.RamCleanIntervalMinutes > 0 ? settings.RamCleanIntervalMinutes : 10;

                    if ((DateTime.Now - _lastCleanTime).TotalMinutes >= intervalMinutes)
                    {
                        shouldClean = true;
                        _lastCleanTime = DateTime.Now;
                    }
                }

                if (shouldClean)
                {
                    CleanNow();
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        private static List<Process> GetRobloxProcesses()
        {
            var list = new List<Process>();

            try
            {
                foreach (var name in new[] { "RobloxPlayerBeta", "RobloxPlayer" })
                {
                    var processes = Process.GetProcessesByName(name);
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Refresh();
                            if (!process.HasExited && process.WorkingSet64 > 0)
                                list.Add(process);
                            else
                                process.Dispose();
                        }
                        catch
                        {
                            process.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }

            return list;
        }

        private static void TrimProcess(Process process)
        {
            try
            {
                EmptyWorkingSet(process.Handle);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
    }
}