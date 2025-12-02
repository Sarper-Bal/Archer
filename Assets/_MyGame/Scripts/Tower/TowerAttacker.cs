using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using IndianOceanAssets.Engine2_5D; 

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    public class TowerAttacker : MonoBehaviour
    {
        [Header("🔧 Sabit Parçalar")]
        [Tooltip("Merminin çıkacağı nokta. (Sabit)")]
        [SerializeField] private Transform _firePoint;  

        [Tooltip("Kulenin dönen kafası. (Sabit)")]
        [SerializeField] private Transform _partToRotate; 
        
        [Header("🎯 Hedef Ayarları")]
        [SerializeField] private LayerMask _enemyLayer; 

        // --- ÇALIŞMA DEĞİŞKENLERİ ---
        private WeaponDefinition _currentWeapon; 
        private Transform _currentTarget;
        private float _nextAttackTime;
        private float _nextSearchTime;
        private float _sqrRange; 
        
        private readonly Collider[] _hitBuffer = new Collider[20];
        private const float SEARCH_INTERVAL = 0.2f;

        public void SetWeapon(WeaponDefinition weaponDef)
        {
            _currentWeapon = weaponDef;
            
            if (_currentWeapon != null)
            {
                _sqrRange = _currentWeapon.Range * _currentWeapon.Range;
                // Silah değiştiğinde bekleme süresini sıfırlama, hemen ateş edebilir
            }
        }

        private void Update()
        {
            if (_currentWeapon == null) return;

            if (IsTargetInvalid())
            {
                if (Time.time >= _nextSearchTime)
                {
                    FindClosestEnemy();
                    _nextSearchTime = Time.time + SEARCH_INTERVAL;
                }
            }
            else 
            {
                float distSqr = (transform.position - _currentTarget.position).sqrMagnitude;
                
                if (distSqr > _sqrRange)
                {
                    _currentTarget = null;
                    return;
                }

                RotatePartToTarget();

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
            Vector3 center = _firePoint != null ? _firePoint.position : transform.position;
            int hitCount = Physics.OverlapSphereNonAlloc(center, _currentWeapon.Range, _hitBuffer, _enemyLayer);
            
            Transform closestEnemy = null;
            float closestDistanceSqr = Mathf.Infinity;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];
                if (hit != null && hit.gameObject.activeInHierarchy && hit.CompareTag("Enemy"))
                {
                    float dSqrToTarget = (hit.transform.position - center).sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        closestEnemy = hit.transform;
                    }
                }
            }
            _currentTarget = closestEnemy;
        }

        private void RotatePartToTarget()
        {
            // Dönecek parça yoksa işlem yapma (Sabit Kule)
            if (_currentTarget == null || _partToRotate == null) return;

            Vector3 direction = (_currentTarget.position - _partToRotate.position).normalized;
            direction.y = 0; 

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                float rotSpeed = _currentWeapon.RotationSpeed > 0 ? _currentWeapon.RotationSpeed : 10f;
                _partToRotate.rotation = Quaternion.Slerp(_partToRotate.rotation, lookRotation, Time.deltaTime * rotSpeed);
            }
        }

        private void Attack()
        {
            if (_currentWeapon.ProjectilePool == null) return;

            BasicProjectile projectile = _currentWeapon.ProjectilePool.Get();
            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
            
            projectile.transform.position = spawnPos;
            projectile.transform.rotation = Quaternion.LookRotation(_currentTarget.position - spawnPos);
            
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