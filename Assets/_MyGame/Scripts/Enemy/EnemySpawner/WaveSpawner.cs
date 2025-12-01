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

        [Header("⏱️ Dalga Ayarları")]
        [Tooltip("Bir dalganın spawn olma süresi (Saniye). Düşmanlar bu süreye yayılır.")]
        [SerializeField] private float _waveDuration = 60f;
        
        [Tooltip("İki dalga arasındaki dinlenme süresi.")]
        [SerializeField] private float _timeBetweenWaves = 5f;

        [Header("📍 Alan Ayarları")]
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(10, 0, 10);
        
        // Multi-Pool: Her düşman tipi için ayrı havuz
        private Dictionary<string, Queue<EnemyBehaviorController>> _poolDictionary = new Dictionary<string, Queue<EnemyBehaviorController>>();
        
        // Aktif düşmanlar
        private List<EnemyBehaviorController> _activeEnemies = new List<EnemyBehaviorController>();

        // Cache
        private WaitForSeconds _checkInterval = new WaitForSeconds(1f); 
        private bool _isWaveActive = false;

        public System.Action<int> OnWaveStarted; 
        public System.Action OnWaveCleared;

        private void Start()
        {
            if (_director == null)
            {
                Debug.LogError("⚠️ WaveSpawner: SmartWaveManager (Director) atanmamış!");
                return;
            }

            StartCoroutine(GameLoopRoutine());
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
                    Debug.LogWarning("⚠️ AI Director boş liste gönderdi. Tekrar deneniyor...");
                    yield return new WaitForSeconds(2f);
                    continue; 
                }

                OnWaveStarted?.Invoke(enemiesToSpawn.Count);
                _isWaveActive = true;

                // 2. SAVAŞ (Zamana yayarak spawn et)
                float spawnDelay = _waveDuration / (float)enemiesToSpawn.Count;
                WaitForSeconds waitDelay = new WaitForSeconds(spawnDelay);

                foreach (EnemyDefinition enemyData in enemiesToSpawn)
                {
                    SpawnEnemy(enemyData);
                    yield return waitDelay; 
                }

                // 3. BEKLEME
                Debug.Log("⏳ Spawn bitti, temizlik bekleniyor...");
                while (_activeEnemies.Count > 0)
                {
                    yield return _checkInterval; 
                }

                // 4. ZAFER
                _isWaveActive = false;
                _director.OnWaveWon(); 
                OnWaveCleared?.Invoke();

                Debug.Log($"🎉 Dalga Temizlendi! {_timeBetweenWaves} saniye mola...");
                yield return new WaitForSeconds(_timeBetweenWaves);
            }
        }

        private void SpawnEnemy(EnemyDefinition data)
        {
            // Havuzdan çek veya yeni yarat (Artık data içindeki Prefab'ı kullanıyor)
            EnemyBehaviorController enemy = GetFromPool(data);

            if (enemy == null) return; // Hata varsa çık

            // Pozisyonla
            Vector3 randomPos = GetRandomPosition();
            enemy.transform.position = randomPos;
            enemy.transform.rotation = Quaternion.identity;

            // [GÜNCELLEME] İstatistikleri Yükle (Runtime Init)
            var stats = enemy.GetComponent<EnemyStats>();
            if (stats != null)
            {
                // EnemyStats scriptine eklediğimiz InitializeRuntime metodunu çağırıyoruz
                // Eğer hata alırsan EnemyStats scriptini güncellemen gerekir.
                stats.InitializeRuntime(data);
            }

            // Devriye rotası varsa ata
            if (data.DefaultBehavior == EnemyBehaviorType.Patrol && data.PatrolRouteID != null)
            {
                // RouteManager entegrasyonu buraya gelecek
            }

            enemy.gameObject.SetActive(true);
            _activeEnemies.Add(enemy);
        }

        private EnemyBehaviorController GetFromPool(EnemyDefinition data)
        {
            string key = data.name; 

            // 1. Havuz var mı?
            if (!_poolDictionary.ContainsKey(key))
            {
                _poolDictionary[key] = new Queue<EnemyBehaviorController>();
            }

            // 2. Havuzda eleman var mı?
            if (_poolDictionary[key].Count > 0)
            {
                EnemyBehaviorController pooledEnemy = _poolDictionary[key].Dequeue();
                if (pooledEnemy != null) 
                {
                    pooledEnemy.OnReturnToPool = ReturnEnemyToPool;
                    return pooledEnemy;
                }
            }

            // 3. YENİ YARATMA (Burayı düzelttik!)
            // Eskiden Resources.Load yapıyorduk, şimdi data.EnemyPrefab kullanıyoruz.
            
            if (data.EnemyPrefab == null)
            {
                Debug.LogError($"🛑 HATA: '{data.name}' isimli Düşman Verisinde (ScriptableObject) 'Enemy Prefab' boş! Lütfen Inspector'dan atayın.");
                return null;
            }

            // Direkt prefabdan yarat
            // NOT: Prefabın üzerinde EnemyBehaviorController componenti olduğundan emin ol.
            GameObject newObj = Instantiate(data.EnemyPrefab, transform);
            
            var controller = newObj.GetComponent<EnemyBehaviorController>();
            if (controller == null)
            {
                 Debug.LogError($"🛑 HATA: '{data.EnemyPrefab.name}' prefabında 'EnemyBehaviorController' scripti yok!");
                 return null;
            }

            controller.OnReturnToPool = ReturnEnemyToPool;
            newObj.SetActive(false); 
            
            return controller;
        }

        private void ReturnEnemyToPool(EnemyBehaviorController enemy)
        {
            if (this == null) return;

            if (_activeEnemies.Contains(enemy)) _activeEnemies.Remove(enemy);

            enemy.gameObject.SetActive(false);

            // Kimliğini kontrol edip doğru rafa koy
            var stats = enemy.GetComponent<EnemyStats>();
            if (stats != null && stats.Definition != null)
            {
                string key = stats.Definition.name;
                if (!_poolDictionary.ContainsKey(key))
                    _poolDictionary[key] = new Queue<EnemyBehaviorController>();

                _poolDictionary[key].Enqueue(enemy);
            }
            else
            {
                Destroy(enemy.gameObject); 
            }
        }

        private Vector3 GetRandomPosition()
        {
            float x = Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2);
            float z = Random.Range(-_spawnAreaSize.z / 2, _spawnAreaSize.z / 2);
            return transform.position + new Vector3(x, 0, z);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isWaveActive ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, _spawnAreaSize);
        }
    }
}