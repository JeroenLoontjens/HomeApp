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

public partial class BudgetPlanPageViewModel : ObservableObject
{
    private readonly BudgetService _budgetService;
    private List<BudgetLine> _allYearLines = new();
    private List<Category> _allCategories = new();

    public ObservableCollection<int> AvailableYears { get; } = new();
    public ObservableCollection<Category> RootCategories { get; } = new();
    public ObservableCollection<Category> SubCategories { get; } = new();

    [ObservableProperty] private int selectedYear;
    [ObservableProperty] private BudgetPlanLayoutMode layoutMode = BudgetPlanLayoutMode.Vertical;
    [ObservableProperty] private bool isBusy;

    [ObservableProperty] private ObservableCollection<MonthlyBudgetGroup> monthGroups = new();

    [ObservableProperty] private decimal totalIncome;
    [ObservableProperty] private decimal totalExpense;
    [ObservableProperty] private decimal totalNet;

    [ObservableProperty] private Category? selectedRootCategory;
    [ObservableProperty] private Category? selectedSubCategory;

    public bool HasSubCategories => SubCategories.Count > 0;

    public BudgetPlanPageViewModel(BudgetService budgetService)
    {
        _budgetService = budgetService;

        var now = DateTime.Today;
        SelectedYear = now.Year;

        foreach (var y in Enumerable.Range(now.Year - 1, 3))
            AvailableYears.Add(y);
    }

    partial void OnSelectedYearChanged(int value) => _ = LoadAsync();

    partial void OnSelectedRootCategoryChanged(Category? value)
    {
        UpdateSubCategories();
        ApplyCategoryFilter();
    }

    partial void OnSelectedSubCategoryChanged(Category? value) => ApplyCategoryFilter();

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var all = await _budgetService.GetAllBudgetItemsAsync();
            _allYearLines = all.Where(b => b.Period.Year == SelectedYear).ToList();

            _allCategories = await _budgetService.GetAllCategoriesAsync();

            var previousRootId = SelectedRootCategory?.Id ?? 0;

            RootCategories.Clear();
            var allCat = new Category { Id = 0, Name = "Alle categorieën" };
            RootCategories.Add(allCat);
            foreach (var cat in _allCategories.Where(c => c.ParentCategoryId == null).OrderBy(c => c.Name))
                RootCategories.Add(cat);

            SelectedRootCategory = previousRootId > 0
                ? (RootCategories.FirstOrDefault(c => c.Id == previousRootId) ?? allCat)
                : allCat;
        }
        finally
        {
            IsBusy = false;
        }
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

            if (subs.Count > 0)
            {
                var allSub = new Category { Id = 0, Name = "Alle subcategorieën" };
                SubCategories.Add(allSub);
                foreach (var sub in subs)
                    SubCategories.Add(sub);

                SelectedSubCategory = allSub;
            }
        }

        OnPropertyChanged(nameof(HasSubCategories));
    }

    private void ApplyCategoryFilter()
    {
        var yearLines = _allYearLines;

        if (SelectedSubCategory != null && SelectedSubCategory.Id != 0)
        {
            yearLines = yearLines.Where(b => b.CategoryId == SelectedSubCategory.Id).ToList();
        }
        else if (SelectedRootCategory != null && SelectedRootCategory.Id != 0)
        {
            var subIds = _allCategories
                .Where(c => c.ParentCategoryId == SelectedRootCategory.Id)
                .Select(c => c.Id)
                .ToHashSet();
            yearLines = yearLines
                .Where(b => b.CategoryId == SelectedRootCategory.Id || subIds.Contains(b.CategoryId))
                .ToList();
        }

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

    [ObservableProperty] private bool showIncome = false;
    [ObservableProperty] private bool showExpense = false;

    public bool HasActiveFilter => ShowIncome || ShowExpense;

    partial void OnShowIncomeChanged(bool value)
    {
        OnPropertyChanged(nameof(HasActiveFilter));
        UpdateDisplayedLines();
    }

    partial void OnShowExpenseChanged(bool value)
    {
        OnPropertyChanged(nameof(HasActiveFilter));
        UpdateDisplayedLines();
    }

    private void UpdateDisplayedLines()
    {
        var filtered = AllLines.Where(x => (x.IsIncome && ShowIncome) || (!x.IsIncome && ShowExpense));
        DisplayedLines = new ObservableCollection<BudgetLine>(filtered);
    }

    [RelayCommand]
    private void FilterIncome()
    {
        ShowIncome = !ShowIncome;
    }

    [RelayCommand]
    private void FilterExpense()
    {
        ShowExpense = !ShowExpense;
    }
}