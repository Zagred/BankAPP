using BankAPP.Services;
using BankAPP.Shared.DTOs;
using System.Net.Http.Json;

namespace BankAPP
{
    public partial class TransferPage : ContentPage
    {
        private readonly TransferApiService _transferApiService;
        private readonly AccountApiService _accountApiService;

        public TransferPage(
            TransferApiService transferApiService,
            AccountApiService accountApiService)
        {
            InitializeComponent();
            _transferApiService = transferApiService;
            _accountApiService = accountApiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var accounts = await _accountApiService.GetMyAccountsAsync();

            FromAccountPicker.ItemsSource = accounts;

            if (accounts.Count > 0)
                FromAccountPicker.SelectedIndex = 0;
        }

        private async void OnTransferClicked(object sender, EventArgs e)
        {
            if (FromAccountPicker.SelectedItem is not AccountDto selectedAccount)
            {
                await DisplayAlert("Error", "Please select source account.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(ToIbanEntry.Text))
            {
                await DisplayAlert("Error", "Please enter IBAN.", "OK");
                return;
            }

            if (!decimal.TryParse(AmountEntry.Text, out decimal amount) || amount <= 0)
            {
                await DisplayAlert("Error", "Invalid amount.", "OK");
                return;
            }

            var toAccountId = await GetAccountIdByIban(ToIbanEntry.Text);

            if (toAccountId == null)
            {
                await DisplayAlert("Error", "IBAN not found.", "OK");
                return;
            }

            var request = new TransferRequest
            {
                FromAccountId = selectedAccount.Id,
                ToAccountId = toAccountId.Value,
                Amount = amount,
                Description = DescriptionEntry.Text ?? ""
            };
            var success = await _transferApiService.TransferAsync(request);

            if (!success)
            {
                await DisplayAlert("Error", "Transfer failed.", "OK");
                return;
            }

            await DisplayAlert("Success", "Transfer completed.", "OK");
            await Navigation.PopAsync();
        }
        private async Task<int?> GetAccountIdByIban(string iban)
        {
            var accounts = await _accountApiService.GetMyAccountsAsync();

            // първо проверяваме дали е наш акаунт
            var own = accounts.FirstOrDefault(a => a.Iban == iban);
            if (own != null)
                return own.Id;

            // ако не е наш – викаме API
            var client = new HttpClient();
            var result = await client.GetFromJsonAsync<AccountDto>($"https://localhost:7083/api/accounts/by-iban/{iban}");

            return result?.Id;
        }
    }
}