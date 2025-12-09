using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; 
using IndianOceanAssets.Engine2_5D.Managers;

namespace IndianOceanAssets.Engine2_5D.Spawners
{
    /// <summary>
    /// [TR] Bütçe hesaplayıp düşman spawn eden ve bunları entegre havuz sistemiyle yöneten sınıf.
    /// [EN] Class that spawns enemies based on budget and manages them with an integrated pooling system.
    /// </summary>
    public class BudgetWaveSpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct BudgetWave
        {
            public string WaveName;
            public EnemyDefinition EnemyType; 
            public float BaseBudget;          
            public string OptionalFilterID;   
            public float SpawnInterval;       
            public float DelayBeforeWave;     
        }

        [Header("🌊 Akıllı Dalga Ayarları")]
        [SerializeField] private List<BudgetWave> _waves;
        
        // --- POOLING SİSTEMİ (Değişkenler) ---
        // Her düşman türü (isim bazlı) için ayrı bir kuyruk tutuyoruz.
        private Dictionary<string, Queue<EnemyBehaviorController>> _poolDictionary = new Dictionary<string, Queue<EnemyBehaviorController>>();
        
        // Sahne takibi
        private Dictionary<string, List<BattleSpawnPoint>> _spawnPointsMap = new Dictionary<string, List<BattleSpawnPoint>>();
        private List<BattleSpawnPoint> _allSpawnPoints = new List<BattleSpawnPoint>();

        private void Start()
        {
            RefreshSpawnPoints();
            StartCoroutine(WaveRoutine());
        }

        public void RefreshSpawnPoints()
        {
            _spawnPointsMap.Clear();
            _allSpawnPoints.Clear();

            var points = FindObjectsOfType<BattleSpawnPoint>();
            foreach (var point in points)
            {
                if (!point.gameObject.activeInHierarchy) continue;

                _allSpawnPoints.Add(point);

                if (!_spawnPointsMap.ContainsKey(point.PointID))
                {
                    _spawnPointsMap[point.PointID] = new List<BattleSpawnPoint>();
                }
                _spawnPointsMap[point.PointID].Add(point);
            }
        }

        private IEnumerator WaveRoutine()
        {
            foreach (var wave in _waves)
            {
                if (wave.DelayBeforeWave > 0) yield return new WaitForSeconds(wave.DelayBeforeWave);

                float difficulty = BattleDifficultyManager.Instance != null ? BattleDifficultyManager.Instance.CurrentMultiplier : 1.0f;
                float totalBudget = wave.BaseBudget * difficulty;
                float enemyCost = wave.EnemyType.ThreatScore > 0 ? wave.EnemyType.ThreatScore : 1f;
                
                int countToSpawn = Mathf.RoundToInt(totalBudget / enemyCost);
                if (countToSpawn < 1 && totalBudget > 0) countToSpawn = 1;

                List<BattleSpawnPoint> activeTargets = GetActiveTargets(wave.OptionalFilterID);

                if (activeTargets.Count == 0)
                {
                    Debug.LogWarning($"⚠️ Wave '{wave.WaveName}' için aktif spawn noktası yok!");
                    continue;
                }

                // Debug.Log($"⚔️ Wave: {wave.WaveName} | Adet: {countToSpawn}");

                for (int i = 0; i < countToSpawn; i++)
                {
                    int targetIndex = i % activeTargets.Count;
                    BattleSpawnPoint selectedPoint = activeTargets[targetIndex];

                    // Pool üzerinden spawn et
                    SpawnSingleEnemy(wave.EnemyType, selectedPoint);
                    
                    if (wave.SpawnInterval > 0) yield return new WaitForSeconds(wave.SpawnInterval);
                }
            }
        }

        private List<BattleSpawnPoint> GetActiveTargets(string filterID)
        {
            List<BattleSpawnPoint> candidates;
            if (!string.IsNullOrEmpty(filterID) && _spawnPointsMap.ContainsKey(filterID))
            {
                candidates = _spawnPointsMap[filterID];
            }
            else
            {
                candidates = _allSpawnPoints;
            }
            return candidates.FindAll(x => x != null && x.gameObject.activeInHierarchy);
        }

        // --- POOL MANTIĞI BURADA ---
        private void SpawnSingleEnemy(EnemyDefinition data, BattleSpawnPoint targetPoint)
        {
            if (targetPoint == null || data.EnemyPrefab == null) return;

            // 1. Havuzdan bir obje çek
            EnemyBehaviorController enemy = GetFromPool(data);
            
            // 2. Pozisyon ve Rotasyon ata
            enemy.transform.position = targetPoint.transform.position;
            enemy.transform.rotation = targetPoint.transform.rotation; // Kapının yönüne dönsün

            // 3. Verileri sıfırla ve başlat
            var stats = enemy.GetComponent<EnemyStats>();
            if (stats != null) stats.InitializeRuntime(data); // Canı fullenir, hızı ayarlanır

            var health = enemy.GetComponent<Health>();
            if (health != null) health.ResetHealth(); // Ölü ise canlan

            // 4. Davranışı ayarla
            enemy.gameObject.SetActive(true);
            enemy.SetBehavior(EnemyBehaviorType.Directional);
        }

        /// <summary>
        /// Havuzdan obje çeker, yoksa yenisini üretir.
        /// </summary>
        private EnemyBehaviorController GetFromPool(EnemyDefinition data)
        {
            string key = data.name; // Prefab/Data adını anahtar olarak kullanıyoruz

            // Eğer bu tip için havuz listesi yoksa oluştur
            if (!_poolDictionary.ContainsKey(key))
            {
                _poolDictionary[key] = new Queue<EnemyBehaviorController>();
            }

            // Havuzda bekleyen eleman var mı?
            if (_poolDictionary[key].Count > 0)
            {
                EnemyBehaviorController pooledObj = _poolDictionary[key].Dequeue();
                
                // Güvenlik: Obje sahnede yanlışlıkla silinmiş olabilir mi?
                if (pooledObj != null)
                {
                    // Geri dönüş biletini yenile (Callback)
                    pooledObj.OnReturnToPool = ReturnEnemyToPool;
                    return pooledObj;
                }
            }

            // Havuz boşsa yeni yarat (Instantiate)
            GameObject newObj = Instantiate(data.EnemyPrefab, transform); // Spawner'ın altında toplu dursunlar
            var controller = newObj.GetComponent<EnemyBehaviorController>();
            
            // Bileti ver: "İşin bitince bu metoda dön"
            controller.OnReturnToPool = ReturnEnemyToPool;
            
            newObj.SetActive(false); // Başlangıçta pasif
            return controller;
        }

        /// <summary>
        /// Düşman öldüğünde veya işi bittiğinde buraya geri döner.
        /// </summary>
        private void ReturnEnemyToPool(EnemyBehaviorController enemy)
        {
            // Obje zaten yoksa veya oyun kapanıyorsa uğraşma
            if (enemy == null || gameObject == null) return;

            enemy.gameObject.SetActive(false);
            
            // Hangi listeye ait olduğunu bulmak için Stats'a bakıyoruz
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
                // Verisi kayıpsa (Hata durumu) yok et gitsin
                Destroy(enemy.gameObject);
            }
        }
    }
}