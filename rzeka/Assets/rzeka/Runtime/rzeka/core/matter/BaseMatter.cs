using System;

namespace Rzeka
{
    
    // dont use this, the point of mater was to be immutable and using structs
    [Obsolete]
    public abstract class BaseMatter : TMatter
    {
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public Type Type { get; set; }
        public string Description => "༼ つ ◕_◕ ༽つ description not set";

        protected BaseMatter(params TMatter[] circumstances)
        {
            (this as TMatter).SetCircumstances(circumstances);
            Guid = Guid.NewGuid();
        }
    }
}
