using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Feedback.Vignette
{
    public enum BFVignetteBlendMode
    {
        AlphaBlend,
        Multiply
    }

    [Serializable]
    public struct BFVignetteEntry
    {
        public string eventName;
        [Range(0f, 1f)] public float intensity;
        [Range(0f, 1f)] public float radius;
        [Range(0f, 1f)] public float softness;
        public Color color;
        [Range(0f, 1f)] public float roundness;
        public float frequency;
        public float amplitude;
        [Range(0f, 1f)] public float spacingVariance;
        [Range(0f, 1f)] public float amplitudeVariance;
        [Range(0f, 1f)] public float jaggedness;
        public float duration;
        public AnimationCurve intensityCurve;
        public BFVignetteBlendMode blendMode;
    }

    public class BFVignetteConfig : ScriptableObject
    {
        [SerializeField] private List<BFVignetteEntry> entries = new List<BFVignetteEntry>();

        public IReadOnlyList<BFVignetteEntry> Entries => entries;

        public bool TryGetEntry(string eventName, out BFVignetteEntry entry)
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