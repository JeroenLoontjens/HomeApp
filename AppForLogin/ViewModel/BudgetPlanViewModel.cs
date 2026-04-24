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

    [ObservableProperty] private Category? selectedRootCategoryFilter;
    [ObservableProperty] private Category? selectedSubCategoryFilter;
    [ObservableProperty] private bool isSubCategoryFilterEnabled;
    private bool _showAllIncomeLines;
    private bool _showAllExpenseLines;

    public bool ShowAllIncomeLines
    {
        get => _showAllIncomeLines;
        set
        {
            if (SetProperty(ref _showAllIncomeLines, value))
                ApplyGlobalLineFilters();
        }
    }

    public bool ShowAllExpenseLines
    {
        get => _showAllExpenseLines;
        set
        {
            if (SetProperty(ref _showAllExpenseLines, value))
                ApplyGlobalLineFilters();
        }
    }

    public BudgetPlanPageViewModel(BudgetService budgetService)
    {
        _budgetService = budgetService;

        var now = DateTime.Today;
        SelectedYear = now.Year;

        foreach (var y in Enumerable.Range(now.Year - 1, 3))
            AvailableYears.Add(y);
    }

    partial void OnSelectedYearChanged(int value) => _ = LoadAsync();

    partial void OnSelectedRootCategoryFilterChanged(Category? value)
    {
        UpdateSubCategories();
        ApplyCategoryFilter();
    }

    partial void OnSelectedSubCategoryFilterChanged(Category? value) => ApplyCategoryFilter();

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
            _allCategories = cats.OrderBy(c => c.Name).ToList();

            var previousRootFilterId = SelectedRootCategoryFilter?.Id ?? 0;
            var previousSubFilterId = SelectedSubCategoryFilter?.Id ?? 0;

            RootCategories.Clear();
            var allRootCat = new Category { Id = 0, Name = "Alle hoofdcategorieën" };
            RootCategories.Add(allRootCat);

            foreach (var cat in _allCategories.Where(c => c.ParentCategoryId == null))
                RootCategories.Add(cat);

            SelectedRootCategoryFilter = previousRootFilterId > 0
                ? (RootCategories.FirstOrDefault(c => c.Id == previousRootFilterId) ?? allRootCat)
                : allRootCat;

            var selectedSub = previousSubFilterId > 0
                ? SubCategories.FirstOrDefault(c => c.Id == previousSubFilterId)
                : null;

            SelectedSubCategoryFilter = selectedSub ?? SubCategories.FirstOrDefault();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyCategoryFilter()
    {
        var yearLines = _allYearLines;

        if (SelectedSubCategoryFilter != null && SelectedSubCategoryFilter.Id != 0)
        {
            yearLines = yearLines.Where(b => b.CategoryId == SelectedSubCategoryFilter.Id).ToList();
        }
        else if (SelectedRootCategoryFilter != null && SelectedRootCategoryFilter.Id != 0)
        {
            var categoryIds = GetDescendantCategoryIds(SelectedRootCategoryFilter.Id);
            yearLines = yearLines.Where(b => categoryIds.Contains(b.CategoryId)).ToList();
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

        ApplyGlobalLineFilters();
    }

    private void ApplyGlobalLineFilters()
    {
        foreach (var monthGroup in MonthGroups)
        {
            monthGroup.ShowIncomeLines = ShowAllIncomeLines;
            monthGroup.ShowExpenseLines = ShowAllExpenseLines;
        }
    }

    private void UpdateSubCategories()
    {
        var previousSubFilterId = SelectedSubCategoryFilter?.Id ?? 0;
        var allSubCat = new Category { Id = 0, Name = "Alle subcategorieën" };

        SubCategories.Clear();
        SubCategories.Add(allSubCat);

        IEnumerable<Category> filteredSubCategories = SelectedRootCategoryFilter?.Id > 0
            ? _allCategories.Where(c => IsDescendantOf(SelectedRootCategoryFilter.Id, c))
            : _allCategories.Where(c => c.ParentCategoryId != null);

        foreach (var cat in filteredSubCategories.OrderBy(c => c.Name))
            SubCategories.Add(cat);

        IsSubCategoryFilterEnabled = SubCategories.Count > 1;

        SelectedSubCategoryFilter = previousSubFilterId > 0
            ? (SubCategories.FirstOrDefault(c => c.Id == previousSubFilterId) ?? allSubCat)
            : allSubCat;
    }

    private HashSet<int> GetDescendantCategoryIds(int rootCategoryId)
    {
        var descendantIds = _allCategories
            .Where(c => IsDescendantOf(rootCategoryId, c))
            .Select(c => c.Id)
            .ToHashSet();

        return descendantIds;
    }

    private bool IsDescendantOf(int rootCategoryId, Category category)
    {
        var parentId = category.ParentCategoryId;

        while (parentId.HasValue)
        {
            if (parentId.Value == rootCategoryId)
                return true;

            parentId = _allCategories.FirstOrDefault(c => c.Id == parentId.Value)?.ParentCategoryId;
        }

        return false;
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

    

    private bool _showIncomeLines;
    private bool _showExpenseLines;

    public bool ShowIncomeLines
    {
        get => _showIncomeLines;
        set
        {
            if (SetProperty(ref _showIncomeLines, value))
            {
                OnPropertyChanged(nameof(IsIncomeFilterActive));
                OnPropertyChanged(nameof(HasActiveFilter));
                UpdateDisplayedLines();
            }
        }
    }

    public bool ShowExpenseLines
    {
        get => _showExpenseLines;
        set
        {
            if (SetProperty(ref _showExpenseLines, value))
            {
                OnPropertyChanged(nameof(IsExpenseFilterActive));
                OnPropertyChanged(nameof(HasActiveFilter));
                UpdateDisplayedLines();
            }
        }
    }

    public bool IsIncomeFilterActive => ShowIncomeLines;
    public bool IsExpenseFilterActive => ShowExpenseLines;
    public bool HasActiveFilter => ShowIncomeLines || ShowExpenseLines;

    private void UpdateDisplayedLines()
    {
        var filtered = AllLines.Where(x =>
            (ShowIncomeLines && x.IsIncome) ||
            (ShowExpenseLines && !x.IsIncome));

        DisplayedLines = new ObservableCollection<BudgetLine>(filtered);
    }

    [RelayCommand]
    private void FilterIncome()
    {
        ShowIncomeLines = !ShowIncomeLines;
    }

    [RelayCommand]
    private void FilterExpense()
    {
        ShowExpenseLines = !ShowExpenseLines;
    }

    
}