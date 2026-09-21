using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avae.DAL;

public partial class MagicCatchableLayer(IServiceProvider provider, string url, int globalCommandTimeout, ILogger? logger = null)
    : MagicOnionLayer(provider, url, globalCommandTimeout)
{
    public override int Execute(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        try
        {
            return base.Execute(sql, param, transaction, commandTimeout, commandType);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return 0;
        }
    }

    public override async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        try
        {
            return await base.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return 0;
        }
    }

    public override IEnumerable<T> FindByAny<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        try
        {
            return base.FindByAny<T>(filters, transaction, commandTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override async Task<IEnumerable<T>> FindByAnyAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        try
        {
            return await base.FindByAnyAsync<T>(filters, transaction, commandTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override T? Get<T>(long id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        try
        {
            return base.Get<T>(id, transaction, commandTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return null;
        }
    }

    public override IEnumerable<T> GetAll<T>(IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        try
        {
            return base.GetAll<T>(transaction, commandTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override async Task<IEnumerable<T>> GetAllAsync<T>(IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        try
        {
            return await base.GetAllAsync<T>(transaction, commandTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override async Task<T?> GetAsync<T>(long id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        try
        {
            return await base.GetAsync<T>(id, transaction, commandTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return null;
        }
    }

    public override IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return base.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return base.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return base.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return base.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return base.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return base.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return await base.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }
    
    public override async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return await base.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return await base.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return await base.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return await base.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
    {
        try
        {
            return await base.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override async Task<DBResult> Remove(DBTransactional transactional, int? commandTimeout = null)
    {
        try
        {
            return await base.Remove(transactional, commandTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return new DBResult()
            {
                Successful = false,
                Exception = ex.Message
            };
        }
    }

    public override async Task<DBResult> Save(DBTransactional transactional, int? commandTimeout = null)
    {
        try
        {
            return await base.Save(transactional, commandTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return new DBResult()
            {
                Successful = false,
                Exception = ex.Message
            };
        }
    }

    public override IEnumerable<T> Where<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        try
        {
            return base.Where<T>(filters, transaction, commandTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }

    public override async Task<IEnumerable<T>> WhereAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        try
        {
            return await base.WhereAsync<T>(filters, transaction, commandTimeout);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex.Message);
            return [];
        }
    }
}
