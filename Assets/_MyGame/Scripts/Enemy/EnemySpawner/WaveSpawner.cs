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
        private List<EnemyBehaviorController> _activeEnemies = new List<EnemyBehaviorController>();
        
        private WaitForSeconds _checkInterval = new WaitForSeconds(0.5f); 

        public System.Action<int> OnWaveStarted; 
        public System.Action OnWaveCleared;

        private void Start()
        {
            if (_director != null) StartCoroutine(GameLoopRoutine());
        }

        private IEnumerator GameLoopRoutine()
        {
            while (true)
            {
                // 1. HAZIRLIK
                _director.GenerateNextWave(); 
                List<EnemyDefinition> enemiesToSpawn = _director.NextWaveEnemies;
                
                if (enemiesToSpawn.Count == 0)
                {
                    yield return new WaitForSeconds(1f);
                    continue; 
                }

                OnWaveStarted?.Invoke(enemiesToSpawn.Count);

                // 2. DİNAMİK SPAWN DÖNGÜSÜ
                foreach (EnemyDefinition enemyData in enemiesToSpawn)
                {
                    SpawnEnemy(enemyData);

                    // [YENİ] AI'ya sor: Bu düşman türü için kaç saniye bekleyeyim?
                    float delay = _director.GetSpawnDelay(enemyData.Category);
                    
                    // Eğer 0 ise bekleme (Frame atlamasın diye null check yapılabilir)
                    if (delay > 0) yield return new WaitForSeconds(delay);
                }

                // 3. BEKLEME (Son düşman ölene kadar)
                Debug.Log("⚔️ Spawn bitti, savaş devam ediyor...");
                while (_activeEnemies.Count > 0)
                {
                    yield return _checkInterval; 
                }

                // 4. BİTİŞ
                _director.OnWaveWon(); 
                OnWaveCleared?.Invoke();
                yield return new WaitForSeconds(_timeBetweenWaves);
            }
        }

        // --- SpawnEnemy, GetFromPool, ReturnEnemyToPool, GetRandomPosition ---
        // (Bu metotlar değişmedi, önceki kodun aynısı kalacak)
        
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
            _activeEnemies.Add(enemy);
        }
        
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
            if (this == null) return;
            if (_activeEnemies.Contains(enemy)) _activeEnemies.Remove(enemy);
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