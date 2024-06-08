using System;
using System.Collections.Generic;

public abstract class RzekaException : Exception
{
    public List<string> Comments { get; private set; }
    
    public void AddComment(string comment)
    {
        Comments ??= new List<string>();
        Comments.Add(comment);
    }
}
