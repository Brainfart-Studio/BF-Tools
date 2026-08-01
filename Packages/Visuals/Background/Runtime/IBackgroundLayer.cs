using UnityEngine;

namespace BFTools.Visuals.Background
{
    public interface IBFBackgroundLayer
    {
        void Init(Transform parent, int sortingOrder);
        void Tick(float dt);
        void Cleanup();
    }
}