using DataAccess.Data;
using DataAccess.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForLogin.Services
{
    public class BudgetService
    {
        private readonly IDbContextFactory<BudgetDBContext> _contextFactory;

        public BudgetService(IDbContextFactory<BudgetDBContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        #region BudgetItem Methods
        public async Task<List<BudgetLine>> GetAllBudgetItemsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BudgetLines
                .AsNoTracking()
                .Include(b => b.Category)
                .Include(b => b.TransactionBudgetLines)
                    .ThenInclude(tbl => tbl.Transaction)
                        .ThenInclude(t => t.Category)
                .ToListAsync();
        }

        public async Task<BudgetLine?> GetBudgetItemByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.BudgetLines
                .AsNoTracking()
                .Include(b => b.Category)
                .Include(b => b.TransactionBudgetLines)
                    .ThenInclude(tbl => tbl.Transaction)
                        .ThenInclude(t => t.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddBudgetItemAsync(BudgetLine item)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.BudgetLines.Add(item);
            await context.SaveChangesAsync();
        }

        public async Task AddBudgetItemsAsync(IEnumerable<BudgetLine> items)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.BudgetLines.AddRange(items);
            await context.SaveChangesAsync();
        }

        public async Task UpdateBudgetItemAsync(BudgetLine item)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.BudgetLines.Update(item);
            await context.SaveChangesAsync();
        }

        public async Task DeleteBudgetItemAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var item = await context.BudgetLines.FirstOrDefaultAsync(b => b.Id == id);
            if (item != null)
            {
                context.BudgetLines.Remove(item);
                await context.SaveChangesAsync();
            }
        }
        #endregion

        #region Category Methods
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Categories.ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCategoryAsync(Category category)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Categories.Update(category);
            await context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category != null)
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
            }
        }
        #endregion

        #region Transaction Methods
        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.TransactionBudgetLines)
                    .ThenInclude(tbl => tbl.BudgetLine)
                        .ThenInclude(b => b.Category)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactionsPerMonthAsync(DateTime date)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Transactions
                .AsNoTracking()
                .Where(t => t.Date.Month == date.Month && t.Date.Year == date.Year)
                .Include(t => t.Category)
                .Include(t => t.TransactionBudgetLines)
                    .ThenInclude(tbl => tbl.BudgetLine)
                        .ThenInclude(b => b.Category)
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.TransactionBudgetLines)
                    .ThenInclude(tbl => tbl.BudgetLine)
                        .ThenInclude(b => b.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            PrepareTransactionForSave(transaction);
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            PrepareTransactionForSave(transaction);
            context.Transactions.Update(transaction);
            await context.SaveChangesAsync();
        }

        public async Task DeleteTransactionAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var stub = new Transaction { Id = id };
            context.Entry(stub).State = EntityState.Deleted;
            await context.SaveChangesAsync();
        }

        private static void PrepareTransactionForSave(Transaction transaction)
        {
            transaction.Category = null;

            if (transaction.TransactionBudgetLines == null)
                return;

            foreach (var split in transaction.TransactionBudgetLines)
            {
                split.Transaction = null;
                split.BudgetLine = null;
            }
        }
        #endregion
    }
}