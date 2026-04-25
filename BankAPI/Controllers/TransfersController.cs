using BankAPP.Shared.Data;
using BankAPP.Shared.DTOs;
using BankAPP.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BankAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TransfersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransfersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(TransferRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Invalid amount");

            var fromAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.FromAccountId);

            var toAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.ToAccountId);

            if (fromAccount == null || toAccount == null)
                return NotFound("Account not found");

            if (fromAccount.Balance < request.Amount)
                return BadRequest("Insufficient funds");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // debit
                fromAccount.Balance -= request.Amount;

                var debit = new Movement
                {
                    AccountId = fromAccount.Id,
                    Amount = request.Amount,
                    MovementType = "transfer",
                    Description = $"Transfer to account {toAccount.Id}: {request.Description}",
                    Currency = "BGN",
                    Status = "completed",
                    ReferenceNumber = Guid.NewGuid().ToString("N"),
                    MovementDateTime = DateTime.Now
                };

                // credit
                toAccount.Balance += request.Amount;

                var credit = new Movement
                {
                    AccountId = toAccount.Id,
                    Amount = request.Amount,
                    MovementType = "transfer",
                    Description = $"Transfer from account {fromAccount.Id}: {request.Description}",
                    Currency = "BGN",
                    Status = "completed",
                    ReferenceNumber = Guid.NewGuid().ToString("N"),
                    MovementDateTime = DateTime.Now
                };

                _context.Movements.AddRange(debit, credit);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Transfer successful" });
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Transfer failed");
            }
        }
    }
}