using BankAPP.Shared.Data;
using BankAPP.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BankAPP.Services
{
    public class MovementService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public MovementService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Movement>> GetMovementsByUserAsync(int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var accountIds = await context.UserAccounts
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AccountId)
                .ToListAsync();

            return await context.Movements
                .Where(m => accountIds.Contains(m.AccountId))
                .OrderByDescending(m => m.MovementDateTime)
                .ToListAsync();
        }

        public async Task<List<Movement>> GetMovementsByUserAndTypeAsync(int userId, string type)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var accountIds = await context.UserAccounts
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AccountId)
                .ToListAsync();

            var query = context.Movements
                .Where(m => accountIds.Contains(m.AccountId));

            if (!string.IsNullOrWhiteSpace(type) && type != "all")
            {
                query = query.Where(m => m.MovementType == type);
            }

            return await query
                .OrderByDescending(m => m.MovementDateTime)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalDebitAsync(int userId)
        {
            var movements = await GetMovementsByUserAsync(userId);

            return movements
                .Where(m => m.MovementType == "card_payment" ||
                            m.MovementType == "cash_withdrawal" ||
                            m.MovementType == "fee")
                .Sum(m => m.Amount);
        }

        public async Task<decimal> GetTotalCreditAsync(int userId)
        {
            var movements = await GetMovementsByUserAsync(userId);

            return movements
                .Where(m => m.MovementType == "deposit" ||
                            m.MovementType == "transfer")
                .Sum(m => m.Amount);
        }

        public async Task<decimal> GetBalanceAsync(int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var accountIds = await context.UserAccounts
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AccountId)
                .ToListAsync();

            return await context.Accounts
                .Where(a => accountIds.Contains(a.Id))
                .SumAsync(a => a.Balance);
        }
        public async Task AddMovementAsync(int userId, Movement movement)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var accountId = await context.UserAccounts
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AccountId)
                .FirstOrDefaultAsync();

            if (accountId == 0)
                throw new Exception("User has no account");

            movement.AccountId = accountId;
            movement.MovementDateTime = DateTime.Now;
            movement.Status = "completed";

            context.Movements.Add(movement);

            // update balance (за момента simple logic)
            var account = await context.Accounts.FirstAsync(a => a.Id == accountId);

            if (movement.MovementType == "deposit" || movement.MovementType == "transfer")
                account.Balance += movement.Amount;
            else
                account.Balance -= movement.Amount;

            await context.SaveChangesAsync();
        }
    }
}