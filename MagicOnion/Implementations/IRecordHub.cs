using MagicOnion;
using System;
using System.Threading.Tasks;

namespace Avae.DAL;

public interface IRecordHubReceiver<TObject> where TObject : class, new()
{
    void OnChanged(Record<TObject> record);
}

public interface IRecordHub<TObject> :
    IStreamingHub<IRecordHub<TObject>, IRecordHubReceiver<TObject>>
    where TObject : class, new()
{
    Task<Guid> AddReceiverAsync();
    Task RemoveAsync();
    void OnRecordChanged(Record<TObject> e);
}
