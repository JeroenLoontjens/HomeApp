using AppForLogin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataAccess.Model;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AppForLogin.ViewModel;

public partial class TransactionChartPageViewModel : ObservableObject
{
    private readonly BudgetService _budgetService;
    private List<Transaction> _allTransactions = new();
    private int _selectedYear = DateTime.Today.Year;
    private bool _isBusy;
    private decimal _totalIncome;
    private decimal _totalExpense;
    private decimal _totalNet;
    private double _maxChartValue = 1;

    public ObservableCollection<int> AvailableYears { get; } = new();
    public ObservableCollection<TransactionChartMonthItem> Months { get; } = new();

    public int SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (SetProperty(ref _selectedYear, value) && value != 0)
                BuildChart();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public decimal TotalIncome
    {
        get => _totalIncome;
        set => SetProperty(ref _totalIncome, value);
    }

    public decimal TotalExpense
    {
        get => _totalExpense;
        set => SetProperty(ref _totalExpense, value);
    }

    public decimal TotalNet
    {
        get => _totalNet;
        set => SetProperty(ref _totalNet, value);
    }

    public double MaxChartValue
    {
        get => _maxChartValue;
        set => SetProperty(ref _maxChartValue, value);
    }

    public TransactionChartPageViewModel(BudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            _allTransactions = await _budgetService.GetAllTransactionsAsync();

            var years = _allTransactions
                .Select(t => t.Date.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            AvailableYears.Clear();
            foreach (var year in years)
                AvailableYears.Add(year);

            if (AvailableYears.Count == 0)
                AvailableYears.Add(DateTime.Today.Year);

            if (!AvailableYears.Contains(SelectedYear))
                SelectedYear = AvailableYears.First();
            else
                BuildChart();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BuildChart()
    {
        var yearTransactions = _allTransactions
            .Where(t => t.Date.Year == SelectedYear)
            .ToList();

        var monthlyData = Enumerable.Range(1, 12)
            .Select(month =>
            {
                var monthTransactions = yearTransactions.Where(t => t.Date.Month == month).ToList();
                var income = monthTransactions.Where(t => t.IsIncome).Sum(t => t.Amount);
                var expense = monthTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
                var maxValue = Math.Max(income, expense);

                return new TransactionChartMonthItem
                {
                    MonthNumber = month,
                    MonthLabel = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month),
                    Income = income,
                    Expense = expense,
                    Net = income - expense,
                    ChartValue = (double)maxValue
                };
            })
            .ToList();

        MaxChartValue = Math.Max(1d, monthlyData.Max(x => x.ChartValue));

        Months.Clear();
        foreach (var item in monthlyData)
        {
            item.MaxChartValue = MaxChartValue;
            Months.Add(item);
        }

        TotalIncome = monthlyData.Sum(x => x.Income);
        TotalExpense = monthlyData.Sum(x => x.Expense);
        TotalNet = TotalIncome - TotalExpense;
    }

    [RelayCommand]
    private async Task ReturnAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

public partial class TransactionChartMonthItem : ObservableObject
{
    public int MonthNumber { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }
    public double ChartValue { get; set; }
    public double MaxChartValue { get; set; } = 1;

    public double IncomeHeight => MaxChartValue <= 0 ? 0 : Math.Max(4, (double)Income / MaxChartValue * 140d);
    public double ExpenseHeight => MaxChartValue <= 0 ? 0 : Math.Max(4, (double)Expense / MaxChartValue * 140d);
    public bool HasIncome => Income > 0;
    public bool HasExpense => Expense > 0;
}
