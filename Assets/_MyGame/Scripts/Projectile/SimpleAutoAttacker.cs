using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;

namespace IndianOceanAssets.Engine2_5D
{
    public class SimpleAutoAttacker : MonoBehaviour
    {
        [Header("Silah Kurulumu")]
        [Tooltip("Veri klasöründe oluşturduğun Silah Verisini (ScriptableObject) buraya sürükle.")]
        [SerializeField] private WeaponDefinition _equippedWeapon;

        [Header("Bağlantılar")]
        [SerializeField] private Transform _firePoint;  // Okun çıkacağı nokta
        [SerializeField] private LayerMask _enemyLayer; // Düşman katmanı (Sadece Enemy seçili olsun)

        // Private Değişkenler
        private Transform _currentTarget;
        private float _nextAttackTime;
        private readonly Collider[] _hitBuffer = new Collider[20]; // Bellek dostu arama

        private void Update()
        {
            // Eğer silah takılı değilse hiçbir şey yapma
            if (_equippedWeapon == null) return;

            // 1. Hedef Kontrolü: Hedef yoksa veya öldüyse yenisini ara
            if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
            {
                FindClosestEnemyDataDriven();
            }

            // 2. Saldırı Döngüsü
            if (_currentTarget != null)
            {
                // Menzil Kontrolü (Veri dosyasından okunuyor)
                float distance = Vector3.Distance(transform.position, _currentTarget.position);
                if (distance > _equippedWeapon.Range)
                {
                    _currentTarget = null; // Hedef çok uzaklaştı, bırak
                    return;
                }

                // Hedefe Dön
                RotateTowardsTarget();

                // Saldırı Hızı Kontrolü (Veri dosyasından okunuyor)
                if (Time.time >= _nextAttackTime)
                {
                    Attack();
                    _nextAttackTime = Time.time + _equippedWeapon.AttackInterval;
                }
            }
        }

        private void FindClosestEnemyDataDriven()
        {
            if (_equippedWeapon == null) return;

            // Physics.OverlapSphereNonAlloc ile optimize arama (Menzil verisi silahtan geliyor)
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _equippedWeapon.Range, _hitBuffer, _enemyLayer);

            Transform closestEnemy = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];
                if (hit != null && hit.gameObject.activeInHierarchy)
                {
                    // Ekstra güvenlik: Tag kontrolü
                    if (hit.CompareTag("Enemy"))
                    {
                        Vector3 directionToTarget = hit.transform.position - currentPos;
                        float dSqrToTarget = directionToTarget.sqrMagnitude;

                        if (dSqrToTarget < closestDistanceSqr)
                        {
                            closestDistanceSqr = dSqrToTarget;
                            closestEnemy = hit.transform;
                        }
                    }
                }
            }
            _currentTarget = closestEnemy;
        }

        private void RotateTowardsTarget()
        {
            if (_currentTarget == null) return;

            Vector3 direction = (_currentTarget.position - transform.position).normalized;
            direction.y = 0; // Sadece Y ekseninde dön (Karakter eğilmesin)

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                // Dönüş hızı silahtan geliyor (Ağır silahlar yavaş dönebilir)
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _equippedWeapon.RotationSpeed);
            }
        }

        private void Attack()
        {
            if (_equippedWeapon.ProjectilePool == null)
            {
                Debug.LogError($"HATA: '{_equippedWeapon.name}' adlı silah verisinde Mermi Havuzu (Projectile Pool) atanmamış!");
                return;
            }

            // 1. Havuzdan mermi çek (Verideki havuzu kullan)
            BasicProjectile projectile = _equippedWeapon.ProjectilePool.Get();
            
            // 2. Pozisyon ve Açı
            projectile.transform.position = _firePoint.position;
            projectile.transform.rotation = Quaternion.LookRotation(_currentTarget.position - _firePoint.position);

            // 3. Mermiyi Başlat (Hasar verisini de gönder)
            projectile.Initialize(_currentTarget, _equippedWeapon.ProjectilePool, _equippedWeapon.Damage);
        }

        // Editörde menzili görmek için
        private void OnDrawGizmosSelected()
        {
            if (_equippedWeapon != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, _equippedWeapon.Range);
            }
        }
    }
}