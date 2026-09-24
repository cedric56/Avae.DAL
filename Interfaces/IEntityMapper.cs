using Dommel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Avae.DAL;

public interface IEntityMapper
{
    IDbConnection CreateConnection();

    IEnumerable<TEntity> Select<TEntity>(IDbConnection connection,
         Expression<Func<TEntity, bool>> predicate, IDbTransaction? transaction = null, CancellationToken cancellationToken = default) where TEntity : class;
    Task<IEnumerable<TEntity>> SelectAsync<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    TEntity? FirstOrDefault<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, IDbTransaction? transaction = null)
        where TEntity : class;

    Task<TEntity?> FirstOrDefaultAsync<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where TEntity : class;

    IEnumerable<TEntity> SelectPaged<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, IDbTransaction? transaction = null, bool buffered = true);

    Task<IEnumerable<TEntity>> SelectPagedAsync<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}

public interface IDbTransaction<T> where T : class
{
    Task<DBResult> SaveAsync(T entity, CancellationToken ct = default);
    Task<DBResult> RemoveAsync(T entity, CancellationToken ct = default);
}

public sealed class DommelEntityMapper : IEntityMapper
{
    IDBFactory factory;

    public DommelEntityMapper(IDBFactory factory, Func<string>? getDBCreateCommand = null)
    {
        this.factory = factory;
        if (getDBCreateCommand != null)
        {
            string text = getDBCreateCommand.Invoke();
            if (!string.IsNullOrWhiteSpace(text))
            {
                using var dbConnection = CreateConnection();
                using var dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = text;
                if (dbConnection.State != ConnectionState.Open)
                {
                    dbConnection.Open();
                }

                dbCommand.ExecuteNonQuery();
            }
        }
    }

    public IDbConnection CreateConnection()
    {
        return factory.CreateConnection()!;
    }

    public Task<IEnumerable<TEntity>> SelectAsync<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        return connection.SelectAsync(predicate, transaction, cancellationToken);
    }

    public TEntity? FirstOrDefault<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, IDbTransaction? transaction = null) where TEntity : class
    {
        return connection.FirstOrDefault(predicate, transaction);
    }

    public Task<TEntity?> FirstOrDefaultAsync<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, IDbTransaction? transaction = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        return connection.FirstOrDefaultAsync(predicate, transaction, cancellationToken);
    }

    public IEnumerable<TEntity> SelectPaged<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, IDbTransaction? transaction = null, bool buffered = true)
    {
        return connection.SelectPaged(predicate, pageNumber, pageSize, transaction, buffered);
    }

    public Task<IEnumerable<TEntity>> SelectPagedAsync<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        return connection.SelectPagedAsync(predicate, pageNumber, pageSize, transaction, cancellationToken);
    }

    public IEnumerable<TEntity> Select<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, IDbTransaction? transaction = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        return connection.Select(predicate, transaction);
    }
}