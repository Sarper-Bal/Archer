using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;

namespace IndianOceanAssets.Engine2_5D
{
    public class SimpleAutoAttacker : MonoBehaviour
    {
        [Header("Silah Kurulumu")]
        [SerializeField] private WeaponDefinition _equippedWeapon;

        [Header("Bağlantılar")]
        [SerializeField] private Transform _firePoint;  
        [SerializeField] private LayerMask _enemyLayer; 

        // --- OPTİMİZASYON AYARLARI ---
        private const float SEARCH_INTERVAL = 0.2f; // Hedef arama sıklığı (Saniyede 5 kere)

        private Rigidbody _rb; 
        private Transform _currentTarget;
        private float _nextAttackTime;
        private float _nextSearchTime; // Arama zamanlayıcısı
        private readonly Collider[] _hitBuffer = new Collider[20];
        private bool _isMoving;
        
        // Optimizasyon için önbelleklenmiş değerler
        private float _sqrRange; // Menzilin karesi (Karekök işleminden kaçmak için)

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            if (_equippedWeapon != null)
            {
                var health = GetComponent<Health>();
                if (health != null) health.InitializeHealth(_equippedWeapon.PlayerMaxHealth);

                // Menzilin karesini bir kere hesaplayıp saklıyoruz
                // Örn: Menzil 10 ise, SqrRange 100 olur.
                _sqrRange = _equippedWeapon.Range * _equippedWeapon.Range;
            }
        }

        private void Update()
        {
            if (_equippedWeapon == null) return;

            // 1. Hareket Kontrolü (Çok ucuz işlem, her frame yapılabilir)
            CheckMovementPhysics();

            // 2. Hedef Kontrolü (Optimize Edildi)
            if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
            {
                // Her karede değil, sadece belirli aralıklarla ara
                if (Time.time >= _nextSearchTime)
                {
                    FindClosestEnemyDataDriven();
                    _nextSearchTime = Time.time + SEARCH_INTERVAL;
                }
            }
            // 3. Saldırı Döngüsü
            else 
            {
                // --- OPTİMİZASYON: Distance yerine sqrMagnitude ---
                float distSqr = (transform.position - _currentTarget.position).sqrMagnitude;
                
                // Menzil dışına çıktı mı? (100 > 100 karşılaştırması yapar, kök almaz)
                if (distSqr > _sqrRange)
                {
                    _currentTarget = null;
                    return;
                }

                RotateTowardsTarget();

                // Hareket halindeyken ateş etme kontrolü
                if (!_equippedWeapon.CanFireWhileMoving && _isMoving)
                {
                    return; 
                }

                if (Time.time >= _nextAttackTime)
                {
                    Attack();
                    _nextAttackTime = Time.time + _equippedWeapon.AttackInterval;
                }
            }
        }

        private void CheckMovementPhysics()
        {
            if (_rb != null)
            {
                // Sadece hızı okuyoruz, işlemciye yükü yok denecek kadar azdır.
                // Unity 6 için: linearVelocity, Eski sürümler için: velocity
                #if UNITY_6000_0_OR_NEWER
                _isMoving = _rb.linearVelocity.sqrMagnitude > 0.01f;
                #else
                _isMoving = _rb.velocity.sqrMagnitude > 0.01f;
                #endif
            }
        }
        
        private void FindClosestEnemyDataDriven()
        {
            // NonAlloc kullanarak çöp (Garbage) oluşturmuyoruz
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _equippedWeapon.Range, _hitBuffer, _enemyLayer);
            
            Transform closestEnemy = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];
                if (hit != null && hit.gameObject.activeInHierarchy && hit.CompareTag("Enemy"))
                {
                    // Zaten elimizde hazır "Squared Magnitude" var, direkt kıyaslıyoruz
                    float dSqrToTarget = (hit.transform.position - currentPos).sqrMagnitude;

                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        closestEnemy = hit.transform;
                    }
                }
            }
            _currentTarget = closestEnemy;
        }

        private void RotateTowardsTarget()
        {
            if (_currentTarget == null) return;

            Vector3 direction = (_currentTarget.position - transform.position).normalized;
            direction.y = 0; 

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _equippedWeapon.RotationSpeed);
            }
        }

        private void Attack()
        {
            if (_equippedWeapon.ProjectilePool == null) return;

            BasicProjectile projectile = _equippedWeapon.ProjectilePool.Get();
            projectile.transform.position = _firePoint.position;
            projectile.transform.rotation = Quaternion.LookRotation(_currentTarget.position - _firePoint.position);
            
            // Veri odaklı başlatma
            projectile.Initialize(_currentTarget, _equippedWeapon.ProjectilePool, _equippedWeapon);
        }

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