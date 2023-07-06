using System;

namespace Rzeka
{
    public class HasBufferAttribute : Attribute
    {
        int _buffer;

        public int Buffer => _buffer;

        public HasBufferAttribute(int buffer)
        {
            // Default value
            _buffer = buffer;
        }
    }

    public class HasStateAttribute : HasBufferAttribute
    {
        readonly object _defaultValue;
        public HasStateAttribute() : base(1) { }
        
        // public HasStateAttribute(object defaultValue = null) : base(1)
        // {
        //     if (defaultValue != null) _defaultValue = defaultValue;
        // }
    }
    
}