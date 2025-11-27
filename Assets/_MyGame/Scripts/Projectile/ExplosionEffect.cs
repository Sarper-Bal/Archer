using UnityEngine;
using System.Collections;

namespace IndianOceanAssets.Engine2_5D
{
    public class ExplosionEffect : MonoBehaviour
    {
        [Header("Ayarlar")]
        [SerializeField] private float _duration = 2f; // Efektin süresi (Particle System süresiyle uyumlu olmalı)
        
        private ExplosionPool _myPool;
        private Coroutine _disableCoroutine;

        /// <summary>
        /// Efekti başlatır ve süre bitince havuza geri gönderir.
        /// </summary>
        public void Initialize(ExplosionPool pool)
        {
            _myPool = pool;
            
            // Eğer önceden çalışıyorsa durdur (güvenlik için)
            if (_disableCoroutine != null) StopCoroutine(_disableCoroutine);
            
            // Otomatik kapanma sayacını başlat
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

            // Havuz referansı varsa oraya dön, yoksa (test için koyduysan) pasif ol.
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