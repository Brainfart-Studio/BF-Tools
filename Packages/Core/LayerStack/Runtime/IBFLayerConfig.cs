namespace BFTools.Core.LayerStack
{
    public interface IBFLayerConfig<out TLayer>
    {
        TLayer CreateLayer();
    }
}