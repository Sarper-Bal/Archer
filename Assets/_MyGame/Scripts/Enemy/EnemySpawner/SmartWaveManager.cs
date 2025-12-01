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
        public float SpawnInterval => _config != null ? _config.TimeBetweenSpawns : 1f; // Spawner bunu okuyacak

        private void Start()
        {
            if (_config != null) _currentTotalBudget = _config.StartingBudget;
        }

        [ContextMenu("Test: Generate Wave")]
        public void GenerateNextWave()
        {
            if (_config == null || _enemyDatabase == null) return;

            NextWaveEnemies.Clear();
            
            // 1. Kuralı Bul (Hangi yüzdeyi kullanacağız?)
            WaveRule rule = _config.GetRuleForWave(_currentWaveNumber);
            
            // Eğer hiç kural yoksa varsayılan bir tane uydur (Hata almamak için)
            if (rule.Equals(default(WaveRule))) 
            {
                Debug.LogWarning("⚠️ Uygun Kural Bulunamadı! Varsayılan %100 Swarm kullanılıyor.");
                rule = new WaveRule { SwarmPercent = 100, RusherPercent = 0, TankPercent = 0 };
            }

            // 2. Bütçeyi Böl (Matematik)
            // Yüzdeleri topla (Kullanıcı 100 yapmadıysa biz normalize ederiz)
            float totalPercent = rule.SwarmPercent + rule.RusherPercent + rule.TankPercent;
            if (totalPercent <= 0) totalPercent = 1; // Bölme hatası önlemi

            float swarmBudget = _currentTotalBudget * (rule.SwarmPercent / totalPercent);
            float rusherBudget = _currentTotalBudget * (rule.RusherPercent / totalPercent);
            float tankBudget = _currentTotalBudget * (rule.TankPercent / totalPercent);

            Debug.Log($"🧮 Dalga {_currentWaveNumber} Planı: %{rule.SwarmPercent} Swarm, %{rule.RusherPercent} Rusher, %{rule.TankPercent} Tank");

            // 3. Alışverişe Başla
            FillBudget(swarmBudget, EnemyCategory.Swarm);
            FillBudget(rusherBudget, EnemyCategory.Rusher);
            FillBudget(tankBudget, EnemyCategory.Tank);
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