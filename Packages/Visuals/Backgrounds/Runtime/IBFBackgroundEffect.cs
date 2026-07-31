namespace BFTools.Visuals.Background
{
    public interface IBFBackgroundEffect
    {
        void Init();
        void Tick(float dt);
        void Cleanup();
    }
}