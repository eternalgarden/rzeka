using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.UI;

namespace Examples.DOTS.ManagedComponents
{
    [GenerateAuthoringComponent]
    public struct TimeToLiveData : IComponentData
    {
        public float TimeToLive;
        public float Value;
    }
}
