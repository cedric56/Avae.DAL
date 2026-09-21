namespace Avae.DAL;

public interface IXmlHttpRequest
{
    byte[] Send(string url, string parameters, byte[] data, int timeout);
}
