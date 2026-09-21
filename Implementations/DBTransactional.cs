using System.Threading.Tasks;

namespace Avae.DAL;

public abstract partial class DBTransactional
{
    public abstract Task<DBResult> Save(IDBLayer layer,IDBFactory factory, int? commandTimeout = null);
    public abstract Task<DBResult> Remove(IDBLayer layer, IDBFactory factory, int? commandTimeout = null);
}
