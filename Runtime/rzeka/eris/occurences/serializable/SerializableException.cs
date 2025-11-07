using System;

namespace Rzeka
{
    [Serializable]
    public class SerializableException
    {
        public string message { get; set; }
        public string[] comments { get; set; }
        public string stackTrace { get; set; }

        public static SerializableException FromException(Exception ex)
        {
            if (ex is null) return new SerializableNullException();
            
;           if (ex is RzekaException rex)
            {
                return new SerializableException()
                {
                    message = ex.Message,
                    comments = rex.Comments.ToArray(),
                    stackTrace = ex.StackTrace
                };
            }
            else
            {
                return new SerializableException()
                {
                    message = ex.Message,
                    comments = null,
                    stackTrace = ex.StackTrace
                };
            }
        }
    }
}