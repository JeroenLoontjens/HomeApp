using AppForLogin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataAccess.Model;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private decimal SplitTotal => Transaction?.TransactionBudgetLines.Sum(x => x.AmountAllocated) ?? 0;
        private bool SplitsMatchTotal => Transaction != null && SplitTotal == Transaction.Amount;


        public ObservableCollection<BudgetLine> BudgetLines { get; } = [];
        public ObservableCollection<BudgetLine> FilteredBudgetLines { get; } = [];

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

        partial void OnBudgetLineSearchTextChanged(string value)
        {
                ApplyBudgetLineFilter();
        }
        

        private void ApplyBudgetLineFilter()
        {
            FilteredBudgetLines.Clear();
            var filtered = BudgetLines.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(BudgetLineSearchText))
            {
                filtered = filtered.Where(b =>
                    b.Category?.Name?.Contains(BudgetLineSearchText, StringComparison.OrdinalIgnoreCase) == true
                    || b.Notes?.Contains(BudgetLineSearchText, StringComparison.OrdinalIgnoreCase) == true);
            }

            foreach (var item in filtered)
                FilteredBudgetLines.Add(item);
        }      



        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Transaction", out var value) && value is Transaction t)
            {
                if (t.Date == default)
                {
                    t.Date = DateTime.Now;
                }
                Transaction = t;
            }
            else
            {
                // No ID means we're creating a new transaction
                Transaction = new Transaction { Date = DateTime.Now };
            }
        }

        #region Split Commands 
        [RelayCommand]
        private void AddSplit()
        {
            if (SelectedBudgetLine == null || SplitAmount <= 0)
                return;

            Transaction.TransactionBudgetLines.Add(new TransactionBudgetLine
            {
                BudgetLine = SelectedBudgetLine,
                BudgetLineId = SelectedBudgetLine.Id,
                AmountAllocated = SplitAmount
            });

            // reset input
            SelectedBudgetLine = null;
            SplitAmount = 0;
            BudgetLineSearchText = string.Empty;
        }

        [RelayCommand]
        private void RemoveSplit(TransactionBudgetLine line)
        {
            if (line == null) return;
            Transaction.TransactionBudgetLines.Remove(line);
        }
        #endregion

        #region Transaction Commands
        [RelayCommand]
        private async Task SaveAsync()
        {
            if (Transaction is null)
                return;

            if (Transaction.TransactionBudgetLines.Count > 0 && !SplitsMatchTotal)
            {
                await Shell.Current.DisplayAlertAsync("Split klopt niet",
                $"Totaal splits ({SplitTotal:0.00}) moet gelijk zijn aan transactie bedrag ({Transaction.Amount:0.00})","OK");
                return;
            }

            try
            {
                IsBusy = true;
                if (Transaction.Id == 0)
                {
                    await _budgetService.AddTransactionAsync(Transaction);
                }
                else
                {
                    await _budgetService.UpdateTransactionAsync(Transaction);
                }

                // Navigate back
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
            await Shell.Current.GoToAsync("..");
        }
        #endregion
        
        

    }
}