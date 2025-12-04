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
        [Header("AI Bağlantısı")]
        [SerializeField] private SmartWaveManager _director;
        
        [Header("Genel Ayarlar")]
        [SerializeField] private float _timeBetweenWaves = 3f;
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(10, 0, 10);
        
        private Dictionary<string, Queue<EnemyBehaviorController>> _poolDictionary = new Dictionary<string, Queue<EnemyBehaviorController>>();

        public System.Action<int> OnWaveStarted; 

        private void Start()
        {
            if (_director != null)
            {
                _director.OnWaveCompleted += StartNextWaveAfterDelay;
                // [DÜZELTME] Reset durumunda spawner'ı durdurmak için abone ol
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

        // [YENİ] ACİL DURDURMA BUTONU
        private void StopAndResetSpawner()
        {
            StopAllCoroutines(); // O anki spawn işlemini bıçak gibi kes
            Debug.Log("🛑 Spawner: Reset sinyali alındı, üretim iptal edildi.");
        }

        private IEnumerator StartFirstWaveRoutine()
        {
            yield return new WaitForSeconds(1f); 
            StartCoroutine(SpawnWaveRoutine());
        }

        private void StartNextWaveAfterDelay()
        {
            // Eğer oyun resetleniyorsa eski rutinleri temizle, yenisini başlat
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
            
            if (enemiesToSpawn.Count == 0)
            {
                _director.OnWaveWon(); 
                yield break;
            }

            OnWaveStarted?.Invoke(enemiesToSpawn.Count);

            // Manager'a "Üretime Başladım" de
            _director.SetSpawningStatus(true);

            foreach (EnemyDefinition enemyData in enemiesToSpawn)
            {
                SpawnEnemy(enemyData);
                float delay = _director.GetSpawnDelay(enemyData.Category);
                if (delay > 0) yield return new WaitForSeconds(delay);
            }

            // Spawn bitti.
            // [ÖNEMLİ] Eğer bu noktaya geldiysek normal bir bitiş olmuştur.
            _director.SetSpawningStatus(false);
        }

        private void SpawnEnemy(EnemyDefinition data)
        {
            EnemyBehaviorController enemy = GetFromPool(data);
            if (enemy == null) return;

            Vector3 randomPos = GetRandomPosition();
            enemy.transform.position = randomPos;
            enemy.transform.rotation = Quaternion.identity;

            var stats = enemy.GetComponent<EnemyStats>();
            if (stats != null) stats.InitializeRuntime(data);

            enemy.gameObject.SetActive(true);
            _director.RegisterEnemy(enemy);
        }
        
        // --- Pool & Helper Methods (Aynı) ---
        private EnemyBehaviorController GetFromPool(EnemyDefinition data)
        {
            string key = data.name;
            if (!_poolDictionary.ContainsKey(key)) _poolDictionary[key] = new Queue<EnemyBehaviorController>();

            if (_poolDictionary[key].Count > 0)
            {
                EnemyBehaviorController pooled = _poolDictionary[key].Dequeue();
                if (pooled != null) { pooled.OnReturnToPool = ReturnEnemyToPool; return pooled; }
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
    }
}