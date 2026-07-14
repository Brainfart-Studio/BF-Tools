using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Feedback.Haptics
{
    [Serializable]
    public struct BFHapticsEntry
    {
        public string eventName;
        [Range(0f, 1f)] public float intensity;
        public float duration;
    }

    public class BFHapticsConfig : ScriptableObject
    {
        [SerializeField] private List<BFHapticsEntry> entries = new List<BFHapticsEntry>();

        public IReadOnlyList<BFHapticsEntry> Entries => entries;

        public bool TryGetEntry(string eventName, out BFHapticsEntry entry)
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