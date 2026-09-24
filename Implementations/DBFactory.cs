using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Avae.DAL;

public class DBFactory<TDbConnection>(string connectionString) : DbProviderFactory,
    IDBFactory
    where TDbConnection : DbConnection, new()
{
    public Dictionary<Type, string> Sessions { get; } = new();

    public List<IDBMonitor> Monitors { get; } = new();

    public override DbConnection? CreateConnection()
    {
        var connection = new TDbConnection()
        {
            ConnectionString = connectionString
        };
        return connection;
    }
}
