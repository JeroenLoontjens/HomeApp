using AppForLogin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataAccess.Model;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AppForLogin.ViewModel
{
    [QueryProperty(nameof(Transaction), "Transaction")]
    public partial class TransactionDetailViewModel : ObservableObject, IQueryAttributable
    {
        private readonly BudgetService _budgetService;

        [ObservableProperty] private Transaction transaction;
        [ObservableProperty] private bool isBusy;

        [ObservableProperty] private string budgetLineSearchText;
        [ObservableProperty] private BudgetLine selectedBudgetLine;
        [ObservableProperty] private decimal splitAmount;
        [ObservableProperty] private DateTime budgetLineStartMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        [ObservableProperty] private DateTime budgetLineEndMonth = new(DateTime.Today.Year, 12, 1);
        [ObservableProperty] private Category selectedBudgetLineCategoryFilter;

        private decimal SplitTotal => Transaction?.TransactionBudgetLines.Sum(x => x.AmountAllocated) ?? 0;
        private bool SplitsMatchTotal => Transaction != null && SplitTotal == Transaction.Amount;

        private Transaction _navigationTransaction;
        private Transaction _originalTransaction;

        public ObservableCollection<BudgetLine> BudgetLines { get; } = [];
        public ObservableCollection<BudgetLine> FilteredBudgetLines { get; } = [];
        public ObservableCollection<Category> BudgetLineCategories { get; } = [];

        public TransactionDetailViewModel(BudgetService budgetService)
        {
            _budgetService = budgetService ?? throw new ArgumentNullException(nameof(budgetService));
            _ = LoadBudgetLinesAsync();
        }

        private async Task LoadBudgetLinesAsync()
        {
            var lines = await _budgetService.GetAllBudgetItemsAsync();
            BudgetLines.Clear();
            foreach (var line in lines)
                BudgetLines.Add(line);

            ApplyBudgetLineFilter();
        }

        private async Task LoadBudgetLineCategoriesAsync()
        {
            var categories = await _budgetService.GetAllCategoriesAsync();

            BudgetLineCategories.Clear();

            var allCategories = new Category { Id = 0, Name = "Alle categorieën" };
            BudgetLineCategories.Add(allCategories);

            foreach (var category in categories.OrderBy(c => c.Name))
                BudgetLineCategories.Add(category);

            SelectedBudgetLineCategoryFilter = BudgetLineCategories.FirstOrDefault();
        }

        partial void OnBudgetLineSearchTextChanged(string value) => ApplyBudgetLineFilter();
        partial void OnBudgetLineStartMonthChanged(DateTime value) => ApplyBudgetLineFilter();
        partial void OnBudgetLineEndMonthChanged(DateTime value) => ApplyBudgetLineFilter();
        partial void OnSelectedBudgetLineCategoryFilterChanged(Category value) => ApplyBudgetLineFilter();

        private void ApplyBudgetLineFilter()
        {
            FilteredBudgetLines.Clear();
            var filtered = BudgetLines.AsEnumerable();

            var startMonth = new DateTime(BudgetLineStartMonth.Year, BudgetLineStartMonth.Month, 1);
            var endMonth = new DateTime(BudgetLineEndMonth.Year, BudgetLineEndMonth.Month, 1);

            if (startMonth > endMonth)
                (startMonth, endMonth) = (endMonth, startMonth);

            filtered = filtered.Where(b =>
            {
                var budgetMonth = new DateTime(b.Period.Year, b.Period.Month, 1);
                return budgetMonth >= startMonth && budgetMonth <= endMonth;
            });

            if (SelectedBudgetLineCategoryFilter?.Id > 0)
                filtered = filtered.Where(b => b.CategoryId == SelectedBudgetLineCategoryFilter.Id);

            if (!string.IsNullOrWhiteSpace(BudgetLineSearchText))
            {
                filtered = filtered.Where(b =>
                    b.Category?.Name?.Contains(BudgetLineSearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    b.Notes?.Contains(BudgetLineSearchText, StringComparison.OrdinalIgnoreCase) == true);
            }

            foreach (var item in filtered
                .OrderByDescending(b => b.Period)
                .ThenBy(b => b.Category?.Name)
                .ThenBy(b => b.Notes))
                FilteredBudgetLines.Add(item);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Transaction", out var value) && value is Transaction t)
            {
                if (t.Date == default)
                    t.Date = DateTime.Now;

                _navigationTransaction = t;
                _originalTransaction = CloneTransaction(t);
                Transaction = CloneTransaction(t);
                InitializeBudgetLineFilterRange(t.Date);
            }
            else
            {
                var newTransaction = new Transaction { Date = DateTime.Now };
                _navigationTransaction = newTransaction;
                _originalTransaction = CloneTransaction(newTransaction);
                Transaction = CloneTransaction(newTransaction);
                InitializeBudgetLineFilterRange(newTransaction.Date);
            }

            _ = LoadBudgetLineCategoriesAsync();
            ApplyBudgetLineFilter();
        }

        [RelayCommand]
        private void ResetBudgetLineFilters()
        {
            InitializeBudgetLineFilterRange(Transaction?.Date ?? DateTime.Today);
            BudgetLineSearchText = string.Empty;
            SelectedBudgetLineCategoryFilter = BudgetLineCategories.FirstOrDefault();
            ApplyBudgetLineFilter();
        }

        #region Split Commands
        [RelayCommand]
        private async Task AddSplit()
        {
            if (SelectedBudgetLine == null || SplitAmount <= 0 || Transaction is null)
                return;

            Transaction.TransactionBudgetLines ??= new ObservableCollection<TransactionBudgetLine>(); // dit betekent dat er nog geen splits zijn, dus we maken een nieuwe lijst aan

            var existingSplit = Transaction.TransactionBudgetLines
                .FirstOrDefault(x => x.BudgetLineId == SelectedBudgetLine.Id);

            if (existingSplit != null)
            {
                var index = Transaction.TransactionBudgetLines.IndexOf(existingSplit);
                var mergedSplit = new TransactionBudgetLine
                {
                    TransactionId = existingSplit.TransactionId,
                    BudgetLineId = existingSplit.BudgetLineId,
                    BudgetLine = existingSplit.BudgetLine,
                    AmountAllocated = existingSplit.AmountAllocated + SplitAmount
                };

                Transaction.TransactionBudgetLines[index] = mergedSplit;

                await Shell.Current.DisplayAlertAsync(
                    "Split samengevoegd",
                    "Deze budgetlijn stond al in de splits. Het bedrag is toegevoegd aan de bestaande split.",
                    "OK");

            }
            else
            {
                Transaction.TransactionBudgetLines.Add(new TransactionBudgetLine
                {
                    BudgetLine = SelectedBudgetLine,
                    BudgetLineId = SelectedBudgetLine.Id,
                    AmountAllocated = SplitAmount
                });
            }

            SelectedBudgetLine = null;
            SplitAmount = 0;
            BudgetLineSearchText = string.Empty;
        }

        [RelayCommand]
        private void RemoveSplit(TransactionBudgetLine line)
        {
            if (line == null || Transaction?.TransactionBudgetLines == null)
                return;

            Transaction.TransactionBudgetLines.Remove(line);
        }
        #endregion

        #region Transaction Commands
        [RelayCommand]
        private async Task SaveAsync()
        {
            if (Transaction is null)
                return;

            if (Transaction.TransactionBudgetLines?.Count > 0 && !SplitsMatchTotal)
            {
                await Shell.Current.DisplayAlertAsync("Split klopt niet",
                    $"Totaal splits ({SplitTotal:0.00}) moet gelijk zijn aan transactie bedrag ({Transaction.Amount:0.00})", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Transaction.Description))
            {
                await Shell.Current.DisplayAlertAsync("No description", "Er lijkt geen omschrijving te zijn ingevuld. Een transactie is beter terugvindbaar met een omschrijving.", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                if (Transaction.Id == 0)
                    await _budgetService.AddTransactionAsync(Transaction);
                else
                    await _budgetService.UpdateTransactionAsync(Transaction);

                CopyTransaction(_navigationTransaction, Transaction);
                _originalTransaction = CloneTransaction(Transaction);

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to save transaction: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (Transaction is null || Transaction.Id == 0)
                return;

            try
            {
                IsBusy = true;
                await _budgetService.DeleteTransactionAsync(Transaction.Id);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to delete transaction: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ApproveAsync()
        {
            if (Transaction is null)
                return;

            try
            {
                IsBusy = true;
                Transaction.Status = StatusTrans.Approved;

                await _budgetService.UpdateTransactionAsync(Transaction);
                CopyTransaction(_navigationTransaction, Transaction);
                _originalTransaction = CloneTransaction(Transaction);

                await Shell.Current.DisplayAlertAsync("Success", "Transaction approved", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to approve transaction: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            if (_originalTransaction != null)
            {
                CopyTransaction(_navigationTransaction, _originalTransaction);
                Transaction = CloneTransaction(_originalTransaction);
            }

            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ReturnAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
        #endregion

        private Transaction CloneTransaction(Transaction source)
        {
            if (source == null)
                return null;

            return new Transaction
            {
                Id = source.Id,
                Date = source.Date,
                Amount = source.Amount,
                Description = source.Description,
                Status = source.Status,
                IsIncome = source.IsIncome,
                CategoryId = source.CategoryId,
                Category = source.Category,
                TransactionBudgetLines = new ObservableCollection<TransactionBudgetLine>(
                    (source.TransactionBudgetLines ?? Enumerable.Empty<TransactionBudgetLine>())
                        .Select(CloneTransactionBudgetLine))
            };
        }

        private TransactionBudgetLine CloneTransactionBudgetLine(TransactionBudgetLine source)
            => new()
            {
                
                TransactionId = source.TransactionId,
                BudgetLineId = source.BudgetLineId,
                BudgetLine = source.BudgetLine,
                AmountAllocated = source.AmountAllocated
            };

        private void CopyTransaction(Transaction target, Transaction source)
        {
            if (target == null || source == null)
                return;

            target.Id = source.Id;
            target.Date = source.Date;
            target.Amount = source.Amount;
            target.Description = source.Description;
            target.Status = source.Status;
            target.IsIncome = source.IsIncome;
            target.CategoryId = source.CategoryId;
            target.Category = source.Category;

            if (target.TransactionBudgetLines is null)
                target.TransactionBudgetLines = new ObservableCollection<TransactionBudgetLine>();
            else
                target.TransactionBudgetLines.Clear();

            foreach (var line in source.TransactionBudgetLines ?? Enumerable.Empty<TransactionBudgetLine>())
                target.TransactionBudgetLines.Add(CloneTransactionBudgetLine(line));
        }

        private void InitializeBudgetLineFilterRange(DateTime referenceDate)
        {
            var effectiveDate = referenceDate == default ? DateTime.Today : referenceDate;
            var startDate = effectiveDate.AddMonths(-1);

            BudgetLineStartMonth = new DateTime(startDate.Year, startDate.Month, 1);
            BudgetLineEndMonth = new DateTime(effectiveDate.Year, effectiveDate.Month, 1);
        }
    }
}