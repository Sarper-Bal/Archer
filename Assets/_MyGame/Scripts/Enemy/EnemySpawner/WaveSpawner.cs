using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; 
using IndianOceanAssets.Engine2_5D.Managers; 
using ArcadeBridge.ArcadeIdleEngine.Enemy;

namespace ArcadeBridge.ArcadeIdleEngine.Spawners
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("🧠 AI Bağlantısı")]
        [SerializeField] private SmartWaveManager _director;
        
        [Header("⚙️ Genel Ayarlar")]
        [SerializeField] private float _timeBetweenWaves = 3f;
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(10, 0, 10);
        
        private Dictionary<string, Queue<EnemyBehaviorController>> _poolDictionary = new Dictionary<string, Queue<EnemyBehaviorController>>();
        public System.Action<int> OnWaveStarted; 

        private void Start()
        {
            if (_director != null)
            {
                _director.OnWaveCompleted += StartNextWaveAfterDelay;
                _director.OnGameReset += StopAndResetSpawner;
                StartCoroutine(StartFirstWaveRoutine());
            }
        }

        private void OnDestroy()
        {
            if (_director != null) 
            {
                _director.OnWaveCompleted -= StartNextWaveAfterDelay;
                _director.OnGameReset -= StopAndResetSpawner;
            }
        }

        private void StopAndResetSpawner()
        {
            StopAllCoroutines(); 
        }

        private IEnumerator StartFirstWaveRoutine()
        {
            yield return new WaitForSeconds(1f); 
            StartCoroutine(SpawnWaveRoutine());
        }

        private void StartNextWaveAfterDelay()
        {
            StopAllCoroutines();
            StartCoroutine(WaitAndStartWave());
        }

        private IEnumerator WaitAndStartWave()
        {
            yield return new WaitForSeconds(_timeBetweenWaves);
            StartCoroutine(SpawnWaveRoutine());
        }

        private IEnumerator SpawnWaveRoutine()
        {
            _director.GenerateNextWave(); 
            List<EnemyDefinition> enemiesToSpawn = _director.NextWaveEnemies;
            
            if (enemiesToSpawn == null || enemiesToSpawn.Count == 0)
            {
                _director.OnWaveWon(); 
                yield break;
            }

            OnWaveStarted?.Invoke(enemiesToSpawn.Count);
            _director.NotifyWaveStarted();
            _director.SetSpawningStatus(true);

            foreach (EnemyDefinition enemyData in enemiesToSpawn)
            {
                SpawnEnemy(enemyData);
                float delay = _director.GetSpawnDelay(enemyData.Category);
                if (delay > 0) yield return new WaitForSeconds(delay);
            }

            _director.SetSpawningStatus(false);
        }

        // --- KRİTİK NOKTA ---
        private void SpawnEnemy(EnemyDefinition data)
        {
            EnemyBehaviorController enemy = GetFromPool(data);
            if (enemy == null) return;

            Vector3 randomPos = GetRandomPosition();
            enemy.transform.position = randomPos;
            
            // [DÜZELTME] Rotasyonu burada zorla 'Geriye' (Back) çevirmek yerine, 
            // varsayılan (Identity) bırakıyoruz. Düşman kendi yapay zekasıyla dönsün.
            enemy.transform.rotation = Quaternion.identity; 

            // [ÖNEMLİ] Burada "SetBehavior" gibi bir kod ASLA olmamalı.
            // Sadece Initialize diyoruz, kararı EnemyBehaviorController veriyor.
            enemy.InitializeEnemy(data);

            _director.RegisterEnemy(enemy);
        }
        
        private EnemyBehaviorController GetFromPool(EnemyDefinition data)
        {
            string key = data.name;
            if (!_poolDictionary.ContainsKey(key)) _poolDictionary[key] = new Queue<EnemyBehaviorController>();

            if (_poolDictionary[key].Count > 0)
            {
                EnemyBehaviorController pooled = _poolDictionary[key].Dequeue();
                if (pooled != null) 
                { 
                    pooled.OnReturnToPool = ReturnEnemyToPool; 
                    return pooled; 
                }
            }

            if (data.EnemyPrefab == null) return null;
            GameObject newObj = Instantiate(data.EnemyPrefab, transform);
            var controller = newObj.GetComponent<EnemyBehaviorController>();
            controller.OnReturnToPool = ReturnEnemyToPool;
            newObj.SetActive(false);
            return controller;
        }

        private void ReturnEnemyToPool(EnemyBehaviorController enemy)
        {
            if (enemy == null) return;
            enemy.gameObject.SetActive(false);
            
            var stats = enemy.GetComponent<EnemyStats>();
            if (stats != null && stats.Definition != null)
            {
                string key = stats.Definition.name;
                if (!_poolDictionary.ContainsKey(key)) _poolDictionary[key] = new Queue<EnemyBehaviorController>();
                _poolDictionary[key].Enqueue(enemy);
            }
            else Destroy(enemy.gameObject);
        }

        private Vector3 GetRandomPosition()
        {
            float x = Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2);
            float z = Random.Range(-_spawnAreaSize.z / 2, _spawnAreaSize.z / 2);
            return transform.position + new Vector3(x, 0, z);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(_spawnAreaSize.x, 1, _spawnAreaSize.z));
        }
    }
}