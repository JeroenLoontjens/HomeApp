using AppForLogin.Views;
using DataAccess.Model;
using AppForLogin.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace AppForLogin.ViewModel
{
    public partial class TransactionsOverViewModel : ObservableObject
    {
        private readonly BudgetService _budgetService;
        private bool _isLoading = false;
                

        //Filter properties
        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private StatusTrans? selectedStatus;

        public List<StatusTrans> StatusOptions { get; } = Enum.GetValues<StatusTrans>().ToList();
        
        //periode filtering
        [ObservableProperty]
        private MonthOption selectedMonth;

        [ObservableProperty]
        private int selectedYear = DateTime.Now.Year;   

        [ObservableProperty]
        private bool selectshowAll;

        public bool ShowPeriodFilters => !SelectshowAll;

        public ObservableCollection<MonthOption> MonthOptions { get; } = new();
        public ObservableCollection<int> Years { get; } = new();

        //
        public Transaction Transaction { get; set; }
        public ObservableCollection<Transaction> Transactions { get; set; } = new();
        public ObservableCollection<Transaction> FilteredTransactions { get; } = new();


        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        //Constructor
        public TransactionsOverViewModel(BudgetService budgetService)
        {
            _budgetService = budgetService;
            FillMonthOptions();
            FillYears();    
            SelectedMonth = MonthOptions.FirstOrDefault(m => m.MonthNumber == DateTime.Now.Month);
            //_ = LoadTransactionsAsync();
        }

        public void FillMonthOptions()
        {
            foreach (var month in Enumerable.Range(1,12))
            {
                MonthOptions.Add(new MonthOption
                {
                    MonthNumber = month,
                    Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                });
            }
        }

        public void FillYears()
        {
            var currYear = DateTime.Now.Year;
            foreach (var year in Enumerable.Range(currYear - 10, 21))
            {
                Years.Add(year);
            }
        }
        
        public async Task LoadTransactionsAsync()
        {
            if (IsLoading) return;
            
            try
            {
                IsLoading = true;

                List<Transaction> transactions;
                if (SelectshowAll)
                {
                    transactions = await _budgetService.GetAllTransactionsAsync();
                }
                else
                {
                    var month = SelectedMonth?.MonthNumber ?? DateTime.Now.Month;
                    var date = new DateTime(SelectedYear, month, 1);
                    transactions = await _budgetService.GetTransactionsPerMonthAsync(date);
                }

                Transactions.Clear();
                foreach (var transaction in transactions)
                {
                    Transactions.Add(transaction);
                }
                ApplyFilters();

            }
            catch(Exception ex)
            { 
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to load transactions: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }

        }


        //OnChanged
        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }
        partial void OnSelectedStatusChanged(StatusTrans? value)
        {
            ApplyFilters();
        }
        partial void OnSelectedMonthChanged(MonthOption value)
        {
            // Use the parameter to avoid IDE0060
            if (value != null)
            {
                _ = LoadTransactionsAsync();
            }
        }
        partial void OnSelectedYearChanged(int value)
        {
            if (value != 0)
                { _ = LoadTransactionsAsync(); }
        }
        partial void OnSelectshowAllChanged(bool value)
        {
            OnPropertyChanged(nameof(ShowPeriodFilters));
            _ = LoadTransactionsAsync();
        }
        public void OnTransactionSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            { Transaction = (Transaction)e.SelectedItem; }
        }


        //Commands
        [RelayCommand]
        private void OpenDetailPage(Transaction transaction)
        {
            if (transaction != null)
            {
                // Navigate to the TransactionDetailPage with the selected transaction
                Shell.Current.GoToAsync(nameof(TransactionDetailPage), true, new Dictionary<string, object>
                {
                    { "Transaction", transaction }
                });
            }
        }
             
        [RelayCommand]
        private void ApplyFilters()
        {
            FilteredTransactions.Clear();
            var filtered = Transactions.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t => t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }
            if (SelectedStatus.HasValue)
            {
                filtered = filtered.Where(t => t.Status == SelectedStatus.Value);
            }
            foreach (var transaction in filtered)
            {
                FilteredTransactions.Add(transaction);
            }
        }

        [RelayCommand]
        private void ResetFilters()
        {
            SearchText = string.Empty;
            SelectedStatus = null;
            ApplyFilters();
        }
                
        [RelayCommand]
        private void CreateTransaction()
        {
            var transaction = new Transaction();
            Shell.Current.GoToAsync(nameof(TransactionDetailPage), true, new Dictionary<string, object>
                {
                    { "Transaction", transaction }
                });

        }

        [RelayCommand]
        private async Task OpenTransactionChart()
        {
            await Shell.Current.GoToAsync(nameof(TransactionChartPage));
        }

        [RelayCommand]
        private async Task ReLoadTransactions()
        {
            await LoadTransactionsAsync();
        }
    }

    public class MonthOption
    {
        public int MonthNumber { get; set; }
        public string Name { get; set; }
    }
}