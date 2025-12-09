using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; 
using IndianOceanAssets.Engine2_5D.Managers;

namespace IndianOceanAssets.Engine2_5D.Spawners
{
    public class BudgetWaveSpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct BudgetWave
        {
            public string WaveName;
            public EnemyDefinition EnemyType; 
            public float BaseBudget;          
            
            [Tooltip("BOŞ BIRAKIRSAN: Sahnedeki tüm aktif kapılardan eşit dağıtarak çıkar.\nBİR ID YAZARSAN: Sadece o ID'ye sahip kapılardan çıkar (Örn: 'SolKapi').")]
            public string OptionalFilterID;   // [GÜNCELLEME] Artık opsiyonel filtre
            
            public float SpawnInterval;       
            public float DelayBeforeWave;     
        }

        [Header("🌊 Akıllı Dalga Ayarları")]
        [SerializeField] private List<BudgetWave> _waves;
        
        // Sahnedeki noktaları takip eden listeler
        private Dictionary<string, List<BattleSpawnPoint>> _spawnPointsMap = new Dictionary<string, List<BattleSpawnPoint>>();
        private List<BattleSpawnPoint> _allSpawnPoints = new List<BattleSpawnPoint>();

        private void Start()
        {
            RefreshSpawnPoints(); // Başlangıçta kapıları bul
            StartCoroutine(WaveRoutine());
        }

        /// <summary>
        /// Sahnedeki BattleSpawnPoint'leri bulur ve hafızaya alır.
        /// </summary>
        public void RefreshSpawnPoints()
        {
            _spawnPointsMap.Clear();
            _allSpawnPoints.Clear();

            var points = FindObjectsOfType<BattleSpawnPoint>();
            foreach (var point in points)
            {
                // Sadece aktif objeleri listeye al
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

                // --- 1. BÜTÇE HESABI ---
                float difficulty = BattleDifficultyManager.Instance != null ? BattleDifficultyManager.Instance.CurrentMultiplier : 1.0f;
                float totalBudget = wave.BaseBudget * difficulty;
                
                float enemyCost = wave.EnemyType.ThreatScore > 0 ? wave.EnemyType.ThreatScore : 1f;
                int countToSpawn = Mathf.RoundToInt(totalBudget / enemyCost);
                if (countToSpawn < 1 && totalBudget > 0) countToSpawn = 1;

                // --- 2. HEDEF KAPILARI BELİRLE ---
                List<BattleSpawnPoint> activeTargets = GetActiveTargets(wave.OptionalFilterID);

                if (activeTargets.Count == 0)
                {
                    Debug.LogWarning($"⚠️ Wave '{wave.WaveName}' için aktif spawn noktası bulunamadı! Atlanıyor.");
                    continue;
                }

                Debug.Log($"⚔️ Wave: {wave.WaveName} | Adet: {countToSpawn} | Aktif Kapı: {activeTargets.Count}");

                // --- 3. DAĞITIMLI SPAWN (SMART DISTRIBUTION) ---
                for (int i = 0; i < countToSpawn; i++)
                {
                    // [MATEMATİK] Modulo (%) operatörü ile kapıları sırayla gez (0, 1, 2, 0, 1, 2...)
                    int targetIndex = i % activeTargets.Count;
                    BattleSpawnPoint selectedPoint = activeTargets[targetIndex];

                    // Seçilen noktadan spawn et
                    SpawnSingleEnemy(wave.EnemyType, selectedPoint);
                    
                    if (wave.SpawnInterval > 0) yield return new WaitForSeconds(wave.SpawnInterval);
                }
            }
        }

        /// <summary>
        /// Filtreye göre veya genel havuzdan AKTİF olan noktaları döndürür.
        /// </summary>
        private List<BattleSpawnPoint> GetActiveTargets(string filterID)
        {
            List<BattleSpawnPoint> candidates;

            // Filtre var mı?
            if (!string.IsNullOrEmpty(filterID) && _spawnPointsMap.ContainsKey(filterID))
            {
                candidates = _spawnPointsMap[filterID];
            }
            else
            {
                // Filtre yoksa hepsini al
                candidates = _allSpawnPoints;
            }

            // [GÜVENLİK] Listenin içindeki objeler yok olmuş veya kapanmış olabilir, temizle.
            // Bu basit LINQ sorgusu null olmayan ve aktif olanları filtreler.
            return candidates.FindAll(x => x != null && x.gameObject.activeInHierarchy);
        }

        private void SpawnSingleEnemy(EnemyDefinition data, BattleSpawnPoint targetPoint)
        {
            if (targetPoint == null) return;

            // Instantiate (Veya ilerde Pool)
            GameObject obj = Instantiate(data.EnemyPrefab, targetPoint.transform.position, targetPoint.transform.rotation);
            
            var controller = obj.GetComponent<EnemyBehaviorController>();
            var stats = obj.GetComponent<EnemyStats>();

            if (stats != null) stats.InitializeRuntime(data);

            if (controller != null)
            {
                obj.SetActive(true);
                controller.SetBehavior(EnemyBehaviorType.Directional);
                // Düşmanın yönünü kapının baktığı yöne çevir
                obj.transform.rotation = targetPoint.transform.rotation;
            }
        }
    }
}