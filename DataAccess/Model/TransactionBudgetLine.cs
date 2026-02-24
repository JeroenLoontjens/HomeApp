using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccess.Model
{
    public class TransactionBudgetLine
    {
        
        [property: Required]
        public int TransactionId { get; set; }

        [property: ForeignKey(nameof(TransactionId))]
        public Transaction Transaction { get; set; } = null!;

        
        [property: Required]
        public int BudgetLineId { get; set; }

        [property: ForeignKey(nameof(BudgetLineId))]
        public BudgetLine BudgetLine { get; set; } = null!;

       
        [property: Required]
        [property: Column(TypeName = "decimal(10,2)")]
        public decimal AmountAllocated { get; set; } // welk deel van de transactie is toegewezen
    }

}
