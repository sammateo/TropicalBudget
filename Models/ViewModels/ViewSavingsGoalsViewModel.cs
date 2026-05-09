using System;

namespace TropicalBudget.Models.ViewModels;

public class ViewSavingsGoalsViewModel
{
    public Budget Budget { get; set; }
    public DateTime StartDate { get; set; }

    public List<SavingsGoal> SavingsGoals { get; set; }

    public List<Transaction> Transactions { get; set; }

}
