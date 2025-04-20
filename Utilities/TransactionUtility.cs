using System.Reflection;
using TestMVC.Models;

namespace TestMVC.Utilities
{
    public class TransactionUtility
    {
        public static string TRANSACTION_TYPE_EXPENSE = "Expense";
        public static string TRANSACTION_TYPE_INCOME = "Income";
        public static decimal GetExpenses(List<Transaction> transactions)
        {
            decimal expenses = 0;
            List<Transaction> expenseTransactions = transactions.Where(x => x.TransactionType == TRANSACTION_TYPE_EXPENSE).ToList();
            expenses = expenseTransactions.Sum(x => x.Amount);
            return expenses;
        }
        
        public static decimal GetIncome(List<Transaction> transactions)
        {
            decimal income = 0;
            List<Transaction> incomeTransactions = transactions.Where(x => x.TransactionType == TRANSACTION_TYPE_INCOME).ToList();
            income = incomeTransactions.Sum(x => x.Amount);
            return income;
        }
    }
}
