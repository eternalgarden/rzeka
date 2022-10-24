using System;

namespace Rzeka.Tests.Integration
{
    public class UserData : TMatter
    {

        public string Name { get; set; }
        public string Zodiac { get; set; }
        public int FavNumber { get; set; }
        public DateTime JoinedDate { get; set; }
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public Type Type { get; set; }
        public string Description { get; }
    }

    public class UserWelcomingText : TMatter
    {
        public Guid Guid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Guid[] Circumstances { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Type Type { get; set; }
        public string Description => throw new NotImplementedException();

        public string WelcomingText { get; set; }
    }
}