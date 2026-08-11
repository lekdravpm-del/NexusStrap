using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using NexusStrap.UI.Elements.Settings.Pages;
using NexusStrap.Resources;

using Wpf.Ui.Mvvm.Contracts;

namespace NexusStrap.UI.ViewModels.Settings
{
    internal class FastFlagEditorWarningViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Page _page;

        private CancellationTokenSource? _cancellationTokenSource;

        public string ContinueButtonText { get; set; } = "";

        public bool CanContinue { get; set; } = false;

        public ICommand GoBackCommand => new RelayCommand(GoBack);

        public ICommand ContinueCommand => new RelayCommand(Continue);

        public FastFlagEditorWarningViewModel(Page page)
        {
            _page = page;
        }

        public void StopCountdown()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        public void StartCountdown()
        {
            StopCountdown();

            _cancellationTokenSource = new CancellationTokenSource();
            DoCountdown(_cancellationTokenSource.Token);
        }

        private async void DoCountdown(CancellationToken token)
        {
            CanContinue = false;
            OnPropertyChanged(nameof(CanContinue));

            for (int i = 3; i > 0; i--)
            {
                ContinueButtonText = $"({i}) {Strings.Menu_FastFlagEditor_Warning_Continue}";
                OnPropertyChanged(nameof(ContinueButtonText));

                try
                {
                    await Task.Delay(1000, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }

            ContinueButtonText = Strings.Menu_FastFlagEditor_Warning_Continue;
            OnPropertyChanged(nameof(ContinueButtonText));

            CanContinue = true;
            OnPropertyChanged(nameof(CanContinue));
        }

        private void Continue()
        {
            if (!CanContinue)
                return;

            App.State.Prop.ShowFFlagEditorWarning = false;
            App.State.Save();

            if (Window.GetWindow(_page) is INavigationWindow window)
                window.Navigate(typeof(FastFlagEditorPage));
        }

        private void GoBack()
        {
            if (Window.GetWindow(_page) is INavigationWindow window)
                window.Navigate(typeof(FastFlagsPage));
        }
    }
}
