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

        // --- DEĞİŞİKLİK 1: Rigidbody Referansı ---
        private Rigidbody _rb; 
        
        private Transform _currentTarget;
        private float _nextAttackTime;
        private readonly Collider[] _hitBuffer = new Collider[20];
        private bool _isMoving;

        private void Awake()
        {
            // Karakterin fizik bileşenini alıyoruz (En güvenilir hareket kaynağı)
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            if (_equippedWeapon != null)
            {
                var health = GetComponent<Health>();
                if (health != null)
                {
                    health.InitializeHealth(_equippedWeapon.PlayerMaxHealth);
                }
            }
        }

        private void Update()
        {
            if (_equippedWeapon == null) return;

            // --- DEĞİŞİKLİK 2: Kesin Hareket Kontrolü ---
            CheckMovementPhysics();

            // Hedef Kontrolü
            if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
            {
                FindClosestEnemyDataDriven();
            }

            // Saldırı Döngüsü
            if (_currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, _currentTarget.position);
                if (distance > _equippedWeapon.Range)
                {
                    _currentTarget = null;
                    return;
                }

                // Karakteri hedefe döndür (Ancak hareket ediyorsak Mover scripti baskın çıkabilir)
                RotateTowardsTarget();

                // --- DEĞİŞİKLİK 3: Atış İzni Kontrolü ---
                // Eğer silah "Hareketliyken Ateş Yasak" diyorsa VE karakter hareket ediyorsa: İPTAL ET.
                if (!_equippedWeapon.CanFireWhileMoving && _isMoving)
                {
                    return; // Buradan geri dön, aşağıdaki Attack() çalışmasın.
                }

                // Zamanı geldiyse ateş et
                if (Time.time >= _nextAttackTime)
                {
                    Attack();
                    _nextAttackTime = Time.time + _equippedWeapon.AttackInterval;
                }
            }
        }

        /// <summary>
        /// Fizik motoruna karakterin hızını sorar. En hatasız yöntemdir.
        /// </summary>
        private void CheckMovementPhysics()
        {
            if (_rb != null)
            {
                // Unity 6 (linearVelocity) kullanıyorsun, bu yüzden uyumlu yazdım.
                // Eğer eski sürümde hata verirse 'linearVelocity' yerine 'velocity' yaz.
                // 0.1f küçük bir tolerans payıdır (titremeleri önler).
                _isMoving = _rb.linearVelocity.sqrMagnitude > 0.1f; 
            }
            else
            {
                // Yedek plan: Rigidbody yoksa eski yöntemi kullan
                // (Ama senin projende ArcadeIdleMover olduğu için Rigidbody kesin vardır)
                _isMoving = false; 
            }
        }

        // ... (Geri kalan kodlar AYNI kalacak: FindClosestEnemy, Rotate, Attack) ...
        
        private void FindClosestEnemyDataDriven()
        {
            if (_equippedWeapon == null) return;

            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _equippedWeapon.Range, _hitBuffer, _enemyLayer);
            Transform closestEnemy = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];
                if (hit != null && hit.gameObject.activeInHierarchy && hit.CompareTag("Enemy"))
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
            _currentTarget = closestEnemy;
        }

        private void RotateTowardsTarget()
        {
            // Eğer hareket ediyorsak ve nişan almamız yasaksa, karakterin hedefe dönmesini de engelleyebiliriz.
            // Ama genelde koşarken de kafasını çevirmesi daha doğal durur. O yüzden burayı ellemiyoruz.
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