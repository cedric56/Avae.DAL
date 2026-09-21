using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avae.DAL;

public abstract class EntityHandler
{
    public static Dictionary<string, EntityHandler> Handlers { get; set; } = [];

    public abstract Type Type { get; }
    public abstract Type Enumerable { get; }

    public abstract Task<object> GetAllAsync(int? commandTimeout = null);
    public abstract Task<object?> GetAsync(long id, int? commandTimeout = null);
    public abstract Task<object> FindByAnyAsync(Dictionary<string, object> filters, int? commandTimeout = null);
    public abstract Task<object> WhereAsync(Dictionary<string, object> filters, int? commandTimeout = null);
}

public class EntityHandler<T>(IDBLayer layer) : EntityHandler where T : class, new()
{
    public override Type Type => typeof(T);

    public override Type Enumerable => typeof(IEnumerable<T>);

    public override async Task<object> GetAllAsync(int? commandTimeout = null)
    {
        return await layer.GetAllAsync<T>(commandTimeout: commandTimeout);
    }
    public override async Task<object?> GetAsync(long id, int? commandTimeout = null)
    {
        return await layer.GetAsync<T>(id, commandTimeout: commandTimeout);
    }
    public override async Task<object> FindByAnyAsync(Dictionary<string, object> filters, int? commandTimeout = null)
    {
        return await layer.FindByAnyAsync<T>(filters);
    }
    public override async Task<object> WhereAsync(Dictionary<string, object> filters, int? commandTimeout = null)
    {
        return await layer.WhereAsync<T>(filters);
    }
}
