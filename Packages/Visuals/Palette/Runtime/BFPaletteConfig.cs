using System;
using System.Collections.Generic;
using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;

namespace BFTools.Visuals.Palette
{
    [Serializable]
    public struct BFPaletteEntry
    {
        public string name;
        public Color color;
    }

    public class BFPaletteConfig : ScriptableObject
    {
        private const string LogTag = "PaletteConfig";

        [SerializeField] private List<BFPaletteEntry> entries = new List<BFPaletteEntry>();

        public IReadOnlyList<BFPaletteEntry> Entries => entries;

        private void OnValidate()
        {
            WarnOnDuplicateNames();
            EventBus<BFPaletteConfigChangedEvent>.Fire(new BFPaletteConfigChangedEvent { config = this });
        }

        private void WarnOnDuplicateNames()
        {
            var seen = new HashSet<string>();
            for (int i = 0; i < entries.Count; i++)
            {
                if (!seen.Add(entries[i].name))
                {
                    BFLogger.Warning(LogTag,
                        $"Duplicate entry name '{entries[i].name}' in '{name}'. First one wins.",
                        this);
                }
            }
        }
    }
}