using System;
using System.Linq;

namespace Rzeka;
public class Whispers : IWhisper
{
    Eris Eris { get; }

    public Whispers(Eris eris)
    {
        Eris = eris;
    }

    public void Whisper(string message, params IMatter[] circumstances)
    {
        Whisper(message, RzekaMessageType.Hint, circumstances);
    }

    public void Whisper(
        string message,
        RzekaMessageType rzekaMessageType,
        params IMatter[] circumstances
    )
    {
        var msg = new MessageOccurence();
        msg.Guid = Guid.NewGuid();
        msg.RzekaMessageType = rzekaMessageType;
        msg.Message = message;
        msg.Circumstances = circumstances.Select(x => x.Guid).ToArray();
        msg.Timestamp = DateTimeOffset.Now;
        Eris.PublishMessage(msg);

#if UNITY_EDITOR
        string color = rzekaMessageType switch
        {
            RzekaMessageType.Hint => "#FFB6C1",
            RzekaMessageType.Hunch => "yellow",
            RzekaMessageType.Horror => "orange",
            _ => "",
        };
        Debug.Log($"<color={color}>Rzeka: {rzekaMessageType.ToString()}: {message}</color>");
#endif
    }

    public void Whisper(Exception exception, params IMatter[] circumstances)
    {
        var msg = new MessageOccurence();
        msg.Guid = Guid.NewGuid();
        msg.RzekaMessageType = RzekaMessageType.Horror;
        msg.Message = "🔥 errer 🏹";
        msg.Circumstances = circumstances.Select(x => x.Guid).ToArray();
        msg.Exception = exception;
        msg.Timestamp = DateTimeOffset.Now;
        Eris.PublishMessage(msg);

#if UNITY_EDITOR
        Debug.Log($"<color=yellow>Rzeka exception:</color>");
        Debug.LogError(exception);
#endif
    }

    public void Whisper(string message, Exception exception, params IMatter[] circumstances)
    {
        var msg = new MessageOccurence();
        msg.Guid = Guid.NewGuid();
        msg.RzekaMessageType = RzekaMessageType.Horror;
        msg.Message = message;
        msg.Circumstances = circumstances.Select(x => x.Guid).ToArray();
        msg.Exception = exception;
        msg.Timestamp = DateTimeOffset.Now;
        Eris.PublishMessage(msg);

#if UNITY_EDITOR
        Debug.Log($"<color=yellow>Rzeka exception:</color>");
        Debug.LogError(exception);
#endif
    }
}
