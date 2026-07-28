using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Core.ObjectPooler
{
    public class BFObjectPooler : MonoBehaviour
    {
        [SerializeField] private BFObjectPoolConfig config;

        private readonly Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> prefabsByKey = new Dictionary<string, GameObject>();
        private readonly Dictionary<GameObject, string> keysByInstance = new Dictionary<GameObject, string>();

        private void Awake()
        {
            Prewarm();
        }

        private void Prewarm()
        {
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
            Queue<GameObject> pool = pools[key];

            GameObject instance = pool.Count > 0
                ? pool.Dequeue()
                : CreateInstance(key, prefabsByKey[key]);

            instance.SetActive(true);
            return instance;
        }

        public void Release(GameObject instance)
        {
            string key = keysByInstance[instance];
            instance.SetActive(false);
            pools[key].Enqueue(instance);
        }
    }
}