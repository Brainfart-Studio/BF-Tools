using UnityEngine;

namespace BFTools.Core.LayerStack
{
    public interface IBFStackLayer
    {
        void Init(Transform parent, int sortingOrder);
        void Cleanup();
    }
}