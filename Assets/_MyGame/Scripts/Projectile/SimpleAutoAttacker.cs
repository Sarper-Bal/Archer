using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools; // Havuz sistemi için
using System.Collections;

namespace IndianOceanAssets.Engine2_5D
{
    public class SimpleAutoAttacker : MonoBehaviour
    {
        [Header("Havuz Ayarları")]
        [Tooltip("Inspector'da oluşturduğun Mermi Havuzunu (Data) buraya sürükle")]
        [SerializeField] private BasicProjectilePool _projectilePool;

        [Header("Saldırı Ayarları")]
        [SerializeField] private float _range = 10f;          // Saldırı Menzili
        [SerializeField] private float _attacksPerSecond = 1f; // Saniyedeki Saldırı Hızı
        [SerializeField] private float _rotationSpeed = 10f;   // Düşmana dönme hızı
        [SerializeField] private Transform _firePoint;         // Merminin çıkış noktası
        
        [Header("Hedefleme Ayarları")]
        [SerializeField] private LayerMask _enemyLayer;        // Sadece düşmanları gör

        // Private Değişkenler
        private Transform _currentTarget;       // Şu anki kilitli hedef
        private float _lastAttackTime;          // Son saldırı zamanı
        private Collider[] _hitBuffer = new Collider[20]; // Bellek dostu arama deposu (NonAlloc)

        private void Update()
        {
            // 1. Eğer bir hedefimiz yoksa veya hedef öldüyse/yok olduysa yeni hedef ara
            if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
            {
                FindClosestEnemy();
            }

            // 2. Eğer geçerli bir hedefimiz varsa saldırı mantığını çalıştır
            if (_currentTarget != null)
            {
                // Menzil Kontrolü: Hedef menzilden çıktı mı?
                float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
                if (distanceToTarget > _range)
                {
                    _currentTarget = null; // Hedefi bırak
                    return;
                }

                // Düşmana Doğru Dön (Yumuşak Dönüş)
                RotateTowardsTarget();

                // Ateş Etme Zamanlaması
                if (Time.time >= _lastAttackTime + (1f / _attacksPerSecond))
                {
                    Attack();
                    _lastAttackTime = Time.time;
                }
            }
        }

        // En Optimize Arama Fonksiyonu
        private void FindClosestEnemy()
        {
            // Physics.OverlapSphereNonAlloc çöp (garbage) üretmez, hafıza dostudur.
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _range, _hitBuffer, _enemyLayer);

            Transform closestEnemy = null;
            float closestDistanceSqr = Mathf.Infinity; // Sonsuz uzaklık
            Vector3 currentPos = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];

                // Eğer nesne aktifse ve "Enemy" etiketine sahipse
                if (hit != null && hit.gameObject.activeInHierarchy && hit.CompareTag("Enemy"))
                {
                    // Uzaklığı hesapla (Karekök almadan işlem yapmak daha hızlıdır: sqrMagnitude)
                    Vector3 directionToTarget = hit.transform.position - currentPos;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;

                    // En yakınını bul
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        closestEnemy = hit.transform;
                    }
                }
            }

            // En yakın düşmanı hedef olarak belirle
            _currentTarget = closestEnemy;
        }

        private void RotateTowardsTarget()
        {
            if (_currentTarget == null) return;

            Vector3 direction = (_currentTarget.position - transform.position).normalized;
            direction.y = 0; // Karakterin yukarı/aşağı eğilmesini engelle (Sadece Y ekseninde dön)

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
                Debug.LogError("HATA: Inspector panelinden Projectile Pool atanmadı!");
                return;
            }

            // Havuzdan Mermi Çek
            BasicProjectile projectile = _projectilePool.Get();
            
            // Pozisyonu ayarla
            projectile.transform.position = _firePoint.position;
            projectile.transform.rotation = Quaternion.LookRotation(_currentTarget.position - _firePoint.position);

            // Mermiyi Başlat (Daha önce yazdığımız fonksiyonu kullanıyoruz)
            projectile.Initialize(_currentTarget, _projectilePool);
        }

        // Editörde Menzili Çiz (Debug İçin)
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _range);
        }
    }
}