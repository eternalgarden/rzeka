using System;

namespace Rzeka.Tests.Integration
{
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