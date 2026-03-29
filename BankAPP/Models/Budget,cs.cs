using SQLite;

namespace BankAPP.Models
{
    public class Budget
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Category { get; set; } = string.Empty;

        public decimal LimitAmount { get; set; }
    }
}