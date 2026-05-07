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

    }

    public class MovementTypeItem
    {
        public string Type { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}