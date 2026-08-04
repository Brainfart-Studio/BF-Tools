using System;

namespace BFTools.Systems.SaveSystem
{
    public interface ISaveable
    {
        Type StateType { get; }
        object CaptureState();
        void RestoreState(object state);
    }
}