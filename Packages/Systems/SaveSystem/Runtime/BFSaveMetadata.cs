using System;

namespace BFTools.Systems.SaveSystem
{
    [Serializable]
    public struct BFSaveMetadata
    {
        public int version;
        public DateTime timestamp;
        public float playtimeSeconds;
    }
}