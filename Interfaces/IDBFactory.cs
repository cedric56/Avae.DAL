using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Avae.DAL;

public interface IDBFactory
{
    List<IDBMonitor> Monitors { get; }

    Dictionary<Type, string> Sessions { get; }


    DbConnection? CreateConnection();
}
