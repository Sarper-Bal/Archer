using UnityEngine;
using System.Collections; // Coroutine için gerekli

namespace IndianOceanAssets.Engine2_5D
{
    public class BasicProjectile : MonoBehaviour
    {
        [Header("Hareket Ayarları")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 3f; // Havada asılı kalma süresi
        
        [Header("Görsel Efektler")]
        [SerializeField] private ParticleSystem trailEffect; // Varsa arkasındaki iz efekti
        [SerializeField] private GameObject explosionPrefab; // Vuruş efekti

        // --- DEĞİŞİKLİK: Havuz Referansı ---
        // Mermi hangi havuzdan geldiğini bilmeli ki oraya dönebilsin.
        private BasicProjectilePool _myPool;
        
        private Transform _target;
        private Vector3 _lastTargetPos;
        private Coroutine _lifeTimeCoroutine;

        // --- DEĞİŞİKLİK: Initialize Metodu Güncellendi ---
        /// <summary>
        /// Mermiyi fırlatıldığında sıfırlar ve başlatır.
        /// </summary>
        /// <param name="targetTransform">Gidilecek hedef</param>
        /// <param name="pool">Merminin ait olduğu havuz</param>
        public void Initialize(Transform targetTransform, BasicProjectilePool pool)
        {
            _target = targetTransform;
            _myPool = pool; // Havuzu kaydet

            // Hedefin konumunu al (hedef null gelse bile hata vermesin)
            if (_target != null) _lastTargetPos = _target.position;

            // İz efektini temizle (Eğer varsa)
            if (trailEffect != null) trailEffect.Clear();

            // Ömür sayacını başlat (Destroy(lifeTime) yerine manuel sayaç)
            if (_lifeTimeCoroutine != null) StopCoroutine(_lifeTimeCoroutine);
            _lifeTimeCoroutine = StartCoroutine(LifeTimeRoutine());
        }

        private void Update()
        {
            // Hedef yaşıyorsa pozisyonunu güncelle
            if (_target != null)
            {
                _lastTargetPos = _target.position;
            }

            // Hedefe veya son görülen konuma doğru git
            Vector3 direction = (_lastTargetPos - transform.position).normalized;
            
            // Sıfır vektör hatasını önle
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
                transform.position += direction * speed * Time.deltaTime;
            }

            // Basit mesafe kontrolü (Çok hızlı mermilerde Raycast kullanılmalı)
            float distanceToTarget = Vector3.Distance(transform.position, _lastTargetPos);
            if (distanceToTarget < 0.5f)
            {
                HitTarget();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Çarpışma ile vuruş
            if (other.CompareTag("Enemy"))
            {
                HitTarget();
            }
        }

        private void HitTarget()
        {
            // Vurulan yerde efekt patlat
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // --- DEĞİŞİKLİK: Yok Etme Yerine İade Etme ---
            ReturnToPool();
        }

        // Mermi hiçbir şeye çarpmadan süre dolarsa çalışır
        private IEnumerator LifeTimeRoutine()
        {
            yield return new WaitForSeconds(lifeTime);
            ReturnToPool();
        }

        // Havuza güvenli dönüş fonksiyonu
        private void ReturnToPool()
        {
            if (_lifeTimeCoroutine != null) StopCoroutine(_lifeTimeCoroutine);

            // Eğer havuz referansı varsa oraya dön, yoksa (test amaçlı koyduysan) yok et.
            if (_myPool != null)
            {
                _myPool.Release(this);
            }
            else
            {
                gameObject.SetActive(false); // Veya Destroy(gameObject);
            }
        }
    }
}