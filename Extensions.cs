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
