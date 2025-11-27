using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools; // Havuz sistemi namespace'i
using System.Collections;

namespace IndianOceanAssets.Engine2_5D
{
    public class SimpleAutoAttacker : MonoBehaviour
    {
        [Header("Havuz Ayarları")]
        [Tooltip("Project panelinde oluşturduğun 'BasicProjectilePool' dosyasını buraya sürükle.")]
        [SerializeField] private BasicProjectilePool _projectilePool;

        [Header("Saldırı Ayarları")]
        [SerializeField] private float _range = 10f;          // Saldırı Menzili
        [SerializeField] private float _attacksPerSecond = 1f; // Saniyedeki Saldırı Hızı
        [SerializeField] private float _rotationSpeed = 10f;   // Dönüş Hızı
        [SerializeField] private Transform _firePoint;         // Ok Çıkış Noktası
        
        [Header("Hedefleme (Layer)")]
        [Tooltip("Sadece 'Enemy' katmanını seçmelisin.")]
        [SerializeField] private LayerMask _enemyLayer;        

        // Private Değişkenler
        private Transform _currentTarget;       
        private float _lastAttackTime;          
        
        // Bellek optimizasyonu için önbellekli dizi (Çöp oluşturmaz)
        private readonly Collider[] _hitBuffer = new Collider[20]; 

        private void Update()
        {
            // 1. Hedef kontrolü: Hedef yoksa veya öldüyse/pasif olduysa yeni hedef ara
            if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
            {
                FindClosestEnemyByLayer();
            }

            // 2. Hedef varsa saldırı moduna geç
            if (_currentTarget != null)
            {
                // Hedef menzilden çıktı mı?
                float distance = Vector3.Distance(transform.position, _currentTarget.position);
                if (distance > _range)
                {
                    _currentTarget = null; // Hedefi bırak
                    return;
                }

                // Karaktere hedefe dönme komutu ver
                RotateTowardsTarget();

                // Saldırı sıklığını kontrol et
                if (Time.time >= _lastAttackTime + (1f / _attacksPerSecond))
                {
                    Attack();
                    _lastAttackTime = Time.time;
                }
            }
        }

        // --- LAYER TABANLI ARAMA (EN OPTİMİZE YÖNTEM) ---
        private void FindClosestEnemyByLayer()
        {
            // Sadece _enemyLayer katmanındaki objeleri tarar. Diğer her şeyi (duvar, zemin) yok sayar.
            // Bu işlemciyi %90 oranında rahatlatır.
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _range, _hitBuffer, _enemyLayer);

            Transform closestEnemy = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];

                if (hit != null && hit.gameObject.activeInHierarchy)
                {
                    // Ekstra güvenlik: Layer doğru olsa bile Tag'i de kontrol edelim
                    // (Eğer Layer'ı yanlışlıkla başka objeye verdiysen hata olmasın diye)
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
            direction.y = 0; // Sadece kendi ekseninde dön (yukarı bakmasın)

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _rotationSpeed);
            }
        }

        private void Attack()
        {
            if (_projectilePool == null) 
            {
                Debug.LogError("Projectile Pool atanmamış!");
                return;
            }

            // Havuzdan Mermi Çek
            BasicProjectile projectile = _projectilePool.Get();
            
            // Konum ve Yön Ata
            projectile.transform.position = _firePoint.position;
            projectile.transform.rotation = Quaternion.LookRotation(_currentTarget.position - _firePoint.position);

            // Mermiyi Başlat
            projectile.Initialize(_currentTarget, _projectilePool);
        }

        // Editörde menzili görmek için
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _range);
        }
    }
}