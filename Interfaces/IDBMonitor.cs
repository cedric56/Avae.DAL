using System;

namespace Avae.DAL;

public interface IDBMonitor
{
}

public interface IDBMonitor<T> : IDBMonitor where T : class, new()
{
    void OnChanged(Record<T> record);
    event EventHandler<Record<T>> OnRecordChanged;
}
