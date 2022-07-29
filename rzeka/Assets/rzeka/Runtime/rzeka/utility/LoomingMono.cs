using UnityEngine;

namespace Rzeka
{
    public abstract class LoomingMono : MonoBehaviour
    {
        protected CollectibleDisposable q { get; set; } = new();

        protected virtual void OnEnable()
        {
            q ??= new();
        }

        protected virtual void OnDisable()
        {
            q.Dispose();
        }
    }
}
