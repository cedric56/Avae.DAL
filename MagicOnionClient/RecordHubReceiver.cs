namespace Avae.DAL;

public partial class RecordHubReceiver<TObject>(IDBMonitor<TObject> monitor) : IRecordHubReceiver<TObject> where TObject : class, new()
{
    public void OnChanged(Record<TObject> record)
    {
        IDBLayer.Sessions.TryGetValue(typeof(TObject), out var sessionId);

        if (record.Contains(sessionId))
            return;

        monitor?.OnChanged(record);
    }
}
