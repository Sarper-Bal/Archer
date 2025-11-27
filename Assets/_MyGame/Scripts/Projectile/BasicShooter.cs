using UnityEngine;

namespace IndianOceanAssets.Engine2_5D
{
    public class BasicShooter : MonoBehaviour
    {
        [Header("Pool Ayarları")]
        // --- DEĞİŞİKLİK: Prefab yerine Pool Verisi İstiyoruz ---
        // Buraya oluşturduğun "BasicProjectilePool" dosyasını sürükleyeceksin.
        [SerializeField] private BasicProjectilePool projectilePool; 
        
        [Header("Mermi Çıkış Noktası")]
        [SerializeField] private Transform firePoint; 
        
        [Header("Saldırı Ayarları")]
        [SerializeField] private float fireRate = 1f; 
        [SerializeField] private float range = 10f;   
        [SerializeField] private LayerMask enemyLayer; 

        private float _nextFireTime = 0f;

        // Performans: Her frame'de 'new WaitForSeconds' yapmamak için cache
        private readonly Collider[] _hitBuffer = new Collider[10]; // Aynı anda max 10 düşman algıla (GC Allocation önler)

        private void Update()
        {
            if (Time.time >= _nextFireTime)
            {
                Transform target = FindClosestEnemyOptimized();
                if (target != null)
                {
                    Shoot(target);
                    _nextFireTime = Time.time + (1f / fireRate);
                }
            }
        }

        private void Shoot(Transform target)
        {
            if (projectilePool == null)
            {
                Debug.LogError("BasicShooter: Projectile Pool atanmamış! Lütfen Inspector'dan atayın.");
                return;
            }

            // --- DEĞİŞİKLİK: Instantiate Yerine Pool.Get() ---
            // 1. Havuzdan pasif bir mermi çek (veya yoksa yenisini yaratır)
            BasicProjectile projectile = projectilePool.Get();
            
            // 2. Pozisyonu ve açıyı ayarla
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = Quaternion.identity;

            // 3. Mermiyi başlat ve havuz referansını gönder (ki geri dönebilsin)
            projectile.Initialize(target, projectilePool);
        }

        // --- DEĞİŞİKLİK: NonAlloc Kullanımı (Sıfır Çöp Üretimi) ---
        private Transform FindClosestEnemyOptimized()
        {
            // Physics.OverlapSphere yerine NonAlloc kullanarak RAM şişmesini önlüyoruz.
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, range, _hitBuffer, enemyLayer);
            
            Transform bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];
                
                // Null kontrolü ve Tag kontrolü
                if (hit != null && hit.CompareTag("Enemy"))
                {
                    Vector3 directionToTarget = hit.transform.position - currentPos;
                    float dSqrToTarget = directionToTarget.sqrMagnitude; // Distance yerine sqrMagnitude daha hızlıdır (karekök almaz)
                    
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        bestTarget = hit.transform;
                    }
                }
            }
            return bestTarget;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}