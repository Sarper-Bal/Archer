using UnityEngine;
using System.Collections;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using ArcadeBridge.ArcadeIdleEngine.Storage;
using ArcadeBridge.ArcadeIdleEngine.Items;

namespace IndianOceanAssets.Engine2_5D
{
    public enum ProjectileTrackingMode
    {
        Guided,
        Linear
    }

    public class BasicProjectile : MonoBehaviour
    {
        private ProjectileTrackingMode _trackingMode;
        private float _speed;
        private float _lifeTime;
        private float _damage;
        private float _explosionRadius;
        private LayerMask _damageLayer;
        
        private BasicProjectilePool _myPool;
        private ExplosionPool _explosionPool;
        private Transform _target;
        private Vector3 _targetPosition;
        private Inventory _ownerInventory;
        
        private Coroutine _lifeTimeCoroutine;
        private readonly Collider[] _explosionHits = new Collider[15];
        private WaitForSeconds _waitLifeTime;
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        public void Initialize(Transform target, BasicProjectilePool pool, WeaponDefinition weaponDef, Inventory ownerInventory = null)
        {
            _target = target;
            _myPool = pool;
            _ownerInventory = ownerInventory;

            _damage = weaponDef.Damage;
            _speed = weaponDef.ProjectileSpeed;
            _lifeTime = weaponDef.ProjectileLifeTime;
            _trackingMode = weaponDef.TrackingMode;
            _explosionPool = weaponDef.ExplosionPool;
            _explosionRadius = weaponDef.ExplosionRadius;
            _damageLayer = weaponDef.DamageLayer;

            if (_waitLifeTime == null) _waitLifeTime = new WaitForSeconds(_lifeTime);

            if (_target != null)
            {
                _targetPosition = _target.position;
                Vector3 dir = (_targetPosition - _transform.position).normalized;
                if (dir.sqrMagnitude > 0.001f) _transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                _targetPosition = _transform.position + _transform.forward * 50f;
            }

            if (_lifeTimeCoroutine != null) StopCoroutine(_lifeTimeCoroutine);
            _lifeTimeCoroutine = StartCoroutine(LifeTimeRoutine());
        }

        private void Update()
        {
            if (_trackingMode == ProjectileTrackingMode.Guided && _target != null)
            {
                _targetPosition = _target.position;
            }

            float moveStep = _speed * Time.deltaTime;
            _transform.position = Vector3.MoveTowards(_transform.position, _targetPosition, moveStep);

            if (_trackingMode == ProjectileTrackingMode.Guided)
            {
                Vector3 dir = (_targetPosition - _transform.position).normalized;
                if (dir.sqrMagnitude > 0.001f) 
                    _transform.rotation = Quaternion.LookRotation(dir);
            }

            if ((_targetPosition - _transform.position).sqrMagnitude < 0.25f)
            {
                Explode();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & _damageLayer) != 0 || other.gameObject.layer == 0)
            {
                Explode();
            }
        }

        private void Explode()
        {
            if (_explosionPool != null)
            {
                var effect = _explosionPool.Get();
                effect.transform.position = _transform.position;
                effect.transform.rotation = Quaternion.identity;
                effect.Initialize(_explosionPool);
            }

            int hitCount = Physics.OverlapSphereNonAlloc(_transform.position, _explosionRadius, _explosionHits, _damageLayer);

            for (int i = 0; i < hitCount; i++)
            {
                if (_explosionHits[i].TryGetComponent(out Health health))
                {
                    bool isKillingBlow = !health.IsDead && (health.CurrentHealth <= _damage);

                    if (isKillingBlow)
                    {
                        TryLootEnemy(_explosionHits[i]);
                    }

                    health.TakeDamage(_damage);
                }
            }

            ReturnToPool();
        }

        private void TryLootEnemy(Collider enemyCollider)
        {
            if (_ownerInventory == null) return; // Sahibi yoksa (Player mermisi vb.) alma.

            if (enemyCollider.TryGetComponent(out EnemyBehaviorController enemyController))
            {
                // [ÖNEMLİ] Daha önce alındıysa tekrar alma
                if (enemyController.LootDropped) return;

                var stats = enemyController.GetComponent<EnemyStats>();
                if (stats != null && stats.Definition != null)
                {
                    ItemDefinition lootItem = stats.Definition.DropItem;
                    if (lootItem == null) return;

                    if (!_ownerInventory.CanAdd(lootItem)) return;

                    if (lootItem.Pool != null)
                    {
                        Item newItem = lootItem.Pool.Get();
                        if (newItem != null)
                        {
                            newItem.transform.position = enemyCollider.transform.position;
                            newItem.gameObject.SetActive(true);
                            newItem.transform.SetParent(null);
                            
                            _ownerInventory.Add(newItem);

                            // [İŞARETLE] Kule bunu aldı, Spawner oyuncuya vermesin.
                            enemyController.LootDropped = true;
                        }
                    }
                }
            }
        }

        private IEnumerator LifeTimeRoutine()
        {
            yield return _waitLifeTime;
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (_lifeTimeCoroutine != null) StopCoroutine(_lifeTimeCoroutine);
            if (_myPool != null) _myPool.Release(this);
            else Destroy(gameObject);
        }
    }
}