using UnityEngine;
using System.Collections;
using ArcadeBridge.ArcadeIdleEngine.Pools;

namespace IndianOceanAssets.Engine2_5D
{
    public enum ProjectileTrackingMode
    {
        Guided, // Hedefi takip eder
        Linear  // Dümdüz gider
    }

    public class BasicProjectile : MonoBehaviour
    {
        // --- DEĞİŞKENLER (Artık Inspector'da görünmelerine gerek yok) ---
        private ProjectileTrackingMode _trackingMode;
        private float _speed;
        private float _lifeTime;
        private float _damage;
        private float _explosionRadius;
        private LayerMask _damageLayer;
        
        // Referanslar
        private BasicProjectilePool _myPool;
        private ExplosionPool _explosionPool;
        private Transform _target;
        private Vector3 _targetPosition;
        private Coroutine _lifeTimeCoroutine;
        private readonly Collider[] _explosionHits = new Collider[15];

        /// <summary>
        /// Mermiyi silah verilerine göre başlatır.
        /// </summary>
        public void Initialize(Transform target, BasicProjectilePool pool, WeaponDefinition weaponDef)
        {
            // 1. Verileri Silah Dosyasından Al
            _target = target;
            _myPool = pool;
            _damage = weaponDef.Damage;
            _speed = weaponDef.ProjectileSpeed;
            _lifeTime = weaponDef.ProjectileLifeTime;
            _trackingMode = weaponDef.TrackingMode;
            _explosionPool = weaponDef.ExplosionPool;
            _explosionRadius = weaponDef.ExplosionRadius;
            _damageLayer = weaponDef.DamageLayer;

            // 2. Hedef Konum Belirle
            if (_target != null)
            {
                _targetPosition = _target.position;
                Vector3 dir = (_targetPosition - transform.position).normalized;
                if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                _targetPosition = transform.position + transform.forward * 50f;
            }

            // 3. Sayaç Başlat
            if (_lifeTimeCoroutine != null) StopCoroutine(_lifeTimeCoroutine);
            _lifeTimeCoroutine = StartCoroutine(LifeTimeRoutine());
        }

        private void Update()
        {
            // Guided Mod: Hedef hareket ederse rotayı güncelle
            if (_trackingMode == ProjectileTrackingMode.Guided && _target != null && _target.gameObject.activeInHierarchy)
            {
                _targetPosition = _target.position;
            }

            // Hareket
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);

            // Yönelme
            if (_trackingMode == ProjectileTrackingMode.Guided)
            {
                Vector3 dir = (_targetPosition - transform.position).normalized;
                if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
            }

            // Hedefe varış kontrolü
            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                Explode();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // LayerMask kontrolü: Çarpan obje bizim hasar vereceğimiz layer'da mı?
            // (1 << layer) bitwise işlemi ile kontrol edilir.
            if (((1 << other.gameObject.layer) & _damageLayer) != 0 || other.gameObject.layer == 0) // Default layer'a çarpınca da patlasın
            {
                Explode();
            }
        }

        private void Explode()
        {
            // 1. Görsel Efekt
            if (_explosionPool != null)
            {
                var effect = _explosionPool.Get();
                effect.transform.position = transform.position;
                effect.transform.rotation = Quaternion.identity;
                effect.Initialize(_explosionPool);
            }

            // 2. Alan Hasarı
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _explosionRadius, _explosionHits, _damageLayer);

            for (int i = 0; i < hitCount; i++)
            {
                if (_explosionHits[i].TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(_damage);
                }
            }

            ReturnToPool();
        }

        private IEnumerator LifeTimeRoutine()
        {
            yield return new WaitForSeconds(_lifeTime);
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (_lifeTimeCoroutine != null) StopCoroutine(_lifeTimeCoroutine);
            if (_myPool != null) _myPool.Release(this);
            else gameObject.SetActive(false);
        }

        // Gizmo çizimi için layer ve radius verisini dışarıdan alamayız (editör modunda çalışmaz)
        // O yüzden sadece basit bir küre çiziyoruz.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0, 0, 0.4f);
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}