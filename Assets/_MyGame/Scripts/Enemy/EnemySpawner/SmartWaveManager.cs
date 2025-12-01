using UnityEngine;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D.Data;

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class SmartWaveManager : MonoBehaviour
    {
        [Header("Beyin ve Katalog")]
        [SerializeField] private DirectorConfig _config;
        [SerializeField] private EnemyDatabase _enemyDatabase;

        [Header("Oyun Durumu (Debug için görüntülenebilir)")]
        [SerializeField] private int _currentWaveNumber = 1;
        [SerializeField] private float _currentDifficultyMultiplier = 1.0f;
        
        // Bu listeyi Spawner okuyacak
        public List<EnemyDefinition> NextWaveEnemies { get; private set; } = new List<EnemyDefinition>();

        // Test etmek için sağ tık menüsü
        [ContextMenu("Test: Generate Next Wave")]
        public void GenerateNextWave()
        {
            if (_config == null || _enemyDatabase == null)
            {
                Debug.LogError("⚠️ SmartWaveManager: Config veya Database eksik!");
                return;
            }

            NextWaveEnemies.Clear();

            // 1. Bütçeyi Hesapla
            // Formül: BaşlangıçParası * (DalgaBüyümesi ^ DalgaSayısı) * ZorlukÇarpanı
            float waveFactor = Mathf.Pow(_config.WaveGrowthMultiplier, _currentWaveNumber - 1);
            float totalBudget = _config.BaseCredit * waveFactor * _currentDifficultyMultiplier;

            Debug.Log($"🧮 [AI Director] Dalga: {_currentWaveNumber} | Bütçe: {totalBudget:F1} (Zorluk: {_currentDifficultyMultiplier})");

            // 2. Alışveriş Yap (Bütçe bitene kadar düşman seç)
            float remainingBudget = totalBudget;
            int safetyBreak = 0; // Sonsuz döngü koruması

            while (remainingBudget > 0 && safetyBreak < 1000)
            {
                // Paramızın yettiği en pahalı (veya rastgele uygun) düşmanı bul
                EnemyDefinition candidate = _enemyDatabase.GetEnemyByCost(remainingBudget);

                if (candidate != null)
                {
                    NextWaveEnemies.Add(candidate);
                    remainingBudget -= candidate.ThreatScore;
                    // Debug.Log($"   🛒 Satın alındı: {candidate.name} (-{candidate.ThreatScore})");
                }
                else
                {
                    // Paramız en ucuz düşmana bile yetmiyor, alışveriş bitti.
                    break;
                }
                safetyBreak++;
            }

            Debug.Log($"✅ Alışveriş Tamamlandı! Toplam {NextWaveEnemies.Count} düşman sahneye sürülecek.");
        }

        // --- OYUN DÖNGÜSÜ METOTLARI ---

        public void OnWaveWon()
        {
            _currentWaveNumber++;
            _currentDifficultyMultiplier += _config.DifficultyIncreaseOnWin;
            Debug.Log("🎉 Dalga Kazanıldı! Zorluk arttırıldı.");
        }

        public void OnWaveLost() // Player öldüğünde çağırılacak
        {
            // Dalga sayısı artmaz, aynı dalgayı tekrar deneriz
            _currentDifficultyMultiplier -= _config.DifficultyDecreaseOnLoss;
            
            // Alt limit kontrolü
            if (_currentDifficultyMultiplier < _config.MinDifficultyMultiplier)
                _currentDifficultyMultiplier = _config.MinDifficultyMultiplier;

            Debug.Log("💀 Kaybedildi. Zorluk düşürüldü, aynı dalga tekrar hazırlanacak.");
        }
    }
}