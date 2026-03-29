using SQLite;
using BankAPP.Models;
using System.Text.Json;
using System.Xml.Linq;

namespace BankAPP.Data
{
    public class AppDatabase
    {
        private SQLiteAsyncConnection _database;

        public async Task InitAsync()
        {
            if (_database != null)
                return;

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "smartbanking.db");

            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<Transaction>();
            await _database.CreateTableAsync<Budget>();
            await _database.CreateTableAsync<User>();
        }

        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            await InitAsync();
            return await _database.Table<Transaction>()
                                  .OrderByDescending(t => t.Date)
                                  .ToListAsync();
        }

        public async Task<int> AddTransactionAsync(Transaction transaction)
        {
            await InitAsync();
            return await _database.InsertAsync(transaction);
        }

        public async Task<int> DeleteTransactionAsync(Transaction transaction)
        {
            await InitAsync();
            return await _database.DeleteAsync(transaction);
        }
        public async Task<decimal> GetTotalIncomeAsync()
        {
            await InitAsync();

            var transactions = await _database.Table<Transaction>().ToListAsync();
            return transactions
                .Where(t => t.Type == "income")
                .Sum(t => t.Amount);
        }

        public async Task<decimal> GetTotalExpenseAsync()
        {
            await InitAsync();

            var transactions = await _database.Table<Transaction>().ToListAsync();
            return transactions
                .Where(t => t.Type == "expense")
                .Sum(t => t.Amount);
        }

        public async Task<decimal> GetBalanceAsync()
        {
            var income = await GetTotalIncomeAsync();
            var expense = await GetTotalExpenseAsync();

            return income - expense;
        }

        public async Task<List<Transaction>> GetTransactionsByTypeAsync(string type)
        {
            await InitAsync();

            var transactions = await _database.Table<Transaction>()
                                              .OrderByDescending(t => t.Date)
                                              .ToListAsync();

            if (string.IsNullOrWhiteSpace(type) || type == "all")
                return transactions;

            return transactions.Where(t => t.Type == type).ToList();
        }
        public async Task<int> AddBudgetAsync(Budget budget)
        {
            await InitAsync();
            return await _database.InsertAsync(budget);
        }
        public async Task<List<Budget>> GetBudgetsAsync()
        {
            await InitAsync();
            return await _database.Table<Budget>().ToListAsync();
        }
        public async Task<int> DeleteBudgetAsync(Budget budget)
        {
            await InitAsync();
            return await _database.DeleteAsync(budget);
        }
        public async Task<List<BudgetSummary>> GetBudgetSummariesAsync()
        {
            await InitAsync();

            var budgets = await _database.Table<Budget>().ToListAsync();
            var transactions = await _database.Table<Transaction>().ToListAsync();

            var result = budgets.Select(b =>
            {
                var spent = transactions
                    .Where(t => t.Type == "expense" && t.Category == b.Category)
                    .Sum(t => t.Amount);

                return new BudgetSummary
                {
                    Category = b.Category,
                    LimitAmount = b.LimitAmount,
                    SpentAmount = spent,
                    RemainingAmount = b.LimitAmount - spent,
                    IsExceeded = spent > b.LimitAmount
                };
            })
            .OrderByDescending(x => x.IsExceeded)
            .ThenBy(x => x.Category)
            .ToList();

            return result;
        }
        public async Task<List<CategorySummary>> GetExpenseSummaryByCategoryAsync()
        {
            await InitAsync();

            var transactions = await _database.Table<Transaction>().ToListAsync();

            var result = transactions
                .Where(t => t.Type == "expense")
                .GroupBy(t => t.Category)
                .Select(g => new CategorySummary
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            return result;
        }
        public async Task<bool> HasExceededBudgetsAsync()
        {
            var budgets = await GetBudgetSummariesAsync();
            return budgets.Any(b => b.IsExceeded);
        }
        public async Task<string> ExportTransactionsToJsonAsync()
        {
            await InitAsync();

            var transactions = await _database.Table<Transaction>()
                                              .OrderByDescending(t => t.Date)
                                              .ToListAsync();

            var json = JsonSerializer.Serialize(transactions, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var fileName = $"transactions_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await File.WriteAllTextAsync(filePath, json);

            return filePath;
        }
        public async Task<string> ExportTransactionsToXmlAsync()
        {
            await InitAsync();

            var transactions = await _database.Table<Transaction>()
                                              .OrderByDescending(t => t.Date)
                                              .ToListAsync();

            var xml = new XDocument(
                new XElement("Transactions",
                    transactions.Select(t =>
                        new XElement("Transaction",
                            new XElement("Id", t.Id),
                            new XElement("Amount", t.Amount),
                            new XElement("Type", t.Type),
                            new XElement("Category", t.Category),
                            new XElement("Description", t.Description),
                            new XElement("Date", t.Date.ToString("yyyy-MM-dd HH:mm:ss"))
                        ))
                )
            );

            var fileName = $"transactions_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await File.WriteAllTextAsync(filePath, xml.ToString());

            return filePath;
        }
        public async Task<int> AddUserAsync(User user)
        {
            await InitAsync();
            return await _database.InsertAsync(user);
        }
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            await InitAsync();

            return await _database.Table<User>()
                                  .FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}