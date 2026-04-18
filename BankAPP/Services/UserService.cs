using BankAPP.Shared.Data;
using BankAPP.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BankAPP.Services
{
    public class UserService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public UserService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> RegisterUserAsync(string username, string password)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (existingUser != null)
                return false;

            var user = new User
            {
                Name = username,
                Egn = Guid.NewGuid().ToString("N").Substring(0, 10),
                RegistrationDate = DateTime.Now,
                Email = $"{username}@demo.local",
                PhoneNumber = null,
                Username = username,
                PasswordHash = password
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return true;
        }
    }
}