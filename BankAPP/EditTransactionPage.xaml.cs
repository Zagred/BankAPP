using BankAPP.Data;
using BankAPP.Models;

namespace BankAPP
{
    public partial class EditTransactionPage : ContentPage
    {
        private readonly AppDatabase _database;
        private readonly Transaction _transaction;

        public EditTransactionPage(AppDatabase database, Transaction transaction)
        {
            InitializeComponent();
            _database = database;
            _transaction = transaction;

            // Populate fields
            AmountEntry.Text = transaction.Amount.ToString();
            CategoryEntry.Text = transaction.Category;
            DescriptionEntry.Text = transaction.Description;
            DatePicker.Date = transaction.Date;

            TypePicker.SelectedItem = transaction.Type;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (!decimal.TryParse(AmountEntry.Text, out decimal amount))
            {
                await DisplayAlert("Error", "Invalid amount", "OK");
                return;
            }

            _transaction.Amount = amount;
            _transaction.Type = TypePicker.SelectedItem?.ToString() ?? "expense";
            _transaction.Category = CategoryEntry.Text ?? "";
            _transaction.Description = DescriptionEntry.Text ?? "";
            _transaction.Date = DatePicker.Date!.Value;

            await _database.UpdateTransactionAsync(_transaction);

            await DisplayAlert("Success", "Transaction updated", "OK");
            await Navigation.PopAsync();
        }
    }
}