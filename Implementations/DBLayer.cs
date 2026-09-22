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

public class DBLayer(IDBFactory factory) : IDBLayer, IDisposable
{
    private TResult UseConnection<TResult>(
        IDbTransaction? transaction,
        Func<IDbConnection, IDbTransaction?, TResult> action)
    {
        if (transaction?.Connection is { } conn)
            return action(conn, transaction);

        using var db = factory.CreateConnection()!;
        return action(db, null);
    }

    private async Task<TResult> UseConnectionAsync<TResult>(
        IDbTransaction? transaction,
        Func<IDbConnection, IDbTransaction?, Task<TResult>> action)
    {
        if (transaction?.Connection is { } conn)
            return await action(conn, transaction).ConfigureAwait(false);

        await using var db = factory.CreateConnection()!;
        return await action(db, null).ConfigureAwait(false);
    }

    public Task<DBResult> Remove(DBTransactional transactional, int? commandTimeout = null)
        => transactional.Remove(this, factory, commandTimeout);

    public Task<DBResult> Save(DBTransactional transactional, int? commandTimeout = null)
        => transactional.Save(this, factory, commandTimeout);

    public T? Get<T>(long id, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => UseConnection(transaction, (conn, tx) => conn.Get<T>(id, tx, commandTimeout));

    public IEnumerable<T> GetAll<T>(IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => UseConnection(transaction, (conn, tx) => conn.GetAll<T>(tx, commandTimeout));

    public async Task<T?> GetAsync<T>(long id, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => await UseConnectionAsync(transaction, (conn, tx) => conn.GetAsync<T>(id, tx, commandTimeout));

    public Task<IEnumerable<T>> GetAllAsync<T>(IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => UseConnectionAsync(transaction, (conn, tx) => conn.GetAllAsync<T>(tx, commandTimeout));

    public IEnumerable<T> Where<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => Filter<T>(filters, " AND ", transaction, commandTimeout);

    public Task<IEnumerable<T>> WhereAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => FilterAsync<T>(filters, " AND ", transaction, commandTimeout);

    public IEnumerable<T> FindByAny<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => Filter<T>(filters, " OR ", transaction, commandTimeout);

    public Task<IEnumerable<T>> FindByAnyAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => FilterAsync<T>(filters, " OR ", transaction, commandTimeout);

    private IEnumerable<T> Filter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Dictionary<string, object> filters, string condition, IDbTransaction? transaction, int? commandTimeout)
        where T : class, new()
    {
        var sql = Create<T>(filters, condition, out var parameters);
        return UseConnection(transaction, (conn, tx) => conn.Query<T>(sql, parameters, tx, commandTimeout: commandTimeout));
    }

    private Task<IEnumerable<T>> FilterAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Dictionary<string, object> filters, string condition, IDbTransaction? transaction, int? commandTimeout)
        where T : class, new()
    {
        var sql = Create<T>(filters, condition, out var parameters);
        return UseConnectionAsync(transaction, (conn, tx) => conn.QueryAsync<T>(sql, parameters, tx, commandTimeout));
    }

    public int Execute(string sql, object? param = null, IDbTransaction? transaction = null,
        int? commandTimeout = null, CommandType? commandType = null)
        => UseConnection(transaction, (conn, tx) => conn.Execute(sql, param, tx, commandTimeout, commandType));

    public Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null,
        int? commandTimeout = null, CommandType? commandType = null)
        => UseConnectionAsync(transaction, (conn, tx) => conn.ExecuteAsync(sql, param, tx, commandTimeout, commandType));

    public IEnumerable<TReturn> Query<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond,
        TReturn>(
        string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new()
        => UseConnection(transaction, (conn, tx) =>
            conn.Query(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    public IEnumerable<TReturn> Query<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird,
        TReturn>(
        string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new() where TThird : new()
        => UseConnection(transaction, (conn, tx) =>
            conn.Query(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    public IEnumerable<TReturn> Query<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth,
        TReturn>(
        string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
        => UseConnection(transaction, (conn, tx) =>
            conn.Query(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    public IEnumerable<TReturn> Query<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth,
        TReturn>(
        string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
        => UseConnection(transaction, (conn, tx) =>
            conn.Query(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    public IEnumerable<TReturn> Query<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth,
        TReturn>(
        string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
        => UseConnection(transaction, (conn, tx) =>
            conn.Query(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    public IEnumerable<TReturn> Query<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSeventh,
        TReturn>(
        string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
        => UseConnection(transaction, (conn, tx) =>
            conn.Query(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
        CommandDefinition command, Func<TFirst, TSecond, TReturn> map, string splitOn = "Id")
    {
        // Deliberately not routed through UseConnectionAsync: a CommandDefinition
        // carries its own transaction/cancellation/timeout, so the transaction-reuse
        // branch used everywhere else doesn't apply here.
        return QueryOwnConnectionAsync(conn => conn.QueryAsync(command, map, splitOn));
    }

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
        string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new()
        => UseConnectionAsync(transaction, (conn, tx) =>
            conn.QueryAsync(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(
        string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new() where TThird : new()
        => UseConnectionAsync(transaction, (conn, tx) =>
            conn.QueryAsync(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
        string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
        => UseConnectionAsync(transaction, (conn, tx) =>
            conn.QueryAsync(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
        string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
        => UseConnectionAsync(transaction, (conn, tx) =>
            conn.QueryAsync(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
        string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
        => UseConnectionAsync(transaction, (conn, tx) =>
            conn.QueryAsync(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
        string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null,
        bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null,
        IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
        => UseConnectionAsync(transaction, (conn, tx) =>
            conn.QueryAsync(sql, map, param, tx, buffered, splitOn, commandTimeout, commandType));

    private async Task<TResult> QueryOwnConnectionAsync<TResult>(Func<IDbConnection, Task<TResult>> action)
    {
        await using var db = factory.CreateConnection()!;
        return await action(db).ConfigureAwait(false);
    }

    private static readonly ConcurrentDictionary<Type, string> _columnCache = new();

    private static List<PropertyInfo> GetMappedProperties<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>()
        => typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => p.GetCustomAttribute<ComputedAttribute>() == null)
            .Where(p => p.GetCustomAttribute<IgnoreMemberAttribute>() == null)
            .ToList();

    private static string GetColumns<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>()
        => _columnCache.GetOrAdd(typeof(T), _ =>
            string.Join(", ", GetMappedProperties<T>().Select(p => p.Name)));

    private static string GetTableName<T>()
    {
        var table = typeof(T).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(T).Name;

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
        return string.IsNullOrEmpty(where)
            ? $"SELECT {GetColumns<T>()} FROM {GetTableName<T>()}"
            : $"SELECT {GetColumns<T>()} FROM {GetTableName<T>()} WHERE {where}";
    }

    public void Dispose()
    {
        _columnCache.Clear();
    }
}