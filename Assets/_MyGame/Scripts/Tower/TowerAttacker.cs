using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using IndianOceanAssets.Engine2_5D; // WeaponDefinition için

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    public class TowerAttacker : MonoBehaviour
    {
        [Header("Bağlantılar")]
        [SerializeField] private Transform _firePoint;  
        [SerializeField] private LayerMask _enemyLayer; 

        // --- ÇALIŞMA DEĞİŞKENLERİ ---
        private WeaponDefinition _currentWeapon; // O anki silah verisi
        private Transform _currentTarget;
        private float _nextAttackTime;
        private float _nextSearchTime;
        private float _sqrRange; // Menzil karesi (Optimizasyon)
        
        // NonAlloc Optimizasyonu
        private readonly Collider[] _hitBuffer = new Collider[20];
        private const float SEARCH_INTERVAL = 0.2f;

        // --- KURULUM (TowerManager Çağıracak) ---
        public void SetWeapon(WeaponDefinition weaponDef)
        {
            _currentWeapon = weaponDef;
            
            if (_currentWeapon != null)
            {
                // Menzil karesini hesapla (Karekök işleminden kaçmak için)
                _sqrRange = _currentWeapon.Range * _currentWeapon.Range;
                _nextAttackTime = 0; // Silah değişince hemen ateş edebilsin
            }
        }

        private void Update()
        {
            if (_currentWeapon == null) return;

            // 1. Hedef Kontrolü
            if (IsTargetInvalid())
            {
                if (Time.time >= _nextSearchTime)
                {
                    FindClosestEnemy();
                    _nextSearchTime = Time.time + SEARCH_INTERVAL;
                }
            }
            // 2. Saldırı Döngüsü
            else 
            {
                // Menzil Kontrolü (sqrMagnitude ile)
                float distSqr = (transform.position - _currentTarget.position).sqrMagnitude;
                
                if (distSqr > _sqrRange)
                {
                    _currentTarget = null;
                    return;
                }

                RotateTowardsTarget();

                if (Time.time >= _nextAttackTime)
                {
                    Attack();
                    _nextAttackTime = Time.time + _currentWeapon.AttackInterval;
                }
            }
        }
        
        private bool IsTargetInvalid()
        {
            return _currentTarget == null || !_currentTarget.gameObject.activeInHierarchy;
        }

        private void FindClosestEnemy()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _currentWeapon.Range, _hitBuffer, _enemyLayer);
            
            Transform closestEnemy = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];
                if (hit != null && hit.gameObject.activeInHierarchy && hit.CompareTag("Enemy"))
                {
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
                // Silahın kendi dönüş hızı varsa onu kullan, yoksa sabit 10f
                float rotSpeed = _currentWeapon.RotationSpeed > 0 ? _currentWeapon.RotationSpeed : 10f;
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotSpeed);
            }
        }

        private void Attack()
        {
            if (_currentWeapon.ProjectilePool == null) return;

            BasicProjectile projectile = _currentWeapon.ProjectilePool.Get();
            projectile.transform.position = _firePoint.position;
            projectile.transform.rotation = Quaternion.LookRotation(_currentTarget.position - _firePoint.position);
            
            // Mermiyi fırlat
            projectile.Initialize(_currentTarget, _currentWeapon.ProjectilePool, _currentWeapon);
        }

        private void OnDrawGizmosSelected()
        {
            if (_currentWeapon != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, _currentWeapon.Range);
            }
        }
    }
}