using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace DqtApi.DataStore.Sql
{
    public static class DbContextTransactionExtensions
    {
        public static async Task AcquireAdvisoryLock(this IDbContextTransaction transaction, long key)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            var dbTransaction = transaction.GetDbTransaction();

            using (var command = dbTransaction.Connection.CreateCommand())
            {
                command.Transaction = dbTransaction;
                command.CommandText = "SELECT pg_advisory_xact_lock(@id);";

                var idParameter = command.CreateParameter();
                idParameter.ParameterName = "id";
                idParameter.Value = key;

                command.Parameters.Add(idParameter);

                await command.ExecuteNonQueryAsync();
            }
        }

        public static Task AcquireAdvisoryLock(this IDbContextTransaction transaction, params object[] keys)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (keys.Length == 0)
            {
                throw new ArgumentException("At least one key must be specified.", nameof(keys));
            }

            var hashCode = new HashCode();

            foreach (var key in keys)
            {
                hashCode.Add(key.GetHashCode());
            }

            var combinedKey = hashCode.ToHashCode();

            return AcquireAdvisoryLock(transaction, combinedKey);
        }
    }
}
