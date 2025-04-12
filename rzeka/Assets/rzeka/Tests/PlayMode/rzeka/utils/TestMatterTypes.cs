using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rzeka.Tests
{
    public class ANumber : Rzeka.Matter
    {
        public ANumber(int number)
        {
            Number = number;
        }

        public int Number { get; }
    }
    public class AName : Rzeka.Matter
    {
        public AName(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
    public class UserData : Rzeka.Matter
    {
        sealed class NameZodiacFavNumberEqualityComparer : IEqualityComparer<UserData>
        {
            public bool Equals(UserData x, UserData y)
            {
                // Debug.Log(x.Name + x.Zodiac + x.FavNumber);
                // Debug.Log(y.Name + y.Zodiac + y.FavNumber);
                return x.Name == y.Name && x.Zodiac == y.Zodiac && x.FavNumber == y.FavNumber;
            }

            public int GetHashCode(UserData obj)
            {
                return HashCode.Combine(obj.Name, obj.Zodiac, obj.FavNumber);
            }
        }

        public static IEqualityComparer<UserData> NameZodiacFavNumberComparer { get; } = new NameZodiacFavNumberEqualityComparer();
        
        public UserData(string name, string zodiac, int favNumber)
        {
            Name = name;
            Zodiac = zodiac;
            FavNumber = favNumber;
        }
        
        public string Name { get; }
        public string Zodiac { get; }
        public int FavNumber { get; }
    }
    public class UserWelcomingText : Rzeka.Matter
    {
        public UserWelcomingText(string welcomingText)
        {
            WelcomingText = welcomingText;
        }

        public string WelcomingText { get; }
    }
    
    public class ArbitraryMatter1 : Rzeka.Matter
    {
        sealed class TextEqualityComparer : IEqualityComparer<ArbitraryMatter1>
        {
            public bool Equals(ArbitraryMatter1 x, ArbitraryMatter1 y)
            {
                return x.Text == y.Text;
            }

            public int GetHashCode(ArbitraryMatter1 obj)
            {
                return (obj.Text != null ? obj.Text.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<ArbitraryMatter1> TextComparer { get; } = new TextEqualityComparer();

        
        public Type Type => typeof(ArbitraryMatter1);
        
        public ArbitraryMatter1()
        {
            Text = "lol";
        }
        
        public ArbitraryMatter1(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
    
    public class ArbitraryMatter2 : Rzeka.Matter
    {
        sealed class TextEqualityComparer : IEqualityComparer<ArbitraryMatter2>
        {
            public bool Equals(ArbitraryMatter2 x, ArbitraryMatter2 y)
            {
                return x.Text == y.Text;
            }

            public int GetHashCode(ArbitraryMatter2 obj)
            {
                return (obj.Text != null ? obj.Text.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<ArbitraryMatter2> TextComparer { get; } = new TextEqualityComparer();

        
        public Type Type => typeof(ArbitraryMatter2);
        
        public ArbitraryMatter2()
        {
            Text = "lol";
        }
        
        public ArbitraryMatter2(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
    
    public class ArbitraryMatter3 : Rzeka.Matter
    {
        sealed class TextEqualityComparer : IEqualityComparer<ArbitraryMatter3>
        {
            public bool Equals(ArbitraryMatter3 x, ArbitraryMatter3 y)
            {
                return x.Text == y.Text;
            }

            public int GetHashCode(ArbitraryMatter3 obj)
            {
                return (obj.Text != null ? obj.Text.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<ArbitraryMatter3> TextComparer { get; } = new TextEqualityComparer();

        
        public Type Type => typeof(ArbitraryMatter3);
        
        public ArbitraryMatter3(string text = "lol")
        {
            Text = text;
        }

        public string Text { get; }
    }
    
    [HasState]
    public class ArbitraryStatefulMatter1 : Rzeka.Matter
    {
        public ArbitraryStatefulMatter1()
        {
            State = 0;
        }
        
        public ArbitraryStatefulMatter1(int state)
        {
            State = state;
        }

        public int State { get; }
    }
}