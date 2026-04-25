using BankAPP.Models;
using BankAPP.Services;
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
            if (string.IsNullOrWhiteSpace(AmountEntry.Text) ||
                !decimal.TryParse(AmountEntry.Text, out decimal amount))
            {
                await DisplayAlert("Error", "Please enter a valid amount.", "OK");
                return;
            }

            if (TypePicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please select a movement type.", "OK");
                return;
            }

            var movement = new Movement
            {
                Amount = amount,
                MovementType = TypePicker.SelectedItem.ToString() ?? "deposit",
                Description = DescriptionEntry.Text ?? string.Empty,
                Currency = "BGN",
                ReferenceNumber = Guid.NewGuid().ToString()
            };


            await DisplayAlert("Success", "Movement added successfully.", "OK");
            await Navigation.PopAsync();
        }
    }
}