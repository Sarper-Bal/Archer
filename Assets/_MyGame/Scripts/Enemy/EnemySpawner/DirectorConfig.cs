using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace IndianOceanAssets.Engine2_5D.Data
{
    [System.Serializable]
    public struct WaveRule
    {
        [Tooltip("Bu kural hangi dalgada başlasın?")]
        public int MinWave; 

        [Header("Dağılım Oranları (Toplam 100 olmalı)")]
        [Range(0, 100)] public float SwarmPercent;
        [Range(0, 100)] public float RusherPercent;
        [Range(0, 100)] public float TankPercent;
    }

    [CreateAssetMenu(fileName = "AI_Director_Config", menuName = "MyGame/AI Director Config")]
    public class DirectorConfig : ScriptableObject
    {
        [Header("💰 Kümülatif Bütçe")]
        public float StartingBudget = 100f;
        
        [Header("📈 Kazanma/Kaybetme")]
        [Range(0f, 1f)] public float WinGrowthPercentage = 0.20f;
        [Range(0f, 1f)] public float LossPenaltyPercentage = 0.10f;

        [Header("⏱️ Spawn Hızı")]
        [Tooltip("Düşmanlar arası bekleme süresi (Saniye). Düşük = Hızlı Spawn")]
        public float TimeBetweenSpawns = 0.5f;

        [Header("📜 Dalga Kuralları (Sıralı Liste)")]
        [Tooltip("Dalgaya özel düşman dağılımlarını buradan ayarla.")]
        public List<WaveRule> WaveRules = new List<WaveRule>();

        // --- YARDIMCI METOT: O anki kuralı bul ---
        public WaveRule GetRuleForWave(int currentWave)
        {
            // Mevcut dalgadan küçük veya eşit olan en son kuralı (en yüksek MinWave'liyi) bul
            // Örn: Rules=[1, 5, 10]. Current=7 ise -> 5. kuralı döndürür.
            return WaveRules
                .Where(r => r.MinWave <= currentWave)
                .OrderByDescending(r => r.MinWave)
                .FirstOrDefault();
        }
    }
}