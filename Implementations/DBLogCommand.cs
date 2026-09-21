using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Avae.DAL;

public class DBLogCommand(ILogger? logger, DbCommand command, IDBIdentity? identity = null) : DbCommand
{
    private bool _disposed;

    [AllowNull]
    public override string CommandText
    {
        get => command.CommandText;
        set
        {

            if (value != null && identity != null)
                value = identity.Parse(value);

            command.CommandText = value;
        }
    }

    public override int CommandTimeout
    {
        get => command.CommandTimeout;
        set => command.CommandTimeout = value;
    }


    public override CommandType CommandType
    {
        get => command.CommandType;
        set => command.CommandType = value;
    }


    public override UpdateRowSource UpdatedRowSource
    {
        get => command.UpdatedRowSource;
        set => command.UpdatedRowSource = value;
    }


    protected override DbConnection? DbConnection
    {
        get => command.Connection;
        set => command.Connection = value;
    }


    protected override DbParameterCollection DbParameterCollection => command.Parameters;


    protected override DbTransaction? DbTransaction
    {
        get => command.Transaction;
        set => command.Transaction = value;
    }


    public override bool DesignTimeVisible
    {
        get => command.DesignTimeVisible;
        set => command.DesignTimeVisible = value;
    }

    ~DBLogCommand() => Dispose(false);


    protected override void Dispose(bool Disposing)
    {
        if (_disposed) return;
        if (Disposing)
        {
            // No managed resources to release.
        }
        // Release unmanaged resources.
        command?.Dispose();
        //command = null;
        // Do not release logger.  Its lifetime is controlled by caller.
        _disposed = true;
    }


    public override void Cancel()
    {
        //_logger.LogDebug("Cancelling database command.");
        command.Cancel();
    }


    public override int ExecuteNonQuery()
    {
        LogCommandBeforeExecuted();
        int result = command.ExecuteNonQuery();
        return result;
    }


    public override object? ExecuteScalar()
    {
        LogCommandBeforeExecuted();
        return command.ExecuteScalar();
    }


    public override void Prepare()
    {
        //_logger.LogDebug("Preparing database command.");
        command.Prepare();
    }


    protected override DbParameter CreateDbParameter() => command.CreateParameter();


    protected override DbDataReader ExecuteDbDataReader(CommandBehavior Behavior)
    {
        LogCommandBeforeExecuted();
        return command.ExecuteReader(Behavior);
    }


    private void LogCommandBeforeExecuted()
    {
        string request = command.CommandText;
        foreach (IDataParameter parameter in command.Parameters)
        {
            if (parameter.Direction == ParameterDirection.Output ||
              parameter.Direction == ParameterDirection.ReturnValue) continue;
            request = request.ReplaceWholeWord($"@{parameter.ParameterName}", parameter.Value?.ToString() ?? string.Empty);
        }

        logger?.LogInformation("Request: {Request}", request);
    }
}
