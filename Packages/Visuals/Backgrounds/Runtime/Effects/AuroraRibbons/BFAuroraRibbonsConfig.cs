using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background.AuroraRibbons
{
    [CreateAssetMenu(fileName = "BFAuroraRibbonsConfig", menuName = "BFTools/Visuals/Background/Aurora Ribbons Config")]
    public class BFAuroraRibbonsConfig : BFBackgroundConfig
    {
        [SerializeField]
        private List<Color> ribbonColors = new List<Color>
        {
            new Color(0.5f, 1f, 0.831f),
            new Color(0.5f, 0.608f, 1f),
            new Color(0.765f, 0.5f, 1f),
            new Color(1f, 0.5f, 0.722f)
        };
        [SerializeField, Range(1, 8)] private int ribbonCount = 4;
        [SerializeField, Range(0.05f, 1.5f)] private float waveSpeed = 0.35f;
        [SerializeField, Range(20f, 260f)] private float amplitude = 110f;
        [SerializeField, Range(1f, 10f)] private float thickness = 3f;
        [SerializeField, Range(0, 60)] private float glow = 18f;
        [SerializeField, Range(0, 300)] private int starCount = 120;
        [SerializeField] private bool twinkle = true;

        internal IReadOnlyList<Color> RibbonColors => ribbonColors;
        internal int RibbonCount => ribbonCount;
        internal float WaveSpeed => waveSpeed;
        internal float Amplitude => amplitude;
        internal float Thickness => thickness;
        internal float Glow => glow;
        internal int StarCount => starCount;
        internal bool Twinkle => twinkle;

        public override IBFBackgroundEffect CreateEffect()
        {
            return new BFAuroraRibbonsEffect(this);
        }
    }
}