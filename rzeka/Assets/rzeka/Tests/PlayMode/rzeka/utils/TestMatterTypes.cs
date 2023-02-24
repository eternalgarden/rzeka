using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rzeka.Tests
{
    public struct ANumber : TMatter
    {
        public string Description => "Test matter carrying hypothetical welcoming text";
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public Type Type { get; }
        
        public ANumber(int number)
        {
            Guid = Guid.NewGuid();
            Type = typeof(ANumber);
            Circumstances = new Guid[] { };
            
            Number = number;
        }

        public int Number { get; }
    }

    public struct AName : TMatter
    {
        public string Description => "Test matter carrying hypothetical welcoming text";
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public Type Type { get; }
        
        public AName(string name)
        {
            Guid = Guid.NewGuid();
            Type = typeof(AName);
            Circumstances = new Guid[] { };
            
            Name = name;
        }

        public string Name { get; }
    }

    public struct ANumberAndName : TMatter
    {
        public string Description => "Test matter carrying hypothetical welcoming text";
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public Type Type { get; }
        
        public ANumberAndName(int number, string name)
        {
            Guid = Guid.NewGuid();
            Type = typeof(ANumberAndName);
            Circumstances = new Guid[] { };

            Number = number;
            Name = name;
        }

        public int Number { get; }
        public string Name { get; }
    }

    public struct UserData : TMatter
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

        public string Description => "Test matter of user data";
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public Type Type { get; }
        
        public UserData(string name, string zodiac, int favNumber)
        {
            Guid = Guid.NewGuid();
            Type = typeof(UserData);
            Circumstances = new Guid[] { };
            
            Name = name;
            Zodiac = zodiac;
            FavNumber = favNumber;
        }
        
        public string Name { get; }
        public string Zodiac { get; }
        public int FavNumber { get; }
    }

    public struct UserWelcomingText : TMatter
    {
        public string Description => "Test matter carrying hypothetical welcoming text";
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public Type Type { get; }
        
        public UserWelcomingText(string welcomingText)
        {
            Guid = Guid.NewGuid();
            Type = typeof(UserWelcomingText);
            Circumstances = new Guid[] { };
            
            WelcomingText = welcomingText;
        }

        public string WelcomingText { get; }
    }
    
    public struct ArbitraryMatter1 : TMatter
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

        public string Description => "Test matter carrying hypothetical welcoming text";
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public Type Type { get; }
        
        public ArbitraryMatter1(string text)
        {
            Guid = Guid.NewGuid();
            Type = typeof(ANumber);
            Circumstances = new Guid[] { };
            Text = text;
        }

        public string Text { get; }
    }
    
    public struct ArbitraryMatter2 : TMatter
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

        public string Description => "Test matter carrying hypothetical welcoming text";
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public Type Type { get; }
        
        public ArbitraryMatter2(string text)
        {
            Guid = Guid.NewGuid();
            Type = typeof(ANumber);
            Circumstances = new Guid[] { };
            Text = text;
        }

        public string Text { get; }
    }
    
    public struct ArbitraryMatter3 : TMatter
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

        public string Description => "Test matter carrying hypothetical welcoming text";
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public Type Type { get; }
        
        public ArbitraryMatter3(string text)
        {
            Guid = Guid.NewGuid();
            Type = typeof(ANumber);
            Circumstances = new Guid[] { };
            Text = text;
        }

        public string Text { get; }
    }
}