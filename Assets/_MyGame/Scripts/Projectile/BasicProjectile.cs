using UnityEngine;
using System.Collections;
using ArcadeBridge.ArcadeIdleEngine.Pools; // Namespace'in doğru olduğundan emin ol

namespace IndianOceanAssets.Engine2_5D
{
    // Mod Seçenekleri
    public enum ProjectileTrackingMode
    {
        Guided, // (Akıllı) Sürekli hedefi takip eder
        Linear  // (Akılsız) İlk atıldığı yöne dümdüz gider
    }

    public class BasicProjectile : MonoBehaviour
    {
        [Header("Hareket Tipi")]
        [Tooltip("Guided: Hedefi takip eder. Linear: Dümdüz gider.")]
        [SerializeField] private ProjectileTrackingMode _trackingMode = ProjectileTrackingMode.Guided;

        [Header("Ayarlar")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private GameObject explosionPrefab;

        private BasicProjectilePool _myPool;
        private Transform _target;
        private Vector3 _lastTargetPos;
        private Coroutine _lifeTimeCoroutine;

        // Linear mod için kilitli yön
        private Vector3 _cachedDirection; 

        public void Initialize(Transform target, BasicProjectilePool pool)
        {
            _target = target;
            _myPool = pool;

            // İlk Pozisyon Kaydı
            if (_target != null) 
            {
                _lastTargetPos = _target.position;
                
                // Linear mod için yönü en başta hesapla ve hafızaya al
                if (_trackingMode == ProjectileTrackingMode.Linear)
                {
                    // (Hedef - Ben) işlemi yönü verir
                    _cachedDirection = (_target.position - transform.position).normalized;
                    
                    // Merminin ucunu o yöne çevir
                    if (_cachedDirection != Vector3.zero)
                        transform.rotation = Quaternion.LookRotation(_cachedDirection);
                }
            }

            // Yaşam süresi sayacını başlat
            if (_lifeTimeCoroutine != null) StopCoroutine(_lifeTimeCoroutine);
            _lifeTimeCoroutine = StartCoroutine(LifeTimeRoutine());
        }

        private void Update()
        {
            // --- MODA GÖRE HAREKET MANTIĞI ---

            if (_trackingMode == ProjectileTrackingMode.Guided)
            {
                // AKILLI MERMİ: Her frame'de hedefi yeniden hesapla
                MoveGuided();
            }
            else
            {
                // AKILSIZ MERMİ: Sadece ileri git (En yüksek performans)
                MoveLinear();
            }

            // Çarpışma Kontrolü (Yedek) - Çok hızlı mermiler için
            // Linear modda hedef null olsa bile son noktaya gitme derdi olmadığı için 
            // sadece fiziksel çarpışma (OnTriggerEnter) yeterli olabilir ama bunu da tutuyoruz.
            if (_trackingMode == ProjectileTrackingMode.Guided && Vector3.Distance(transform.position, _lastTargetPos) < 0.5f)
            {
                HitTarget();
            }
        }

        // AKILLI HAREKET (Eski Sistem)
        private void MoveGuided()
        {
            if (_target != null)
            {
                _lastTargetPos = _target.position;
            }

            Vector3 dir = (_lastTargetPos - transform.position).normalized;
            
            if (dir != Vector3.zero)
            {
                // Yönü güncelle
                transform.rotation = Quaternion.LookRotation(dir);
                // İlerle
                transform.position += dir * speed * Time.deltaTime;
            }
        }

        // AKILSIZ HAREKET (Yeni Optimize Sistem)
        private void MoveLinear()
        {
            // Hiçbir hesap yapma, sadece baktığın yöne (forward) git.
            // Bu işlemci için toplama işleminden farksızdır, çok hızlıdır.
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Düşmana çarparsa patlat
            if (other.CompareTag("Enemy"))
            {
                HitTarget();
            }
            // Linear modda duvara çarpınca da yok olsun istersek:
            else if (_trackingMode == ProjectileTrackingMode.Linear && !other.CompareTag("Player"))
            {
                // Player hariç bir şeye çarparsa (duvar vs.) yok et
                ReturnToPool();
            }
        }

        private void HitTarget()
        {
            if (explosionPrefab) 
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
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
    }
}