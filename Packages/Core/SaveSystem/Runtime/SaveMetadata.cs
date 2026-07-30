using System;

namespace BFTools.Core.SaveSystem
{
    [Serializable]
    public struct SaveMetadata
    {
        public int version;
        public DateTime timestamp;
        public float playtimeSeconds;
    }
}