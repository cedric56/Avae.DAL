using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Avae.DAL;

public class DBLogConnection(IServiceProvider provider, DbConnection connection) : DbConnection
{
    protected override DbCommand CreateDbCommand()
        => new DBLogCommand(provider.GetService<ILogger>(), connection.CreateCommand(), provider.GetService<IDBIdentity>());

    [AllowNull]
    public override string ConnectionString { get => connection.ConnectionString; set => connection.ConnectionString = value; }
    public override string Database => connection.Database;
    public override string DataSource => connection.DataSource;
    public override string ServerVersion => connection.ServerVersion;
    public override ConnectionState State => connection.State;
    public override void ChangeDatabase(string databaseName) => connection.ChangeDatabase(databaseName);
    public override void Close() => connection.Close();
    public override void Open() => connection.Open();

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        => connection.BeginTransaction(isolationLevel);

    public override void EnlistTransaction(System.Transactions.Transaction? transaction)
        => connection.EnlistTransaction(transaction);

    //public override bool CanRaiseEvents => false;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            connection.Dispose();
        base.Dispose(disposing);
    }
}
