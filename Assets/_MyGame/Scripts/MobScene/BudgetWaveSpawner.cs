using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; // EnemyDefinition ve Controller için
using IndianOceanAssets.Engine2_5D.Managers; // DifficultyManager için

namespace IndianOceanAssets.Engine2_5D.Spawners
{
    /// <summary>
    /// [TR] Verilen puan bütçesine göre düşman sayısını hesaplayıp spawn eden sistem.
    /// [EN] System that calculates enemy count based on budget and spawns them.
    /// </summary>
    public class BudgetWaveSpawner : MonoBehaviour
    {
        // Inspector'da dalga tasarlamak için basit yapı
        [System.Serializable]
        public struct BudgetWave
        {
            public string WaveName;
            public EnemyDefinition EnemyType; // Hangi düşman?
            public float BaseBudget;          // Kaç puanlık? (Örn: 100)
            public string TargetSpawnPointID; // Hangi kapıdan? (Boş bırakılırsa rastgele)
            public float SpawnInterval;       // Pıtır pıtır çıkma hızı
            public float DelayBeforeWave;     // Başlamadan önceki bekleme
        }

        [Header("🌊 Dalga Ayarları")]
        [SerializeField] private List<BudgetWave> _waves;
        
        // Sahnedeki spawn noktalarını tutan sözlük
        private Dictionary<string, List<BattleSpawnPoint>> _spawnPointsMap = new Dictionary<string, List<BattleSpawnPoint>>();
        private List<BattleSpawnPoint> _allSpawnPoints = new List<BattleSpawnPoint>();

        private void Start()
        {
            // 1. Sahnedeki tüm spawn noktalarını bul ve kaydet
            RegisterSpawnPoints();

            // 2. Dalga döngüsünü başlat
            StartCoroutine(WaveRoutine());
        }

        private void RegisterSpawnPoints()
        {
            var points = FindObjectsOfType<BattleSpawnPoint>();
            foreach (var point in points)
            {
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
                // Bekleme süresi
                if (wave.DelayBeforeWave > 0) yield return new WaitForSeconds(wave.DelayBeforeWave);

                // --- HESAPLAMA ANI ---
                float difficulty = 1.0f;
                if (BattleDifficultyManager.Instance != null)
                {
                    difficulty = BattleDifficultyManager.Instance.CurrentMultiplier;
                }

                // 1. Formül: Bütçe * Zorluk
                float totalBudget = wave.BaseBudget * difficulty;

                // 2. Düşman Maliyeti (Threat Score)
                float enemyCost = wave.EnemyType.ThreatScore;
                if (enemyCost <= 0) enemyCost = 1; // Sıfıra bölünme hatası önlemi

                // 3. Adet Hesapla (Option 2: Yakına Yuvarla)
                int countToSpawn = Mathf.RoundToInt(totalBudget / enemyCost);
                
                // En az 1 tane spawn olsun (eğer bütçe çok düşükse bile)
                if (countToSpawn < 1 && totalBudget > 0) countToSpawn = 1;

                Debug.Log($"⚔️ Wave: {wave.WaveName} | Bütçe: {totalBudget} | Adet: {countToSpawn}");

                // --- SPAWN İŞLEMİ ---
                for (int i = 0; i < countToSpawn; i++)
                {
                    SpawnSingleEnemy(wave.EnemyType, wave.TargetSpawnPointID);
                    
                    // Aralıklarla spawn et (Interval)
                    if (wave.SpawnInterval > 0) yield return new WaitForSeconds(wave.SpawnInterval);
                }
            }
        }

        private void SpawnSingleEnemy(EnemyDefinition data, string pointID)
        {
            // Hedef noktayı bul
            BattleSpawnPoint targetPoint = GetSpawnPoint(pointID);
            if (targetPoint == null) return;

            // --- Basit Instantiate (Pooling daha sonra entegre edilebilir) ---
            // Not: Senin projende Pool sistemi var, burayı ona bağlayabiliriz. 
            // Şimdilik mantığı göstermek için Instantiate kullanıyorum.
            GameObject obj = Instantiate(data.EnemyPrefab, targetPoint.transform.position, targetPoint.transform.rotation);
            
            // Gerekli bileşenleri al
            var controller = obj.GetComponent<EnemyBehaviorController>();
            var stats = obj.GetComponent<EnemyStats>();

            // Datayı yükle
            if (stats != null) stats.InitializeRuntime(data);

            // [KRİTİK] Düşmanı kapının baktığı yöne (Sana doğru) yolla
            if (controller != null)
            {
                obj.SetActive(true);
                // Directional moda zorla
                controller.SetBehavior(EnemyBehaviorType.Directional);
                
                // Directional Mover'ın yönünü kapının yönü olarak ayarla (Burası önemli!)
                // Bu kısım DirectionalEnemyMover'ın yeni koduna uyumludur (transform.forward kullanır)
                obj.transform.rotation = targetPoint.transform.rotation;
            }
        }

        private BattleSpawnPoint GetSpawnPoint(string id)
        {
            // Eğer ID boşsa rastgele bir nokta seç
            if (string.IsNullOrEmpty(id))
            {
                if (_allSpawnPoints.Count > 0) 
                    return _allSpawnPoints[Random.Range(0, _allSpawnPoints.Count)];
                return null;
            }

            // ID'ye uygun listeden birini seç
            if (_spawnPointsMap.ContainsKey(id))
            {
                var list = _spawnPointsMap[id];
                return list[Random.Range(0, list.Count)];
            }

            Debug.LogWarning($"⚠️ Spawn Point ID bulunamadı: {id}");
            return null;
        }
    }
}