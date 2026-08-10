using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public sealed class BFImpossibleGardenLayerConfig : BFBackgroundLayerConfig
    {
        [Header("Garden")]
        [SerializeField, Range(1, 32)]
        private int gardenCount = 10;

        [SerializeField, Range(0.05f, 0.8f)]
        private float gardenRadius = 0.28f;

        [SerializeField, Range(0f, 0.25f)]
        private float gardenSpread = 0.12f;

        [SerializeField, Range(0f, 1f)]
        private float overallOpacity = 0.8f;

        [Header("Bloom")]
        [SerializeField, Range(0.01f, 0.5f)]
        private float minBloomRadius = 0.04f;

        [SerializeField, Range(0.01f, 0.8f)]
        private float maxBloomRadius = 0.18f;

        [SerializeField, Range(0f, 1f)]
        private float bloomOrbitStrength = 0.08f;

        [SerializeField, Range(0f, 1f)]
        private float pulseStrength = 0.25f;

        [SerializeField, Range(0f, 2f)]
        private float pulseSpeed = 0.35f;

        [SerializeField, Range(0f, 1f)]
        private float inversionChance = 0.08f;

        [SerializeField, Range(0.1f, 10f)]
        private float inversionDuration = 1.5f;

        [SerializeField, Range(0f, 1f)]
        private float inversionStrength = 0.8f;

        [SerializeField, Range(0f, 1f)]
        private float hueDrift = 0.08f;

        [SerializeField, Range(0f, 60f)]
        private float glow = 18f;

        [SerializeField]
        private List<Color> bloomColors = new List<Color>
        {
            new Color(0.15f, 1f, 0.72f, 1f),
            new Color(0.5f, 0.3f, 1f, 1f),
            new Color(1f, 0.2f, 0.55f, 1f),
            new Color(0.2f, 0.8f, 1f, 1f)
        };

        [Header("Petals")]
        [SerializeField, Range(3, 16)]
        private int petalsPerGarden = 7;

        [SerializeField, Range(0.01f, 0.2f)]
        private float petalLength = 0.08f;

        [SerializeField, Range(1f, 30f)]
        private float petalWidth = 5f;

        [SerializeField, Range(0f, 1f)]
        private float petalCurl = 0.65f;

        [Header("Animation")]
        [SerializeField, Range(-30f, 30f)]
        private float rotationSpeed = 4f;

        [SerializeField, Range(0f, 2f)]
        private float breathingSpeed = 0.35f;

        [SerializeField, Range(0f, 0.3f)]
        private float breathingAmount = 0.08f;

        [SerializeField, Range(0f, 1f)]
        private float orbitalSpeed = 0.12f;

        [SerializeField, Range(0f, 1f)]
        private float orbitalAmount = 0.08f;

        [SerializeField, Range(0f, 1f)]
        private float wobble = 0.18f;

        [SerializeField, Range(0f, 1f)]
        private float bloomDrift = 0.12f;

        [SerializeField, Range(0f, 1f)]
        private float voidBreathing = 0.15f;

        [SerializeField, Range(0f, 1f)]
        private float membraneDistortion = 0.15f;

        internal int GardenCount => gardenCount;
        internal float GardenRadius => gardenRadius;
        internal float GardenSpread => gardenSpread;

        internal float OverallOpacity => overallOpacity;
        internal float Opacity => overallOpacity;

        internal float MinBloomRadius => minBloomRadius;
        internal float MaxBloomRadius => maxBloomRadius;
        internal float BloomOrbitStrength => bloomOrbitStrength;
        internal float PulseStrength => pulseStrength;
        internal float PulseSpeed => pulseSpeed;
        internal float InversionChance => inversionChance;
        internal float InversionDuration => inversionDuration;
        internal float InversionStrength => inversionStrength;
        internal float HueDrift => hueDrift;
        internal float Glow => glow;

        internal IReadOnlyList<Color> BloomColors => bloomColors;

        internal int PetalsPerGarden => petalsPerGarden;
        internal float PetalLength => petalLength;
        internal float PetalWidth => petalWidth;
        internal float PetalCurl => petalCurl;

        internal float RotationSpeed => rotationSpeed;
        internal float BreathingSpeed => breathingSpeed;
        internal float BreathingAmount => breathingAmount;
        internal float OrbitalSpeed => orbitalSpeed;
        internal float OrbitalAmount => orbitalAmount;
        internal float Wobble => wobble;
        internal float BloomDrift => bloomDrift;
        internal float VoidBreathing => voidBreathing;
        internal float MembraneDistortion => membraneDistortion;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFImpossibleGardenLayer(this);
        }
    }
}