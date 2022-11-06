using System;

namespace Rzeka.Tests
{
    public struct ANumber : TMatter
    {
        public string Description => "Test matter carrying hypothetical welcoming text";
        public Guid Guid { get; }
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
        public Guid Guid { get; }
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
        public Guid Guid { get; }
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
        public string Description => "Test matter of user data";
        public Guid Guid { get; }
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
        public Guid Guid { get; }
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
}