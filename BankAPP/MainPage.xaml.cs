using BankAPP.Services;
using BankAPP.Shared.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace BankAPP
{
    public partial class MainPage : ContentPage
    {
        private readonly MovementApiService _movementApiService;
        private readonly IServiceProvider _serviceProvider;
        private bool _isInitialized;
        private readonly AccountApiService _accountApiService;

        public MainPage(
    MovementApiService movementApiService,
    AccountApiService accountApiService,
    IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _movementApiService = movementApiService;
            _accountApiService = accountApiService;
            _serviceProvider = serviceProvider;

            FilterPicker.SelectedIndex = 0;
        }
        private async Task<List<AccountDto>> LoadAccountsAsync()
        {
            return await _accountApiService.GetMyAccountsAsync();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }
        private async Task LoadDataAsync()
        {
            await LoadMovementsAsync();
            await LoadSummaryAsync();

            var accounts = await _accountApiService.GetMyAccountsAsync();
            AccountsCollection.ItemsSource = accounts;
        }

        private async Task LoadMovementsAsync()
        {
            string selectedFilter = FilterPicker?.SelectedItem?.ToString() ?? "all";

            var movements = await _movementApiService.GetMovementsByUserAndTypeAsync(selectedFilter);

            TransactionsCollection.ItemsSource = movements;
        }

        private async Task LoadSummaryAsync()
        {
            var accounts = await _accountApiService.GetMyAccountsAsync();

            var totalBalance = accounts.Sum(a => a.Balance);

            var totalDebit = await _movementApiService.GetTotalDebitAsync();
            var totalCredit = await _movementApiService.GetTotalCreditAsync();

            BalanceLabel.Text = totalBalance.ToString("F2");
            IncomeLabel.Text = totalCredit.ToString("F2");
            ExpenseLabel.Text = totalDebit.ToString("F2");
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Do you want to log out?", "Yes", "No");

            if (!confirm)
                return;

            SessionManager.Logout();

            var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
            Application.Current!.MainPage = new NavigationPage(loginPage);
        }

        private async void OnFilterChanged(object sender, EventArgs e)
        {
            if (!_isInitialized)
                return;

            await LoadMovementsAsync();
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

    }
}