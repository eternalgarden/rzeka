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
        public HasStateAttribute() : base(1) { }
    }
}