using CommunityToolkit.Mvvm.ComponentModel;

namespace NexusStrap.UI.ViewModels.Settings
{
    public partial class MemoryViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _enableRamCleaner;

        [ObservableProperty]
        private int _ramCleanIntervalMinutes;

        [ObservableProperty]
        private bool _enableMemoryLimiter;

        [ObservableProperty]
        private int _memoryLimitMB;

        public MemoryViewModel()
        {
            _enableRamCleaner = App.Settings.Prop.EnableRamCleaner;
            _ramCleanIntervalMinutes = App.Settings.Prop.RamCleanIntervalMinutes;
            _enableMemoryLimiter = App.Settings.Prop.EnableMemoryLimiter;
            _memoryLimitMB = App.Settings.Prop.MemoryLimitMB;

            PropertyChanged += (_, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(EnableRamCleaner):
                        App.Settings.Prop.EnableRamCleaner = EnableRamCleaner;
                        break;
                    case nameof(RamCleanIntervalMinutes):
                        if (RamCleanIntervalMinutes > 0)
                            App.Settings.Prop.RamCleanIntervalMinutes = RamCleanIntervalMinutes;
                        break;
                    case nameof(EnableMemoryLimiter):
                        App.Settings.Prop.EnableMemoryLimiter = EnableMemoryLimiter;
                        break;
                    case nameof(MemoryLimitMB):
                        if (MemoryLimitMB > 0)
                            App.Settings.Prop.MemoryLimitMB = MemoryLimitMB;
                        break;
                }

                App.Settings.Save();
            };
        }
    }
}
