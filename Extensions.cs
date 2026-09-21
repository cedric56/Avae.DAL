using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Avae.DAL;

public static class Extensions
{
    public static void UseFactory<TDBConnection>(this IServiceCollection services,
        string connectionString)
        where TDBConnection : DbConnection, new()
    {
        var factory = new DBFactory<TDBConnection>(connectionString);
        services.AddSingleton<IDBFactory>(sp => factory);
        services.AddTransient<IDbConnection>(_ => factory.CreateConnection()!);
    }

    public static void UseLayer(this IServiceCollection services,
        Func<IServiceProvider, IDBLayer> getLayer,
        Func<string>? getDBCreateCommand = null)
    {
        services.AddSingleton<IDBLayer>(sp =>
        {
            if (getDBCreateCommand != null)
                CreateDB(sp);

            return getLayer(sp);
        });

        void CreateDB(IServiceProvider provider)
        {
            using var connection = provider.GetService<IDbConnection>();
            if (connection is not null)
            {
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = getDBCreateCommand?.Invoke();
                cmd.ExecuteNonQuery();
            }
        }
    }

    internal static string ReplaceWholeWord(this string s, string word, string bywhat)
    {
        char firstLetter = word[0];
        var sb = new StringBuilder();
        bool previousWasLetterOrDigit = false;
        int i = 0;
        while (i < s.Length - word.Length + 1)
        {
            bool wordFound = false;
            char c = s[i];
            if (c == firstLetter)
                if (!previousWasLetterOrDigit)
                    if (s.Substring(i, word.Length).Equals(word))
                    {
                        wordFound = true;
                        bool wholeWordFound = true;
                        if (s.Length > i + word.Length)
                        {
                            if (char.IsLetterOrDigit(s[i + word.Length]))
                                wholeWordFound = false;
                        }

                        if (wholeWordFound)
                            sb.Append(bywhat);
                        else
                            sb.Append(word);

                        i += word.Length;
                    }

            if (!wordFound)
            {
                previousWasLetterOrDigit = char.IsLetterOrDigit(c);
                sb.Append(c);
                i++;
            }
        }

        if (s.Length - i > 0)
            sb.Append(s.AsSpan(i));

        return sb.ToString();
    }
}
