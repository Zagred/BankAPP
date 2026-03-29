using BankAPP.Data;
using BankAPP.Models;

namespace BankAPP
{
    public partial class AddTransactionPage : ContentPage
    {
        private readonly AppDatabase _database;

        public AddTransactionPage(AppDatabase database)
        {
            InitializeComponent();
            _database = database;

            TransactionDatePicker.Date = DateTime.Now;
            TypePicker.SelectedIndex = 1; // default = expense
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AmountEntry.Text) ||
                !decimal.TryParse(AmountEntry.Text, out decimal amount))
            {
                await DisplayAlert("Error", "Please enter a valid amount.", "OK");
                return;
            }

            if (TypePicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please select a transaction type.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(CategoryEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a category.", "OK");
                return;
            }

            var transaction = new Transaction
            {
                Amount = amount,
                Type = TypePicker.SelectedItem.ToString() ?? "expense",
                Category = CategoryEntry.Text,
                Description = DescriptionEntry.Text ?? string.Empty,
                Date = TransactionDatePicker.Date.Value
            };

            await _database.AddTransactionAsync(transaction);

            await DisplayAlert("Success", "Transaction saved successfully.", "OK");
            await Navigation.PopAsync();
        }
    }
}