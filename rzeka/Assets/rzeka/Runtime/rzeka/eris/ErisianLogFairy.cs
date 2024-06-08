using System;
using System.Linq;
using UnityEngine;

namespace Rzeka
{
    public class ErisianLogFairy : IRzekaLogFairy
    {
        Eris Eris { get; }

        public ErisianLogFairy(Eris eris)
        {
            Eris = eris;
        }

        public void Speak(string message, params TMatter[] circumstances)
        {
            var msg = new MessageOccurence();
            msg.Guid = Guid.NewGuid();
            msg.RzekaMessageType = RzekaMessageType.Hint;
            msg.Message = message;
            msg.Circumstances = circumstances.Select(x => x.Guid).ToArray();
            msg.Timestamp = DateTimeOffset.Now;
            Eris.PublishMessage(msg);
            
#if UNITY_EDITOR
            Debug.Log($"<color=magenta>{message}</color>");
#endif
        }

        public void Speak(string message, RzekaMessageType rzekaMessageType, params TMatter[] circumstances)
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
                RzekaMessageType.Hint => "cyan",
                RzekaMessageType.Hunch => "yellow",
                RzekaMessageType.Horror => "orange",
                _ => ""
            };
            Debug.Log($"<color={color}>{message}</color>");
#endif
        }

        public void Speak(Exception exception, params TMatter[] circumstances)
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

        public void Speak(string message, Exception exception, params TMatter[] circumstances)
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
}