using UnityEngine;

namespace BFTools.Visuals.Background
{
    public interface IBFBackgroundEffect
    {
        void Init(Transform parent);
        void Tick(float dt);
        void Cleanup();
    }
}