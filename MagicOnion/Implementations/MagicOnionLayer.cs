using Dapper;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Avae.DAL;

public partial class MagicOnionLayer(IServiceProvider provider, string url, int globalCommandTimeout) : IDBLayer
{
    public virtual async Task<DBResult> Remove(DBTransactional transactional, int? commandTimeout = null)
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        IDBLayer.Sessions.TryGetValue(transactional.GetType(), out var connectionId);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        return await service
            .WithCancellationToken(tcs.Token)
            .Remove(transactional, connectionId ?? string.Empty, commandTimeout)
            .ConfigureAwait(false);
    }

    public virtual async Task<DBResult> Save(DBTransactional transactional, int? commandTimeout = null)
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        IDBLayer.Sessions.TryGetValue(transactional.GetType(), out var connectionId);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        return await service
            .WithCancellationToken(tcs.Token)
            .Save(transactional, connectionId ?? string.Empty, commandTimeout)
            .ConfigureAwait(false);
    }

    public virtual IEnumerable<T> FindByAny<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, int? commandTimeout = null) where T : class, new()
    {
        if (OperatingSystem.IsBrowser())
        {
            var request = provider.GetRequiredService<IXmlHttpRequest>();
            var result = request.Send(url, nameof(FindByAnyAsync), MessagePackSerializer.Serialize(new object[] { typeof(T).Name, filters, commandTimeout ?? int.MaxValue }), globalCommandTimeout);
            if (result == Array.Empty<byte>()) return [];
            return MessagePackSerializer.Deserialize<IEnumerable<T>>(result) ?? [];
        }
        return AsyncHelper.RunSync(() => FindByAnyAsync<T>(filters, commandTimeout));
    }

    public virtual async Task<IEnumerable<T>> FindByAnyAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, int? commandTimeout = null) where T : class, new()
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        var result = await service
            .WithCancellationToken(tcs.Token)
            .FindByAnyAsync(typeof(T).Name, filters, commandTimeout)
            .ConfigureAwait(false);
        if (!result.Successful) throw new Exception(result.Exception);
        if (result.Data == Array.Empty<byte>()) return [];
        return MessagePackSerializer.Deserialize<IEnumerable<T>>(result.Data);
    }

    public virtual T? Get<T>(long id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        if (OperatingSystem.IsBrowser())
        {
            var request = provider.GetRequiredService<IXmlHttpRequest>();
            var result = request.Send(url, nameof(GetAsync), MessagePackSerializer.Serialize(new object[] { typeof(T).Name, id, commandTimeout ?? int.MaxValue }), globalCommandTimeout);
            if (result == Array.Empty<byte>()) return null;
            return MessagePackSerializer.Deserialize<T>(result);
        }
        return AsyncHelper.RunSync(() => GetAsync<T>(id, transaction, commandTimeout));
    }

    public virtual IEnumerable<T> GetAll<T>(IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        if (OperatingSystem.IsBrowser())
        {
            var request = provider.GetRequiredService<IXmlHttpRequest>();
            var result = request.Send(url, nameof(GetAllAsync), MessagePackSerializer.Serialize(new object[] { typeof(T).Name, commandTimeout ?? int.MaxValue }), globalCommandTimeout);
            if (result == Array.Empty<byte>()) return [];
            return MessagePackSerializer.Deserialize<IEnumerable<T>>(result) ?? [];
        }
        return AsyncHelper.RunSync(() => GetAllAsync<T>(transaction, commandTimeout));
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync<T>(IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        var result = await service
            .WithCancellationToken(tcs.Token)
            .GetAllAsync(typeof(T).Name, commandTimeout)
            .ConfigureAwait(false);
        if (!result.Successful) throw new Exception(result.Exception);
        if (result.Data == Array.Empty<byte>()) return [];
        return MessagePackSerializer.Deserialize<IEnumerable<T>>(result.Data) ?? [];
    }

    public virtual async Task<T?> GetAsync<T>(long id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new()
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        var result = await service
            .WithCancellationToken(tcs.Token)
            .GetAsync(typeof(T).Name, id, commandTimeout)
            .ConfigureAwait(false);
        if (!result.Successful) throw new Exception(result.Exception);
        if (result.Data == Array.Empty<byte>()) return null;
        return MessagePackSerializer.Deserialize<T>(result.Data);
    }

    public virtual IEnumerable<T> Where<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, int? commandTimeout = null) where T : class, new()
    {
        if (OperatingSystem.IsBrowser())
        {
            var request = provider.GetRequiredService<IXmlHttpRequest>();
            var result = request.Send(url, nameof(WhereAsync), MessagePackSerializer.Serialize(new object[] { typeof(T).Name, filters, commandTimeout ?? int.MaxValue }), globalCommandTimeout);
            if (result == Array.Empty<byte>()) return [];
            return MessagePackSerializer.Deserialize<IEnumerable<T>>(result) ?? [];
        }
        return AsyncHelper.RunSync(() => WhereAsync<T>(filters, commandTimeout));
    }

    public virtual async Task<IEnumerable<T>> WhereAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Dictionary<string, object> filters, int? commandTimeout = null) where T : class, new()
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        var result = await service
            .WithCancellationToken(tcs.Token)
            .WhereAsync(typeof(T).Name, filters, commandTimeout)
            .ConfigureAwait(false);
        if (!result.Successful) throw new Exception(result.Exception);
        if (result.Data == Array.Empty<byte>()) return [];
        return MessagePackSerializer.Deserialize<IEnumerable<T>>(result.Data) ?? [];
    }

    public virtual int Execute(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        if (OperatingSystem.IsBrowser())
        {
            var request = provider.GetRequiredService<IXmlHttpRequest>();
            var result = request.Send(url, nameof(ExecuteAsync), MessagePackSerializer.Serialize(new object[] { sql, param ?? new object(), commandTimeout ?? int.MaxValue, commandType ?? CommandType.Text }), globalCommandTimeout);
            if (result == Array.Empty<byte>()) return 0;
            return MessagePackSerializer.Deserialize<int>(result);
        }
        return AsyncHelper.RunSync(() => ExecuteAsync(sql, param, transaction, commandTimeout, commandType));
    }

    public virtual async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        var result = await service
            .WithCancellationToken(tcs.Token)
            .ExecuteAsync(sql, param, commandTimeout, commandType ?? CommandType.Text)
            .ConfigureAwait(false);
        if (!result.Successful) throw new Exception(result.Exception);
        if (result.Data == Array.Empty<byte>()) return 0;
        return MessagePackSerializer.Deserialize<int>(result.Data);
    }

    public virtual IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new()
    {
        if (OperatingSystem.IsBrowser())
        {
            var request = provider.GetRequiredService<IXmlHttpRequest>();
            var result = request.Send(url, nameof(QueryAsync), MessagePackSerializer.Serialize(new object[] { sql, param ?? new object(), commandTimeout ?? int.MaxValue, commandType ?? CommandType.Text }), globalCommandTimeout);
            if (result == Array.Empty<byte>()) return [];
            var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result) ?? [];
            return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
        }
        return AsyncHelper.RunSync(() => QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases));
    }

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new()
    {
        return QueryAsync(command.CommandText, map, command.Parameters, command.Transaction, command.Buffered, splitOn, command.CommandTimeout, command.CommandType, aliases);
    }

    public virtual IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new()
    {
        if (OperatingSystem.IsBrowser())
        {
            var request = provider.GetRequiredService<IXmlHttpRequest>();
            var result = request.Send(url, nameof(QueryAsync), MessagePackSerializer.Serialize(new object[] { sql, param ?? new object(), commandTimeout ?? int.MaxValue, commandType ?? CommandType.Text }), globalCommandTimeout);
            if (result == Array.Empty<byte>()) return [];
            var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result) ?? [];
            return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
        }
        return AsyncHelper.RunSync(() => QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases));
    }

    public virtual IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
    {
        if (OperatingSystem.IsBrowser())
        {
            var request = provider.GetRequiredService<IXmlHttpRequest>();
            var result = request.Send(url, nameof(QueryAsync), MessagePackSerializer.Serialize(new object[] { sql, param ?? new object(), commandTimeout ?? int.MaxValue, commandType ?? CommandType.Text }), globalCommandTimeout);
            if (result == Array.Empty<byte>()) return [];
            var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result) ?? [];
            return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
        }
        return AsyncHelper.RunSync(() => QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases));
    }

    public virtual IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
    {
        if (OperatingSystem.IsBrowser())
        {
            var request = provider.GetRequiredService<IXmlHttpRequest>();
            var result = request.Send(url, nameof(QueryAsync), MessagePackSerializer.Serialize(new object[] { sql, param ?? new object(), commandTimeout ?? int.MaxValue, commandType ?? CommandType.Text }), globalCommandTimeout);
            if (result == Array.Empty<byte>()) return [];
            var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result) ?? [];
            return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
        }
        return AsyncHelper.RunSync(() => QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases));
    }

    public virtual IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
    {
        if (OperatingSystem.IsBrowser())
        {
            var request = provider.GetRequiredService<IXmlHttpRequest>();
            var result = request.Send(url, nameof(QueryAsync), MessagePackSerializer.Serialize(new object[] { sql, param ?? new object(), commandTimeout ?? int.MaxValue, commandType ?? CommandType.Text }), globalCommandTimeout);
            if (result == Array.Empty<byte>()) return [];
            var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result) ?? [];
            return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
        }
        return AsyncHelper.RunSync(() => QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases));
    }

    public virtual IEnumerable<TReturn> Query<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFirst, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSecond, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TThird, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFourth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFifth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSixth, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
    {
        if (OperatingSystem.IsBrowser())
        {
            var request = provider.GetRequiredService<IXmlHttpRequest>();
            var result = request.Send(url, nameof(QueryAsync), MessagePackSerializer.Serialize(new object[] { sql, param ?? new object(), commandTimeout ?? int.MaxValue, commandType ?? CommandType.Text }), globalCommandTimeout);
            if (result == Array.Empty<byte>()) return [];
            var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result) ?? [];
            return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
        }
        return AsyncHelper.RunSync(() => QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType, aliases));
    }

    public virtual async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new()
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        var result = await service
            .WithCancellationToken(tcs.Token)
            .QueryAsync(sql, param, commandTimeout, commandType ?? CommandType.Text)
            .ConfigureAwait(false);
        if (!result.Successful) throw new Exception(result.Exception);
        if (result.Data == Array.Empty<byte>()) return [];
        var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result.Data) ?? [];
        return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
    }

    public virtual async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new()
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        var result = await service
            .WithCancellationToken(tcs.Token)
            .QueryAsync(sql, param, commandTimeout, commandType ?? CommandType.Text)
            .ConfigureAwait(false);
        if (!result.Successful) throw new Exception(result.Exception);
        if (result.Data == Array.Empty<byte>()) return [];
        var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result.Data) ?? [];
        return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
    }

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new()
    {
        return QueryAsync(command.CommandText, map, command.Parameters, command.Transaction, command.Buffered, splitOn, command.CommandTimeout, command.CommandType, aliases);
    }

    public virtual async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        var result = await service
            .WithCancellationToken(tcs.Token)
            .QueryAsync(sql, param, commandTimeout, commandType ?? CommandType.Text)
            .ConfigureAwait(false);
        if (!result.Successful) throw new Exception(result.Exception);
        if (result.Data == Array.Empty<byte>()) return [];
        var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result.Data) ?? [];
        return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
    }

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new()
    {
        return QueryAsync(command.CommandText, map, command.Parameters, command.Transaction, command.Buffered, splitOn, command.CommandTimeout, command.CommandType, aliases);
    }

    public virtual async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        var result = await service
            .WithCancellationToken(tcs.Token)
            .QueryAsync(sql, param, commandTimeout, commandType ?? CommandType.Text)
            .ConfigureAwait(false);
        if (!result.Successful) throw new Exception(result.Exception);
        if (result.Data == Array.Empty<byte>()) return [];
        var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result.Data) ?? [];
        return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
    }

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new()
    {
        return QueryAsync(command.CommandText, map, command.Parameters, command.Transaction, command.Buffered, splitOn, command.CommandTimeout, command.CommandType, aliases);
    }

    public virtual async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        var result = await service
            .WithCancellationToken(tcs.Token)
            .QueryAsync(sql, param, commandTimeout, commandType ?? CommandType.Text)
            .ConfigureAwait(false);
        if (!result.Successful) throw new Exception(result.Exception);
        if (result.Data == Array.Empty<byte>()) return [];
        var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result.Data) ?? [];
        return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
    }

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new()
    {
        return QueryAsync(command.CommandText, map, command.Parameters, command.Transaction, command.Buffered, splitOn, command.CommandTimeout, command.CommandType, aliases);
    }

    public virtual async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object? param = null, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
    {
        using var tcs = new CancellationTokenSource(globalCommandTimeout);
        var service = provider.GetRequiredService<IMagicOnionLayer>();
        var result = await service
            .WithCancellationToken(tcs.Token)
            .QueryAsync(sql, param, commandTimeout, commandType ?? CommandType.Text)
            .ConfigureAwait(false);
        if (!result.Successful) throw new Exception(result.Exception);
        if (result.Data == Array.Empty<byte>()) return [];
        var rows = MessagePackSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(result.Data) ?? [];
        return rows.Select(row => MapRow(row, map, splitOn, aliases)).ToList();
    }

    public virtual Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, string splitOn = "Id", IEnumerable<DBAlias>? aliases = null) where TFirst : new() where TSecond : new() where TThird : new() where TFourth : new() where TFifth : new() where TSixth : new() where TSeventh : new()
    {
        return QueryAsync(command.CommandText, map, command.Parameters, command.Transaction, command.Buffered, splitOn, command.CommandTimeout, command.CommandType, aliases);
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
