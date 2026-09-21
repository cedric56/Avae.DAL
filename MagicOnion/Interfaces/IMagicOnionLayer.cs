using MagicOnion;
using System.Collections.Generic;
using System.Data;

namespace Avae.DAL;

public interface IMagicOnionLayer : IService<IMagicOnionLayer>
{
    UnaryResult<DBResult> Remove(DBTransactional transactional, string connectionId, int? commandTimeout = null);

    UnaryResult<DBResult> Save(DBTransactional transactional, string connectionId, int? commandTimeout = null);

    UnaryResult<DBResult> FindByAnyAsync(string type, Dictionary<string, object> filters, int? commandTimeout = null);

    UnaryResult<DBResult> GetAllAsync(string type, int? commandTimeout = null);

    UnaryResult<DBResult> GetAsync(string type, long id, int? commandTimeout = null);

    UnaryResult<DBResult> WhereAsync(string type, Dictionary<string, object> filters, int? commandTimeout = null);

    UnaryResult<DBResult> QueryAsync(string sql, object? param = null, int? commandTimeout = null, CommandType commandType = CommandType.Text);

    UnaryResult<DBResult> ExecuteAsync(string sql, object? param = null, int? commandTimeout = null, CommandType commandType = CommandType.Text);
}
