/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/

namespace Rzeka
{
    public interface IGlyph
    {
        int Size { get; }
    }
    
    public class Glyph<T1, T2> : IGlyph
        where T1 : TMatter
        where T2 : TMatter
    {
        // -------------

        public int Size => 2;
        
        public T1 One { get; }
        public T2 Two { get; }

        public Glyph(T1 one, T2 two)
        {
            One = one;
            Two = two;
        }

        // -------------
    }
    
    public class Glyph<T1, T2, T3> : IGlyph
        where T1 : TMatter
        where T2 : TMatter
    {
        // -------------

        public int Size => 3;

        public T1 One { get; }
        public T2 Two { get; }
        public T3 Three { get; }
        
        public Glyph(T1 one, T2 two, T3 three)
        {
            One = one;
            Two = two;
            Three = three;
        }

        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 05 November 2022 🌊 */