using Dapper;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Avae.DAL;

/// <summary>
/// Base MagicOnion service exposing generic entity CRUD/query operations, transactional save/remove,
/// and raw SQL query/execute operations over the configured data access layer.
/// </summary>
public abstract class MagicOnionService(IDBLayer layer, IDBFactory factory) : ServiceBase<IMagicOnionLayer>, IMagicOnionLayer
{
    /// <summary>
    /// Resolves the <see cref="EntityHandler"/> registered for <paramref name="type"/> and invokes
    /// <paramref name="serialize"/> against it, wrapping the result (or any thrown exception) in a <see cref="DBResult"/>.
    /// </summary>
    /// <param name="type">The registered entity type name to resolve a handler for.</param>
    /// <param name="serialize">
    /// A callback that performs the actual entity operation and serializes its result to bytes,
    /// given the resolved handler and its serializer options.
    /// </param>
    /// <returns>
    /// A <see cref="DBResult"/> indicating success with the serialized data, or failure with an error
    /// message if <paramref name="type"/> is missing/unrecognized or the operation throws.
    /// </returns>
    private async UnaryResult<DBResult> Request(string type, Func<EntityHandler, DBTransactionalSerializerOptions?, Task<byte[]>> serialize)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return new DBResult()
            {
                Successful = false,
                Exception = "Type parameter is required"
            };
        }
        else if (!EntityHandler.Handlers.TryGetValue(type, out var handler))
        {
            return new DBResult()
            {
                Successful = false,
                Exception = "Unable to find entity handler"
            };
        }
        else
        {
            try
            {
                return new DBResult()
                {
                    Successful = true,
                    Data = await serialize(handler, GetOptions(type))
                };
            }
            catch (Exception ex)
            {
                return new DBResult()
                {
                    Successful = false,
                    Exception = ex.Message
                };
            }
        }
    }

    /// <summary>
    /// Finds entities of the specified type matching any of the supplied filters.
    /// </summary>
    /// <param name="type">The registered entity type name to query.</param>
    /// <param name="filters">Field/value filters; entities matching any one filter are included.</param>
    /// <param name="commandTimeout">Optional command timeout, in seconds.</param>
    /// <returns>A <see cref="DBResult"/> containing the serialized matching entities, or failure details.</returns>
    public UnaryResult<DBResult> FindByAnyAsync(string type, Dictionary<string, object> filters, int? commandTimeout = null)
    {
        return Request(type, async (entity, options) => MessagePackSerializer.Serialize(entity.Enumerable, await entity.FindByAnyAsync(filters, commandTimeout), options));
    }

    /// <summary>
    /// Retrieves all entities of the specified type.
    /// </summary>
    /// <param name="type">The registered entity type name to query.</param>
    /// <param name="commandTimeout">Optional command timeout, in seconds.</param>
    /// <returns>A <see cref="DBResult"/> containing the serialized entities, or failure details.</returns>
    public UnaryResult<DBResult> GetAllAsync(string type, int? commandTimeout = null)
    {
        return Request(type, async (entity, options) => MessagePackSerializer.Serialize(entity.Enumerable, await entity.GetAllAsync(commandTimeout), options));
    }

    /// <summary>
    /// Retrieves the entity of the specified type with the given identifier.
    /// </summary>
    /// <param name="type">The registered entity type name to query.</param>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="commandTimeout">Optional command timeout, in seconds.</param>
    /// <returns>A <see cref="DBResult"/> containing the serialized entity, or failure details.</returns>
    public UnaryResult<DBResult> GetAsync(string type, long id, int? commandTimeout = null)
    {
        return Request(type, async (entity, options) => MessagePackSerializer.Serialize(entity.Type, await entity.GetAsync(id, commandTimeout), options));
    }

    /// <summary>
    /// Finds entities of the specified type matching all of the supplied filters.
    /// </summary>
    /// <param name="type">The registered entity type name to query.</param>
    /// <param name="filters">Field/value filters; only entities matching every filter are included.</param>
    /// <param name="commandTimeout">Optional command timeout, in seconds.</param>
    /// <returns>A <see cref="DBResult"/> containing the serialized matching entities, or failure details.</returns>
    public UnaryResult<DBResult> WhereAsync(string type, Dictionary<string, object> filters, int? commandTimeout = null)
    {
        return Request(type, async (entity, options) => MessagePackSerializer.Serialize(entity.Enumerable, await entity.WhereAsync(filters, commandTimeout), options));
    }

    /// <summary>
    /// When overridden, supplies the MessagePack serializer options to use when serializing results
    /// for the specified entity type. The base implementation returns <see langword="null"/> (default options).
    /// </summary>
    /// <param name="type">The registered entity type name being serialized.</param>
    /// <returns>The serializer options to use, or <see langword="null"/> to use the default.</returns>
    protected virtual DBTransactionalSerializerOptions? GetOptions(string type)
    {
        return null;
    }

    /// <summary>
    /// Removes the entities described by the supplied transactional payload, scoping the operation
    /// to the specified connection.
    /// </summary>
    /// <param name="transactional">The transactional payload describing what to remove.</param>
    /// <param name="connectionId">The connection identifier to scope the operation to for the duration of the call.</param>
    /// <param name="commandTimeout">Optional command timeout, in seconds.</param>
    /// <returns>A <see cref="DBResult"/> indicating the outcome of the removal.</returns>
    public async UnaryResult<DBResult> Remove(DBTransactional transactional, string connectionId, int? commandTimeout = null)
    {
        DBContext.CurrentConnectionId.Value = connectionId;
        try
        {
            return await transactional.Remove(layer, factory, commandTimeout);
        }
        finally
        {
            DBContext.CurrentConnectionId.Value = null;
        }
    }

    /// <summary>
    /// Saves the entities described by the supplied transactional payload, scoping the operation
    /// to the specified connection.
    /// </summary>
    /// <param name="transactional">The transactional payload describing what to save.</param>
    /// <param name="connectionId">The connection identifier to scope the operation to for the duration of the call.</param>
    /// <param name="commandTimeout">Optional command timeout, in seconds.</param>
    /// <returns>A <see cref="DBResult"/> indicating the outcome of the save.</returns>
    public async UnaryResult<DBResult> Save(DBTransactional transactional, string connectionId, int? commandTimeout = null)
    {
        DBContext.CurrentConnectionId.Value = connectionId;
        try
        {
            return await transactional.Save(layer, factory, commandTimeout);
        }
        finally
        {
            DBContext.CurrentConnectionId.Value = null;
        }
    }

    /// <summary>
    /// Executes a raw SQL query and returns the resulting rows.
    /// </summary>
    /// <param name="sql">The SQL query text.</param>
    /// <param name="param">Optional query parameters; an <see cref="IEnumerable"/> of key/value pairs is converted to <see cref="DynamicParameters"/>.</param>
    /// <param name="commandTimeout">Optional command timeout, in seconds.</param>
    /// <param name="commandType">The type of command <paramref name="sql"/> represents. Defaults to <see cref="CommandType.Text"/>.</param>
    /// <returns>A <see cref="DBResult"/> containing the serialized result rows, or failure details if the query throws.</returns>
    public async UnaryResult<DBResult> QueryAsync(string sql, object? param = null, int? commandTimeout = null, CommandType commandType = CommandType.Text)
    {
        try
        {
            using var db = factory.CreateConnection();
            var results = await db.QueryAsync(sql, GetParam(param), commandTimeout: commandTimeout, commandType: commandType);
            return new DBResult()
            {
                Successful = true,
                Data = MessagePackSerializer.Serialize(results.Select(row => (IDictionary<string, object>)row))
            };
        }
        catch (Exception ex)
        {
            return new DBResult()
            {
                Successful = false,
                Exception = ex.Message
            };
        }
    }

    /// <summary>
    /// Executes a raw SQL command and returns the number of affected rows.
    /// </summary>
    /// <param name="sql">The SQL command text.</param>
    /// <param name="param">Optional command parameters; an <see cref="IEnumerable"/> of key/value pairs is converted to <see cref="DynamicParameters"/>.</param>
    /// <param name="commandTimeout">Optional command timeout, in seconds.</param>
    /// <param name="commandType">The type of command <paramref name="sql"/> represents. Defaults to <see cref="CommandType.Text"/>.</param>
    /// <returns>A <see cref="DBResult"/> containing the serialized affected-row count, or failure details if the command throws.</returns>
    public async UnaryResult<DBResult> ExecuteAsync(string sql, object? param = null, int? commandTimeout = null, CommandType commandType = CommandType.Text)
    {
        try
        {
            using var db = factory.CreateConnection();
            var results = await db.ExecuteAsync(sql, GetParam(param), commandTimeout: commandTimeout, commandType: commandType);
            return new DBResult()
            {
                Successful = true,
                Data = MessagePackSerializer.Serialize(results)
            };
        }
        catch (Exception ex)
        {
            return new DBResult()
            {
                Successful = false,
                Exception = ex.Message
            };
        }
    }

    /// <summary>
    /// Converts an enumerable of key/value-shaped objects (e.g. anonymous types or tuples with
    /// "Key"/"Value" properties) into Dapper <see cref="DynamicParameters"/>. Non-enumerable values
    /// are passed through unchanged.
    /// </summary>
    /// <param name="param">The parameter object to convert.</param>
    /// <returns>
    /// A <see cref="DynamicParameters"/> instance built from <paramref name="param"/> if it is an
    /// <see cref="IEnumerable"/> of key/value-shaped items; otherwise <paramref name="param"/> unchanged.
    /// </returns>
    private static object? GetParam(object? param)
    {
        if (param is IEnumerable ie)
        {
            var dp = new DynamicParameters();
            foreach (var item in ie)
            {
                var type = item.GetType();
                var keyProp = type.GetProperty("Key");
                var valueProp = type.GetProperty("Value");

                if (keyProp != null && valueProp != null)
                {
                    var key = keyProp.GetValue(item)?.ToString();
                    var value = valueProp.GetValue(item);
                    if (key != null)
                        dp.Add(key, value);
                }
            }
            return dp;
        }

        return param;
    }
}