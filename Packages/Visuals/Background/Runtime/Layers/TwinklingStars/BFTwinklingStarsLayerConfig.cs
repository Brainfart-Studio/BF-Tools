using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTwinklingStarsLayerConfig : BFBackgroundLayerConfig
    {
        [SerializeField, Range(0, 300)] private int starCount = 120;
        [SerializeField] private bool twinkle = true;

        internal int StarCount => starCount;
        internal bool Twinkle => twinkle;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFTwinklingStarsLayer(this);
        }
    }
}