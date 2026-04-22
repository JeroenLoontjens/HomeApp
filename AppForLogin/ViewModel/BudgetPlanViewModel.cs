using AppForLogin.Services;
using AppForLogin.Views;
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

public enum BudgetLineFilter
{
    None,
    Income,
    Expense
}

public partial class BudgetPlanPageViewModel : ObservableObject
{
    private readonly BudgetService _budgetService;
    private List<BudgetLine> _allYearLines = new();

    public ObservableCollection<int> AvailableYears { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    [ObservableProperty] private int selectedYear;
    [ObservableProperty] private BudgetPlanLayoutMode layoutMode = BudgetPlanLayoutMode.Vertical;
    [ObservableProperty] private bool isBusy;

    [ObservableProperty] private ObservableCollection<MonthlyBudgetGroup> monthGroups = new();

    [ObservableProperty] private decimal totalIncome;
    [ObservableProperty] private decimal totalExpense;
    [ObservableProperty] private decimal totalNet;

    [ObservableProperty] private Category? selectedCategoryFilter;

    public BudgetPlanPageViewModel(BudgetService budgetService)
    {
        _budgetService = budgetService;

        var now = DateTime.Today;
        SelectedYear = now.Year;

        foreach (var y in Enumerable.Range(now.Year - 1, 3))
            AvailableYears.Add(y);
    }

    partial void OnSelectedYearChanged(int value) => _ = LoadAsync();

    partial void OnSelectedCategoryFilterChanged(Category? value) => ApplyCategoryFilter();

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var all = await _budgetService.GetAllBudgetItemsAsync();
            _allYearLines = all.Where(b => b.Period.Year == SelectedYear).ToList();

            var cats = await _budgetService.GetAllCategoriesAsync();
            var previousFilterId = SelectedCategoryFilter?.Id ?? 0;

            Categories.Clear();
            var allCat = new Category { Id = 0, Name = "Alle categorieën" };
            Categories.Add(allCat);
            foreach (var cat in cats.OrderBy(c => c.Name))
                Categories.Add(cat);

            // Restore previous selection or default to "Alle"
            SelectedCategoryFilter = previousFilterId > 0
                ? (Categories.FirstOrDefault(c => c.Id == previousFilterId) ?? allCat)
                : allCat;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyCategoryFilter()
    {
        var yearLines = _allYearLines;
        if (SelectedCategoryFilter != null && SelectedCategoryFilter.Id != 0)
            yearLines = yearLines.Where(b => b.CategoryId == SelectedCategoryFilter.Id).ToList();

        MonthGroups.Clear();

        for (int m = 1; m <= 12; m++)
        {
            var monthStart = new DateTime(SelectedYear, m, 1);

            var monthLines = yearLines
                .Where(b => b.Period.Month == m)
                .OrderByDescending(b => b.IsIncome)
                .ThenBy(b => b.Category?.Name)
                .ToList();

            var income = monthLines.Where(x => x.IsIncome).Sum(x => x.PlannedAmount);
            var expense = monthLines.Where(x => !x.IsIncome).Sum(x => x.PlannedAmount);

            MonthGroups.Add(new MonthlyBudgetGroup
            {
                MonthStart = monthStart,
                MonthLabel = monthStart.ToString("MMM", CultureInfo.CurrentCulture),
                MonthLabelLong = monthStart.ToString("MMMM", CultureInfo.CurrentCulture),
                AllLines = monthLines,
                Income = income,
                Expense = expense,
                Net = income - expense
            });
        }

        TotalIncome = MonthGroups.Sum(g => g.Income);
        TotalExpense = MonthGroups.Sum(g => g.Expense);
        TotalNet = TotalIncome - TotalExpense;
    }

    [RelayCommand]
    private async Task NavigateToBudgetLineDetail(BudgetLine? line)
    {
        if (line is null) return;
        await Shell.Current.GoToAsync(nameof(BudgetLineDetailPage), new Dictionary<string, object>
        {
            ["BudgetLineId"] = line.Id
        });
    }

    [RelayCommand]
    private async Task NavigateToCreateBudgetItem()
    {
        await Shell.Current.GoToAsync(nameof(CreateBudgetItemPage));
    }
}

public partial class MonthlyBudgetGroup : ObservableObject
{
    public DateTime MonthStart { get; set; }
    public string MonthLabel { get; set; } = "";
    public string MonthLabelLong { get; set; } = "";

    public List<BudgetLine> AllLines { get; set; } = new();

    [ObservableProperty]
    private ObservableCollection<BudgetLine> displayedLines = new();

    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }

    [ObservableProperty]
    private BudgetLineFilter selectedLineFilter = BudgetLineFilter.None;

    public bool IsIncomeFilterActive => SelectedLineFilter == BudgetLineFilter.Income;
    public bool IsExpenseFilterActive => SelectedLineFilter == BudgetLineFilter.Expense;
    public bool HasActiveFilter => SelectedLineFilter != BudgetLineFilter.None;

    partial void OnSelectedLineFilterChanged(BudgetLineFilter value)
    {
        OnPropertyChanged(nameof(IsIncomeFilterActive));
        OnPropertyChanged(nameof(IsExpenseFilterActive));
        OnPropertyChanged(nameof(HasActiveFilter));
        UpdateDisplayedLines();
    }

    private void UpdateDisplayedLines()
    {
        var filtered = SelectedLineFilter switch
        {
            BudgetLineFilter.Income => AllLines.Where(x => x.IsIncome),
            BudgetLineFilter.Expense => AllLines.Where(x => !x.IsIncome),
            _ => Enumerable.Empty<BudgetLine>()
        };
        DisplayedLines = new ObservableCollection<BudgetLine>(filtered);
    }

    [RelayCommand]
    private void FilterIncome()
    {
        SelectedLineFilter = SelectedLineFilter == BudgetLineFilter.Income
            ? BudgetLineFilter.None
            : BudgetLineFilter.Income;
    }

    [RelayCommand]
    private void FilterExpense()
    {
        SelectedLineFilter = SelectedLineFilter == BudgetLineFilter.Expense
            ? BudgetLineFilter.None
            : BudgetLineFilter.Expense;
    }
}