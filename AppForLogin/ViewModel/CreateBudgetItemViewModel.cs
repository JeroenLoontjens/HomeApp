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

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private decimal plannedAmount;

        [ObservableProperty]
        private DateTime period = DateTime.Now; // Changed from DateOnly to DateTime for DatePicker

        [ObservableProperty]
        private Category? selectedCategory; // Changed from int to Category object

        [ObservableProperty]
        private string description = string.Empty;

        public ObservableCollection<Category> Categories { get; } = new();

        public CreateBudgetItemViewModel(BudgetService budgetService, NavigationService navigationService)
        {
            _budgetService = budgetService;
            _navigationService = navigationService;
            
            // Load categories when ViewModel is initialized
            LoadCategoriesAsync();
        }

        public async Task LoadCategoriesAsync()
        {
            var categories = await _budgetService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
            
            // Select first category by default if available
            if (Categories.Any())
            {
                SelectedCategory = Categories.First();
            }
        }

        [RelayCommand]
        private async Task CreateBudgetItemAsync()
        {
            try
            {
                if (SelectedCategory is null)
                {
                    await Shell.Current.DisplayAlertAsync("Error", "Please select a category", "OK");
                    return;
                }

                // Create new budget item
                var newBudgetItem = new BudgetLine
                {
                    CategoryId = SelectedCategory.Id,
                    Period = new DateTime(period.Year, period.Month, 1) , // Convert DateTime to DateOnly
                    PlannedAmount = PlannedAmount
                };

                // Add to database
                await _budgetService.AddBudgetItemAsync(newBudgetItem);

                // Optionally: Add initial transaction if description is provided
                if (!string.IsNullOrWhiteSpace(Description))
                {
                    var transaction = new Transaction
                    {
                        Description = Description,
                        Date = Period, // Use the DateTime directly
                        CategoryId = SelectedCategory.Id,
                        Amount = PlannedAmount,
                        IsIncome = false, // Assuming this is an expense
                        Status = StatusTrans.Manual
                    };
                    
                    await _budgetService.AddTransactionAsync(transaction);
                    
                    
                }

                // Navigate back or show success message
                await _navigationService.GoBackAsync();
            }
            catch (Exception ex)
            {
                // Handle error - show to user
                await Shell.Current.DisplayAlert("Error", $"Failed to create budget item: {ex.Message}", "OK");
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