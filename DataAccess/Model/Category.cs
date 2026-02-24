using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccess.Model
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(100)]
        public string Icon { get; set; }

        [MaxLength(7)]
        public string ColorHex { get; set; }          // optioneel: kleur voor grafieken

        //Self reference for subcategories
        public int? ParentCategoryId { get; set; }

        [ForeignKey(nameof(ParentCategoryId))]
        public Category ParentCategory { get; set; } 

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        //Navigatie
        public ICollection<BudgetLine>BudgetLines { get; set; } = new List<BudgetLine>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    }

}
