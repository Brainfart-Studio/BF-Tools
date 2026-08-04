using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    public interface IBFParallaxLayer
    {
        void Init(Transform parent, int sortingOrder);
        void Tick(Vector2 cameraDisplacement, float dt);
        void Cleanup();
    }
}