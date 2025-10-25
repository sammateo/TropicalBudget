namespace TropicalBudget.Models
{
    public class Plan
    {
        public Budget Budget { get; set; }
        public List<PlanItem> PlanItems { get; set; }
        public List<Transaction> Transactions { get; set; }
        public DateTime startDate { get; set; }
        public string currentMonth { get
            {
                if(startDate == DateTime.MinValue) return "";
                return $"{startDate.ToString("MMMM")}, {startDate.ToString("yyyy")}";
            }
        }
    }
}
