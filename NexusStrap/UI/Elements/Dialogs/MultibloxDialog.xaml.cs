using System;
using NexusStrap.UI.ViewModels.Dialogs;

namespace NexusStrap.UI.Elements.Dialogs
{
    public partial class MultibloxDialog
    {
        private readonly MultibloxViewModel _viewModel = new();

        public MultibloxDialog()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.Dispose();
            App.Settings.Save();
            base.OnClosed(e);
        }
    }
}