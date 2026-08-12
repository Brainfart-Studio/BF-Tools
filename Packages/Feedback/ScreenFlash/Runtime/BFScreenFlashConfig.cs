using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Feedback.ScreenFlash
{
    [Serializable]
    public struct BFScreenFlashEntry
    {
        public string eventName;
        public Color flashColor;
        public float duration;
        public int flashCount;
    }

    public class BFScreenFlashConfig : ScriptableObject
    {
        [SerializeField] private List<BFScreenFlashEntry> entries = new List<BFScreenFlashEntry>();

        public IReadOnlyList<BFScreenFlashEntry> Entries => entries;

        public bool TryGetEntry(string eventName, out BFScreenFlashEntry entry)
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