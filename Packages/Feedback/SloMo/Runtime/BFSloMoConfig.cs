using System;
using System.Collections.Generic;
using UnityEngine;
namespace BFTools.Feedback.SloMo
{
    [Serializable]
    public struct BFSloMoEntry
    {
        public string eventName;
        public float duration;
        public AnimationCurve curve;
    }
    public class BFSloMoConfig : ScriptableObject
    {
        [SerializeField] private List<BFSloMoEntry> entries = new List<BFSloMoEntry>();
        public IReadOnlyList<BFSloMoEntry> Entries => entries;
        public bool TryGetEntry(string eventName, out BFSloMoEntry entry)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].eventName == eventName)
                {
                    entry = entries[i];
                    return true;
                }
            }
            entry = default;
            return false;
        }
    }
}