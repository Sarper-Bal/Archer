using UnityEngine;
using System; 
using ArcadeBridge.ArcadeIdleEngine.Pools;
using IndianOceanAssets.Engine2_5D; 

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    public class TowerAttacker : MonoBehaviour
    {
        [Header("🎯 Hedef Ayarları")]
        [SerializeField] private LayerMask _enemyLayer; 

        // --- Event Sistemi ---
        public event Action OnFired;

        // --- Değişkenler ---
        private WeaponDefinition _currentWeapon; 
        private Transform _currentTarget;
        
        // Dinamik Referanslar (Görsel değiştikçe güncellenir)
        private Transform _dynamicFirePoint;
        private Transform _dynamicRotatingPart;

        private float _nextAttackTime;
        private float _nextSearchTime;
        private float _sqrRange; 
        
        private readonly Collider[] _hitBuffer = new Collider[20];
        private const float SEARCH_INTERVAL = 0.2f;

        // Görsel kontrolcü bu metodu çağırarak referansları günceller
        public void UpdateVisualReferences(Transform newFirePoint, Transform newRotatingPart)
        {
            _dynamicFirePoint = newFirePoint;
            _dynamicRotatingPart = newRotatingPart;
        }

        public void SetWeapon(WeaponDefinition weaponDef)
        {
            _currentWeapon = weaponDef;
            
            if (_currentWeapon != null)
            {
                _sqrRange = _currentWeapon.Range * _currentWeapon.Range;
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
            Vector3 center = _dynamicFirePoint != null ? _dynamicFirePoint.position : transform.position;
            
            // GC Alloc yaratmaz
            int hitCount = Physics.OverlapSphereNonAlloc(center, _currentWeapon.Range, _hitBuffer, _enemyLayer);
            
            Transform closestEnemy = null;
            float closestDistanceSqr = Mathf.Infinity;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];
                if (hit != null && hit.gameObject.activeInHierarchy) 
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
            // Parça yoksa veya hedef yoksa çık (Optimizasyon)
            if (_currentTarget == null || _dynamicRotatingPart == null) return;

            Vector3 direction = (_currentTarget.position - _dynamicRotatingPart.position);
            direction.y = 0; 

            // Çok yakınsa işlem yapma (Sıfıra bölme hatası önlemi)
            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // OPTİMİZASYON: Açı farkı çok azsa (0.5 derece) döndürme işlemini (Slerp) pas geç. CPU tasarrufu.
            if (Quaternion.Angle(_dynamicRotatingPart.rotation, targetRotation) < 0.5f) return;

            float rotSpeed = _currentWeapon.RotationSpeed > 0 ? _currentWeapon.RotationSpeed : 10f;
            _dynamicRotatingPart.rotation = Quaternion.Slerp(_dynamicRotatingPart.rotation, targetRotation, Time.deltaTime * rotSpeed);
        }

        private void Attack()
        {
            if (_currentWeapon.ProjectilePool == null) return;

            BasicProjectile projectile = _currentWeapon.ProjectilePool.Get();
            
            Vector3 spawnPos = _dynamicFirePoint != null ? _dynamicFirePoint.position : transform.position;
            
            projectile.transform.position = spawnPos;
            projectile.transform.rotation = Quaternion.LookRotation(_currentTarget.position - spawnPos);
            
            projectile.Initialize(_currentTarget, _currentWeapon.ProjectilePool, _currentWeapon);

            // Animasyonu tetikle
            OnFired?.Invoke();
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