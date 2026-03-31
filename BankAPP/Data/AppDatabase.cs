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

        public async Task<List<Transaction>> GetTransactionsAsync(int userId)
        {
            await InitAsync();

            return await _database.Table<Transaction>()
                                  .Where(t => t.UserId == userId)
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
        public async Task<decimal> GetTotalIncomeAsync(int userId)
        {
            await InitAsync();

            var transactions = await _database.Table<Transaction>()
                                              .Where(t => t.UserId == userId)
                                              .ToListAsync();

            return transactions
                .Where(t => t.Type == "income")
                .Sum(t => t.Amount);
        }

        public async Task<decimal> GetTotalExpenseAsync(int userId)
        {
            await InitAsync();

            var transactions = await _database.Table<Transaction>()
                                              .Where(t => t.UserId == userId)
                                              .ToListAsync();

            return transactions
                .Where(t => t.Type == "expense")
                .Sum(t => t.Amount);
        }

        public async Task<decimal> GetBalanceAsync(int userId)
        {
            var income = await GetTotalIncomeAsync(userId);
            var expense = await GetTotalExpenseAsync(userId);

            return income - expense;
        }

        public async Task<List<Transaction>> GetTransactionsByTypeAsync(int userId, string type)
        {
            await InitAsync();

            var transactions = await _database.Table<Transaction>()
                                              .Where(t => t.UserId == userId)
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
        public async Task<List<Budget>> GetBudgetsAsync(int userId)
        {
            await InitAsync();
            return await _database.Table<Budget>()
                                  .Where(b => b.UserId == userId)
                                  .ToListAsync();
        }
        public async Task<int> DeleteBudgetAsync(Budget budget)
        {
            await InitAsync();
            return await _database.DeleteAsync(budget);
        }
        public async Task<List<BudgetSummary>> GetBudgetSummariesAsync(int userId)
        {
            await InitAsync();

            var budgets = await _database.Table<Budget>()
                                         .Where(b => b.UserId == userId)
                                         .ToListAsync();

            var transactions = await _database.Table<Transaction>()
                                              .Where(t => t.UserId == userId)
                                              .ToListAsync();

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
        public async Task<List<CategorySummary>> GetExpenseSummaryByCategoryAsync(int userId)
        {
            await InitAsync();

            var transactions = await _database.Table<Transaction>()
                                              .Where(t => t.UserId == userId)
                                              .ToListAsync();

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
        public async Task<bool> HasExceededBudgetsAsync(int userId)
        {
            var budgets = await GetBudgetSummariesAsync(userId);
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
        public async Task<int> UpdateTransactionAsync(Transaction transaction)
        {
            await InitAsync();
            return await _database.UpdateAsync(transaction);
        }
        public async Task ImportTransactionsFromJsonAsync(string filePath)
        {
            await InitAsync();

            var json = await File.ReadAllTextAsync(filePath);

            var transactions = JsonSerializer.Deserialize<List<Transaction>>(json);

            if (transactions == null)
                return;

            foreach (var t in transactions)
            {
                t.Id = 0;
                await _database.InsertAsync(t);
            }
        }
        public async Task ImportTransactionsFromXmlAsync(string filePath)
        {
            await InitAsync();

            var xml = XDocument.Load(filePath);

            var transactions = xml.Root?
                .Elements("Transaction")
                .Select(t => new Transaction
                {
                    Id = 0,
                    Amount = decimal.Parse(t.Element("Amount")?.Value ?? "0"),
                    Type = t.Element("Type")?.Value ?? "expense",
                    Category = t.Element("Category")?.Value ?? "",
                    Description = t.Element("Description")?.Value ?? "",
                    Date = DateTime.Parse(t.Element("Date")?.Value ?? DateTime.Now.ToString())
                })
                .ToList();

            if (transactions == null)
                return;

            foreach (var t in transactions)
            {
                await _database.InsertAsync(t);
            }
        }
    }
}