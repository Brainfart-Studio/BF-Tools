using BFTools.Core.LayerStack;

namespace BFTools.Visuals.Background
{
    public class BFBackgroundStack : BFLayerStackBase<BFBackgroundLayerConfig, IBFBackgroundLayer>
    {
        private const string LogTag = "Background";

        public BFBackgroundStack(BFBackgroundStackConfig config)
            : base(LogTag, nameof(BFBackgroundStack), config.Layers, config)
        {
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < layers.Count; i++)
                layers[i].Tick(dt);
        }
    }
}