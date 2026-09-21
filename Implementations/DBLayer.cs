using Dapper;
using Dapper.Contrib.Extensions;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
        if (transaction?.Connection is { } conn)
            return conn.Get<T>(id, transaction, commandTimeout);

        using var db = factory.CreateConnection()!;
        return db.Get<T>(id, transaction: null, commandTimeout);
    }

    public IEnumerable<T> GetAll<T>(IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        if (transaction?.Connection is { } conn)
            return conn.GetAll<T>(transaction, commandTimeout);

        using var db = factory.CreateConnection()!;
        return db.GetAll<T>(transaction: null, commandTimeout);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>(IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        if (transaction?.Connection is { } conn)
            return await conn.GetAllAsync<T>(transaction, commandTimeout).ConfigureAwait(false);

        using var db = factory.CreateConnection()!;
        return await db.GetAllAsync<T>(transaction: null, commandTimeout).ConfigureAwait(false);
    }

    public async Task<T?> GetAsync<T>(long id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        if (transaction?.Connection is { } conn)
            return await conn.GetAsync<T>(id, transaction, commandTimeout).ConfigureAwait(false);

        using var db = factory.CreateConnection()!;
        return await db.GetAsync<T>(id, transaction: null, commandTimeout).ConfigureAwait(false);
    }

    public async Task<IEnumerable<T>> FindByAnyAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        var sql = Create<T>(filters, " OR ", out var parameters);
        if (transaction?.Connection is { } conn)
            return await conn.QueryAsync<T>(sql, parameters, transaction, commandTimeout).ConfigureAwait(false);

        using var db = factory.CreateConnection()!;
        return await db.QueryAsync<T>(sql, parameters, transaction: null, commandTimeout).ConfigureAwait(false);
    }

    public IEnumerable<T> FindByAny<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        var sql = Create<T>(filters, " OR ", out var parameters);
        if (transaction?.Connection is { } conn)
            return conn.Query<T>(sql, parameters, transaction, commandTimeout: commandTimeout);

        using var db = factory.CreateConnection()!;
        return db.Query<T>(sql, parameters, transaction: null, commandTimeout: commandTimeout);
    }

    public async Task<IEnumerable<T>> WhereAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        var sql = Create<T>(filters, " AND ", out var parameters);
        if (transaction?.Connection is { } conn)
            return await conn.QueryAsync<T>(sql, parameters, transaction, commandTimeout).ConfigureAwait(false);

        using var db = factory.CreateConnection()!;
        return await db.QueryAsync<T>(sql, parameters, transaction: null, commandTimeout).ConfigureAwait(false);
    }

    public IEnumerable<T> Where<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        var sql = Create<T>(filters, " AND ", out var parameters);
        if (transaction?.Connection is { } conn)
            return conn.Query<T>(sql, parameters, transaction, commandTimeout: commandTimeout);

        using var db = factory.CreateConnection()!;
        return db.Query<T>(sql, parameters, transaction: null, commandTimeout: commandTimeout);
    }

    public int Execute(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        if (transaction?.Connection is { } conn)
            return conn.Execute(sql, param, transaction, commandTimeout, commandType);

        using var db = factory.CreateConnection()!;
        return db.Execute(sql, param, transaction: null, commandTimeout, commandType);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        if (transaction?.Connection is { } conn)
            return await conn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);

        using var db = factory.CreateConnection()!;
        return await db.ExecuteAsync(sql, param, transaction: null, commandTimeout, commandType);
    }


    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new()
    {
        if (transaction?.Connection is { } conn)
            return conn.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);

        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType);
    }

    public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TReturn> map, string splitOn = "Id")
    {
        using var db = factory.CreateConnection()!;
        return await db.QueryAsync(command, map, splitOn).ConfigureAwait(false);
    }

    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new()
    {
        if (transaction?.Connection is { } conn)
            return conn.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);

        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType);
    }

    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
    {
        if (transaction?.Connection is { } conn)
            return conn.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);

        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType);
    }

    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
    {
        if (transaction?.Connection is { } conn)
            return conn.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);

        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType);
    }

    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
    {
        if (transaction?.Connection is { } conn)
            return conn.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);

        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType);
    }

    public IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
    {
        if (transaction?.Connection is { } conn)
            return conn.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);

        using var db = factory.CreateConnection()!;
        return db.Query(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType);
    }

    public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new()
    {
        if (transaction?.Connection is { } conn)
            return await conn.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);

        using var db = factory.CreateConnection()!;
        return await db.QueryAsync(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new()
    {
        if (transaction?.Connection is { } conn)
            return await conn.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);

        using var db = factory.CreateConnection()!;
        return await db.QueryAsync(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
    {
        if (transaction?.Connection is { } conn)
            return await conn.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);

        using var db = factory.CreateConnection()!;
        return await db.QueryAsync(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
    {
        if (transaction?.Connection is { } conn)
            return await conn.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);

        using var db = factory.CreateConnection()!;
        return await db.QueryAsync(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);

    }

    public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
    {
        if (transaction?.Connection is { } conn)
            return await conn.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);

        using var db = factory.CreateConnection()!;
        return await db.QueryAsync(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
    {
        if (transaction?.Connection is { } conn)
            return await conn.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);

        using var db = factory.CreateConnection()!;
        return await db.QueryAsync(sql, map, param, transaction: null, buffered, splitOn, commandTimeout, commandType).ConfigureAwait(false);
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

    //private static string Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, string condition, out DynamicParameters parameters)
    //{
    //    var conditions = new List<string>();
    //    parameters = new DynamicParameters();

    //    foreach (var pair in filters)
    //    {
    //        parameters.Add(pair.Key, pair.Value);
    //        conditions.Add($"(@{pair.Key} IS NOT NULL AND {pair.Key} = @{pair.Key})");
    //    }

    //    string where = string.Join(condition, conditions);
    //    string columns = GetColumns<T>();
    //    if(string.IsNullOrEmpty(where))
    //        return $"SELECT {columns} FROM {typeof(T).Name}";
    //    return $"SELECT {columns} FROM {typeof(T).Name} WHERE {where}";
    //}

    private static string GetTableName<T>()
    {
        var table = typeof(T).GetCustomAttribute<TableAttribute>()?.Name
                    ?? typeof(T).Name;

        if (!table.All(c => char.IsLetterOrDigit(c) || c == '_'))
            throw new InvalidOperationException($"Unsafe table name: {table}");

        return table;
    }

    private static string Create<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
    Dictionary<string, object> filters,
    string condition,
    out DynamicParameters parameters)
    {
        parameters = new DynamicParameters();

        if (filters is null || filters.Count == 0)
            return $"SELECT {GetColumns<T>()} FROM {GetTableName<T>()}";

        var allowed = GetMappedProperties<T>()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var conditions = new List<string>();
        var i = 0;

        foreach (var (key, value) in filters)
        {
            if (!allowed.Contains(key))
                throw new ArgumentException($"Invalid filter column '{key}' for {typeof(T).Name}.");

            var paramName = $"p{i++}";
            var column = allowed.First(c => c.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (value is null)
                conditions.Add($"({column} IS NULL)");
            else
            {
                parameters.Add(paramName, value);
                conditions.Add($"({column} = @{paramName})");
            }
        }

        var where = string.Join(condition, conditions);
        if (string.IsNullOrEmpty(where))
            return $"SELECT {GetColumns<T>()} FROM {GetTableName<T>()}";
        return $"SELECT {GetColumns<T>()} FROM {GetTableName<T>()} WHERE {where}";
    }
}
