using System.Data.Common;

namespace Avae.DAL;

public class DBFactory<TDbConnection>(string connectionString) : DbProviderFactory,
    IDBFactory
    where TDbConnection : DbConnection, new()
{
    public override DbConnection? CreateConnection()
    {
        var connection = new TDbConnection()
        {
            ConnectionString = connectionString
        };
        connection.Open();
        return connection;
    }
}
