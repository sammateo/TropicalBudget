using System.ComponentModel.DataAnnotations.Schema;

namespace TropicalBudget.Models
{
    public class PlanItem
    {
        [Column("Id")]
        public Guid ID { get; set; }

        [Column("budget_id")]
        public Guid BudgetID { get; set; }

        [Column("category_id")]
        public Guid CategoryID { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("month")]
        public int Month { get; set; }

        [Column("year")]
        public int Year { get; set; }

        public Guid TransactionTypeID { get; set; }
        public string TransactionType { get; set; }

        public string CategoryName { get; set; }
        public string CategoryColor { get; set; }

    }
}
