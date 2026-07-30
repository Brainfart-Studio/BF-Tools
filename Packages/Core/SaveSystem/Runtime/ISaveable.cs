namespace BFTools.Core.SaveSystem
{
    public interface ISaveable
    {
        object CaptureState();
        void RestoreState(object state);
    }
}