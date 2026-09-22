namespace Avae.DAL;

public partial class RecordHubReceiver<TObject>(IDBMonitor<TObject> monitor, IDBSessions dBSessions) : IRecordHubReceiver<TObject> where TObject : class, new()
{
    public void OnChanged(Record<TObject> record)
    {
        dBSessions.Sessions.TryGetValue(typeof(TObject), out var sessionId);

        if (record.Contains(sessionId))
            return;

        monitor?.OnChanged(record);
    }
}
