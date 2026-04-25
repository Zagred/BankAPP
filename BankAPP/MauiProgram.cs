using BankAPP.Services;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

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

            builder.Services.AddHttpClient("BankApi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7083/");
            })
 .AddHttpMessageHandler<AuthMessageHandler>();

            builder.Services.AddTransient<UserApiService>();
            builder.Services.AddTransient<MovementApiService>();
            builder.Services.AddTransient<AuthMessageHandler>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AddMovementPage>();
            builder.Services.AddTransient<AccountApiService>();

            return builder.Build();
        }
    }
}