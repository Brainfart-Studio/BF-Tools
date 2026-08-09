using System;

namespace BFTools.Systems.SaveSystem
{
    public interface IStateCapturable
    {
        Type StateType { get; }
        object CaptureState();
        void RestoreState(object state);
    }
}