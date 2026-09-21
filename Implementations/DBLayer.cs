using Dapper;
using Dapper.Contrib.Extensions;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace Avae.DAL;

public class DBLayer(IDBFactory factory) : IDBLayer
{
    public Task<DBResult> Remove(DBTransactional transactional, int? commandTimeout = null)
    {
        return transactional.Remove(this, factory, commandTimeout);
    }

    public Task<DBResult> Save(DBTransactional transactional, int? commandTimeout = null)
    {
        return transactional.Save(this, factory, commandTimeout);
    }

    public T? Get<T>(long id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        using var db = factory.CreateConnection()!;
        return db.Get<T>(id, transaction, commandTimeout);
    }

    public IEnumerable<T> GetAll<T>(IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        using var db = factory.CreateConnection()!;
        return db.GetAll<T>(transaction, commandTimeout);
    }

    public Task<IEnumerable<T>> GetAllAsync<T>(IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        using var db = factory.CreateConnection()!;
        return db.GetAllAsync<T>(transaction, commandTimeout);
    }

    public async Task<T?> GetAsync<T>(long id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        using var db = factory.CreateConnection()!;
        return await db.GetAsync<T>(id, transaction, commandTimeout);
    }

    public Task<IEnumerable<T>> FindByAnyAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, int? commandTimeout = null) where T : class, new()
    {
        var sql = Create<T>(filters, " OR ", out var parameters);
        using var db = factory.CreateConnection()!;
        return db.QueryAsync<T>(sql, parameters);
    }

    public IEnumerable<T> FindByAny<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, int? commandTimeout = null) where T : class, new()
    {
        var sql = Create<T>(filters, " OR ", out var parameters);
        using var db = factory.CreateConnection()!;
        return db.Query<T>(sql, parameters);
    }

    public Task<IEnumerable<T>> WhereAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, int? commandTimeout = null) where T : class, new()
    {
        var sql = Create<T>(filters, " AND ", out var parameters);
        using var db = factory.CreateConnection()!;
        return db.QueryAsync<T>(sql, parameters);
    }

    public IEnumerable<T> Where<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, int? commandTimeout = null) where T : class, new()
    {
        var sql = Create<T>(filters, " AND ", out var parameters);
        using var db = factory.CreateConnection()!;
        return db.Query<T>(sql, parameters);
    }

    public int Execute(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var db = factory.CreateConnection()!;
        return db.Execute(sql, param, transaction, commandTimeout, commandType);
    }

    public Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var db = factory.CreateConnection()!;
        return db.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
    }


    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new()
    {
        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TReturn> map, string splitOn = "Id")
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(command, map, splitOn);
    }

    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new()
    {
        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
    {
        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
    {
        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
    {
        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
    {
        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(command, map, splitOn);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(command, map, splitOn);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(command, map, splitOn);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(command, map, splitOn);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(command, map, splitOn);
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new()
    {
        using var db = factory.CreateConnection()!;
        return db.QueryAsync(command, map, splitOn);
    }

    private static readonly ConcurrentDictionary<Type, string> _columnCache = new();


    private static List<PropertyInfo> GetMappedProperties<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => p.GetCustomAttribute<ComputedAttribute>() == null)
            .Where(p => p.GetCustomAttribute<IgnoreMemberAttribute>() == null)
            .ToList();
    }

    private static string GetColumns<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>()
    {
        return _columnCache.GetOrAdd(typeof(T), _ =>
            string.Join(", ", GetMappedProperties<T>().Select(p => p.Name)));
    }

    private static string Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, string condition, out DynamicParameters parameters)
    {
        var conditions = new List<string>();
        parameters = new DynamicParameters();

        foreach (var pair in filters)
        {
            parameters.Add(pair.Key, pair.Value);
            conditions.Add($"(@{pair.Key} IS NOT NULL AND {pair.Key} = @{pair.Key})");
        }

        string where = string.Join(condition, conditions);
        string columns = GetColumns<T>();

        return $"SELECT {columns} FROM {typeof(T).Name} WHERE {where}";
    }
}
