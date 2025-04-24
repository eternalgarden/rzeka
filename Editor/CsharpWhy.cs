
using System;
using System.Collections.Generic;

namespace Why
{
    // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/covariance-contravariance/
    // https://www.hexacta.com/advanced-generics-language/
    /*

    
    Invariant:  The parametric type of the generic class can’t be changed, as we 
                just saw in the previous example. In C#, parametric types are invariant 
                by default.

    Contravariant:  The parameterized type can be converted to a derived class. -
                    Contravariant parameters can only be used at entrance points as 
                    the argument of a method. They are specified by the keyword in 
                    (eg: Action <in T>).

    Covariant:  The parameterized type can be converted to a base class. Contravariant 
                can only be used at exit points as the return type of a method. 
                They are specified by the keyword out (eg Func <in T1, out T2>)


    */

    interface ICatCollection<out T> where T : Meow
    {
        T Do();
    }

    class Cattaloguer<T> : ICatCollection<T> where T : Meow
    {
        public T Do()
        {
            throw new NotImplementedException();
        }
    }

    abstract class Meow { }

    class SiameseMeow : Meow { }

    class CsharpWhy
    {
        void Main()
        {
            // wyu we cant do that 😿
            // What happens is that generics allows us to use parameterized types
            // in classes, but it doesn’t go as far as taking polymorphism into 
            // account. For this reason we can’t assign to a generic instance 
            // another with a derived parametric type, [...]
            ICatCollection<Meow> collectibleMeow = new Cattaloguer<SiameseMeow>();

            Dictionary<Type, ICatCollection<Meow>> LanguageOfMeow = new();
            LanguageOfMeow.Add(typeof(SiameseMeow), new Cattaloguer<SiameseMeow>());
        }
    }
}