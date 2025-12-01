using UnityEngine;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D.Data;

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class SmartWaveManager : MonoBehaviour
    {
        [Header("Ayarlar")]
        [SerializeField] private DirectorConfig _config;
        [SerializeField] private EnemyDatabase _enemyDatabase;

        [Header("Oyun Durumu")]
        [SerializeField] private int _currentWaveNumber = 1;
        [SerializeField] private float _currentTotalBudget;

        public List<EnemyDefinition> NextWaveEnemies { get; private set; } = new List<EnemyDefinition>();
        
        // Aktif kuralı sakla ki sürekli arama yapmayalım
        private WaveRule _currentRule;

        private void Start()
        {
            if (_config != null) _currentTotalBudget = _config.StartingBudget;
        }

        // [YENİ] Spawner bu fonksiyonu çağırıp bekleme süresini alacak
        public float GetSpawnDelay(EnemyCategory category)
        {
            if (_config == null) return 1f;

            // Eğer kural boşsa varsayılan değerler döndür
            if (_currentRule.Equals(default(WaveRule))) return 1f;

            switch (category)
            {
                case EnemyCategory.Swarm: return _currentRule.SwarmInterval;
                case EnemyCategory.Rusher: return _currentRule.RusherInterval;
                case EnemyCategory.Tank: return _currentRule.TankInterval;
                default: return 1f;
            }
        }

        [ContextMenu("Test: Generate Wave")]
        public void GenerateNextWave()
        {
            if (_config == null || _enemyDatabase == null) return;

            NextWaveEnemies.Clear();
            
            // 1. Kuralı Bul ve Kaydet
            _currentRule = _config.GetRuleForWave(_currentWaveNumber);
            
            if (_currentRule.Equals(default(WaveRule))) 
            {
                Debug.LogWarning("⚠️ Uygun Kural Bulunamadı! Varsayılanlar kullanılıyor.");
                _currentRule = new WaveRule { SwarmPercent = 100, SwarmInterval = 0.5f };
            }

            // 2. Bütçeyi Böl
            float totalPercent = _currentRule.SwarmPercent + _currentRule.RusherPercent + _currentRule.TankPercent;
            if (totalPercent <= 0) totalPercent = 1;

            float swarmBudget = _currentTotalBudget * (_currentRule.SwarmPercent / totalPercent);
            float rusherBudget = _currentTotalBudget * (_currentRule.RusherPercent / totalPercent);
            float tankBudget = _currentTotalBudget * (_currentRule.TankPercent / totalPercent);

            Debug.Log($"🧮 Dalga {_currentWaveNumber} Hazırlanıyor...");

            // 3. Alışveriş
            FillBudget(swarmBudget, EnemyCategory.Swarm);
            FillBudget(rusherBudget, EnemyCategory.Rusher);
            FillBudget(tankBudget, EnemyCategory.Tank);
            
            // [OPTİMİZASYON] Listeyi karıştır (Shuffle) ki hepsi sırayla gelmesin
            // Önce Tanklar, sonra Sürüler gelmesin; karışık gelsin.
            ShuffleList(NextWaveEnemies);
        }

        private void FillBudget(float budget, EnemyCategory category)
        {
            int safety = 0;
            while (budget > 0 && safety < 500)
            {
                EnemyDefinition enemy = _enemyDatabase.GetEnemyByCategory(category, budget);
                if (enemy != null)
                {
                    NextWaveEnemies.Add(enemy);
                    budget -= enemy.ThreatScore;
                }
                else break;
                safety++;
            }
        }
        
        // Fisher-Yates Shuffle (Liste Karıştırıcı)
        private void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public void OnWaveWon()
        {
            float bonus = _currentTotalBudget * _config.WinGrowthPercentage;
            _currentTotalBudget += bonus;
            _currentWaveNumber++;
            Debug.Log($"🎉 KAZANDIN! Yeni Bütçe: {_currentTotalBudget:F0}");
        }

        public void OnWaveLost()
        {
            float penalty = _currentTotalBudget * _config.LossPenaltyPercentage;
            _currentTotalBudget -= penalty;
            if (_currentTotalBudget < _config.StartingBudget) _currentTotalBudget = _config.StartingBudget;
            Debug.Log($"💀 KAYBETTİN! Bütçe: {_currentTotalBudget:F0}");
        }
    }
}