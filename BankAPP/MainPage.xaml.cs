using BankAPP.Data;
using BankAPP.Models;

namespace BankAPP
{
    public partial class MainPage : ContentPage
    {
        private readonly AppDatabase _database;

        public MainPage(AppDatabase database)
        {
            InitializeComponent();
            _database = database;

            FilterPicker.SelectedIndex = 0;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadTransactionsAsync();
            await LoadSummaryAsync();
            await LoadCategorySummaryAsync();
            await LoadBudgetSummaryAsync();
            await LoadBudgetWarningAsync();
        }

        private async Task LoadTransactionsAsync()
        {
            string selectedFilter = "all";

            if (FilterPicker.SelectedItem != null)
                selectedFilter = FilterPicker.SelectedItem.ToString() ?? "all";

            var transactions = await _database.GetTransactionsByTypeAsync(selectedFilter);
            TransactionsCollection.ItemsSource = transactions;
        }

        private async Task LoadSummaryAsync()
        {
            var income = await _database.GetTotalIncomeAsync();
            var expense = await _database.GetTotalExpenseAsync();
            var balance = await _database.GetBalanceAsync();

            IncomeLabel.Text = income.ToString("F2");
            ExpenseLabel.Text = expense.ToString("F2");
            BalanceLabel.Text = balance.ToString("F2");
        }

        private async void OnOpenAddTransactionPageClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddTransactionPage(_database));
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async void OnFilterChanged(object sender, EventArgs e)
        {
            await LoadTransactionsAsync();
        }

        private async void OnTransactionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
                return;

            var selectedTransaction = e.CurrentSelection[0] as Transaction;

            if (selectedTransaction == null)
                return;

            bool confirm = await DisplayAlert(
                "Delete Transaction",
                $"Do you want to delete '{selectedTransaction.Category} - {selectedTransaction.Amount:F2}'?",
                "Yes",
                "No");

            if (confirm)
            {
                await _database.DeleteTransactionAsync(selectedTransaction);
                await LoadDataAsync();
            }

            TransactionsCollection.SelectedItem = null;
        }
        private async Task LoadCategorySummaryAsync()
        {
            var categorySummary = await _database.GetExpenseSummaryByCategoryAsync();
            CategorySummaryCollection.ItemsSource = categorySummary;
        }
        private async Task LoadBudgetSummaryAsync()
        {
            var budgets = await _database.GetBudgetSummariesAsync();
            BudgetSummaryCollection.ItemsSource = budgets;
        }
        private async void OnOpenAddBudgetPageClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddBudgetPage(_database));
        }
        private async void OnBudgetSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
                return;

            var selectedSummary = e.CurrentSelection[0] as BudgetSummary;

            if (selectedSummary == null)
                return;

            var budgets = await _database.GetBudgetsAsync();
            var budgetToDelete = budgets.FirstOrDefault(b => b.Category == selectedSummary.Category);

            if (budgetToDelete == null)
                return;

            bool confirm = await DisplayAlert(
                "Delete Budget",
                $"Do you want to delete the budget for '{budgetToDelete.Category}'?",
                "Yes",
                "No");

            if (confirm)
            {
                await _database.DeleteBudgetAsync(budgetToDelete);
                await LoadDataAsync();
            }

            BudgetSummaryCollection.SelectedItem = null;
        }
        private async Task LoadBudgetWarningAsync()
        {
            bool hasExceeded = await _database.HasExceededBudgetsAsync();

            if (hasExceeded)
            {
                BudgetWarningLabel.Text = "Warning: One or more budgets have been exceeded!";
                BudgetWarningLabel.IsVisible = true;
            }
            else
            {
                BudgetWarningLabel.IsVisible = false;
            }
        }
        private async void OnExportJsonClicked(object sender, EventArgs e)
        {
            try
            {
                var filePath = await _database.ExportTransactionsToJsonAsync();

                await DisplayAlert(
                    "Export Successful",
                    $"Transactions exported successfully.\n\nFile path:\n{filePath}",
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Export Error",
                    $"An error occurred while exporting:\n{ex.Message}",
                    "OK");
            }
        }
        private async void OnExportXmlClicked(object sender, EventArgs e)
        {
            try
            {
                var filePath = await _database.ExportTransactionsToXmlAsync();

                await DisplayAlert(
                    "Export Successful",
                    $"Transactions exported successfully.\n\nFile path:\n{filePath}",
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Export Error",
                    $"An error occurred while exporting:\n{ex.Message}",
                    "OK");
            }
        }
    }
}