using System;
using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private List<BFPaletteEntry> entries = new List<BFPaletteEntry>();

        public IReadOnlyList<BFPaletteEntry> Entries => entries;
    }
}