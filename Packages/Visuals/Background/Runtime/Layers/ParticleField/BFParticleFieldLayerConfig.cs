using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFParticleFieldLayerConfig : BFBackgroundLayerConfig
    {
        [Header("Emission")]
        [Tooltip("Number of particles spawned per second.")]
        public float SpawnRate = 50f;

        [Header("Kinematics")]
        [Tooltip("Base movement direction in degrees.")]
        [Range(0f, 360f)]
        public float Angle = 90f;

        [Tooltip("Minimum starting speed.")]
        public float MinSpeed = 1f;

        [Tooltip("Maximum starting speed.")]
        public float MaxSpeed = 3f;

        [Tooltip("Gravity modifier applied to particles.")]
        public float GravityModifier = 0f;

        [Header("Lifetime & Size")]
        [Tooltip("Minimum particle lifetime in seconds.")]
        public float MinLifetime = 3f;

        [Tooltip("Maximum particle lifetime in seconds.")]
        public float MaxLifetime = 6f;

        [Tooltip("Minimum start size.")]
        public float MinSize = 0.5f;

        [Tooltip("Maximum start size.")]
        public float MaxSize = 1.5f;

        [Header("Appearance")]
        [Tooltip("Particle texture/sprite.")]
        public Texture2D ParticleTexture;

        [Tooltip("Color gradient over particle lifetime.")]
        public Gradient ColorGradient = new Gradient();

        [Header("Turbulence")]
        [Tooltip("Enables noise-based turbulence.")]
        public bool EnableTurbulence = true;

        [Tooltip("Strength of the turbulence noise.")]
        public float TurbulenceStrength = 1f;

        [Tooltip("Frequency of the turbulence noise.")]
        public float TurbulenceFrequency = 0.5f;

        [Tooltip("Scroll speed of the turbulence noise.")]
        public float TurbulenceSpeed = 1f;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFParticleFieldLayer(this);
        }

    }
}