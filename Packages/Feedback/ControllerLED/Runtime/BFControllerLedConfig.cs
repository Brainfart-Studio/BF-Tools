using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Feedback.ControllerLED
{
    [Serializable]
    public struct BFControllerLedEntry
    {
        public string eventName;
        public Color color;
    }

    public class BFControllerLedConfig : ScriptableObject
    {
        [SerializeField] private List<BFControllerLedEntry> entries = new List<BFControllerLedEntry>();

        public IReadOnlyList<BFControllerLedEntry> Entries => entries;

        public bool TryGetEntry(string eventName, out BFControllerLedEntry entry)
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