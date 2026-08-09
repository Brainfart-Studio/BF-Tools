using UnityEngine;

namespace BFTools.Visuals.Background
{
    [CreateAssetMenu(fileName = "BFAtmosphericFieldLayerConfig", menuName = "BFTools/Backgrounds/Layers/Atmospheric Field Layer Config")]
    public class BFAtmosphericFieldLayerConfig : BFBackgroundLayerConfig
    {
        [Header("Appearance")]
        [Tooltip("Base texture for clouds, fog, or smoke.")]
        public Texture2D FieldTexture;

        [Tooltip("Tint color and overall opacity.")]
        public Color Color = Color.white;

        [Header("Movement & Flow")]
        [Tooltip("Base movement direction in degrees.")]
        [Range(0f, 360f)]
        public float Angle = 0f;

        [Tooltip("Movement speed.")]
        public float ScrollSpeed = 1f;

        [Header("Noise & Distortion")]
        [Tooltip("Enables procedural distortion.")]
        public bool EnableDistortion = true;

        [Tooltip("Scale of the noise pattern.")]
        public float NoiseScale = 1f;

        [Tooltip("Intensity of the distortion effect.")]
        public float DistortionAmount = 0.5f;

        [Tooltip("Speed at which the distortion animates.")]
        public float DistortionSpeed = 0.5f;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFAtmosphericFieldLayer(this);
        }
    }
}