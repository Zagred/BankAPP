namespace BankAPP.Models
{
    public class BudgetSummary
    {
        public string Category { get; set; } = string.Empty;
        public decimal LimitAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool IsExceeded { get; set; }
    }
}