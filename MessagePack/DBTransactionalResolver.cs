using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Avae.DAL;

//[GeneratedMessagePackResolver]
public sealed class DBTransactionalResolver : IFormatterResolver
{
    List<IMessagePackFormatter> _formatters = [new DBTransactionalFormatter()];

    public void Register<T>(IDBTransactionalFormatter formatter) where T : DBTransactional?
    {
        _formatters.Add(formatter);
        DBTransactionalFormatter.Register<T>(formatter);
    }

    private static DBTransactionalResolver GetInstance()
    {
        var resolver = new DBTransactionalResolver();
        MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver.Create(
            resolver
        ));
        return resolver;
    }

    public static DBTransactionalResolver Instance = GetInstance();

    private DBTransactionalResolver()
    {

    }

    public IMessagePackFormatter? GetFormatter(Type? type)
    {
        if (type == null)
            return null;

        return _formatters
        .OfType<IMessagePackFormatterFor>()
        .FirstOrDefault(f => f.TargetType == type);
    }

    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        var formatter = _formatters.OfType<IMessagePackFormatter<T>>().FirstOrDefault();
        return formatter ?? BuiltinResolver.Instance.GetFormatter<T>() ?? StandardResolver.Instance.GetFormatter<T>() ?? throw new NotImplementedException();
    }
}

public class DBTransactionalSerializerOptions(DBTransactionalResolver resolver) : MessagePackSerializerOptions(resolver)
{

}
