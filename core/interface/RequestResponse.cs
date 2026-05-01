namespace Rzeka;

public interface IRequest : IMatter { }

public interface IResponse<out T> : IMatter
    where T : IRequest
{
    T Request { get; }
    bool WasSuccessful { get; }
}

public abstract class Request : Matter, IRequest { }

public abstract class Response<T> : Matter, IResponse<T>
    where T : IRequest
{
    public T Request { get; }
    public bool WasSuccessful { get; }

    protected Response(T request, bool wasSuccessful)
    {
        Request = request;
        WasSuccessful = wasSuccessful;
    }
}
