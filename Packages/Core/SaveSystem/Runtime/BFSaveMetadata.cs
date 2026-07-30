using System;

namespace BFTools.Core.SaveSystem
{
    [Serializable]
    public struct BFSaveMetadata
    {
        public int version;
        public DateTime timestamp;
        public float playtimeSeconds;
    }
}