using System;

namespace Rzeka
{
    public interface IRzekaLogFairy
    {
        void Speak(string message, params TMatter[] circumstances);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">Note you are not allowed to pass json here, this will cause problems.</param>
        /// <param name="rzekaMessageType"></param>
        /// <param name="circumstances"></param>
        void Speak(string message, RzekaMessageType rzekaMessageType, params TMatter[] circumstances);
        void Speak(Exception exception, params TMatter[] circumstances);
        void Speak(string message, Exception exception, params TMatter[] circumstances);
    }
}