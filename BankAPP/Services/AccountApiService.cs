using System.Net.Http.Json;

namespace BankAPP.Services
{
    public class AccountApiService
    {
        private readonly HttpClient _httpClient;

        public AccountApiService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("BankApi");
        }

        public async Task<List<AccountDto>> GetMyAccountsAsync()
        {
            var accounts = await _httpClient.GetFromJsonAsync<List<AccountDto>>("api/accounts/me");
            return accounts ?? new List<AccountDto>();
        }
    }

    public class AccountDto
    {
        public int Id { get; set; }
        public string Iban { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}