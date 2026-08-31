using System.Collections.Generic;
using UnityEngine;

namespace ShadowFire.Core
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        private readonly Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void RegisterPool(string poolKey, GameObject prefab, int initialSize = 10)
        {
            if (prefab == null) return;
            if (!_pools.ContainsKey(poolKey))
            {
                _pools[poolKey] = new Queue<GameObject>();
                _prefabs[poolKey] = prefab;

                GameObject folder = new GameObject($"[Pool] {poolKey}");
                folder.transform.SetParent(transform);

                for (int i = 0; i < initialSize; i++)
                {
                    GameObject obj = Instantiate(prefab, folder.transform);
                    obj.SetActive(false);
                    _pools[poolKey].Enqueue(obj);
                }
            }
        }

        public GameObject Spawn(string poolKey, Vector3 position, Quaternion rotation)
        {
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogWarning($"Pool with key '{poolKey}' not registered.");
                return null;
            }

            Queue<GameObject> queue = _pools[poolKey];
            GameObject obj;

            if (queue.Count > 0 && queue.Peek() != null && !queue.Peek().activeInHierarchy)
            {
                obj = queue.Dequeue();
            }
            else
            {
                obj = Instantiate(_prefabs[poolKey], transform);
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            queue.Enqueue(obj);
            return obj;
        }

        public void Despawn(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}
