using BankAPP.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BankAPP
{
    public partial class LoginPage : ContentPage
    {
        private readonly UserService _userService;
        private readonly IServiceProvider _serviceProvider;

        public LoginPage(UserService userService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _userService = userService;
            _serviceProvider = serviceProvider;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter username and password.", "OK");
                return;
            }

            var user = await _userService.GetUserByUsernameAsync(username);

            if (user == null || user.PasswordHash != password)
            {
                await DisplayAlert("Error", "Invalid credentials", "OK");
                return;
            }

            SessionManager.CurrentUserId = user.Id;
            SessionManager.CurrentUsername = user.Username;

            var mainPage = _serviceProvider.GetRequiredService<MainPage>();
            await Navigation.PushAsync(mainPage);
        }

        private async void OnGoToRegisterClicked(object sender, EventArgs e)
        {
            var registerPage = _serviceProvider.GetRequiredService<RegisterPage>();
            await Navigation.PushAsync(registerPage);
        }
    }
}