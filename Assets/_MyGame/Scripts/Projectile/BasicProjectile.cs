using UnityEngine;
using System.Collections;
using ArcadeBridge.ArcadeIdleEngine.Pools;

namespace IndianOceanAssets.Engine2_5D
{
    public enum ProjectileTrackingMode
    {
        Guided, // Hedefi takip eder
        Linear  // Dümdüz gider (Doğrusal)
    }

    public class BasicProjectile : MonoBehaviour
    {
        [Header("Hareket Ayarları")]
        [SerializeField] private ProjectileTrackingMode _trackingMode = ProjectileTrackingMode.Guided;
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 5f;

        [Header("Efektler")]
        [SerializeField] private ExplosionPool explosionPool; // Patlama Efekti Havuzu
        [SerializeField] private float explosionRadius = 2f;  // Alan hasarı çapı
        [SerializeField] private LayerMask damageLayer;       // Kim hasar alacak? (Enemy)

        private BasicProjectilePool _myPool;
        private Transform _target;
        private Vector3 _targetPosition;
        private float _currentDamage; // Silahdan gelen hasar bilgisi
        private Coroutine _lifeTimeCoroutine;

        // Bellek optimizasyonu (GC Free)
        private readonly Collider[] _explosionHits = new Collider[15];

        /// <summary>
        /// Mermiyi başlatır ve özelliklerini yükler.
        /// </summary>
        /// <param name="target">Hedef</param>
        /// <param name="pool">Merminin ait olduğu havuz</param>
        /// <param name="damage">Silahın hasar değeri</param>
        public void Initialize(Transform target, BasicProjectilePool pool, float damage)
        {
            _target = target;
            _myPool = pool;
            _currentDamage = damage; // Hasarı kaydet

            // Hedef konum belirleme
            if (_target != null)
            {
                _targetPosition = _target.position;
                
                // İlk yönlendirme
                Vector3 dir = (_targetPosition - transform.position).normalized;
                if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                // Hedefsiz atış (İleri git)
                _targetPosition = transform.position + transform.forward * 50f;
            }

            // Yaşam süresi sayacını başlat
            if (_lifeTimeCoroutine != null) StopCoroutine(_lifeTimeCoroutine);
            _lifeTimeCoroutine = StartCoroutine(LifeTimeRoutine());
        }

        private void Update()
        {
            // Guided Mod: Hedef hareket ederse rotayı güncelle
            if (_trackingMode == ProjectileTrackingMode.Guided && _target != null && _target.gameObject.activeInHierarchy)
            {
                _targetPosition = _target.position;
            }

            // Hareket
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);

            // Yönelme (Guided modda sürekli, Linear modda başta yapıldı)
            if (_trackingMode == ProjectileTrackingMode.Guided)
            {
                Vector3 dir = (_targetPosition - transform.position).normalized;
                if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
            }

            // Hedefe varış kontrolü
            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                Explode();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Düşmana veya duvara çarpınca patla
            if (((1 << other.gameObject.layer) & damageLayer) != 0 || other.gameObject.layer == 0) // Layer check + Default layer
            {
                Explode();
            }
        }

        private void Explode()
        {
            // 1. Görsel Efekt (Pool'dan çek)
            if (explosionPool != null)
            {
                var effect = explosionPool.Get();
                effect.transform.position = transform.position;
                effect.transform.rotation = Quaternion.identity;
                effect.Initialize(explosionPool);
            }

            // 2. Alan Hasarı (AOE)
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, _explosionHits, damageLayer);

            for (int i = 0; i < hitCount; i++)
            {
                Collider victim = _explosionHits[i];
                
                // IDamageable arayüzünü ara ve hasar ver
                if (victim.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(_currentDamage);
                }
            }

            // 3. Mermiyi iade et
            ReturnToPool();
        }

        private IEnumerator LifeTimeRoutine()
        {
            yield return new WaitForSeconds(lifeTime);
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (_lifeTimeCoroutine != null) StopCoroutine(_lifeTimeCoroutine);

            if (_myPool != null) _myPool.Release(this);
            else gameObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0, 0, 0.4f);
            Gizmos.DrawSphere(transform.position, explosionRadius);
        }
    }
}