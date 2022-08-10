using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.UI;

namespace Examples.DOTS.ManagedComponents
{
    /* !
      By making it a class becomes a Managed Component

      The idea is separation of managed and unmanaged data. Unmanaged data can be processed with a burst compiler
     */
    [GenerateAuthoringComponent]
    public class HealthBarUIData : IComponentData
    {
        public Slider Slider;
        public float3 Offset;
    }
}
