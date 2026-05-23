using BankAPP.Services;
using BankAPP.Shared.Constants;
using BankAPP.Shared.DTOs;
using BankAPP.Shared.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BankAPP
{
    public partial class AccountsPage : ContentPage
    {
        private readonly MovementApiService _movementApiService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AccountApiService _accountApiService;

        public AccountsPage(
            MovementApiService movementApiService,
            AccountApiService accountApiService,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _movementApiService = movementApiService;
            _accountApiService = accountApiService;
            _serviceProvider = serviceProvider;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var accounts = await _accountApiService.GetMyAccountsAsync();
            var movements = await _movementApiService.GetMyMovementsAsync();

            var username = SessionManager.CurrentUsername ?? "?";
            var initial = username.Length > 0 ? username[0].ToString().ToUpper() : "?";

            try { AccountsCollection.ItemsSource = accounts; } catch { }
            try { AccountsCollectionMobile.ItemsSource = accounts; } catch { }

            try { WelcomeLabel.Text = $"Здравей, {username}!"; } catch { }
            try { WelcomeLabelM.Text = $"Добро утро,"; } catch { }
            try { AvatarLabel.Text = initial; } catch { }
            try { AvatarLabelM.Text = initial; } catch { }
            try { SidebarUsernameLabel.Text = username; } catch { }
            try { UsernameLabelM.Text = username; } catch { }

            UpdateSummary(accounts, movements);
        }

        private void UpdateSummary(List<AccountDto> accounts, List<Movement> movements)
        {
            var totalBalance = accounts.Sum(a => a.Balance);
            var totalDebit = movements.Where(m => MovementTypes.IsExpense(m.MovementType)).Sum(m => m.Amount);
            var totalCredit = movements.Where(m => MovementTypes.IsIncome(m.MovementType)).Sum(m => m.Amount);

            try { BalanceLabel.Text = totalBalance.ToString("F2"); } catch { }
            try { BalanceLabelM.Text = totalBalance.ToString("F2"); } catch { }
            try { IncomeLabel.Text = totalCredit.ToString("F2"); } catch { }
            try { IncomeLabelM.Text = totalCredit.ToString("F2"); } catch { }
            try { ExpenseLabel.Text = totalDebit.ToString("F2"); } catch { }
            try { ExpenseLabelM.Text = totalDebit.ToString("F2"); } catch { }
            try { BalanceLabelAlt.Text = totalBalance.ToString("F2"); } catch { }
            try { IncomeLabelAlt.Text = totalCredit.ToString("F2"); } catch { }
            try { ExpenseLabelAlt.Text = totalDebit.ToString("F2"); } catch { }
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Do you want to log out?", "Yes", "No");
            if (!confirm) return;
            SessionManager.Logout();
            var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
            Application.Current!.MainPage = new NavigationPage(loginPage);
        }

        private async void OnAddMovementClicked(object sender, EventArgs e)
        {
            var page = _serviceProvider.GetRequiredService<AddMovementPage>();
            await Navigation.PushAsync(page);
        }

        private async void OnTransferClicked(object sender, EventArgs e)
        {
            var page = _serviceProvider.GetRequiredService<TransferPage>();
            await Navigation.PushAsync(page);
        }

        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Create Account", "Create a new account?", "Yes", "No");
            if (!confirm) return;
            var newAccount = await _accountApiService.CreateAccountAsync();
            if (newAccount == null)
            {
                await DisplayAlert("Error", "Failed to create account", "OK");
                return;
            }
            await DisplayAlert("Success", $"Account created: {newAccount.Iban}", "OK");
            await LoadDataAsync();
        }

        private void OnNavigateToTransfers(object sender, EventArgs e)
        {
            ((AppShell)Shell.Current).NavigateTo("Transfers");
        }

        private void OnNavigateToPayments(object sender, EventArgs e)
        {
            ((AppShell)Shell.Current).NavigateTo("Payments");
        }
    }
}