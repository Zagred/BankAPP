using BankAPP.Models;
using BankAPP.Services;
using BankAPP.Shared.DTOs;
using BankAPP.Shared.Models;

namespace BankAPP
{
    public partial class AddMovementPage : ContentPage
    {
        private readonly MovementApiService _movementApiService;

        public AddMovementPage(MovementApiService movementApiService)
        {
            InitializeComponent();
            _movementApiService = movementApiService;
        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            if (!decimal.TryParse(AmountEntry.Text, out decimal amount))
            {
                await DisplayAlert("Error", "Invalid amount", "OK");
                return;
            }

            var request = new CreateMovementRequest
            {
                Amount = amount,
                MovementType = TypePicker.SelectedItem?.ToString() ?? "deposit",
                Description = DescriptionEntry.Text ?? ""
            };

            var success = await _movementApiService.AddMovementAsync(request);

            if (!success)
            {
                await DisplayAlert("Error", "Failed to add movement", "OK");
                return;
            }

            await DisplayAlert("Success", "Movement added", "OK");

            AmountEntry.Text = "";
            DescriptionEntry.Text = "";
            TypePicker.SelectedIndex = 0;

            await Navigation.PopAsync();
        }
    }
}