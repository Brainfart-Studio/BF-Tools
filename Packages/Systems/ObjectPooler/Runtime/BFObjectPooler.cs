using System.Collections.Generic;
using BFTools.Core.Logger;
using BFTools.Core.ServiceLocator;
using UnityEngine;

namespace BFTools.Systems.ObjectPooler
{
    public class BFObjectPooler : MonoBehaviour
    {
        private const string LogTag = "ObjectPooler";

        [SerializeField] private BFObjectPoolConfig config;

        private readonly Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> prefabsByKey = new Dictionary<string, GameObject>();
        private readonly Dictionary<GameObject, string> keysByInstance = new Dictionary<GameObject, string>();

        private void Awake()
        {
            Prewarm();
            BFServiceLocator.Register(this);
            BFLogger.Trace(LogTag, "Registered with ServiceLocator");
        }

        private void OnDestroy()
        {
            BFServiceLocator.Unregister<BFObjectPooler>();
            BFLogger.Trace(LogTag, "Unregistered from ServiceLocator");
        }

        private void Prewarm()
        {
            if (config == null)
            {
                BFLogger.Error(LogTag, "No config assigned. Skipping prewarm.");
                return;
            }

            foreach (BFObjectPoolConfig.PoolEntry entry in config.PoolEntries)
            {
                if (!pools.ContainsKey(entry.key))
                    pools[entry.key] = new Queue<GameObject>();

                prefabsByKey[entry.key] = entry.prefab;

                for (int i = 0; i < entry.prewarmCount; i++)
                {
                    GameObject instance = CreateInstance(entry.key, entry.prefab);
                    instance.SetActive(false);
                    pools[entry.key].Enqueue(instance);
                }

                BFLogger.Trace(LogTag, $"Prewarmed {entry.prewarmCount} instance(s) for '{entry.key}'");
            }
        }

        private GameObject CreateInstance(string key, GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, transform);
            keysByInstance[instance] = key;
            return instance;
        }

        public GameObject Get(string key)
        {
            if (!pools.TryGetValue(key, out Queue<GameObject> pool))
            {
                BFLogger.Error(LogTag, $"Get called with unknown key '{key}'. No pool exists for this key.");
                return null;
            }

            GameObject instance;
            if (pool.Count > 0)
            {
                instance = pool.Dequeue();
            }
            else
            {
                BFLogger.Warning(LogTag, $"Pool '{key}' exhausted, instantiating new instance. Consider increasing prewarm count.");
                instance = CreateInstance(key, prefabsByKey[key]);
            }

            instance.SetActive(true);
            BFLogger.Trace(LogTag, $"Got instance from pool '{key}'");
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (!keysByInstance.TryGetValue(instance, out string key))
            {
                BFLogger.Error(LogTag, $"Release called with an instance not tracked by this pooler: '{instance?.name}'.");
                return;
            }

            instance.SetActive(false);
            pools[key].Enqueue(instance);
            BFLogger.Trace(LogTag, $"Released instance to pool '{key}'");
        }
    }
}