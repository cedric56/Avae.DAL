using System.Collections.Generic;
using System.Data.Common;

namespace Avae.DAL;

public interface IDBFactory
{
    public static List<IDBMonitor> Monitors { get; } = [];

    DbConnection? CreateConnection();
}
