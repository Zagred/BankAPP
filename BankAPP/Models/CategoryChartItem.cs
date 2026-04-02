using System;
using System.Collections.Generic;
using System.Text;

namespace BankAPP.Models
{
    public class CategoryChartItem
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public double Percentage { get; set; }
        public string PercentageText => $"{Percentage * 100:F0}%";
    }
}