using UnityEngine;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D;
using ArcadeBridge.ArcadeIdleEngine.Actors; 
using ArcadeBridge.ArcadeIdleEngine.Weapon; 

namespace ArcadeBridge.ArcadeIdleEngine.Controller
{
    public class PlayerCannonController : MonoBehaviour
    {
        #region Configuration
        [Header("🎮 Input & Data")]
        [SerializeField] private InputChannel _inputChannel;
        [SerializeField] private PlayerWeaponDefinition _defaultWeapon;

        [Header("📍 Setup")]
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Transform _visualRoot; 
        #endregion

        #region Runtime State
        // [TR] Runtime'da değişebilecek tüm verileri bu struct içinde tutuyoruz.
        [System.Serializable]
        public struct WeaponStats
        {
            public float FireRate;
            public int ProjectilesPerShot;
            public float SpreadAngle;
            // [YENİ] Hareket verileri de buraya eklendi
            public float SwerveSpeed;
            public float MaxSwerveOffset; 
        }

        private WeaponStats _currentStats;
        private EnemyDefinition _currentAmmo;
        
        private float _nextFireTime;
        private float _currentInputX;
        private bool _isFiring;
        
        // [YENİ] Başlangıç pozisyonunu saklamak için
        private float _initialXPosition;

        // Pooling
        private Queue<EnemyBehaviorController> _pool = new Queue<EnemyBehaviorController>();
        #endregion

        private void Awake()
        {
            // [KRİTİK] Oyun başladığı an top nerede duruyorsa orayı "Merkez" kabul et.
            _initialXPosition = transform.position.x;

            if (_defaultWeapon != null)
            {
                InitializeWeapon(_defaultWeapon);
            }
        }

        private void OnEnable()
        {
            if (_inputChannel != null)
            {
                _inputChannel.JoystickUpdate += HandleJoystickInput;
                _inputChannel.PointerDown += OnPointerDown;
                _inputChannel.PointerUp += OnPointerUp;
            }
        }

        private void OnDisable()
        {
            if (_inputChannel != null)
            {
                _inputChannel.JoystickUpdate -= HandleJoystickInput;
                _inputChannel.PointerDown -= OnPointerDown;
                _inputChannel.PointerUp -= OnPointerUp;
            }
        }

        public void InitializeWeapon(PlayerWeaponDefinition weaponDef)
        {
            _currentAmmo = weaponDef.UnitToSpawn;
            
            // Scriptable Object verilerini Runtime Struct'a kopyala
            _currentStats.FireRate = weaponDef.FireRate;
            _currentStats.ProjectilesPerShot = weaponDef.ProjectilesPerShot;
            _currentStats.SpreadAngle = weaponDef.SpreadAngle;
            
            // [YENİ] Hareket verilerini de kopyala
            _currentStats.SwerveSpeed = weaponDef.SwerveSpeed;
            _currentStats.MaxSwerveOffset = weaponDef.MaxSwerveOffset;
        }

        private void Update()
        {
            HandleMovement();
            HandleFiring();
        }

        #region Movement Logic
        private void HandleJoystickInput(Vector2 input)
        {
            _currentInputX = input.x;
            if (input.sqrMagnitude > 0.01f) _isFiring = true;
        }

        private void OnPointerDown() => _isFiring = true;
        private void OnPointerUp() => _isFiring = false;

        private void HandleMovement()
        {
            if (Mathf.Abs(_currentInputX) < 0.01f) return;

            // 1. Yeni pozisyonu hesapla (Datadaki hızı kullan)
            Vector3 pos = transform.position;
            pos.x += _currentInputX * _currentStats.SwerveSpeed * Time.deltaTime;

            // 2. [GÜNCELLENDİ] Başlangıç noktasına göre sınırla (Relative Clamping)
            // _initialX - Offset (Sol Limit) ile _initialX + Offset (Sağ Limit) arasında tut.
            float minX = _initialXPosition - _currentStats.MaxSwerveOffset;
            float maxX = _initialXPosition + _currentStats.MaxSwerveOffset;
            
            pos.x = Mathf.Clamp(pos.x, minX, maxX);

            transform.position = pos;

            // Görseli eğme (Tilt effect)
            if (_visualRoot != null)
            {
                float targetZRot = -_currentInputX * 15f; 
                Quaternion targetRot = Quaternion.Euler(0, 0, targetZRot);
                _visualRoot.localRotation = Quaternion.Slerp(_visualRoot.localRotation, targetRot, Time.deltaTime * 10f);
            }
        }
        #endregion

        #region Firing Logic
        private void HandleFiring()
        {
            if (!_isFiring || Time.time < _nextFireTime) return;
            if (_currentAmmo == null) return;

            Fire();
            _nextFireTime = Time.time + (1f / _currentStats.FireRate);
        }

        private void Fire()
        {
            int projectileCount = _currentStats.ProjectilesPerShot;
            
            if (projectileCount == 1)
            {
                SpawnProjectile(0); 
            }
            else
            {
                float currentAngle = -_currentStats.SpreadAngle / 2f;
                float angleStep = _currentStats.SpreadAngle / (projectileCount - 1);

                for (int i = 0; i < projectileCount; i++)
                {
                    SpawnProjectile(currentAngle);
                    currentAngle += angleStep;
                }
            }
        }

        private void SpawnProjectile(float angleOffset)
        {
            EnemyBehaviorController unit = GetFromPool();
            if (unit == null) return;
            
            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
            unit.transform.position = spawnPos;

            Quaternion rotation = Quaternion.Euler(0, angleOffset, 0);
            Vector3 moveDir = rotation * Vector3.forward; 
            
            unit.transform.rotation = Quaternion.LookRotation(moveDir);

            var stats = unit.GetComponent<EnemyStats>();
            if (stats != null) stats.InitializeRuntime(_currentAmmo);

            unit.gameObject.SetActive(true);
            unit.SetBehavior(EnemyBehaviorType.Directional);
        }
        #endregion

        #region Pooling Logic
        private EnemyBehaviorController GetFromPool()
        {
            if (_pool.Count > 0)
            {
                var pooled = _pool.Dequeue();
                if (pooled != null)
                {
                    pooled.OnReturnToPool = ReturnToPool;
                    return pooled;
                }
            }

            if (_currentAmmo == null || _currentAmmo.EnemyPrefab == null) return null;

            GameObject newObj = Instantiate(_currentAmmo.EnemyPrefab, transform.parent); 
            var controller = newObj.GetComponent<EnemyBehaviorController>();
            
            if (newObj.GetComponent<DirectionalEnemyMover>() == null)
                newObj.AddComponent<DirectionalEnemyMover>();

            controller.OnReturnToPool = ReturnToPool;
            newObj.SetActive(false);
            return controller;
        }

        private void ReturnToPool(EnemyBehaviorController unit)
        {
            unit.gameObject.SetActive(false);
            _pool.Enqueue(unit);
        }
        #endregion
        
        #region Public API
        public void MultiplyFireRate(float multiplier)
        {
            _currentStats.FireRate *= multiplier;
        }

        public void AddProjectileCount(int amount)
        {
            _currentStats.ProjectilesPerShot += amount;
        }
        #endregion
        
        // [YENİ] Editörde sınırları görebilmek için Gizmo
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) // Sadece editör modunda, henüz initialX belli değilken tahmini göster
            {
                Gizmos.color = Color.yellow;
                float range = 4.5f; 
                // Eğer data varsa datadan oku
                if (_defaultWeapon != null) range = _defaultWeapon.MaxSwerveOffset;
                
                Vector3 center = transform.position;
                Gizmos.DrawLine(center + Vector3.left * range, center + Vector3.right * range);
                Gizmos.DrawSphere(center + Vector3.left * range, 0.2f);
                Gizmos.DrawSphere(center + Vector3.right * range, 0.2f);
            }
        }
    }
}