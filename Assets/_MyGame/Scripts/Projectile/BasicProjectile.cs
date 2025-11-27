using UnityEngine;
using System.Collections;
using ArcadeBridge.ArcadeIdleEngine.Pools;

namespace IndianOceanAssets.Engine2_5D
{
    // Mermi Davranış Tipleri
    public enum ProjectileTrackingMode
    {
        Guided, // (Güdümlü) Hedefi kovalar
        Linear  // (Havan Topu/Büyü Gibi) Belirlenen noktaya gider ve patlar
    }

    public class BasicProjectile : MonoBehaviour
    {
        [Header("Hareket Ayarları")]
        [Tooltip("Guided: Hedefi takip eder. Linear: Hedefin ilk konumuna gider.")]
        [SerializeField] private ProjectileTrackingMode _trackingMode = ProjectileTrackingMode.Guided;
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 5f;

        [Header("Patlama & Hasar (AoE)")]
        // --- DEĞİŞİKLİK: Prefab yerine Pool Verisi ---
        [Tooltip("Inspector'da oluşturduğun 'ExplosionPool' dosyasını buraya sürükle.")]
        [SerializeField] private ExplosionPool explosionPool; 
        
        [SerializeField] private float explosionRadius = 3f; // Patlama Yarıçapı
        [SerializeField] private float damageAmount = 10f;   // Verilecek Hasar
        [SerializeField] private LayerMask damageLayer;      // Kimler hasar alacak? (Enemy)

        private BasicProjectilePool _myPool;
        private Transform _target;
        private Vector3 _targetPosition;
        private Coroutine _lifeTimeCoroutine;

        // Performans için önbellekli dizi
        private readonly Collider[] _explosionHits = new Collider[20]; 

        public void Initialize(Transform target, BasicProjectilePool pool)
        {
            _target = target;
            _myPool = pool;

            // Hedef Noktayı Belirle
            if (_target != null)
            {
                _targetPosition = _target.position;
                
                Vector3 dir = (_targetPosition - transform.position).normalized;
                if(dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                _targetPosition = transform.position + transform.forward * 50f;
            }

            if (_lifeTimeCoroutine != null) StopCoroutine(_lifeTimeCoroutine);
            _lifeTimeCoroutine = StartCoroutine(LifeTimeRoutine());
        }

        private void Update()
        {
            if (_trackingMode == ProjectileTrackingMode.Guided)
            {
                if (_target != null && _target.gameObject.activeInHierarchy)
                {
                    _targetPosition = _target.position;
                }
            }
            
            // Hedefe İlerle
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);

            if (_trackingMode == ProjectileTrackingMode.Guided)
            {
                Vector3 dir = (_targetPosition - transform.position).normalized;
                if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
            }

            // Hedefe Vardık mı?
            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                Explode();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                Explode();
            }
        }

        // --- PATLAMA VE HASAR SİSTEMİ (GÜNCELLENDİ) ---
       // --- BasicProjectile.cs İçindeki Explode Fonksiyonu ---

private void Explode()
{
    // 1. Görsel Efekt
    if (explosionPool != null)
    {
        var effect = explosionPool.Get();
        effect.transform.position = transform.position;
        effect.transform.rotation = Quaternion.identity;
        
        // HATA ALMAMAK İÇİN: effect.Initialize parametresine dikkat et.
        // Eğer ExplosionEffect.cs'yi güncellemediysen burası hata verebilir.
        // Şimdilik varsayalım ki ExplosionPool kullanıyorsun.
        effect.Initialize(explosionPool); 
    }

    // 2. Alan Hasarı (Health System Entegrasyonu)
    int hitCount = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, _explosionHits, damageLayer);

    for (int i = 0; i < hitCount; i++)
    {
        Collider victim = _explosionHits[i];
        
        // --- DEĞİŞİKLİK BURADA ---
        // Çarptığımız objede IDamageable (Health) var mı?
        if (victim.TryGetComponent(out IDamageable damageable))
{
    damageable.TakeDamage(damageAmount);
}
    }

    // 3. Mermiyi Kaldır
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
            
            if (_myPool != null)
            {
                _myPool.Release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, explosionRadius);
        }
    }
}