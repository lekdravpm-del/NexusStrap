using NexusStrap.AppData;
using NexusStrap.Integrations;

namespace NexusStrap
{
    public class Watcher : IDisposable
    {
        private readonly InterProcessLock _lock = new("Watcher");

        private readonly WatcherData? _watcherData;

        private readonly NotifyIconWrapper? _notifyIcon;

        public readonly ActivityWatcher? ActivityWatcher;

        public readonly IntegrationWatcher? IntegrationWatcher;

        public readonly PlayerDiscordRichPresence? PlayerRichPresence;
        public readonly StudioDiscordRichPresence? StudioRichPresence;

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _isDisposed = false;

        public Watcher()
        {
            const string LOG_IDENT = "Watcher";

            if (!_lock.IsAcquired)
            {
                App.Logger.WriteLine(LOG_IDENT, "Watcher instance already exists");
                return;
            }

            string? watcherDataArg = App.LaunchSettings.WatcherFlag.Data;

            if (String.IsNullOrEmpty(watcherDataArg))
            {
#if DEBUG
                string path = new RobloxPlayerData().ExecutablePath;
                if (!File.Exists(path))
                    throw new ApplicationException("Roblox player is not been installed");

                using var gameClientProcess = Process.Start(path);

                _watcherData = new() { ProcessId = gameClientProcess.Id };
#else
                throw new Exception("Watcher data not specified");
#endif
            }
            else
            {
                _watcherData = JsonSerializer.Deserialize<WatcherData>(Encoding.UTF8.GetString(Convert.FromBase64String(watcherDataArg)));
            }

            if (_watcherData is null)
                throw new Exception("Watcher data is invalid");

            if (App.Settings.Prop.EnableActivityTracking)
            {
                ActivityWatcher = new(_watcherData.LogFile, _watcherData.LaunchMode, _watcherData.ProcessId);

                if (App.Settings.Prop.UseDisableAppPatch)
                {
                    ActivityWatcher.OnAppClose += delegate
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Received desktop app exit, closing Roblox");
                        using var process = Process.GetProcessById(_watcherData.ProcessId);
                        process.CloseMainWindow();
                    };
                }

                if ((_watcherData.LaunchMode == LaunchMode.Studio || _watcherData.LaunchMode == LaunchMode.StudioAuth) && App.Settings.Prop.StudioRPC)
                    StudioRichPresence = new(ActivityWatcher);
                else if (_watcherData.LaunchMode == LaunchMode.Player && App.Settings.Prop.UseDiscordRichPresence)
                    PlayerRichPresence = new(ActivityWatcher);
            }

            _notifyIcon = new(this);
        }

        public void KillRobloxProcess() => CloseProcess(_watcherData!.ProcessId, true);

        public void CloseProcess(int pid, bool force = false)
        {
            const string LOG_IDENT = "Watcher::CloseProcess";

            try
            {
                using var process = Process.GetProcessById(pid);

                App.Logger.WriteLine(LOG_IDENT, $"Killing process '{process.ProcessName}' (pid={pid}, force={force})");

                if (process.HasExited)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"PID {pid} has already exited");
                    return;
                }

                if (force)
                    process.Kill();
                else
                    process.CloseMainWindow();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"PID {pid} could not be closed");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public async Task Run()
        {
            if (!_lock.IsAcquired || _watcherData is null)
                return;

            ActivityWatcher?.Start();

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested &&
                       Utilities.GetProcessesSafe().Any(x => x.Id == _watcherData.ProcessId))
                {
                    await Task.Delay(1000, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                App.Logger.WriteLine("Watcher::Run", "Watcher was cancelled");
                return;
            }

            if (_cancellationTokenSource.Token.IsCancellationRequested)
                return;

            // Crash Recovery: detect abnormal exit and auto-relaunch
            if (App.Settings.Prop.CrashRecoveryEnabled && _watcherData.LaunchMode == LaunchMode.Player)
            {
                try
                {
                    using var process = Process.GetProcessById(_watcherData.ProcessId);
                    if (process.HasExited && process.ExitCode != 0)
                    {
                        App.Logger.WriteLine("Watcher::Run", $"Roblox crashed with exit code {process.ExitCode}. Crash recovery triggered.");
                        for (int attempt = 1; attempt <= App.Settings.Prop.CrashRecoveryMaxRetries; attempt++)
                        {
                            App.Logger.WriteLine("Watcher::Run", $"Crash recovery attempt {attempt}/{App.Settings.Prop.CrashRecoveryMaxRetries}");
                            await Task.Delay(App.Settings.Prop.CrashRecoveryDelayMs);

                            try
                            {
                                Process.Start(Paths.Process, "-player");
                                App.Logger.WriteLine("Watcher::Run", "Crash recovery: relaunched Roblox successfully");
                                return;
                            }
                            catch (Exception ex)
                            {
                                App.Logger.WriteException("Watcher::Run", ex);
                            }
                        }
                        App.Logger.WriteLine("Watcher::Run", "Crash recovery: all retry attempts exhausted");
                    }
                }
                catch
                {
                    // Process may have already been cleaned up
                }
            }

            if (_watcherData.AutoclosePids is not null)
            {
                foreach (int pid in _watcherData.AutoclosePids)
                    CloseProcess(pid);
            }

            if (App.LaunchSettings.TestModeFlag.Active)
                Process.Start(Paths.Process, "-settings -testmode");
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            App.Logger.WriteLine("Watcher::Dispose", "Disposing Watcher");

            _cancellationTokenSource.Cancel();

            if (App.Settings.Prop.MultiInstanceLaunching)
            {
                App.Logger.WriteLine("Watcher::Dispose", "Starting multi-instance cleanup");
                App.Bootstrapper?.CleanupMultiInstanceResources();
            }

            IntegrationWatcher?.Dispose();
            _notifyIcon?.Dispose();
            PlayerRichPresence?.Dispose();
            StudioRichPresence?.Dispose();
            _cancellationTokenSource.Dispose();

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}