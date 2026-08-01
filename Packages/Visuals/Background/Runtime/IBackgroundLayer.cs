using UnityEngine;

namespace BFTools.Visuals.BackgroundStacks
{
    public interface IBFBackgroundLayer
    {
        void Init(Transform parent, int sortingOrder);
        void Tick(float dt);
        void Cleanup();
    }
}