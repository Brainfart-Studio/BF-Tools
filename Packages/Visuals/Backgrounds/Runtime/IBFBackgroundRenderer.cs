using UnityEngine;

namespace BFTools.Visuals.Background
{
    public interface IBFBackgroundRenderer
    {
        void Init(Transform parent);
        void Render();
        void Cleanup();
    }
}