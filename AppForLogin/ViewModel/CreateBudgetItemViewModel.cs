using DataAccess.Model;
using AppForLogin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AppForLogin.ViewModel
{
    public partial class CreateBudgetItemViewModel : ObservableObject
    {
        private readonly BudgetService _budgetService;
        private readonly NavigationService _navigationService;
        private List<Category> _allCategories = new();

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private decimal plannedAmount;

        [ObservableProperty]
        private DateTime period = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private bool isRecurring = false;

        [ObservableProperty]
        private bool isIncome = false;

        public ObservableCollection<Category> RootCategories { get; } = new();
        public ObservableCollection<Category> SubCategories { get; } = new();

        [ObservableProperty]
        private Category? selectedRootCategory;

        [ObservableProperty]
        private Category? selectedSubCategory;

        public bool HasSubCategories => SubCategories.Count > 0;

        public DateTime MinimumDate => new DateTime(DateTime.Today.Year, 1, 1);
        public DateTime MaximumDate => new DateTime(DateTime.Today.Year, 12, 31);

        public CreateBudgetItemViewModel(BudgetService budgetService, NavigationService navigationService)
        {
            _budgetService = budgetService;
            _navigationService = navigationService;

            LoadCategoriesAsync();
        }

        partial void OnSelectedRootCategoryChanged(Category? value)
        {
            UpdateSubCategories();
        }

        private void UpdateSubCategories()
        {
            SelectedSubCategory = null;
            SubCategories.Clear();

            if (SelectedRootCategory != null && SelectedRootCategory.Id != 0)
            {
                var subs = _allCategories
                    .Where(c => c.ParentCategoryId == SelectedRootCategory.Id)
                    .OrderBy(c => c.Name)
                    .ToList();

                foreach (var sub in subs)
                    SubCategories.Add(sub);
            }

            OnPropertyChanged(nameof(HasSubCategories));
        }

        public async Task LoadCategoriesAsync()
        {
            _allCategories = await _budgetService.GetAllCategoriesAsync();

            RootCategories.Clear();
            foreach (var cat in _allCategories.Where(c => c.ParentCategoryId == null).OrderBy(c => c.Name))
                RootCategories.Add(cat);

            if (RootCategories.Any())
                SelectedRootCategory = RootCategories.First();
        }

        [RelayCommand]
        private async Task CreateBudgetItemAsync()
        {
            try
            {
                // Determine effective category: prefer sub if selected, otherwise root
                var effectiveCategory = (SelectedSubCategory != null)
                    ? SelectedSubCategory
                    : SelectedRootCategory;

                if (effectiveCategory is null)
                {
                    await Shell.Current.DisplayAlertAsync("Error", "Selecteer een categorie", "OK");
                    return;
                }

                // If subcategories exist, require the user to pick one explicitly
                if (HasSubCategories && SelectedSubCategory is null)
                {
                    await Shell.Current.DisplayAlertAsync("Error", "Selecteer een subcategorie", "OK");
                    return;
                }

                if (IsRecurring)
                {
                    for (int month = 1; month <= 12; month++)
                    {
                        var budgetLine = new BudgetLine
                        {
                            CategoryId = effectiveCategory.Id,
                            Period = new DateTime(DateTime.Today.Year, month, 1),
                            PlannedAmount = PlannedAmount,
                            IsIncome = IsIncome,
                            Notes = string.IsNullOrWhiteSpace(Description) ? null : Description
                        };
                        await _budgetService.AddBudgetItemAsync(budgetLine);
                    }
                }
                else
                {
                    var newBudgetItem = new BudgetLine
                    {
                        CategoryId = effectiveCategory.Id,
                        Period = new DateTime(Period.Year, Period.Month, 1),
                        PlannedAmount = PlannedAmount,
                        IsIncome = IsIncome,
                        Notes = string.IsNullOrWhiteSpace(Description) ? null : Description
                    };

                    await _budgetService.AddBudgetItemAsync(newBudgetItem);

                    if (!string.IsNullOrWhiteSpace(Description))
                    {
                        var transaction = new Transaction
                        {
                            Description = Description,
                            Date = Period,
                            CategoryId = effectiveCategory.Id,
                            Amount = PlannedAmount,
                            IsIncome = IsIncome,
                            Status = StatusTrans.Manual
                        };

                        await _budgetService.AddTransactionAsync(transaction);
                    }
                }

                await _navigationService.GoBackAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Aanmaken mislukt: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await _navigationService.GoBackAsync();
        }
    }

    // Simple navigation service interface for demonstration
    public interface INavigationService
    {
        Task GoBackAsync();
    }

    // Simple implementation
    public class NavigationService : INavigationService
    {
        public Task GoBackAsync()
        {
            return Shell.Current.GoToAsync("..");
        }
    }
}