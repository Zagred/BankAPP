using SQLite;

namespace BankAPP.Models
{
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string Type { get; set; } = string.Empty;
        // income / expense

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }
    }
}