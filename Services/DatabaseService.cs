using System.Net;
using Dapper;
using Npgsql;
using TestMVC.Models;

namespace TestMVC.Services
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
                    WHERE DATE(transaction_date) BETWEEN DATE(@startDate) AND DATE(@endDate)          
";
            users = (await conn.QueryAsync<Transaction>(query, new {startDate, endDate})).ToList();
            return users;
        }
        
        public async Task<Transaction> GetTransaction(Guid transaction_id)
        {
            var users = new Transaction();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT t.id,amount, note, tc.name AS categoryname, ts.name AS sourcename, tt.name AS transactiontype,
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
                (amount, note, transaction_date, category_id, source_id, transaction_type_id)
                VALUES 
                (@amount, @note, @transaction_date, @category_id, @source_id, @transaction_type_id)";
            int result = (await conn.ExecuteAsync(query, new
            {
                amount = transaction.Amount,
                note = transaction.Note,
                transaction_date = transaction.TransactionDate,
                category_id = transaction.CategoryID,
                source_id = transaction.SourceID,
                transaction_type_id = transaction.TransactionTypeID
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
                    transaction_type_id = @transaction_type_id
                    WHERE ID = @ID";
            int result = (await conn.ExecuteAsync(query, new
            {
                amount = transaction.Amount,
                note = transaction.Note,
                transaction_date = transaction.TransactionDate,
                category_id = transaction.CategoryID,
                source_id = transaction.SourceID,
                transaction_type_id = transaction.TransactionTypeID,
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


        public async Task<List<TransactionCategory>> GetTransactionCategories()
        {
            var users = new List<TransactionCategory>();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT *
                    FROM transaction_category";
            users = (await conn.QueryAsync<TransactionCategory>(query)).ToList();
            return users;
        }
        
        public async Task<List<TransactionSource>> GetTransactionSources()
        {
            var users = new List<TransactionSource>();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT *
                    FROM transaction_source";
            users = (await conn.QueryAsync<TransactionSource>(query)).ToList();
            return users;
        }
        
        public async Task<List<TransactionType>> GetTransactionTypes()
        {
            var users = new List<TransactionType>();

            using var conn = new NpgsqlConnection(_connectionString);
            string query = @"SELECT *
                    FROM transaction_type";
            users = (await conn.QueryAsync<TransactionType>(query)).ToList();
            return users;
        }

    }
}
