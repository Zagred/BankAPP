using BankAPP.Shared.Models;
using BankAPP.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BankAPP
{
    public partial class MainPage : ContentPage
    {
        private readonly MovementService _movementService;
        private readonly IServiceProvider _serviceProvider;
        private bool _isInitialized;

        public MainPage(MovementService movementService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _movementService = movementService;
            _serviceProvider = serviceProvider;

            FilterPicker.SelectedIndex = 0;
            _isInitialized = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            WelcomeLabel.Text = $"Welcome, {SessionManager.CurrentUsername}!";
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadMovementsAsync();
            await LoadSummaryAsync();
        }

        private async Task LoadMovementsAsync()
        {
            string selectedFilter = FilterPicker?.SelectedItem?.ToString() ?? "all";

            var movements = await _movementService.GetMovementsByUserAndTypeAsync(
                SessionManager.CurrentUserId,
                selectedFilter);

            TransactionsCollection.ItemsSource = movements;
        }

        private async Task LoadSummaryAsync()
        {
            var totalDebit = await _movementService.GetTotalDebitAsync(SessionManager.CurrentUserId);
            var totalCredit = await _movementService.GetTotalCreditAsync(SessionManager.CurrentUserId);
            var balance = await _movementService.GetBalanceAsync(SessionManager.CurrentUserId);

            IncomeLabel.Text = totalCredit.ToString("F2");
            ExpenseLabel.Text = totalDebit.ToString("F2");
            BalanceLabel.Text = balance.ToString("F2");
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Logout",
                "Do you want to log out?",
                "Yes",
                "No");

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

    }
}