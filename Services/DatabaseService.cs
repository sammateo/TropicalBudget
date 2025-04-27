using System.Net;
using Dapper;
using Npgsql;
using TropicalBudget.Models;

namespace TropicalBudget.Services
{
    public class DatabaseService
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public DatabaseService(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("SupabaseConnection");
        }

        #region Budget
        public async Task<List<Budget>> GetBudgets(string userID)
        {
            var users = new List<Budget>();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT ID, name, user_id AS userid from budget
                            WHERE user_id = @userID";
            users = (await conn.QueryAsync<Budget>(query, new { userID })).ToList();
            return users;
        }
        
        public async Task<Budget> GetBudget(string userID, Guid budgetID)
        {
            var users = new Budget();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT ID, name, user_id AS userid from budget
                            WHERE user_id = @userID AND ID = @budgetID";
            users = (await conn.QueryAsync<Budget>(query, new { userID, budgetID })).SingleOrDefault();
            return users;
        }

        public async Task InsertBudget(Budget budget)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            string query = @"INSERT INTO budget 
                (name, user_id)
                VALUES 
                (@name, @user_id)";
            int result = (await conn.ExecuteAsync(query, new
            {
                name = budget.Name,
                user_id = budget.UserID
            }));
        }
        
        public async Task UpdateBudget(Budget budget)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            string query = @"UPDATE budget 
                SET name = @name,
                    user_id = @user_id
                    WHERE ID = @budgetID AND user_id = @user_id";
            int result = (await conn.ExecuteAsync(query, new
            {
                name = budget.Name,
                user_id = budget.UserID,
                budgetID = budget.ID
            }));
        }
        public async Task DeleteBudget(Budget budget)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            string query = @"DELETE FROM budget 
                    WHERE ID = @budgetID AND user_id = @user_id";
            int result = (await conn.ExecuteAsync(query, new
            {
                user_id = budget.UserID,
                budgetID = budget.ID
            }));
        }
        #endregion

        #region Transactions 
        public async Task<List<Transaction>> GetTransactions()
        {
            var users = new List<Transaction>();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT t.id,amount, note, tc.name AS categoryname, ts.name AS sourcename, tt.name AS transactiontype,
                    transaction_date as transactiondate, category_id AS categoryid, 
                    source_id as sourceid, tt.id AS transactiontypeid, t.created_at AS createdtimestamp, t.updated_at AS updatedtimestamp  
                    FROM transactions t
                    LEFT JOIN transaction_category tc ON t.category_id = tc.id
                    LEFT JOIN transaction_source ts ON t.source_id = ts.id
                    LEFT JOIN transaction_type tt ON t.transaction_type_id = tt.id";
            users = (await conn.QueryAsync<Transaction>(query)).ToList();
            return users;
        }

        /// <summary>
        /// Get Transactions between a certain date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<List<Transaction>> GetTransactions(DateTime startDate, DateTime endDate)
        {
            var users = new List<Transaction>();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT t.id,amount, note, tc.name AS categoryname, ts.name AS sourcename, tt.name AS transactiontype,
                    transaction_date as transactiondate, category_id AS categoryid, 
                    source_id as sourceid, tt.id AS transactiontypeid, t.created_at AS createdtimestamp, t.updated_at AS updatedtimestamp  
                    FROM transactions t
                    LEFT JOIN transaction_category tc ON t.category_id = tc.id
                    LEFT JOIN transaction_source ts ON t.source_id = ts.id
                    LEFT JOIN transaction_type tt ON t.transaction_type_id = tt.id
                    WHERE DATE(transaction_date) BETWEEN DATE(@startDate) AND DATE(@endDate)";
            users = (await conn.QueryAsync<Transaction>(query, new { startDate, endDate })).ToList();
            return users;
        }

        public async Task<List<Transaction>> GetTransactions(Guid budgetID, DateTime startDate, DateTime endDate)
        {
            var users = new List<Transaction>();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT t.id,amount, note, tc.name AS categoryname, tc.color AS categorycolor, ts.name AS sourcename, tt.name AS transactiontype,
                    transaction_date as transactiondate, category_id AS categoryid, 
                    source_id as sourceid, tt.id AS transactiontypeid, t.created_at AS createdtimestamp, t.updated_at AS updatedtimestamp  
                    FROM transactions t
                    LEFT JOIN transaction_category tc ON t.category_id = tc.id
                    LEFT JOIN transaction_source ts ON t.source_id = ts.id
                    LEFT JOIN transaction_type tt ON t.transaction_type_id = tt.id
                    WHERE budget_id = @budgetID AND DATE(transaction_date) BETWEEN DATE(@startDate) AND DATE(@endDate)";
            users = (await conn.QueryAsync<Transaction>(query, new { budgetID, startDate, endDate })).ToList();
            return users;
        }

        public async Task<Transaction> GetTransaction(Guid transaction_id)
        {
            var users = new Transaction();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT t.id,amount, note, tc.name AS categoryname, ts.name AS sourcename, tt.name AS transactiontype, budget_id AS budgetid,
                    transaction_date as transactiondate, category_id AS categoryid, 
                    source_id as sourceid,tt.id AS transactiontypeid, t.created_at AS createdtimestamp, t.updated_at AS updatedtimestamp  
                    FROM transactions t
                    LEFT JOIN transaction_category tc ON t.category_id = tc.id
                    LEFT JOIN transaction_source ts ON t.source_id = ts.id
                    LEFT JOIN transaction_type tt ON t.transaction_type_id = tt.id
                    WHERE t.id = @transaction_id";
            users = (await conn.QueryAsync<Transaction>(query, new { transaction_id })).SingleOrDefault();
            return users;
        }

        public async Task InsertTransaction(Transaction transaction)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            string query = @"INSERT INTO transactions 
                (amount, note, transaction_date, category_id, source_id, transaction_type_id, budget_id)
                VALUES 
                (@amount, @note, @transaction_date, @category_id, @source_id, @transaction_type_id, @budget_id)";
            int result = (await conn.ExecuteAsync(query, new
            {
                amount = transaction.Amount,
                note = transaction.Note,
                transaction_date = transaction.TransactionDate,
                category_id = transaction.CategoryID,
                source_id = transaction.SourceID,
                transaction_type_id = transaction.TransactionTypeID,
                budget_id = transaction.BudgetID
            }
                ));
        }
        public async Task UpdateTransaction(Transaction transaction)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            string query = @"UPDATE transactions 
                SET amount = @amount,
                    note = @note,
                    transaction_date = @transaction_date,
                    category_id = @category_id,
                    source_id = @source_id,
                    transaction_type_id = @transaction_type_id,
                    budget_id = @budget_id
                    WHERE ID = @ID";
            int result = (await conn.ExecuteAsync(query, new
            {
                amount = transaction.Amount,
                note = transaction.Note,
                transaction_date = transaction.TransactionDate,
                category_id = transaction.CategoryID,
                source_id = transaction.SourceID,
                transaction_type_id = transaction.TransactionTypeID,
                budget_id = transaction.BudgetID,
                ID = transaction.ID
            }
                ));
        }

        public async Task DeleteTransaction(Guid transactionID)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            string query = @"DELETE FROM transactions 
                WHERE ID = @transactionID";
            int result = (await conn.ExecuteAsync(query, new
            {
                transactionID
            }));
        }
        #endregion

        #region Categories
        public async Task<List<TransactionCategory>> GetTransactionCategories(string userID)
        {
            var users = new List<TransactionCategory>();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT id, name, user_id AS userid, color
                    FROM transaction_category WHERE user_id = @userID";
            users = (await conn.QueryAsync<TransactionCategory>(query, new {userID})).ToList();
            return users;
        }

        public async Task<TransactionCategory> GetTransactionCategory(Guid category_id, string userID)
        {
            var users = new TransactionCategory();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT id, name, user_id AS userid, color
                    FROM transaction_category WHERE ID = @category_id AND user_id = @userID";
            users = (await conn.QueryAsync<TransactionCategory>(query, new { category_id, userID })).SingleOrDefault();
            return users;
        }
        public async Task InsertTransactionCategory(TransactionCategory transactionCategory)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            string query = @"INSERT INTO transaction_category 
                (name, user_id, color)
                VALUES 
                (@name, @user_id, @color)";
            int result = (await conn.ExecuteAsync(query, new { name = transactionCategory.Name, user_id = transactionCategory.UserID, color=transactionCategory.Color }));
        }

        public async Task UpdateTransactionCategory(TransactionCategory transactionCategory)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            string query = @"UPDATE transaction_category 
                SET name = @name,
                    user_id = @user_id,
                    color = @color
                    WHERE ID = @ID";
            int result = (await conn.ExecuteAsync(query, new
            {
                name = transactionCategory.Name,
                user_id = transactionCategory.UserID,
                ID = transactionCategory.ID,
                color = transactionCategory.Color
            }
            ));
        }

        public async Task DeleteTransactionCategory(Guid categoryID)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"DELETE FROM transaction_category WHERE ID = @categoryID";
            int result = (await conn.ExecuteAsync(query, new { categoryID }));
        }
        #endregion

        #region Sources
        public async Task<List<TransactionSource>> GetTransactionSources()
        {
            var users = new List<TransactionSource>();
            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT *
                    FROM transaction_source";
            users = (await conn.QueryAsync<TransactionSource>(query)).ToList();
            return users;
        }
        #endregion

        #region Types
        public async Task<List<TransactionType>> GetTransactionTypes()
        {
            var users = new List<TransactionType>();
            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT *
                    FROM transaction_type";
            users = (await conn.QueryAsync<TransactionType>(query)).ToList();
            return users;
        }
        #endregion
    }
}
