using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Systems.ObjectPooler
{
    public class BFObjectPoolConfig : ScriptableObject
    {
        [Serializable]
        public struct PoolEntry
        {
            public string key;
            public GameObject prefab;
            public int prewarmCount;
        }

        [SerializeField] private List<PoolEntry> poolEntries = new List<PoolEntry>();

        internal IReadOnlyList<PoolEntry> PoolEntries => poolEntries;
    }
}