using System;
using System.Collections.Generic;
using UnityEngine;
namespace BFTools.Feedback.Hitstop
{
    [Serializable]
    public struct BFHitstopEntry
    {
        public string eventName;
        [Range(0f, 1f)] public float timescale;
        public float duration;
    }
    public class BFHitstopConfig : ScriptableObject
    {
        [SerializeField] private List<BFHitstopEntry> entries = new List<BFHitstopEntry>();
        public IReadOnlyList<BFHitstopEntry> Entries => entries;
        public bool TryGetEntry(string eventName, out BFHitstopEntry entry)
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