using System;

namespace Rzeka
{
    public interface IRzekaLogFairy
    {
        void Speak(string message, params TMatter[] circumstances);
        void Speak(string message, RzekaMessageType rzekaMessageType, params TMatter[] circumstances);
        void Speak(Exception exception, params TMatter[] circumstances);
        void Speak(string message, Exception exception, params TMatter[] circumstances);
    }
}