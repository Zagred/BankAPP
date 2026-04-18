using Microsoft.Extensions.Logging;
using BankAPP.Shared.Data;
using BankAPP.Shared.Models;
using BankAPP.Services;
using Microsoft.EntityFrameworkCore;

namespace BankAPP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(
                    "Server=localhost\\SQLEXPRESS;Database=BankAppDb;Trusted_Connection=True;TrustServerCertificate=True;"));

            builder.Services.AddTransient<UserService>();
            builder.Services.AddTransient<MovementService>();

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AddMovementPage>();

            return builder.Build();
        }
    }
}