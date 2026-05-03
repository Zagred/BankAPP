using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAPP.Shared.DTOs
{
    public class CreateMovementRequest
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}