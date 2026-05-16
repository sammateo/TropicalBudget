using System;

namespace TropicalBudget.Models.ViewModels;

public class EditSavingsGoalViewModal
{
    public SavingsGoal SavingsGoal { get; set; }

    public List<Transaction> Transactions { get; set; }

}
