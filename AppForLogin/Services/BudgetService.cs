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
        private readonly BudgetDBContext _context;

        public BudgetService(BudgetDBContext context)
        {
            _context = context;
        }

        #region BudgetItem Methods
        public async Task<List<BudgetLine>> GetAllBudgetItemsAsync()
        {
            return await _context.BudgetLines
                .Include(b => b.Category)
                .Include(b => b.TransactionBudgetLines)
                    .ThenInclude(tbl => tbl.Transaction)
                        .ThenInclude(t => t.Category)
                .ToListAsync();
                            
        }

        // zelfde maar dan met ID
        public async Task<BudgetLine?> GetBudgetItemByIdAsync(int id)
        {
            return await _context.BudgetLines
                .Include(b => b.Category)
                .Include(b => b.TransactionBudgetLines)
                    .ThenInclude(tbl => tbl.Transaction)
                        .ThenInclude(t => t.Category)   
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        //BudgetItem toevoegen
        public async Task AddBudgetItemAsync(BudgetLine item)
        {
            _context.BudgetLines.Add(item);
            await  _context.SaveChangesAsync();
        }

        // BudgetItem bijwerken
        public async Task UpdateBudgetItemAsync(BudgetLine item)
        {
            _context.BudgetLines.Update(item);
            await _context.SaveChangesAsync();
        }

        // BudgetItem verwijderen
        public async Task DeleteBudgetItemAsync(int id)
        {
            var item = await GetBudgetItemByIdAsync(id);
            if (item != null)
            {
                _context.BudgetLines.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
        #endregion

        #region Category Methods
        // Alle categorieën ophalen
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        // Categorie ophalen door ID
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        // Nieuwe categorie toevoegen
        public async Task AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        // Categorie bijwerken
        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        // Categorie verwijderen
        public async Task DeleteCategoryAsync(int id)
        {
            var category = await GetCategoryByIdAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
        #endregion

        #region Transaction Methods
        // Alle transacties ophalen met gerelateerde categorieën (eager loading)
        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions
                .Include(t => t.Category)  // Laad de gerelateerde Category
                .Include(t => t.TransactionBudgetLines) // Laad gerelateerde TransactionBudgetLines
                    .ThenInclude(tbl => tbl.BudgetLine) // Laad gerelateerde BudgetLine binnen TransactionBudgetLines
                .ToListAsync();
        }

        //Idem als hierboven maar met alleen een specifieke maand
        public async Task<List<Transaction>> GetTransactionsPerMonthAsync(DateTime date)
        {
            var Date = date;
            return await _context.Transactions
                .Where(t => t.Date.Month == Date.Month && t.Date.Year == Date.Year)
                .Include(t => t.Category)
                .Include(t => t.TransactionBudgetLines)
                    .ThenInclude(tbl => tbl.BudgetLine)
                .ToListAsync();
        }

        // Transactie ophalen door ID met gerelateerde categorie
        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Nieuwe transactie toevoegen
        public async Task AddTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        // Transactie bijwerken
        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        // Transactie verwijderen
        public async Task DeleteTransactionAsync(int id)
        {
            var transaction = await GetTransactionByIdAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }

        // Transacties ophalen voor een specifieke categorie met gerelateerde categorieën
        public async Task<List<Transaction>> GetTransactionsByCategoryIdAsync(int categoryId)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.CategoryId == categoryId)
                .ToListAsync();
        }
        #endregion
    }
}