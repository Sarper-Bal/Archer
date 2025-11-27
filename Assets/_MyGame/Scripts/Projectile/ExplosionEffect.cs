using UnityEngine;
using System.Collections;
using ArcadeBridge.ArcadeIdleEngine.Pools; // Havuz kütüphanesi

namespace IndianOceanAssets.Engine2_5D
{
    public class ExplosionEffect : MonoBehaviour
    {
        [Header("Ayarlar")]
        [SerializeField] private float _duration = 2f; 
        
        // DEĞİŞİKLİK: Artık özel "ExplosionPool" değil, genel "ObjectPool" tutuyor.
        // Böylece hem DeathEffectPool hem de ExplosionPool kabul edebilir.
        private ObjectPool<ExplosionEffect> _myPool;
        private Coroutine _disableCoroutine;

        /// <summary>
        /// Efekti başlatır. Her türlü ExplosionEffect havuzunu kabul eder.
        /// </summary>
        public void Initialize(ObjectPool<ExplosionEffect> pool)
        {
            _myPool = pool;
            
            if (_disableCoroutine != null) StopCoroutine(_disableCoroutine);
            _disableCoroutine = StartCoroutine(DisableRoutine());
        }

        private IEnumerator DisableRoutine()
        {
            yield return new WaitForSeconds(_duration);
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (_disableCoroutine != null) StopCoroutine(_disableCoroutine);

            if (_myPool != null)
            {
                _myPool.Release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}