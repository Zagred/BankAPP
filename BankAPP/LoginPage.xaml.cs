using BankAPP.Data;
using BankAPP.Services;

namespace BankAPP
{
    public partial class LoginPage : ContentPage
    {
        private readonly AppDatabase _database;

        public LoginPage(AppDatabase database)
        {
            InitializeComponent();
            _database = database;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text?.Trim();
            var password = PasswordEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Username and password are required.", "OK");
                return;
            }

            var user = await _database.GetUserByUsernameAsync(username);

            if (user == null)
            {
                await DisplayAlert("Error", "User not found.", "OK");
                return;
            }

            var passwordHash = SecurityService.ComputeSha256Hash(password);

            if (user.PasswordHash != passwordHash)
            {
                await DisplayAlert("Error", "Invalid password.", "OK");
                return;
            }
            SessionManager.CurrentUserId = user.Id;
            SessionManager.CurrentUsername = user.Username;

            await DisplayAlert("Success", "Login successful.", "OK");
            Application.Current!.MainPage = new NavigationPage(new MainPage(_database));
        }

        private async void OnGoToRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage(_database));
        }
    }
}