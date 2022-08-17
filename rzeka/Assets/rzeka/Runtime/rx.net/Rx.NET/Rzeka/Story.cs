using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.Reactive
{
    public abstract class Story
    {

        public Story(Z origin)
        {
            
        }
        
        public void MakeTurn(Z newTurn)
        {
            
        }
    }
    
    
    // ! Instead of 'Turn' Z is a good sign for that
    public abstract class Z
    {
        
    }

    public class Z<T> : Z
    {


        public T Value { get; }


    }
}
