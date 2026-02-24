using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccess.Model
{
    [Table("BudgetLine")]
    public class BudgetLine 
    {
        [Key]
        public int Id { get; set; }

        [Required]        
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } 

        [Required]
        public DateTime Period { get; set; } 

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PlannedAmount { get; set; }

        public bool IsIncome { get; set; } = false;

        [MaxLength(255)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to related transactions
       
        public ICollection<TransactionBudgetLine> TransactionBudgetLines { get; set; } = new List<TransactionBudgetLine>();


    }
}
