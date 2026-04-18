using BankAPP.Shared.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovementsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovementsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movements = await _context.Movements
                .OrderByDescending(m => m.MovementDateTime)
                .ToListAsync();

            return Ok(movements);
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var accountIds = await _context.UserAccounts
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AccountId)
                .ToListAsync();

            var movements = await _context.Movements
                .Where(m => accountIds.Contains(m.AccountId))
                .OrderByDescending(m => m.MovementDateTime)
                .ToListAsync();

            return Ok(movements);
        }
    }
}