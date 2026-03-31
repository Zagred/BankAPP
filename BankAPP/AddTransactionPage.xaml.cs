using BankAPP.Data;
using BankAPP.Models;
using BankAPP.Services;
using System.Collections.Generic;

namespace BankAPP
{
    public partial class AddTransactionPage : ContentPage
    {
        private readonly List<string> expenseCategories = new()
        {
            "Food",
            "Transport",
            "Bills",
            "Entertainment",
            "Shopping"
        };

        private readonly List<string> incomeCategories = new()
        {
            "Salary",
            "Bonus"
        };

        private readonly AppDatabase _database;

        public AddTransactionPage(AppDatabase database)
        {
            InitializeComponent();
            _database = database;

            TransactionDatePicker.Date = DateTime.Now;
            TypePicker.SelectedIndex = 1; // default = expense
            UpdateCategoryPicker();
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

            if (CategoryPicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please select a category.", "OK");
                return;
            }

            var transaction = new Transaction
            {
                UserId = SessionManager.CurrentUserId,
                Amount = amount,
                Type = TypePicker.SelectedItem.ToString() ?? "expense",
                Category = CategoryPicker.SelectedItem.ToString() ?? "",
                Description = DescriptionEntry.Text ?? string.Empty,
                Date = TransactionDatePicker.Date ?? DateTime.Now
            };

            await _database.AddTransactionAsync(transaction);

            await DisplayAlert("Success", "Transaction saved successfully.", "OK");
            await Navigation.PopAsync();
        }

        private void UpdateCategoryPicker()
        {
            var selectedType = TypePicker.SelectedItem?.ToString();

            if (selectedType == "income")
            {
                CategoryPicker.ItemsSource = incomeCategories;
            }
            else
            {
                CategoryPicker.ItemsSource = expenseCategories;
            }

            CategoryPicker.SelectedIndex = 0;
        }

        private void OnTypeChanged(object sender, EventArgs e)
        {
            UpdateCategoryPicker();
        }
    }
}