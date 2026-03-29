using BankAPP.Data;
using BankAPP.Models;
using BankAPP.Services;

namespace BankAPP
{
    public partial class RegisterPage : ContentPage
    {
        private readonly AppDatabase _database;

        public RegisterPage(AppDatabase database)
        {
            InitializeComponent();
            _database = database;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text?.Trim();
            var password = PasswordEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Username and password are required.", "OK");
                return;
            }

            var existingUser = await _database.GetUserByUsernameAsync(username);
            if (existingUser != null)
            {
                await DisplayAlert("Error", "Username already exists.", "OK");
                return;
            }

            var passwordHash = SecurityService.ComputeSha256Hash(password);

            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash
            };

            await _database.AddUserAsync(user);

            await DisplayAlert("Success", "Account created successfully.", "OK");
            await Navigation.PushAsync(new LoginPage(_database));
        }

        private async void OnGoToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage(_database));
        }
    }
}