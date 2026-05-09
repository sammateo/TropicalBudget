using System;

namespace TropicalBudget.Models.ViewModels;

public class NewTransactionViewModel
{
    public List<TransactionCategory> TransactionCategories { get; set; }
    public List<TransactionSource> TransactionSources { get; set; }
    public List<TransactionType> TransactionTypes { get; set; }
    public List<SavingsGoal> SavingsGoals { get; set; }
    public Transaction NewTransaction { get; set; }
}
