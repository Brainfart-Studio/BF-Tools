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
        [Header("Trigger")]
        [Tooltip("Event name this entry responds to when raised on the BFVignetteEvent bus.")]
        public string eventName;

        [Header("Appearance")]
        [Range(0f, 1f), Tooltip("Peak alpha applied to the vignette at full intensity.")]
        public float intensity;
        [Tooltip("Solid tint applied to the vignette when Use Gradient is disabled.")]
        public Color color;
        [Tooltip("Samples Color Gradient across the mask instead of using a single solid color.")]
        public bool useGradient;
        [Tooltip("Gradient sampled across the vignette mask when Use Gradient is enabled.")]
        public Gradient colorGradient;
        [Tooltip("How the vignette composites with what's behind it. Only Alpha Blend is currently implemented.")]
        public BFVignetteBlendMode blendMode;

        [Header("Mask Shape")]
        [Range(0f, 1f), Tooltip("Base radius of the vignette mask, relative to screen size.")]
        public float radius;
        [Range(0f, 1f), Tooltip("How gradual the mask's edge fades from opaque to transparent.")]
        public float softness;
        [Range(0f, 1f), Tooltip("How circular vs. rectangular the mask silhouette is.")]
        public float roundness;

        [Header("Wave Shape")]
        [Tooltip("How many wave cycles appear around the mask's edge.")]
        public float frequency;
        [Tooltip("Outer bound the wave's crests reach, in mask units.")]
        public float waveCrest;
        [Tooltip("Inner bound the wave's troughs reach, in mask units.")]
        public float waveTrough;
        [Range(0f, 1f), Tooltip("Randomizes the spacing between wave peaks. 0 keeps peaks evenly spaced.")]
        public float spacingVariance;
        [Range(0f, 1f), Tooltip("Randomizes each wave peak's height between crest and trough. 0 keeps every peak the same height.")]
        public float waveHeightVariance;
        [Range(0f, 1f), Tooltip("Adds high-frequency noise to the mask edge for a rougher, less smooth wave line.")]
        public float jaggedness;

        [Header("Timing")]
        [Tooltip("How long the vignette plays before fading out, in seconds.")]
        public float duration;
        [Tooltip("Shapes the vignette's alpha over the course of Duration. Evaluated from 0 to 1.")]
        public AnimationCurve intensityCurve;
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