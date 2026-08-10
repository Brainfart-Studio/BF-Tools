using System;

namespace BFTools.Core.Serialization
{
    public interface IStateCapturable
    {
        Type StateType { get; }
        object CaptureState();
        void RestoreState(object state);
    }
}