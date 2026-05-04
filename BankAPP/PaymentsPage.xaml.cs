using BankAPP.Services;
using BankAPP.Shared.DTOs;
using BankAPP.Shared.Models;

namespace BankAPP
{
    public partial class PaymentsPage : ContentPage
    {
        private readonly MovementApiService _movementApiService;
        private readonly AccountApiService _accountApiService;

        public PaymentsPage(
            MovementApiService movementApiService,
            AccountApiService accountApiService)
        {
            InitializeComponent();
            _movementApiService = movementApiService;
            _accountApiService = accountApiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var accounts = await _accountApiService.GetMyAccountsAsync();
            AccountPicker.ItemsSource = accounts;

            if (accounts.Count > 0)
                AccountPicker.SelectedIndex = 0;

            // Load movement types
            var types = new List<MovementTypeItem>
            {
                new MovementTypeItem { Type = "card_payment", DisplayName = "Card Payment" },
                new MovementTypeItem { Type = "cash_withdrawal", DisplayName = "Cash Withdrawal" },
                new MovementTypeItem { Type = "deposit", DisplayName = "Deposit" },
                new MovementTypeItem { Type = "fee", DisplayName = "Fee" }
            };
            TypePicker.ItemsSource = types;

            if (types.Count > 0)
                TypePicker.SelectedIndex = 0;

            await LoadPaymentsAsync();
            await LoadChartDataAsync();
        }

        private async Task LoadPaymentsAsync()
        {
            var filter = FilterPicker.SelectedItem?.ToString() ?? "all";
            var movements = await _movementApiService.GetMovementsByUserAsync();

            if (filter != "all")
            {
                movements = movements.Where(m => m.MovementType == filter).ToList();
            }

            PaymentsCollection.ItemsSource = movements;

            var totalSpent = movements.Where(m => m.IsExpense).Sum(m => m.Amount);
            var totalIncome = movements.Where(m => !m.IsExpense).Sum(m => m.Amount);

            TotalSpentLabel.Text = totalSpent.ToString("F2");
            TotalIncomeLabel.Text = totalIncome.ToString("F2");
        }

        private async Task LoadChartDataAsync()
        {
            var movements = await _movementApiService.GetMyMovementsAsync();
            var chartData = BuildChartData(movements);
            ChartCollectionView.ItemsSource = chartData;
        }

        private static List<ChartBar> BuildChartData(List<Movement> movements)
        {
            var today = DateTime.Today;
            var lastSevenDays = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(i - 6))
                .Select(day => new
                {
                    Day = day,
                    Amount = movements
                        .Where(m => m.IsExpense && m.MovementDateTime.Date == day.Date)
                        .Sum(m => m.Amount)
                })
                .ToList();

            var maxAmount = lastSevenDays.Max(x => x.Amount);
            if (maxAmount <= 0) maxAmount = 1;

            return lastSevenDays
                .Select(x => new ChartBar(
                    x.Day.ToString("dd"),
                    (double)x.Amount,
                    x.Amount > 0 ? x.Amount.ToString("F0") : "0",
                    Math.Max(10, (double)x.Amount / (double)maxAmount * 130)))
                .ToList();
        }

        private sealed record ChartBar(string DayLabel, double Amount, string AmountText, double Height);

        private async void OnFilterChanged(object sender, EventArgs e)
        {
            await LoadPaymentsAsync();
        }

        private async void OnAddMovementClicked(object sender, EventArgs e)
        {
            if (AccountPicker.SelectedItem is not AccountDto selectedAccount)
            {
                await DisplayAlert("Error", "Please select an account.", "OK");
                return;
            }

            if (!decimal.TryParse(AmountEntry.Text, out decimal amount) || amount <= 0)
            {
                await DisplayAlert("Error", "Invalid amount.", "OK");
                return;
            }

            if (TypePicker.SelectedItem is not MovementTypeItem selectedType)
            {
                await DisplayAlert("Error", "Please select movement type.", "OK");
                return;
            }

            var request = new CreateMovementRequest
            {
                AccountId = selectedAccount.Id,
                Amount = amount,
                MovementType = selectedType.Type,
                Description = DescriptionEntry.Text?.Trim() ?? string.Empty
            };

            var success = await _movementApiService.AddMovementAsync(request);
            if (!success)
            {
                await DisplayAlert("Error", "Failed to add movement.", "OK");
                return;
            }

            await DisplayAlert("Success", "Movement added successfully.", "OK");

            // Clear form
            AmountEntry.Text = string.Empty;
            DescriptionEntry.Text = string.Empty;

            await LoadPaymentsAsync();
            await LoadChartDataAsync();
        }
    }

    public class MovementTypeItem
    {
        public string Type { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}