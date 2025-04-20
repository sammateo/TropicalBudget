using System.Reflection;
using TestMVC.Models;

namespace TestMVC.Utilities
{
    public class TransactionUtility
    {
        public static decimal GetExpenses(List<Transaction> transactions)
        {
            decimal expenses = 0;
            List<Transaction> expenseTransactions = transactions.Where(x => x.TransactionType == "Expense").ToList();
            expenses = expenseTransactions.Sum(x => x.Amount);
            return expenses;
        }
        
        public static decimal GetIncome(List<Transaction> transactions)
        {
            decimal income = 0;
            List<Transaction> incomeTransactions = transactions.Where(x => x.TransactionType == "Income").ToList();
            income = incomeTransactions.Sum(x => x.Amount);
            return income;
        }
    }
}
