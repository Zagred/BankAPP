using BankAPP.Services;

namespace BankAPP
{
    public partial class RegisterPage : ContentPage
    {
        private readonly UserService _userService;

        public RegisterPage(UserService userService)
        {
            InitializeComponent();
            _userService = userService;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter username and password.", "OK");
                return;
            }

            var success = await _userService.RegisterUserAsync(username, password);

            if (!success)
            {
                await DisplayAlert("Error", "User already exists", "OK");
                return;
            }

            await DisplayAlert("Success", "Account created", "OK");
            await Navigation.PopAsync();
        }

        private async void OnGoToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}