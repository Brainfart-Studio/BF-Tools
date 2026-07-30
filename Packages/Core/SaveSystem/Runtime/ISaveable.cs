using System;

namespace BFTools.Core.SaveSystem
{
    public interface ISaveable
    {
        Type StateType { get; }
        object CaptureState();
        void RestoreState(object state);
    }
}