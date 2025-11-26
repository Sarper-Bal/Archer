using System.Collections.Generic;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Pools
{
    public abstract class ObjectPool<T> : ScriptableObject where T : MonoBehaviour
    {
        [SerializeField] protected int MaxSize;
        [SerializeField] protected T Behaviour;

        readonly Queue<T> _pooledObjectQueue = new Queue<T>();
        PoolInformer _poolInformer;
        
        public T Get()
        {
            if (ReferenceEquals(_poolInformer, null))
            {
                GameObject go = new GameObject();
                _poolInformer = go.AddComponent<PoolInformer>();
                _poolInformer.Destroyed += PoolInformerOnDestroyed;
            }
            
            T obj;
            if (_pooledObjectQueue.Count > 0)
            {
                obj = _pooledObjectQueue.Dequeue();
                obj.gameObject.SetActive(true);
            }
            else
            {
                obj = Instantiate(Behaviour);
            }

            return obj;
        }

        public void Release(T t)
        {
            if (_pooledObjectQueue.Count > MaxSize)
            {
                Destroy(t.gameObject);
            }
            else
            {
                t.gameObject.SetActive(false);
                _pooledObjectQueue.Enqueue(t);
            }
        }

        void PoolInformerOnDestroyed()
        {
            _poolInformer.Destroyed -= PoolInformerOnDestroyed;
            _poolInformer = null;
            _pooledObjectQueue.Clear();
        }
    }
}