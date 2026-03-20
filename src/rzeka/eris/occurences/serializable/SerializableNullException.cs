using Rzeka;

public class SerializableNullException : SerializableException
{
    public SerializableNullException()
    {
        message = null;
        stackTrace = null;
        comments = null;
    }
}
