using System;
using System.Collections.Generic;

namespace BFTools.Systems.SaveSystem
{
    [Serializable]
    public class BFSaveData
    {
        public BFSaveMetadata metadata;
        public Dictionary<string, object> saveableStates = new Dictionary<string, object>();
    }
}