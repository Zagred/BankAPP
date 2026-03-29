using Microsoft.Extensions.Logging;
using BankAPP.Data;

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

            builder.Services.AddSingleton<AppDatabase>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<AddTransactionPage>();
            builder.Services.AddTransient<AddBudgetPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();

            return builder.Build();
        }
    }
}