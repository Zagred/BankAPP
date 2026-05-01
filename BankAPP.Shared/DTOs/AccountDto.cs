using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAPP.Shared.DTOs
{
    public class AccountDto
    {
        public int Id { get; set; }
        public string Iban { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public string DisplayText => $"{Iban} - {Balance:F2} {Currency}";
    }
}
