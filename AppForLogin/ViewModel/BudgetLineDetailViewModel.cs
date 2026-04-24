using AppForLogin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataAccess.Model;

namespace AppForLogin.ViewModel;

[QueryProperty(nameof(BudgetLineId), "BudgetLineId")]
public partial class BudgetLineDetailViewModel : ObservableObject
{
    private readonly BudgetService _budgetService;

    [ObservableProperty] private int budgetLineId;
    [ObservableProperty] private BudgetLine? budgetLine;
    [ObservableProperty] private bool isBusy;

    [ObservableProperty] private string categoryName = "";
    [ObservableProperty] private DateTime period = DateTime.Today;
    [ObservableProperty] private decimal plannedAmount;
    [ObservableProperty] private bool isIncome;
    [ObservableProperty] private string notes = "";

    public BudgetLineDetailViewModel(BudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    partial void OnBudgetLineIdChanged(int value) => _ = LoadAsync();

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (BudgetLineId <= 0) return;

        try
        {
            IsBusy = true;
            BudgetLine = await _budgetService.GetBudgetItemByIdAsync(BudgetLineId);
            if (BudgetLine != null)
            {
                CategoryName = BudgetLine.Category?.Name ?? "";
                Period = BudgetLine.Period;
                PlannedAmount = BudgetLine.PlannedAmount;
                IsIncome = BudgetLine.IsIncome;
                Notes = BudgetLine.Notes ?? "";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (BudgetLine is null) return;

        try
        {
            IsBusy = true;
            BudgetLine.Period = new DateTime(Period.Year, Period.Month, 1);
            BudgetLine.PlannedAmount = PlannedAmount;
            BudgetLine.IsIncome = IsIncome;
            BudgetLine.Notes = Notes;
            BudgetLine.UpdatedAt = DateTime.UtcNow;

            await _budgetService.UpdateBudgetItemAsync(BudgetLine);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Fout", $"Opslaan mislukt: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ReturnAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (BudgetLine is null) return;
        var confirm = await Shell.Current.DisplayAlertAsync("Bevestigen", "Weet je zeker dat je dit budgetitem wilt verwijderen?", "Ja", "Nee");
        if (!confirm) return;
        try
        {
            IsBusy = true;
            await _budgetService.DeleteBudgetItemAsync(BudgetLine.Id);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Fout", $"Verwijderen mislukt: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
