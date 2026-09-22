using System;
using System.Collections.Generic;

namespace Avae.DAL;

public interface IDBSessions
{
    Dictionary<Type, string> Sessions { get; set; }
}

public class DBSessions : IDBSessions
{
    public Dictionary<Type, string> Sessions { get; set; } = new();
}
