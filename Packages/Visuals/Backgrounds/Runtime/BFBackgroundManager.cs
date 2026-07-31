using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFBackgroundManager : MonoBehaviour
    {
        [SerializeField] private BFBackgroundConfig config;

        private IBFBackgroundEffect activeEffect;

        private void OnEnable()
        {
            if (config == null)
                return;

            activeEffect = config.CreateEffect();
            activeEffect.Init();
        }

        private void Update()
        {
            activeEffect?.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            activeEffect?.Cleanup();
            activeEffect = null;
        }
    }
}