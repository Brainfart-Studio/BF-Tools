using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Feedback.SpriteFlash
{
    [Serializable]
    public struct BFSpriteFlashEntry
    {
        public string eventName;
        public float interval;
        public int flashCount;
    }

    public class BFSpriteFlashConfig : ScriptableObject
    {
        [SerializeField] private List<BFSpriteFlashEntry> entries = new List<BFSpriteFlashEntry>();

        public IReadOnlyList<BFSpriteFlashEntry> Entries => entries;

        public bool TryGetEntry(string eventName, out BFSpriteFlashEntry entry)
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