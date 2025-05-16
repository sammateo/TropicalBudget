namespace TropicalBudget.Models
{
    public class TransactionExport
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public DateOnly TransactionDate { get; set; }
        public string CategoryColor { get; set; }
        public string Source { get; set; } //transaction source name
        public string TransactionType { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
