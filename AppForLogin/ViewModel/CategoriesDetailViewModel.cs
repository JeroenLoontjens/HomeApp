using AppForLogin.Configuration;
using AppForLogin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataAccess.Model;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AppForLogin.ViewModel
{
    public partial class CategoriesDetailViewModel : ObservableObject, IQueryAttributable
    {
        private readonly BudgetService _budgetService;

        [ObservableProperty]
        public ObservableCollection<Category> possibleParents = new();

        [ObservableProperty]
        private Category selectedParent;

        [ObservableProperty] private Category category;

        [ObservableProperty] private string name     = string.Empty;
        [ObservableProperty] private string icon     = string.Empty;
        [ObservableProperty] private string colorHex = string.Empty;

        [ObservableProperty] private bool   isBusy;
        

        public bool IsExistingCategory => Category?.Id > 0;

        // Icon catalog — loaded from CategoryIconCatalog so it stays in sync
        public IReadOnlyList<string> AvailableIcons { get; } = CategoryIconCatalog.All;

        // Curated color palette for categories
        public IReadOnlyList<string> AvailableColors { get; } = new[]
        {
            "#E53E3E", "#DD6B20", "#D69E2E", "#38A169",
            "#319795", "#3182CE", "#5A67D8", "#6B46C1",
            "#D53F8C", "#9F7AEA", "#718096", "#2D3748"
        };

        // Parsed Color used for the live preview badge
        public Color PreviewColor => ParseColor(ColorHex);

        public CategoriesDetailViewModel(BudgetService budgetService)
        {
            _budgetService = budgetService;
             

        }

        private async Task LoadPossibleParentsAsync()
        {
            var categories = await _budgetService.GetAllCategoriesAsync();

            // Remove self from possible parents
            var filteredCategories = categories
                .Where(c => c.Id != Category?.Id && c.ParentCategoryId != Category?.Id)
                .ToList();
                    
                       
            var DummyItem = new Category { Id = 0, Name = "NO Parent" };

            PossibleParents.Clear();
            PossibleParents.Add(DummyItem);

            foreach (var category in filteredCategories)
                PossibleParents.Add(category);

            // 3) Init SelectedParent o.b.v. huidige ParentCategoryId
            var parentId = Category?.ParentCategoryId;

            if (parentId.HasValue && parentId.Value > 0)
                SelectedParent = PossibleParents.FirstOrDefault(x => x.Id == parentId.Value) ?? DummyItem;
            else
                SelectedParent = DummyItem;


        }



        // Re-compute preview whenever the hex value changes
        partial void OnColorHexChanged(string value) => OnPropertyChanged(nameof(PreviewColor));

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Category", out var value) && value is Category c)
            {
                Category = c;
                Name     = c.Name     ?? string.Empty;
                Icon     = c.Icon     ?? string.Empty;
                ColorHex = c.ColorHex ?? string.Empty;
                OnPropertyChanged(nameof(IsExistingCategory));

                await LoadPossibleParentsAsync();
            }
        }

        // Called by CollectionView TwoWay SelectedItem — kept as explicit command
        // so the icon/color pickers can also be driven from the CollectionView selection
        [RelayCommand]
        private void SelectIcon(string icon) => Icon = icon;

        [RelayCommand]
        private void SelectColor(string color) => ColorHex = color;

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlertAsync("Validatie", "Naam is verplicht.", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                Category.Name     = Name.Trim();
                Category.Icon     = Icon?.Trim();
                Category.ColorHex = ColorHex?.Trim();
                Category.ParentCategoryId = (SelectedParent?.Id ?? 0) == 0 ? (int?)null : SelectedParent.Id;

                if (Category.Id == 0)
                    await _budgetService.AddCategoryAsync(Category);
                else
                    await _budgetService.UpdateCategoryAsync(Category);

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Fout", $"Opslaan mislukt: {ex.Message}", "OK");
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (Category is null || Category.Id == 0)
                return;

            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Verwijderen",
                $"Weet je zeker dat je '{Category.Name}' wilt verwijderen?",
                "Ja", "Nee");

            if (!confirm) return;

            try
            {
                IsBusy = true;
                await _budgetService.DeleteCategoryAsync(Category.Id);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Fout", $"Verwijderen mislukt: {ex.Message}", "OK");
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task ReturnAsync() => await Shell.Current.GoToAsync("..");

        private static Color ParseColor(string? hex)
        {
            try { return Color.FromArgb(hex ?? string.Empty); }
            catch { return Colors.DarkSlateBlue; }
        }
    }
}
