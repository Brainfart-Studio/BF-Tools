using System;
using System.Collections.Generic;
using UnityEngine;
namespace BFTools.Feedback.ScreenShake
{
    [Serializable]
    public struct BFScreenShakeEntry
    {
        public string eventName;
        [Range(0f, 1f)] public float amplitude;
        public float duration;
    }
    public class BFScreenShakeConfig : ScriptableObject
    {
        [SerializeField] private List<BFScreenShakeEntry> entries = new List<BFScreenShakeEntry>();
        public IReadOnlyList<BFScreenShakeEntry> Entries => entries;
        public bool TryGetEntry(string eventName, out BFScreenShakeEntry entry)
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