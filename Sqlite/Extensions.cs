using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Avae.DAL;

public static class SqliteExtensions
{
    class SqliteIdentity : IDBIdentity
    {
        public string Parse(string commandText)
        {
            return commandText.Replace("SCOPE_IDENTITY", "last_insert_rowid");
        }
    }

    public class SqliteFactory : DBFactory<SqliteConnection>
    {
        private readonly string connectionString;
        private readonly bool isTransaction;

        IServiceProvider provider;

        public SqliteFactory(IServiceProvider provider, string connectionString, bool isTransaction = true)
            : base(connectionString)
        {
            this.connectionString = connectionString;
            this.isTransaction = isTransaction;
            this.provider = provider;
        }

        public override DbConnection? CreateConnection()
        {
            var connection = new SqliteConnection()
            {
                ConnectionString = connectionString
            };
            connection.Open();

            var records = new List<Record>();

            //Sqlite only raise database changes on current connection
            raw.sqlite3_commit_hook(connection.Handle, (user_data) =>
            {
                if (isTransaction)
                {
                    _ = Task.Run(RaiseMonitors);
                }
                return 0;

            }, null);

            raw.sqlite3_update_hook(connection.Handle, (user_data, type, database, table, rowid) =>
            {
                records.Add(new Record()
                {
                    database = database,
                    type = type switch
                    {
                        9 => ChangeType.Delete,
                        18 => ChangeType.Insert,
                        23 => ChangeType.Update,
                        _ => ChangeType.None
                    },
                    rowid = rowid,
                    table = table
                });

                if (!isTransaction)
                {
                    RaiseMonitors();
                }

            }, null);

            void RaiseMonitors()
            {
                foreach (var monitor in IDBFactory.Monitors.OfType<DBMonitor>())
                    foreach (var record in records.DistinctBy(r => r.rowid))
                    {
                        monitor.OnChanged(record.type, record.database, record.table, record.rowid, DBContext.CurrentConnectionId.Value);
                    }
            }

            return new DBLogConnection(provider, connection);
        }
    }

    public static void UseSqliteFactory(this IServiceCollection services,
       string connectionString, bool isTransaction = true)
    {
        services.AddSingleton<IDBIdentity, SqliteIdentity>();
        services.AddSingleton<IDBFactory>(sp => new SqliteFactory(sp, connectionString, isTransaction));
        services.AddTransient<IDbConnection>(sp => sp.GetRequiredService<IDBFactory>().CreateConnection()!);
    }
}
