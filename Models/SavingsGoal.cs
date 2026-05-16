using System;

namespace TropicalBudget.Models;

public class SavingsGoal
{
    public Guid ID { get; set; }
    public Guid BudgetID { get; set; }
    public string? Title { get; set; }

    public decimal GoalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public string? Color { get; set; }
}
