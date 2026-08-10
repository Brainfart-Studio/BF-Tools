using UnityEngine;

namespace BFTools.Visuals.Background
{
    [CreateAssetMenu(fileName = "BFFireFieldLayerConfig", menuName = "BFTools/Backgrounds/Layers/Fire Field Layer Config")]
    public class BFFireFieldLayerConfig : BFBackgroundLayerConfig
    {
        [Header("Appearance")]
        [Tooltip("Color tint of the flames (HDR supported).")]
        [ColorUsage(false, true)]
        public Color FireColor = new Color(3f, 1f, 0.2f, 1f);

        [Header("Flame Dynamics")]
        [Tooltip("Number of distinct flame tongues/tongue columns across the screen.")]
        [Range(2, 32)]
        public int FlameColumns = 12;

        [Tooltip("Speed at which the flames rise and lick upward.")]
        public float RiseSpeed = 2f;

        [Tooltip("Height variance and stretching of the flames.")]
        public float FlameHeight = 1.5f;

        [Tooltip("Intensity of the turbulent wind swaying the flames side-to-side.")]
        public float Turbulence = 0.5f;

        [Tooltip("Speed of the swaying turbulence.")]
        public float TurbulenceSpeed = 3f;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFFireFieldLayer(this);
        }
    }
}