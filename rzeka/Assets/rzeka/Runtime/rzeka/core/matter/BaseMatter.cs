using System;

namespace Rzeka
{
    public abstract class BaseMatter : TMatter
    {
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public virtual string Description => throw new NotImplementedException();

        public BaseMatter(params TMatter[] circumstances)
        {
            (this as TMatter).SetCircumstances(circumstances);
        }
    }
}
