using System;

namespace Rzeka
{
    public abstract class BaseMatter : TMatter
    {
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public virtual string Description => "༼ つ ◕_◕ ༽つ description not set";
        public Type Type => this.GetType();

        protected BaseMatter(params TMatter[] circumstances)
        {
            (this as TMatter).SetCircumstances(circumstances);
            Guid = Guid.NewGuid();
        }
    }
}
