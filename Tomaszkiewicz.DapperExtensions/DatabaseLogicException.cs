using System;
using System.Data.SqlClient;
using System.Text;

namespace Tomaszkiewicz.DapperExtensions
{
    public class DatabaseLogicException : Exception
    {
        public DatabaseLogicException(SqlException exception, string resolvedDatabaseCode, string query, object args)
            : base(FormatMessage(exception.Message, resolvedDatabaseCode, query, args), exception)
        { }

        private static string FormatMessage(string message, string resolvedDatabaseCode, string query, object args)
        {
            var sb = new StringBuilder();

            sb.AppendLine(resolvedDatabaseCode);
            sb.AppendLine(message);
            sb.Append("Query: ");
            sb.Append(query);

            foreach (var property in args.GetType().GetProperties())
            {
                sb.Append("\t");
                sb.Append(property.Name);
                sb.Append(": ");
                sb.AppendLine(property.GetValue(args).ToString());
            }

            return sb.ToString();
        }
    }
}