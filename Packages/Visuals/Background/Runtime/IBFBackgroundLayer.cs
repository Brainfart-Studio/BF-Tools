using BFTools.Core.LayerStack;

namespace BFTools.Visuals.Background
{
    public interface IBFBackgroundLayer : IBFStackLayer
    {
        void Tick(float dt);
    }
}