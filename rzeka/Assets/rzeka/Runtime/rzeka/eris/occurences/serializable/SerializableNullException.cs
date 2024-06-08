using System.Collections;
using System.Collections.Generic;
using Rzeka;
using UnityEngine;

public class SerializableNullException : SerializableException
{
    public SerializableNullException()
    {
        message = null;
        stackTrace = null;
        comments = null;
    }
}
