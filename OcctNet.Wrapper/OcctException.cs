namespace OcctNet.Wrapper;

public sealed class OcctException : Exception
{
    public OcctException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
