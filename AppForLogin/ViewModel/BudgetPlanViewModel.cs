using AppForLogin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataAccess.Model;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AppForLogin.ViewModel;

public enum BudgetPlanLayoutMode
{
    Vertical,
    Horizontal
}

public partial class BudgetPlanPageViewModel : ObservableObject
{
    private readonly BudgetService _budgetService;

    public ObservableCollection<int> AvailableYears { get; } = new();

    [ObservableProperty] private int selectedYear;
    [ObservableProperty] private BudgetPlanLayoutMode layoutMode = BudgetPlanLayoutMode.Vertical;
    [ObservableProperty] private bool isBusy;

    [ObservableProperty] private ObservableCollection<MonthlyBudgetGroup> monthGroups = new();

    [ObservableProperty] private decimal totalIncome;
    [ObservableProperty] private decimal totalExpense;
    [ObservableProperty] private decimal totalNet;

    public BudgetPlanPageViewModel(BudgetService budgetService)
    {
        _budgetService = budgetService;

        var now = DateTime.Today;
        SelectedYear = now.Year;

        foreach (var y in Enumerable.Range(now.Year - 1, 3))
            AvailableYears.Add(y);
    }

    partial void OnSelectedYearChanged(int value) => _ = LoadAsync();

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var all = await _budgetService.GetAllBudgetItemsAsync();
            var yearLines = all.Where(b => b.Period.Year == SelectedYear).ToList();

            MonthGroups.Clear();

            for (int m = 1; m <= 12; m++)
            {
                var monthStart = new DateTime(SelectedYear, m, 1);

                var monthLines = yearLines
                    .Where(b => b.Period.Month == m)
                    .OrderBy(b => b.IsIncome)
                    .ThenBy(b => b.Category?.Name)
                    .ToList();

                var income = monthLines.Where(x => x.IsIncome).Sum(x => x.PlannedAmount);
                var expense = monthLines.Where(x => !x.IsIncome).Sum(x => x.PlannedAmount);

                MonthGroups.Add(new MonthlyBudgetGroup
                {
                    MonthStart = monthStart,
                    MonthLabel = monthStart.ToString("MMM", CultureInfo.CurrentCulture),   // kort voor horizontaal
                    MonthLabelLong = monthStart.ToString("MMMM", CultureInfo.CurrentCulture), // lang voor verticaal
                    Lines = new ObservableCollection<BudgetLine>(monthLines),
                    Income = income,
                    Expense = expense,
                    Net = income - expense
                });
            }

            TotalIncome = MonthGroups.Sum(g => g.Income);
            TotalExpense = MonthGroups.Sum(g => g.Expense);
            TotalNet = TotalIncome - TotalExpense;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ToggleMonth(MonthlyBudgetGroup month)
    {
        if (month is null) return;

        
        month.IsExpanded = !month.IsExpanded;
    }

}

public partial class MonthlyBudgetGroup : ObservableObject
{
    public DateTime MonthStart { get; set; }
    public string MonthLabel { get; set; } = "";
    public string MonthLabelLong { get; set; } = "";

    public ObservableCollection<BudgetLine> Lines { get; set; } = new();

    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }

    [ObservableProperty]
    private bool isExpanded;
}