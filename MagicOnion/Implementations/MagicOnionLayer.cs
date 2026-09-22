using Dapper;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Avae.DAL;

public partial class MagicOnionLayer(
    IServiceProvider provider,
    string url,
    int globalCommandTimeout,
    IDBSessions dBSessions) : IDBLayer
{
    private IMagicOnionLayer Rpc =>
        provider.GetRequiredService<IMagicOnionLayer>();

    private IXmlHttpRequest Xhr =>
        provider.GetRequiredService<IXmlHttpRequest>();

    private async Task<T> InvokeAsync<T>(
        Func<IMagicOnionLayer, Task<DBResult>> call,
        Func<byte[], T> deserialize,
        T empty)
    {
        using var cts = new CancellationTokenSource(globalCommandTimeout);
        var result = await call(Rpc.WithCancellationToken(cts.Token))
            .ConfigureAwait(false);

        if (!result.Successful)
            throw new InvalidOperationException(result.Exception ?? "RPC failed");

        if (result.Data is null || result.Data.Length == 0 || result.Data == Array.Empty<byte>())
            return empty;

        return deserialize(result.Data);
    }

    private T BrowserSend<T>(string method, object[] args, Func<byte[], T> deserialize, T empty)
    {
        var bytes = Xhr.Send(
            url,
            method,
            MessagePackSerializer.Serialize(args),
            globalCommandTimeout);

        if (bytes is null || bytes.Length == 0 || bytes == Array.Empty<byte>())
            return empty;

        return deserialize(bytes);
    }

    private static T SyncOverAsync<T>(Func<Task<T>> work) =>
        AsyncHelper.RunSync(work);

    private static IEnumerable<T> DeserializeMany<T>(byte[] data) =>
        MessagePackSerializer.Deserialize<IEnumerable<T>>(data) ?? [];

    private static T? DeserializeOne<T>(byte[] data) =>
        MessagePackSerializer.Deserialize<T>(data);

    private static int DeserializeInt(byte[] data) =>
        MessagePackSerializer.Deserialize<int>(data);

    private static IEnumerable<IDictionary<string, object>> DeserializeRows(byte[] data) =>
        MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(data) ?? [];

    public virtual Task<DBResult> Remove(DBTransactional transactional, int? commandTimeout = null)
    {
        dBSessions.Sessions.TryGetValue(transactional.GetType(), out var connectionId);
        return InvokeRawAsync(async s => await s.Remove(transactional, connectionId ?? "", commandTimeout));
    }

    public virtual Task<DBResult> Save(DBTransactional transactional, int? commandTimeout = null)
    {
        dBSessions.Sessions.TryGetValue(transactional.GetType(), out var connectionId);
        return InvokeRawAsync(async s => await s.Save(transactional, connectionId ?? "", commandTimeout));
    }

    private async Task<DBResult> InvokeRawAsync(Func<IMagicOnionLayer, Task<DBResult>> call)
    {
        using var cts = new CancellationTokenSource(globalCommandTimeout);
        return await call(Rpc.WithCancellationToken(cts.Token)).ConfigureAwait(false);
    }

    public virtual T? Get<T>(long id, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
    {
        if (OperatingSystem.IsBrowser())
        {
            return BrowserSend(
                nameof(GetAsync),
                [typeof(T).Name, id, commandTimeout ?? int.MaxValue],
                DeserializeOne<T>,
                default);
        }

        return SyncOverAsync(() => GetAsync<T>(id, transaction, commandTimeout));
    }

    public virtual Task<T?> GetAsync<T>(long id, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => InvokeAsync(
            async s => await s.GetAsync(typeof(T).Name, id, commandTimeout),
            DeserializeOne<T>,
            default);

    public virtual IEnumerable<T> GetAll<T>(IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
    {
        if (OperatingSystem.IsBrowser())
        {
            return BrowserSend(
                nameof(GetAllAsync),
                [typeof(T).Name, commandTimeout ?? int.MaxValue],
                DeserializeMany<T>,
                []);
        }

        return SyncOverAsync(() => GetAllAsync<T>(transaction, commandTimeout));
    }

    public virtual Task<IEnumerable<T>> GetAllAsync<T>(IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => InvokeAsync(
            async s => await s.GetAllAsync(typeof(T).Name, commandTimeout),
            DeserializeMany<T>,
            Enumerable.Empty<T>());

    public virtual IEnumerable<T> Where<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
    {
        if (OperatingSystem.IsBrowser())
        {
            return BrowserSend(
                nameof(WhereAsync),
                [typeof(T).Name, filters, commandTimeout ?? int.MaxValue],
                DeserializeMany<T>,
                []);
        }

        return SyncOverAsync(() => WhereAsync<T>(filters, transaction, commandTimeout));
    }

    public virtual Task<IEnumerable<T>> WhereAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => InvokeAsync(
            async s => await s.WhereAsync(typeof(T).Name, filters, commandTimeout),
            DeserializeMany<T>,
            Enumerable.Empty<T>());

    public virtual IEnumerable<T> FindByAny<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
    {
        if (OperatingSystem.IsBrowser())
        {
            return BrowserSend(
                nameof(FindByAnyAsync),
                [typeof(T).Name, filters, commandTimeout ?? int.MaxValue],
                DeserializeMany<T>,
                []);
        }

        return SyncOverAsync(() => FindByAnyAsync<T>(filters, transaction, commandTimeout));
    }

    public virtual Task<IEnumerable<T>> FindByAnyAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        Dictionary<string, object> filters, IDbTransaction? transaction = null, int? commandTimeout = null)
        where T : class, new()
        => InvokeAsync(
            async s => await s.FindByAnyAsync(typeof(T).Name, filters, commandTimeout),
            DeserializeMany<T>,
            Enumerable.Empty<T>());

    public virtual int Execute(
        string sql, object? param = null, IDbTransaction? transaction = null,
        int? commandTimeout = null, CommandType? commandType = null)
    {
        if (OperatingSystem.IsBrowser())
        {
            return BrowserSend(
                nameof(ExecuteAsync),
                [sql, param ?? new object(), commandTimeout ?? int.MaxValue, commandType ?? CommandType.Text],
                DeserializeInt,
                0);
        }

        return SyncOverAsync(() => ExecuteAsync(sql, param, transaction, commandTimeout, commandType));
    }

    public virtual Task<int> ExecuteAsync(
        string sql, object? param = null, IDbTransaction? transaction = null,
        int? commandTimeout = null, CommandType? commandType = null)
        => InvokeAsync(
            async s => await s.ExecuteAsync(sql, param, commandTimeout, commandType ?? CommandType.Text),
            DeserializeInt,
            0);

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
        string sql, Func<TFirst, TSecond, TReturn> map, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id",
        int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new()
        => QueryCoreAsync(sql, param, commandTimeout, commandType,
            row => MapRow(row, map, splitOn, aliases));

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(
        string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id",
        int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new() where TThird : new()
        => QueryCoreAsync(sql, param, commandTimeout, commandType,
            row => MapRow(row, map, splitOn, aliases));

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
        => QueryCoreAsync(sql, param, commandTimeout, commandType,
            row => MapRow(row, map, splitOn, aliases));

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
    => QueryCoreAsync(sql, param, commandTimeout, commandType,
            row => MapRow(row, map, splitOn, aliases));

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
    => QueryCoreAsync(sql, param, commandTimeout, commandType,
            row => MapRow(row, map, splitOn, aliases));

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
    => QueryCoreAsync(sql, param, commandTimeout, commandType,
            row => MapRow(row, map, splitOn, aliases));

    public virtual IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(
        string sql, Func<TFirst, TSecond, TReturn> map, object? param = null,
        IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id",
        int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null)
        where TFirst : new() where TSecond : new()
        => QuerySync(sql, param, commandTimeout, commandType, row => MapRow(row, map, splitOn, aliases));

    public virtual IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new()
    => QuerySync(sql, param, commandTimeout, commandType, row => MapRow(row, map, splitOn, aliases));

    public virtual IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
    => QuerySync(sql, param, commandTimeout, commandType, row => MapRow(row, map, splitOn, aliases));

    public virtual IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() 
        => QuerySync(sql, param, commandTimeout, commandType, row => MapRow(row, map, splitOn, aliases));

    public virtual IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() 
        => QuerySync(sql, param, commandTimeout, commandType, row => MapRow(row, map, splitOn, aliases));

    public virtual IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new() 
        => QuerySync(sql, param, commandTimeout, commandType, row => MapRow(row, map, splitOn, aliases));

    private Task<IEnumerable<TReturn>> QueryCoreAsync<TReturn>(
        string sql, object? param, int? commandTimeout, CommandType? commandType,
        Func<IDictionary<string, object>, TReturn> mapRow)
        => InvokeAsync(
            async s => await s.QueryAsync(sql, param, commandTimeout, commandType ?? CommandType.Text),
            data => DeserializeRows(data).Select(mapRow).ToList(),
            Enumerable.Empty<TReturn>());

    private IEnumerable<TReturn> QuerySync<TReturn>(
        string sql, object? param, int? commandTimeout, CommandType? commandType,
        Func<IDictionary<string, object>, TReturn> mapRow)
    {
        if (OperatingSystem.IsBrowser())
        {
            return BrowserSend(
                nameof(QueryAsync),
                [sql, param ?? new object(), commandTimeout ?? int.MaxValue, commandType ?? CommandType.Text],
                data => DeserializeRows(data).Select(mapRow).ToList(),
                []);
        }

        return SyncOverAsync(() => QueryCoreAsync(sql, param, commandTimeout, commandType, mapRow));
    }


    private static List<Dictionary<string, object>> SplitRow(IDictionary<string, object> row, string splitOn, int groupCount, IEnumerable<DBAlias>? aliases)
    {
        var splitOns = splitOn.Split(',', StringSplitOptions.TrimEntries);
        var keys = row.Keys.ToList();
        var groups = new List<Dictionary<string, object>>();
        int groupStart = 0;

        for (int g = 0; g < groupCount; g++)
        {
            int groupEnd;

            if (g == groupCount - 1)
            {
                groupEnd = keys.Count;
            }
            else
            {
                // Use the matching splitOn if one was given per-group, else reuse the single value (Dapper convention)
                string splitKey = g < splitOns.Length ? splitOns[g] : splitOns[^1];
                groupEnd = keys.Count;

                for (int i = groupStart + 1; i < keys.Count; i++)
                {
                    if (string.Equals(keys[i], splitKey, StringComparison.OrdinalIgnoreCase))
                    {
                        groupEnd = i;
                        break;
                    }
                }
            }

            var dict = new Dictionary<string, object>();
            for (int i = groupStart; i < groupEnd; i++)
                dict[GetKey(keys[i], aliases)] = row[keys[i]];

            groups.Add(dict);
            groupStart = groupEnd;
        }

        return groups;

        static string GetKey(string key, IEnumerable<DBAlias>? aliases)
        {
            if (aliases is null) return key;
            var alias = aliases.SingleOrDefault(a => a.alias == key);
            return alias?.columnName ?? key;
        }
    }

    private static T MapToObject<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IDictionary<string, object> dict) where T : new()
    {
        var obj = new T();
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var prop in props)
        {
            var match = dict.Keys.FirstOrDefault(k => string.Equals(k, prop.Name, StringComparison.OrdinalIgnoreCase));
            if (match is null) continue;

            var value = dict[match];
            if (value is null)
            {
                prop.SetValue(obj, null);
                continue;
            }

            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            var converted = targetType.IsEnum
                ? Enum.ToObject(targetType, value)
                : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);

            prop.SetValue(obj, converted);
        }

        return obj;
    }

    private static TReturn MapRow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, TReturn>(IDictionary<string, object> row, Func<TFirst, TSecond, TReturn> map, string splitOn, IEnumerable<DBAlias>? aliases) where TFirst : new() where TSecond : new()
    {
        var groups = SplitRow(row, splitOn, 2, aliases);

        var first = MapToObject<TFirst>(groups[0]);
        var second = MapToObject<TSecond>(groups[1]);

        return map(first, second);
    }

    private static TReturn MapRow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, TReturn>(IDictionary<string, object> row, Func<TFirst, TSecond, TThird, TReturn> map, string splitOn, IEnumerable<DBAlias>? aliases) where TFirst : new() where TSecond : new() where TThird : new()
    {
        var groups = SplitRow(row, splitOn, 3, aliases);

        var first = MapToObject<TFirst>(groups[0]);
        var second = MapToObject<TSecond>(groups[1]);
        var third = MapToObject<TThird>(groups[2]);

        return map(first, second, third);
    }

    private static TReturn MapRow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, TReturn>(IDictionary<string, object> row, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, string splitOn, IEnumerable<DBAlias>? aliases) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
    {
        var groups = SplitRow(row, splitOn, 4, aliases);

        var first = MapToObject<TFirst>(groups[0]);
        var second = MapToObject<TSecond>(groups[1]);
        var third = MapToObject<TThird>(groups[2]);
        var fourth = MapToObject<TFourth>(groups[3]);

        return map(first, second, third, fourth);
    }

    private static TReturn MapRow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, TReturn>(IDictionary<string, object> row, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, string splitOn, IEnumerable<DBAlias>? aliases) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
    {
        var groups = SplitRow(row, splitOn, 5, aliases);

        var first = MapToObject<TFirst>(groups[0]);
        var second = MapToObject<TSecond>(groups[1]);
        var third = MapToObject<TThird>(groups[2]);
        var fourth = MapToObject<TFourth>(groups[3]);
        var fifth = MapToObject<TFifth>(groups[4]);

        return map(first, second, third, fourth, fifth);
    }

    private static TReturn MapRow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, TReturn>(IDictionary<string, object> row, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, string splitOn, IEnumerable<DBAlias>? aliases) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
    {
        var groups = SplitRow(row, splitOn, 6, aliases);

        var first = MapToObject<TFirst>(groups[0]);
        var second = MapToObject<TSecond>(groups[1]);
        var third = MapToObject<TThird>(groups[2]);
        var fourth = MapToObject<TFourth>(groups[3]);
        var fifth = MapToObject<TFifth>(groups[4]);
        var sixth = MapToObject<TSixth>(groups[5]);

        return map(first, second, third, fourth, fifth, sixth);
    }

    private static TReturn MapRow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSeventh, TReturn>(IDictionary<string, object> row, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, string splitOn, IEnumerable<DBAlias>? aliases) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
    {
        var groups = SplitRow(row, splitOn, 7, aliases);

        var first = MapToObject<TFirst>(groups[0]);
        var second = MapToObject<TSecond>(groups[1]);
        var third = MapToObject<TThird>(groups[2]);
        var fourth = MapToObject<TFourth>(groups[3]);
        var fifth = MapToObject<TFifth>(groups[4]);
        var sixth = MapToObject<TSixth>(groups[5]);
        var seventh = MapToObject<TSeventh>(groups[6]);

        return map(first, second, third, fourth, fifth, sixth, seventh);
    }
}