using System.Security.Claims;
using BankAPI.Services;
using BankAPP.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankAPI.Services;

namespace BankAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class BudgetReportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public BudgetReportController(
            AppDbContext context,
            EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("monthly-email")]
        public async Task<IActionResult> SendMonthlyReport()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var accountIds = await _context.UserAccounts
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AccountId)
                .ToListAsync();

            var now = DateTime.Now;

            var monthMovements = await _context.Movements
                .Where(m =>
                    accountIds.Contains(m.AccountId) &&
                    m.MovementDateTime.Month == now.Month &&
                    m.MovementDateTime.Year == now.Year)
                .ToListAsync();

            var income = monthMovements
                .Where(m =>
                    m.MovementType == "deposit" ||
                    m.MovementType == "transfer")
                .Sum(m => m.Amount);

            var expenses = monthMovements
                .Where(m =>
                    m.MovementType == "card_payment" ||
                    m.MovementType == "cash_withdrawal" ||
                    m.MovementType == "fee")
                .Sum(m => m.Amount);

            var body = $"""
            Monthly Budget Report

            Income: {income:F2} BGN
            Expenses: {expenses:F2} BGN
            Remaining: {(income - expenses):F2} BGN

            Total transactions: {monthMovements.Count}
            """;

            await _emailService.SendAsync(
                user.Email,
                "Your Monthly Budget Report",
                body);

            return Ok(new
            {
                message = "Monthly report sent successfully."
            });
        }
    }
}