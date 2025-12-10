using UnityEngine;
using System; 
using System.Collections; 
using ArcadeBridge.ArcadeIdleEngine.Pools;
using ArcadeBridge.ArcadeIdleEngine.Storage; // [EKLENDI]
using IndianOceanAssets.Engine2_5D; 

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    public class TowerAttacker : MonoBehaviour
    {
        #region Konfigurasyon
        [Header("🎯 Hedef Ayarları")]
        [SerializeField] private LayerMask _enemyLayer; 
        #endregion

        public event Action OnFired;

        private WeaponDefinition _currentWeapon; 
        private Transform _currentTarget;
        
        private Transform _dynamicFirePoint;
        private Transform _dynamicRotatingPart;
        
        // [YENİ] Kulenin kendi deposu
        private Inventory _myInventory;

        private float _nextAttackTime;
        private float _sqrRange; 
        
        private readonly Collider[] _hitBuffer = new Collider[20];
        private readonly WaitForSeconds _searchWait = new WaitForSeconds(0.2f);
        private Coroutine _searchCoroutine;

        private void Awake()
        {
            // [YENİ] Kule üzerindeki envanteri bul (Depo)
            _myInventory = GetComponent<Inventory>();
            if (_myInventory == null)
                _myInventory = GetComponentInParent<Inventory>();
        }

        private void OnEnable()
        {
            if (_searchCoroutine == null)
                _searchCoroutine = StartCoroutine(SearchTargetRoutine());
        }

        private void OnDisable()
        {
            if (_searchCoroutine != null)
            {
                StopCoroutine(_searchCoroutine);
                _searchCoroutine = null;
            }
        }

        private void Update()
        {
            if (_currentWeapon == null || _currentTarget == null) return;

            if (IsTargetInvalidOrOutOfRange())
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

        private IEnumerator SearchTargetRoutine()
        {
            while (true)
            {
                if (_currentTarget == null) FindClosestEnemy();
                yield return _searchWait;
            }
        }

        private bool IsTargetInvalidOrOutOfRange()
        {
            if (_currentTarget == null) return true;
            if (!_currentTarget.gameObject.activeInHierarchy) return true;

            float distSqr = (transform.position - _currentTarget.position).sqrMagnitude;
            return distSqr > _sqrRange;
        }

        private void FindClosestEnemy()
        {
            if (_currentWeapon == null) return;

            Vector3 center = _dynamicFirePoint != null ? _dynamicFirePoint.position : transform.position;
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
            if (_dynamicRotatingPart == null || _currentTarget == null) return;

            Vector3 direction = (_currentTarget.position - _dynamicRotatingPart.position);
            direction.y = 0; 

            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
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
            if (_currentTarget != null)
            {
                projectile.transform.rotation = Quaternion.LookRotation(_currentTarget.position - spawnPos);
            }
            
            // [GÜNCELLENDİ] Mermiye "Benim kasam (_myInventory) bu" bilgisini veriyoruz.
            projectile.Initialize(_currentTarget, _currentWeapon.ProjectilePool, _currentWeapon, _myInventory);

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