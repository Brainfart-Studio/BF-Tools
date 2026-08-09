using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFParticleFieldLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags = { "Background", "ParticleField" };

        private readonly BFParticleFieldLayerConfig config;

        private Transform root;
        private ParticleSystem particleSystem;
        private ParticleSystemRenderer particleRenderer;
        private Material material;

        private int lastWidth = -1;
        private int lastHeight = -1;

        public BFParticleFieldLayer(BFParticleFieldLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            GameObject obj = new GameObject("BFParticleFieldLayer");
            obj.transform.SetParent(parent, false);
            obj.transform.position = new Vector3(0f, 0f, 10f);
            obj.layer = BFBackgroundStackManager.BackgroundLayer;
            root = obj.transform;

            particleSystem = obj.AddComponent<ParticleSystem>();
            ConfigureParticleSystem();

            particleRenderer = obj.GetComponent<ParticleSystemRenderer>();

            material = new Material(Shader.Find("Particles/Standard Unlit"));
            if (config.ParticleTexture != null)
            {
                material.mainTexture = config.ParticleTexture;
            }
            particleRenderer.material = material;
            particleRenderer.sortingOrder = sortingOrder;

            UpdateEmitterBounds();

            BFLogger.Info(LogTags, "BFParticleFieldLayer: initialized.");
        }

        public void Tick(float dt)
        {
            ConfigureParticleSystem();

            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                UpdateEmitterBounds();
            }
        }

        public void Cleanup()
        {
            if (root != null)
                DestroyObject(root.gameObject);
            root = null;

            particleSystem = null;
            particleRenderer = null;
            material = null;
        }

        private static void DestroyObject(Object obj)
        {
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }

        private void ConfigureParticleSystem()
        {
            if (particleSystem == null) return;

            var main = particleSystem.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(config.MinLifetime, config.MaxLifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(config.MinSpeed, config.MaxSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(config.MinSize, config.MaxSize);
            main.gravityModifier = config.GravityModifier;
            main.startColor = Color.white;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            float angleRad = config.Angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(angleRad), Mathf.Cos(angleRad), 0f);
            particleSystem.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);

            var emission = particleSystem.emission;
            emission.enabled = true;
            emission.rateOverTime = config.SpawnRate;

            var colorOverLife = particleSystem.colorOverLifetime;
            colorOverLife.enabled = true;
            colorOverLife.color = new ParticleSystem.MinMaxGradient(config.ColorGradient);

            var noise = particleSystem.noise;
            if (config.EnableTurbulence)
            {
                noise.enabled = true;
                noise.strength = config.TurbulenceStrength;
                noise.frequency = config.TurbulenceFrequency;
                noise.scrollSpeed = config.TurbulenceSpeed;
            }
            else
            {
                noise.enabled = false;
            }

            if (particleRenderer != null && config.ParticleTexture != null && particleRenderer.sharedMaterial != null)
            {
                if (particleRenderer.sharedMaterial.mainTexture != config.ParticleTexture)
                {
                    particleRenderer.sharedMaterial.mainTexture = config.ParticleTexture;
                }
            }
        }

        private void UpdateEmitterBounds()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            float worldHeight = Screen.height;
            float worldWidth = Screen.width;

            root.position = new Vector3(worldWidth * 0.5f, worldHeight * 0.5f, 10f);

            var shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(worldWidth * 1.5f, worldHeight * 1.5f, 1f);
        }
    }
}