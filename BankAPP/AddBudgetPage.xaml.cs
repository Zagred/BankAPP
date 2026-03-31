using BankAPP.Data;
using BankAPP.Models;
using BankAPP.Services;

namespace BankAPP
{
    public partial class AddBudgetPage : ContentPage
    {
        private readonly AppDatabase _database;

        public AddBudgetPage(AppDatabase database)
        {
            InitializeComponent();
            _database = database;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a category.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(LimitEntry.Text) ||
                !decimal.TryParse(LimitEntry.Text, out decimal limit))
            {
                await DisplayAlert("Error", "Please enter a valid budget limit.", "OK");
                return;
            }

            var budget = new Budget
            {
                UserId = SessionManager.CurrentUserId,
                Category = CategoryEntry.Text,
                LimitAmount = limit
            };

            await _database.AddBudgetAsync(budget);

            await DisplayAlert("Success", "Budget saved successfully.", "OK");
            await Navigation.PopAsync();
        }
    }
}