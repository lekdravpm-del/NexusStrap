using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using NexusStrap.UI.ViewModels.Installer;

namespace NexusStrap.UI.Elements.Installer.Pages
{
    /// <summary>
    /// Interaction logic for CompletionPage.xaml
    /// </summary>
    public partial class CompletionPage
    {
        private static readonly RoutedUICommand OpenDiscord = new("Open Discord", "OpenDiscord", typeof(CompletionPage));
        public static ICommand OpenDiscordCommand => OpenDiscord;

        static CompletionPage()
        {
            CommandManager.RegisterClassCommandBinding(typeof(CompletionPage),
                new CommandBinding(OpenDiscord, (_, _) =>
                    Process.Start(new ProcessStartInfo("https://discord.gg/PHbaZR8SJ") { UseShellExecute = true })));
        }

        private readonly CompletionViewModel _viewModel = new();
        public CompletionPage()
        {
            _viewModel.CloseWindowRequest += (_, closeAction) =>
            {
                if (Window.GetWindow(this) is MainWindow window)
                {
                    window.CloseAction = closeAction;
                    window.Close();
                }
            };

            DataContext = _viewModel;
            InitializeComponent();
        }

        private void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow window)
            {
                window.SetNextButtonText(Strings.Common_Navigation_Next);
                window.SetButtonEnabled("back", false);
            }
        }
    }
}
