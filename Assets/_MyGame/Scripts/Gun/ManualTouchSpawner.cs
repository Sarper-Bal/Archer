using UnityEngine;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D;
using ArcadeBridge.ArcadeIdleEngine.Actors; // InputChannel için

namespace ArcadeBridge.ArcadeIdleEngine.Spawners
{
    public class ManualTouchSpawner : MonoBehaviour
    {
        [Header("🎮 Input Entegrasyonu")]
        [Tooltip("Projedeki 'Input Channel' ScriptableObject dosyasını buraya sürükle.")]
        [SerializeField] private InputChannel _inputChannel;

        [Header("⚙️ Spawner Ayarları")]
        [SerializeField] private EnemyDefinition _unitData;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _spawnInterval = 0.1f;

        [Header("🏃 Hareket Ayarları (Swerve)")]
        [Tooltip("Sağa sola kayma hızı")]
        [SerializeField] private float _moveSpeed = 10f;
        [Tooltip("Sağa sola gidebileceği maksimum mesafe (Örn: 4 ise -4 ile 4 arası)")]
        [SerializeField] private float _xLimit = 4.5f;
        [Tooltip("Birimlerin koşacağı yön")]
        [SerializeField] private Vector3 _moveDirection = new Vector3(0, 0, 1); 

        // --- Private Variables ---
        private Queue<EnemyBehaviorController> _pool = new Queue<EnemyBehaviorController>();
        private float _nextSpawnTime;
        private float _currentXInput;
        private bool _isTouching; // Joystick kullanılıyor mu?

        private void OnEnable()
        {
            // Joystick eventine abone ol
            if (_inputChannel != null)
            {
                _inputChannel.JoystickUpdate += HandleJoystickInput;
                _inputChannel.PointerDown += HandlePointerDown; // Eğer InputChannel'da varsa
                _inputChannel.PointerUp += HandlePointerUp;     // Eğer InputChannel'da varsa
            }
        }

        private void OnDisable()
        {
            if (_inputChannel != null)
            {
                _inputChannel.JoystickUpdate -= HandleJoystickInput;
                _inputChannel.PointerDown -= HandlePointerDown;
                _inputChannel.PointerUp -= HandlePointerUp;
            }
        }

        private void Update()
        {
            // 1. Hareket Mantığı (Swerve)
            ProcessMovement();

            // 2. Spawn Mantığı
            // InputChannel eventleri yoksa manuel Input kontrolü (Yedek)
            bool isPressing = _isTouching || Input.GetMouseButton(0);
            
            if (isPressing)
            {
                TrySpawn();
            }
        }

        // Joystick'ten gelen veriyi alıyoruz
        private void HandleJoystickInput(Vector2 input)
        {
            // Sadece X eksenini alıyoruz (Sağ-Sol)
            _currentXInput = input.x;
            
            // Veri geliyorsa dokunuyordur
            _isTouching = input.sqrMagnitude > 0.01f;
        }

        // Eğer InputChannel'da Pointer eventleri tanımlı değilse bu metotlar hata vermez, boş kalır.
        private void HandlePointerDown() => _isTouching = true;
        private void HandlePointerUp() => _isTouching = false;

        private void ProcessMovement()
        {
            // Eğer hiç girdi yoksa hareket etme
            if (Mathf.Abs(_currentXInput) < 0.01f) return;

            // Yeni pozisyonu hesapla
            Vector3 position = transform.position;
            position.x += _currentXInput * _moveSpeed * Time.deltaTime;

            // Sınırların dışına çıkmasını engelle (Clamp)
            position.x = Mathf.Clamp(position.x, -_xLimit, _xLimit);

            transform.position = position;
        }

        private void TrySpawn()
        {
            if (Time.time < _nextSpawnTime) return;

            SpawnUnit();
            _nextSpawnTime = Time.time + _spawnInterval;
        }

        private void SpawnUnit()
        {
            if (_unitData == null || _unitData.EnemyPrefab == null) return;

            EnemyBehaviorController unit = GetFromPool();
            
            // Spawner'ın tam o anki konumundan doğsun
            Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : transform.position;
            
            unit.transform.position = spawnPos;
            unit.transform.rotation = Quaternion.LookRotation(_moveDirection);

            var stats = unit.GetComponent<EnemyStats>();
            if (stats != null) stats.InitializeRuntime(_unitData);

            unit.gameObject.SetActive(true);
            unit.SetBehavior(EnemyBehaviorType.Directional);
        }

        // --- Basit Pooling Sistemi (Değişmedi) ---
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

            GameObject newObj = Instantiate(_unitData.EnemyPrefab, transform);
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
    }
}