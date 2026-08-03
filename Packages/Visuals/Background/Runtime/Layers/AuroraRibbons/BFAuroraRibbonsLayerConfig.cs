using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFAuroraRibbonsLayerConfig : BFBackgroundLayerConfig
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
        [SerializeField, Range(1f, 100f)] private float thickness = 3f;
        [SerializeField, Range(0, 60)] private float glow = 18f;
        [SerializeField, Range(0f, 1f)] private float overallOpacity = 1f;

        internal IReadOnlyList<Color> RibbonColors => ribbonColors;
        internal int RibbonCount => ribbonCount;
        internal float WaveSpeed => waveSpeed;
        internal float Amplitude => amplitude;
        internal float Thickness => thickness;
        internal float Glow => glow;
        internal float OverallOpacity => overallOpacity;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFAuroraRibbonsLayer(this);
        }
    }
}