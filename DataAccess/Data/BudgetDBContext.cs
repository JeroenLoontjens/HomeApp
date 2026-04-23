using DataAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Data
{
    public class BudgetDBContext : DbContext
    {
        public DbSet<BudgetLine> BudgetLines { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        // Optioneel: junction table voor splitsingen
        public DbSet<TransactionBudgetLine> TransactionBudgetLines { get; set; }

        public DbSet<RabobankImport> RabobankImports { get; set; }

        public BudgetDBContext(DbContextOptions<BudgetDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tabelnamen expliciet instellen (optioneel)
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<BudgetLine>().ToTable("BudgetLine");
            modelBuilder.Entity<Transaction>().ToTable("Transaction");
            modelBuilder.Entity<TransactionBudgetLine>().ToTable("TransactionBudgetLine");
            modelBuilder.Entity<RabobankImport>().ToTable("RabobankImport");

            // Category: self reference category voor hierarchie
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Category -> BudgetLine (1 - 0..*)
            modelBuilder.Entity<BudgetLine>()
                .HasOne(b => b.Category)
                .WithMany(c => c.BudgetLines)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            
            // Transaction -> Category (many - 1)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enum mapping: StatusTrans -> string (en MySQL enum kolomtype)
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (StatusTrans)Enum.Parse(typeof(StatusTrans), v))
                .HasColumnType("enum('Manual','Imported','Pending','Approved','Rejected')")
                .IsRequired();

            // Indexen voor performance
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.CategoryId, t.Date })
                .HasDatabaseName("idx_transaction_category_date");
                                   
            modelBuilder.Entity<BudgetLine>()
                .HasIndex(b => new { b.CategoryId, b.Period })
                .HasDatabaseName("idx_budgetline_category_period");

            modelBuilder.Entity<TransactionBudgetLine>()
                .HasKey(tbl => new { tbl.TransactionId, tbl.BudgetLineId });

            modelBuilder.Entity<TransactionBudgetLine>()
                .HasOne(tbl => tbl.Transaction)
                .WithMany(t => t.TransactionBudgetLines)
                .HasForeignKey(tbl => tbl.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransactionBudgetLine>()
                .HasOne(tbl => tbl.BudgetLine)
                .WithMany(b => b.TransactionBudgetLines)
                .HasForeignKey(tbl => tbl.BudgetLineId)
                .OnDelete(DeleteBehavior.Cascade);

            // Kolomtypen en precisie (optioneel, expliciet)
            modelBuilder.Entity<BudgetLine>()
                .Property(b => b.PlannedAmount)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(10,2)");

            // Timestamps: default waarden kunnen via DB of SaveChanges worden ingesteld.
            // (Je kunt hier ook ValueGeneratedOnAdd/OnUpdate configureren indien gewenst.)

            // RabobankImport configuratie
            modelBuilder.Entity<RabobankImport>()
                .HasIndex(r => r.Volgnr)
                .IsUnique()
                .HasDatabaseName("idx_rabobankimport_volgnr");

            modelBuilder.Entity<RabobankImport>()
                .Property(r => r.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (RabobankImportStatus)Enum.Parse(typeof(RabobankImportStatus), v))
                .HasColumnType("enum('Pending','Processed','Skipped','Duplicate')")
                .IsRequired();

            modelBuilder.Entity<RabobankImport>()
                .Property(r => r.Bedrag)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<RabobankImport>()
                .Property(r => r.SaldoNaTrn)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<RabobankImport>()
                .HasOne(r => r.Transaction)
                .WithMany()
                .HasForeignKey(r => r.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RabobankImport>()
                .HasOne(r => r.SuggestedBudgetLine)
                .WithMany()
                .HasForeignKey(r => r.SuggestedBudgetLineId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
