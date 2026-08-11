using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using NexusStrap.Models;

namespace NexusStrap.UI.ViewModels.Settings
{
    public class FFlagTemplatesViewModel : NotifyPropertyChangedViewModel
    {
        private string _searchQuery = "";
        private string _selectedCategory = "";

        public ObservableCollection<FFlagTemplate> Templates { get; } = new();
        public IReadOnlyList<string> Categories => FFlagTemplateCategories.All;

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                RefreshTemplates();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                RefreshTemplates();
            }
        }

        public ICommand RefreshTemplatesCommand => new RelayCommand(RefreshTemplates);
        public RelayCommand<FFlagTemplate> ApplyTemplateCommand => new(ApplyTemplate);
        public ICommand ImportTemplateCommand => new RelayCommand(ImportTemplate);
        public ICommand ClearFilterCommand => new RelayCommand(ClearFilter);

        public FFlagTemplatesViewModel()
        {
            RefreshTemplates();
        }

        private void RefreshTemplates()
        {
            Templates.Clear();

            IReadOnlyList<FFlagTemplate> results;

            if (!string.IsNullOrWhiteSpace(SelectedCategory) && !string.IsNullOrWhiteSpace(SearchQuery))
            {
                string lower = SearchQuery.ToLowerInvariant();
                results = FFlagTemplateManager.GetByCategory(SelectedCategory)
                    .Where(t => t.Name.ToLowerInvariant().Contains(lower)
                             || t.Description.ToLowerInvariant().Contains(lower))
                    .ToList();
            }
            else if (!string.IsNullOrWhiteSpace(SelectedCategory))
            {
                results = FFlagTemplateManager.GetByCategory(SelectedCategory);
            }
            else if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                results = FFlagTemplateManager.Search(SearchQuery);
            }
            else
            {
                results = FFlagTemplateManager.GetAll();
            }

            foreach (var template in results)
                Templates.Add(template);
        }

        private void ApplyTemplate(FFlagTemplate? template)
        {
            if (template == null) return;

            var result = Frontend.ShowMessageBox(
                $"Apply '{template.Name}'?\n\nThis will set {template.Flags.Count} FFlag(s).",
                System.Windows.MessageBoxImage.Question,
                System.Windows.MessageBoxButton.YesNo);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                FFlagTemplateManager.ApplyTemplate(template);

                Frontend.ShowMessageBox(
                    $"Template '{template.Name}' applied successfully!\n\nSave your settings to make changes permanent.",
                    System.Windows.MessageBoxImage.Information);
            }
        }

        private void ImportTemplate()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Import FFlag Template"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(dialog.FileName);
                    var template = FFlagTemplateManager.ImportFromJson(json);

                    if (template == null)
                    {
                        Frontend.ShowMessageBox("Invalid template file. Make sure it contains a valid FFlagTemplate JSON.", System.Windows.MessageBoxImage.Error);
                        return;
                    }

                    FFlagTemplateManager.ApplyTemplate(template);

                    Frontend.ShowMessageBox(
                        $"Imported and applied '{template.Name}' successfully!\n\n{template.Flags.Count} flags were set. Save your settings to make changes permanent.",
                        System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Frontend.ShowMessageBox($"Failed to import template: {ex.Message}", System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ClearFilter()
        {
            SearchQuery = "";
            SelectedCategory = "";
        }
    }
}
