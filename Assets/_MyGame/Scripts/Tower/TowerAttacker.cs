using UnityEngine;
using System; // Action eventi için gerekli
using ArcadeBridge.ArcadeIdleEngine.Pools;
using IndianOceanAssets.Engine2_5D; 

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    public class TowerAttacker : MonoBehaviour
    {
        // [DEĞİŞİKLİK] Artık Inspector'dan elle verilmiyor, kod ile atanıyor.
        // [SerializeField] private Transform _firePoint;  <- KALDIRILDI
        // [SerializeField] private Transform _partToRotate; <- KALDIRILDI
        
        [Header("🎯 Hedef Ayarları")]
        [SerializeField] private LayerMask _enemyLayer; 

        // --- PUBLIC EVENT ---
        // [DEĞİŞİKLİK] Ateş edildiğinde animasyon scriptinin duyması için event eklendi.
        public event Action OnFired;

        // --- ÇALIŞMA DEĞİŞKENLERİ ---
        private WeaponDefinition _currentWeapon; 
        private Transform _currentTarget;
        
        // Dinamik Referanslar
        private Transform _dynamicFirePoint;
        private Transform _dynamicRotatingPart;

        private float _nextAttackTime;
        private float _nextSearchTime;
        private float _sqrRange; 
        
        private readonly Collider[] _hitBuffer = new Collider[20];
        private const float SEARCH_INTERVAL = 0.2f;

        // [DEĞİŞİKLİK] VisualController bu metodu çağırarak referansları günceller
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
            // FirePoint yoksa (henüz atanmadıysa) kulenin kendi pozisyonunu kullan
            Vector3 center = _dynamicFirePoint != null ? _dynamicFirePoint.position : transform.position;
            
            int hitCount = Physics.OverlapSphereNonAlloc(center, _currentWeapon.Range, _hitBuffer, _enemyLayer);
            
            Transform closestEnemy = null;
            float closestDistanceSqr = Mathf.Infinity;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];
                // Tag kontrolü de eklenebilir ama LayerMask genelde yeterlidir.
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
            // [DEĞİŞİKLİK] Dinamik parça referansı kontrol ediliyor
            if (_currentTarget == null || _dynamicRotatingPart == null) return;

            Vector3 direction = (_currentTarget.position - _dynamicRotatingPart.position).normalized;
            direction.y = 0; 

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                float rotSpeed = _currentWeapon.RotationSpeed > 0 ? _currentWeapon.RotationSpeed : 10f;
                _dynamicRotatingPart.rotation = Quaternion.Slerp(_dynamicRotatingPart.rotation, lookRotation, Time.deltaTime * rotSpeed);
            }
        }

        private void Attack()
        {
            if (_currentWeapon.ProjectilePool == null) return;

            BasicProjectile projectile = _currentWeapon.ProjectilePool.Get();
            
            // [DEĞİŞİKLİK] Dinamik FirePoint kullanılıyor
            Vector3 spawnPos = _dynamicFirePoint != null ? _dynamicFirePoint.position : transform.position;
            
            projectile.transform.position = spawnPos;
            projectile.transform.rotation = Quaternion.LookRotation(_currentTarget.position - spawnPos);
            
            projectile.Initialize(_currentTarget, _currentWeapon.ProjectilePool, _currentWeapon);

            // [DEĞİŞİKLİK] Event tetikleniyor (Animasyon oynaması için)
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