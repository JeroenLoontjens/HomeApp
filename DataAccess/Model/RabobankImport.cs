using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Model
{
    [Table("RabobankImport")]
    public class RabobankImport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Volgnr { get; set; } = string.Empty;

        [Required]
        [MaxLength(34)]
        public string IBAN { get; set; } = string.Empty;

        [Required]
        public DateTime Datum { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Bedrag { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal SaldoNaTrn { get; set; }

        [MaxLength(34)]
        public string? TegenrekeningIBAN { get; set; }

        [MaxLength(255)]
        public string? NaamTegenpartij { get; set; }

        [MaxLength(500)]
        public string? Omschrijving { get; set; }

        [MaxLength(10)]
        public string? Code { get; set; }

        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

        public RabobankImportStatus Status { get; set; } = RabobankImportStatus.Pending;

        public int? TransactionId { get; set; }

        [ForeignKey(nameof(TransactionId))]
        public Transaction? Transaction { get; set; }

        public int? SuggestedBudgetLineId { get; set; }

        [ForeignKey(nameof(SuggestedBudgetLineId))]
        public BudgetLine? SuggestedBudgetLine { get; set; }
    }
}
