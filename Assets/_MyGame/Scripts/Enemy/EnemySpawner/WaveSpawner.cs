using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; 
using IndianOceanAssets.Engine2_5D.Spawners; 
// [KRİTİK] WaypointRoute ve EnemyBehaviorController için gerekli namespace:
using ArcadeBridge.ArcadeIdleEngine.Enemy; 

namespace ArcadeBridge.ArcadeIdleEngine.Spawners
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("Pool Settings / Havuz Ayarları")]
        [Tooltip("Prefab to use for pooling. / Havuza eklenecek düşman prefabı")]
        [SerializeField] private EnemyBehaviorController _enemyPrefab; 
        
        [Tooltip("Initial pool size. / Oyun başında kaç düşman üretip hazır bekletelim?")]
        [SerializeField] private int _initialPoolSize = 30;

        [Header("Data (Brain)")]
        [SerializeField] private WaveConfig _waveConfig;

        [Header("Spawn Settings")]
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(5, 0, 5);
        [SerializeField] private WaypointRoute _forcePatrolRoute;

        // [YENİ SİSTEM] Local Pool (Queue is O(1) - Fastest)
        // Artık dışarıdaki ScriptableObject'e bağımlı değiliz.
        private Queue<EnemyBehaviorController> _localEnemyPool = new Queue<EnemyBehaviorController>();
        
        // Active enemies list / Aktif düşmanları takip listesi
        private List<EnemyBehaviorController> _activeEnemies = new List<EnemyBehaviorController>();

        // [OPTIMIZATION] Cached WaitForSeconds to avoid GC / Çöp oluşumunu önlemek için önbellek
        private WaitForSeconds _checkInterval; 
        private int _currentWaveIndex = 0;

        public System.Action<int, int> OnWaveChanged; 
        public System.Action OnAllWavesComplete;

        private void Awake()
        {
            _checkInterval = new WaitForSeconds(0.5f);
            
            // [PREWARM] Fill the pool before game starts / Havuzu oyundan önce doldur
            InitializeLocalPool();
        }

        private void InitializeLocalPool()
        {
            if (_enemyPrefab == null)
            {
                Debug.LogError("⚠️ WaveSpawner: Enemy Prefab is missing! / Düşman prefabı atanmamış!");
                return;
            }

            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewEnemyForPool();
            }
        }

        // Creates a new enemy and adds it to the pool / Yeni düşman yaratır ve havuza ekler
        private EnemyBehaviorController CreateNewEnemyForPool()
        {
            EnemyBehaviorController enemy = Instantiate(_enemyPrefab, transform);
            enemy.gameObject.SetActive(false);
            
            // [BAĞLANTI] Düşman öldüğünde bu spawner'a haber versin
            enemy.OnReturnToPool = ReturnEnemyToPool;
            
            _localEnemyPool.Enqueue(enemy);
            return enemy;
        }

        private void Start()
        {
            if (_waveConfig != null)
            {
                StartCoroutine(ProcessWaves());
            }
        }

        // Callback function when enemy dies / Düşman öldüğünde çağrılan fonksiyon
        private void ReturnEnemyToPool(EnemyBehaviorController enemy)
        {
            if (this == null || gameObject == null) return;

            // Remove from active list
            if (_activeEnemies.Contains(enemy))
            {
                _activeEnemies.Remove(enemy);
            }

            enemy.gameObject.SetActive(false);
            
            // Return to Queue / Kuyruğa geri koy
            _localEnemyPool.Enqueue(enemy);
        }

        private IEnumerator ProcessWaves()
        {
            _currentWaveIndex = 0;

            while (true)
            {
                if (_currentWaveIndex >= _waveConfig.Waves.Count)
                {
                    if (_waveConfig.LoopWaves) _currentWaveIndex = 0; 
                    else
                    {
                        OnAllWavesComplete?.Invoke();
                        yield break; 
                    }
                }

                WaveDefinition currentWave = _waveConfig.Waves[_currentWaveIndex];
                OnWaveChanged?.Invoke(_currentWaveIndex + 1, _waveConfig.Waves.Count);
                
                Debug.Log($"🌊 Wave Started: {currentWave.WaveName}");

                // 1. Spawn Groups
                foreach (var group in currentWave.Groups)
                {
                    yield return StartCoroutine(SpawnGroupRoutine(group));
                }

                // 2. Wait for clear
                if (currentWave.WaitForAllDead)
                {
                    while (_activeEnemies.Count > 0)
                    {
                        yield return _checkInterval; 
                    }
                }

                // 3. Cooldown
                if (currentWave.TimeToNextWave > 0)
                    yield return new WaitForSeconds(currentWave.TimeToNextWave);

                _currentWaveIndex++;
            }
        }

        private IEnumerator SpawnGroupRoutine(WaveGroup group)
        {
            WaitForSeconds delay = new WaitForSeconds(group.DelayBetweenSpawns);

            for (int i = 0; i < group.Count; i++)
            {
                SpawnFromPool(); // Eski 'SpawnEnemy(pool)' yerine bunu kullanıyoruz
                if (group.DelayBetweenSpawns > 0) yield return delay;
            }
        }

        private void SpawnFromPool()
        {
            EnemyBehaviorController enemy;

            // Check if pool has available enemies / Havuzda asker var mı?
            if (_localEnemyPool.Count > 0)
            {
                enemy = _localEnemyPool.Dequeue();
            }
            else
            {
                // Pool is empty, create new one (Auto-Expand) / Havuz boşsa yeni yarat
                enemy = CreateNewEnemyForPool();
                _localEnemyPool.Dequeue(); // Kuyruktan hemen al
            }

            // Positioning / Konumlandırma
            Vector3 randomOffset = new Vector3(
                Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2),
                0,
                Random.Range(-_spawnAreaSize.z / 2, _spawnAreaSize.z / 2)
            );
            
            enemy.transform.position = transform.position + randomOffset;
            enemy.transform.rotation = transform.rotation;

            // Safe Activation / Güvenli Aktivasyon
            enemy.gameObject.SetActive(true);
            
            // Assign Patrol Route / Devriye Rotası Ata
            if (_forcePatrolRoute != null)
            {
                enemy.SetPatrolRoute(_forcePatrolRoute);
            }

            _activeEnemies.Add(enemy);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, _spawnAreaSize);
        }
    }
}