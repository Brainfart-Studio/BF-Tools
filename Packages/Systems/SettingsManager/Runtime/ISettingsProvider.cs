using System;

namespace BFTools.Systems.SettingsManager
{
    public interface ISettingsProvider
    {
        Type StateType { get; }
        object CaptureState();
        void RestoreState(object state);
    }
}