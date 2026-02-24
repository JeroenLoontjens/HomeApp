using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DataAccess.Model
{
    [Table("Transaction")]
    public partial class Transaction : ObservableObject
    {
        [ObservableProperty]
        [property: Key]
        private int id;

        [ObservableProperty]
        [property: Required]
        private int categoryId;

        [ObservableProperty]
        [property: ForeignKey(nameof(CategoryId))]
        private Category category = null!;

        [ObservableProperty]
        [property: Required]
        private DateTime date;

        [ObservableProperty]
        [property: Required]
        [property: Column(TypeName = "decimal(10,2)")]
        private decimal amount;

        [ObservableProperty]
        private bool isIncome;

        [ObservableProperty]
        [property: Required]
        [property: MaxLength(2)]
        private StatusTrans status = StatusTrans.Manual;

        [ObservableProperty]
        [property: MaxLength(255)]
        private string? description;

        [ObservableProperty]
        private ObservableCollection<TransactionBudgetLine> transactionBudgetLines = new();

        public string SubText => $"{Date:dd MMM yyyy} • {Category.Name}";
       

        public Transaction()
        {
            transactionBudgetLines.CollectionChanged += TransactionBudgetLinesChanged;
        }


        //Together they cover both scenarios: swapping out the collection object and mutating the contents of the current one.

        partial void OnTransactionBudgetLinesChanged(ObservableCollection<TransactionBudgetLine> value)
        {
            value.CollectionChanged += TransactionBudgetLinesChanged;
            UpdateCategoryFromSplits();
            OnPropertyChanged(nameof(SubText));
        }

        private void TransactionBudgetLinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateCategoryFromSplits();
            OnPropertyChanged(nameof(SubText));
        }



        partial void OnDateChanged(DateTime value) => OnPropertyChanged(nameof(SubText));
        partial void OnStatusChanged(StatusTrans value) => OnPropertyChanged(nameof(SubText));
        partial void OnCategoryChanged(Category value) => OnPropertyChanged(nameof(SubText));



        private void UpdateCategoryFromSplits()
        {
            var dominant = TransactionBudgetLines
                .Where(tbl => tbl.BudgetLine?.Category != null)
                .GroupBy(tbl => tbl.BudgetLine!.Category)
                .Select(g => new { Category = g.Key, Amount = g.Sum(tbl => tbl.AmountAllocated) })
                .OrderByDescending(x => x.Amount)
                .FirstOrDefault();

            if (dominant is null)
                return;

            Category = dominant.Category;
            CategoryId = dominant.Category.Id;
        }
    }
}
