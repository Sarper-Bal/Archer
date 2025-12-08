using UnityEngine;
using System; 
using System.Collections; // [EKLENDI] Coroutine kullanımı için
using ArcadeBridge.ArcadeIdleEngine.Pools;
using IndianOceanAssets.Engine2_5D; 

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    public class TowerAttacker : MonoBehaviour
    {
        #region Konfigurasyon
        [Header("🎯 Hedef Ayarları")]
        [SerializeField] private LayerMask _enemyLayer; 
        #endregion

        #region Eventler
        // --- Event Sistemi ---
        public event Action OnFired;
        #endregion

        #region Degiskenler
        // --- Değişkenler ---
        private WeaponDefinition _currentWeapon; 
        private Transform _currentTarget;
        
        // Dinamik Referanslar (Görsel değiştikçe güncellenir)
        private Transform _dynamicFirePoint;
        private Transform _dynamicRotatingPart;

        private float _nextAttackTime;
        private float _sqrRange; // [OPTIMIZASYON] Menzilin karesini saklar, her defasında çarpma işlemi yapmaz.
        
        // [OPTIMIZASYON] Garbage Collection (GC) oluşumunu önlemek için statik boyutlu buffer.
        private readonly Collider[] _hitBuffer = new Collider[20];
        
        // [OPTIMIZASYON] WaitForSeconds objesini cache'liyoruz (GC tasarrufu).
        private readonly WaitForSeconds _searchWait = new WaitForSeconds(0.2f);
        private Coroutine _searchCoroutine;
        #endregion

        #region Unity Metotlari

        private void OnEnable()
        {
            // [2025-11-02] Script aktif olduğunda arama döngüsünü başlat.
            if (_searchCoroutine == null)
                _searchCoroutine = StartCoroutine(SearchTargetRoutine());
        }

        private void OnDisable()
        {
            // [2025-11-02] Script pasif olduğunda döngüyü durdur (CPU tasarrufu).
            if (_searchCoroutine != null)
            {
                StopCoroutine(_searchCoroutine);
                _searchCoroutine = null;
            }
        }

        private void Update()
        {
            // Silah yoksa veya hedef yoksa Update içinde işlem yapma (Hızlı çıkış).
            if (_currentWeapon == null || _currentTarget == null) return;

            // Hedef hala geçerli mi (Aktif mi ve menzil içinde mi?)
            if (IsTargetInvalidOrOutOfRange())
            {
                _currentTarget = null;
                // Hedef kaybolduysa, Coroutine zaten arka planda yeni hedef arayacaktır.
                return;
            }

            // Görseli hedefe döndür
            RotatePartToTarget();

            // Saldırı zamanlaması
            if (Time.time >= _nextAttackTime)
            {
                Attack();
                // [2025-11-02] Bir sonraki saldırı zamanını hesapla.
                _nextAttackTime = Time.time + _currentWeapon.AttackInterval;
            }
        }

        // [2025-11-02] Editörde menzili görmek için.
        private void OnDrawGizmosSelected()
        {
            if (_currentWeapon != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, _currentWeapon.Range);
            }
        }

        #endregion

        #region Public Metotlar

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
                // [OPTIMIZASYON] Menzil karesini bir kere hesaplayıp saklıyoruz.
                _sqrRange = _currentWeapon.Range * _currentWeapon.Range;
            }
        }

        #endregion

        #region Ozel Metotlar

        /// <summary>
        /// [2025-11-02] [OPTIMIZASYON] Hedef arama işlemini Update'den ayırıp belirli aralıklarla yapar.
        /// Bu sayede her frame'de (60FPS'de saniyede 60 kere) yerine saniyede 5 kere çalışır.
        /// </summary>
        private IEnumerator SearchTargetRoutine()
        {
            while (true)
            {
                // Eğer zaten geçerli bir hedefimiz varsa aramaya gerek yok.
                if (_currentTarget == null)
                {
                    FindClosestEnemy();
                }
                
                yield return _searchWait;
            }
        }

        /// <summary>
        /// [2025-11-02] Hedefin null olup olmadığını, pasif olup olmadığını veya menzil dışına çıkıp çıkmadığını kontrol eder.
        /// </summary>
        private bool IsTargetInvalidOrOutOfRange()
        {
            // 1. Hedef yok mu?
            if (_currentTarget == null) return true;
            
            // 2. Hedef objesi pasif mi? (Öldü mü?)
            if (!_currentTarget.gameObject.activeInHierarchy) return true;

            // 3. Menzil dışına çıktı mı? (SqrMagnitude karekök almadığı için hızlıdır)
            float distSqr = (transform.position - _currentTarget.position).sqrMagnitude;
            return distSqr > _sqrRange;
        }

        private void FindClosestEnemy()
        {
            if (_currentWeapon == null) return;

            Vector3 center = _dynamicFirePoint != null ? _dynamicFirePoint.position : transform.position;
            
            // [OPTIMIZASYON] NonAlloc versiyonu hafıza tahsis etmez (Garbage Collection dostu).
            int hitCount = Physics.OverlapSphereNonAlloc(center, _currentWeapon.Range, _hitBuffer, _enemyLayer);
            
            Transform closestEnemy = null;
            float closestDistanceSqr = Mathf.Infinity;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];
                // Hit null kontrolü ve obje aktiflik kontrolü
                if (hit != null && hit.gameObject.activeInHierarchy) 
                {
                    float dSqrToTarget = (hit.transform.position - center).sqrMagnitude;
                    
                    // En yakın olanı bul
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
            // Parça yoksa veya hedef yoksa çık (Güvenlik)
            if (_dynamicRotatingPart == null || _currentTarget == null) return;

            Vector3 direction = (_currentTarget.position - _dynamicRotatingPart.position);
            direction.y = 0; // Sadece Y ekseninde (yatayda) dönmesi için.

            // [OPTIMIZASYON] Sıfıra çok yakınsa (hedef tam üstündeyse) hesaplama yapma (LookRotation hatasını önler).
            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // [OPTIMIZASYON] Açı farkı çok azsa (0.5 derece) döndürme işlemini (Slerp) pas geç. CPU tasarrufu sağlar.
            if (Quaternion.Angle(_dynamicRotatingPart.rotation, targetRotation) < 0.5f) return;

            float rotSpeed = _currentWeapon.RotationSpeed > 0 ? _currentWeapon.RotationSpeed : 10f;
            _dynamicRotatingPart.rotation = Quaternion.Slerp(_dynamicRotatingPart.rotation, targetRotation, Time.deltaTime * rotSpeed);
        }

        private void Attack()
        {
            if (_currentWeapon.ProjectilePool == null) return;

            // Obje Havuzundan (Pool) mermi çek
            BasicProjectile projectile = _currentWeapon.ProjectilePool.Get();
            
            Vector3 spawnPos = _dynamicFirePoint != null ? _dynamicFirePoint.position : transform.position;
            
            projectile.transform.position = spawnPos;
            // Merminin rotasyonunu hedefe çevir
            if (_currentTarget != null)
            {
                projectile.transform.rotation = Quaternion.LookRotation(_currentTarget.position - spawnPos);
            }
            
            projectile.Initialize(_currentTarget, _currentWeapon.ProjectilePool, _currentWeapon);

            // Animasyonu ve sesi tetikle
            OnFired?.Invoke();
        }

        #endregion
    }
}