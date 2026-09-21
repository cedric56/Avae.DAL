using System.Threading;

namespace Avae.DAL;

public static class DBContext
{
    public static readonly AsyncLocal<string?> CurrentConnectionId = new();
}
