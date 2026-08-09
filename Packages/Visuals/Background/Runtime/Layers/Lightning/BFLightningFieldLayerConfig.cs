using UnityEngine;

namespace BFTools.Visuals.Background
{
    [CreateAssetMenu(fileName = "BFLightningFieldLayerConfig", menuName = "BFTools/Backgrounds/Layers/Lightning Field Layer Config")]
    public class BFLightningFieldLayerConfig : BFBackgroundLayerConfig
    {
        [Header("Appearance")]
        [Tooltip("Base texture for the lightning bolt/flash element.")]
        public Texture2D BoltTexture;

        [Tooltip("Color and intensity tint of the lightning flashes.")]
        [ColorUsage(false, true)]
        public Color BoltColor = new Color(0.8f, 0.9f, 2f, 1f);

        [Header("Frequency & Timing")]
        [Tooltip("Average number of flashes per second.")]
        public float FlashFrequency = 0.5f;

        [Tooltip("Random variance added to flash frequency.")]
        public float FrequencyVariance = 0.3f;

        [Tooltip("Duration of a single flash/strike in seconds.")]
        public float FlashDuration = 0.15f;

        [Header("Structure & Layout")]
        [Tooltip("Scale of the procedural bolt structure.")]
        public float BoltScale = 1f;

        [Tooltip("Amount of jagged branching displacement.")]
        public float BranchingIntensity = 0.5f;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFLightningFieldLayer(this);
        }
    }
}