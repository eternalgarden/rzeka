using System;

namespace Rzeka
{
    public interface IWhisper
    {
        void Speak(string message, params TMatter[] circumstances);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">Note you are not allowed to pass json here, this will cause problems.</param>
        /// <param name="rzekaMessageType"></param>
        /// <param name="circumstances"></param>
        // TODO Above comment is problematic, we could consider stripping json from the string instead.
        void Speak(string message, RzekaMessageType rzekaMessageType, params TMatter[] circumstances);
        void Speak(Exception exception, params TMatter[] circumstances);
        void Speak(string message, Exception exception, params TMatter[] circumstances);
    }
}
