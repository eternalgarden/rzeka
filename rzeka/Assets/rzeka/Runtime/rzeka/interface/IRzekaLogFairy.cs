using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Rzeka
{
    public enum MessageType
    {
        Hint,
        Hunch,
        Horror
    }

    public class MessageOccurence : IOccurence
    {
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public MessageType MessageType { get; set; }
        public string Message { get; set; }
        [CanBeNull] public Exception Exception { get; set; }
    }
    
    [Serializable]
    public class SerializableException
    {
        public string message { get; set; }
        public string stackTrace { get; set; }
    }
    
    [Serializable]
    public class SerializableMessageOccurence
    {
        public OccurenceCategory occurenceCategory => OccurenceCategory.Message;
        public Guid guid { get; set; }
        public MessageType messageType { get; set; }
        public Guid[] circumstances { get; set; }
        public long timestamp { get; set; }
        public string message { get; set; }
        [CanBeNull] public SerializableException exception { get; set; }
    }
    
    public interface IRzekaLogFairy
    {
        void Speak(string message, params TMatter[] circumstances);
        void Speak(string message, MessageType messageType, params TMatter[] circumstances);
        void Speak(Exception exception, params TMatter[] circumstances);
    }

    public class LogFairy : IRzekaLogFairy
    {
        Eris Eris { get; }

        public LogFairy(Eris eris)
        {
            Eris = eris;
        }

        public void Speak(string message, params TMatter[] circumstances)
        {
            var msg = new MessageOccurence();
            msg.Guid = Guid.NewGuid();
            msg.MessageType = MessageType.Hint;
            msg.Message = message;
            msg.Circumstances = circumstances.Select(x => x.Guid).ToArray();
            msg.Timestamp = DateTimeOffset.Now;
            Eris.PublishMessage(msg);
            
#if UNITY_EDITOR
            Debug.Log($"<color=magenta>{message}</color>");
#endif
        }

        public void Speak(string message, MessageType messageType, params TMatter[] circumstances)
        {
            var msg = new MessageOccurence();
            msg.Guid = Guid.NewGuid();
            msg.MessageType = messageType;
            msg.Message = message;
            msg.Circumstances = circumstances.Select(x => x.Guid).ToArray();
            msg.Timestamp = DateTimeOffset.Now;
            Eris.PublishMessage(msg);
            
            
#if UNITY_EDITOR
            string color = messageType switch
            {
                MessageType.Hint => "cyan",
                MessageType.Hunch => "yellow",
                MessageType.Horror => "orange",
                _ => ""
            };
            Debug.Log($"<color={color}>{message}</color>");
#endif
        }

        public void Speak(Exception exception, params TMatter[] circumstances)
        {
            var msg = new MessageOccurence();
            msg.Guid = Guid.NewGuid();
            msg.MessageType = MessageType.Horror;
            msg.Message = "🔥 errer: ";
            msg.Circumstances = circumstances.Select(x => x.Guid).ToArray();
            msg.Exception = exception;
            msg.Timestamp = DateTimeOffset.Now;
            Eris.PublishMessage(msg);

#if UNITY_EDITOR
            Debug.LogError(exception);
#endif
            
        }
    }
}