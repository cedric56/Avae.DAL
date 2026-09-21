namespace Avae.DAL;

public interface IDBIdentity
{
    string Parse(string commandText)
    {
        return commandText;
    }
}
