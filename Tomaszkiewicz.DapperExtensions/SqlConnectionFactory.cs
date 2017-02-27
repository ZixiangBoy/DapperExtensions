using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Tomaszkiewicz.DapperExtensions
{
    /// <summary>
    /// This is handy class that covers creating connection, executing queries on it and handling errors in a nice way :)
    /// </summary>
    public class SqlConnectionFactory
    {
        private readonly string _connectionString;
        private readonly IDatabaseCodeResolver _databaseCodeResolver;

        public SqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="databaseCodeResolver">Resolver that translates exception codes thrown from database to named errors.</param>
        public SqlConnectionFactory(string connectionString, IDatabaseCodeResolver databaseCodeResolver)
        {
            _connectionString = connectionString;
            _databaseCodeResolver = databaseCodeResolver;
        }


        public async Task<SqlConnection> GetSqlConnection()
        {
            var conn = new SqlConnection(_connectionString);

            await conn.OpenAsync();

            return conn;
        }

        public async Task<int> Execute(string query, object args = null)
        {
            using (var conn = await GetSqlConnection())
            {
                try
                {
                    return await conn.ExecuteAsync(query, args);
                }
                catch (SqlException ex)
                {
                    throw new DatabaseLogicException(ex, ResolveDatabaseCode(ex.Number), query, args);
                }
            }
        }

        public async Task<T> ExecuteScalar<T>(string query, object args = null)
        {
            using (var conn = await GetSqlConnection())
            {
                try
                {
                    return await conn.ExecuteScalarAsync<T>(query, args);
                }
                catch (SqlException ex)
                {
                    throw new DatabaseLogicException(ex, ResolveDatabaseCode(ex.Number), query, args);
                }
            }
        }

        public async Task<IEnumerable<T>> Query<T>(string query, object args = null)
        {
            using (var conn = await GetSqlConnection())
            {
                try
                {
                    return await conn.QueryAsync<T>(query, args);
                }
                catch (SqlException ex)
                {
                    throw new DatabaseLogicException(ex, ResolveDatabaseCode(ex.Number), query, args);
                }
            }
        }

        public async Task<T> Single<T>(string query, object args = null)
        {
            using (var conn = await GetSqlConnection())
            {
                try
                {
                    return (await conn.QueryAsync<T>(query, args)).FirstOrDefault();
                }
                catch (SqlException ex)
                {
                    throw new DatabaseLogicException(ex, ResolveDatabaseCode(ex.Number), query, args);
                }
            }
        }
        
        private string ResolveDatabaseCode(int code)
        {
            return _databaseCodeResolver != null ? _databaseCodeResolver.Resolve(code) : "Unresolved error";
        }
    }
}