using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace IndianOceanAssets.Engine2_5D.Data
{
    [System.Serializable]
    public struct WaveRule
    {
        [Tooltip("Bu kuralın geçerli olması için oyuncunun HANGİ DALGAYA ulaşması gerekir? (Örn: 5 yazarsan, oyuncu 5. dalgaya gelene kadar bu kural çalışmaz).")]
        public int MinWinWave; 

        [Header("📊 Dağılım Oranları (Toplam 100 olmalı)")]
        [Range(0, 100)] public float SwarmPercent;
        [Range(0, 100)] public float RusherPercent;
        [Range(0, 100)] public float TankPercent;

        [Header("⏱️ Spawn Aralıkları (Saniye)")]
        [Tooltip("Sürü (Swarm) spawn olduktan sonra kaç saniye beklensin?")]
        public float SwarmInterval;
        
        [Tooltip("Baskıncı (Rusher) spawn olduktan sonra kaç saniye beklensin?")]
        public float RusherInterval;
        
        [Tooltip("Tank spawn olduktan sonra kaç saniye beklensin?")]
        public float TankInterval;
    }

    [CreateAssetMenu(fileName = "AI_Director_Config", menuName = "MyGame/AI Director Config")]
    public class DirectorConfig : ScriptableObject
    {
        [Header("💰 Kümülatif Bütçe")]
        public float StartingBudget = 100f;
        
        [Header("📈 Kazanma/Kaybetme")]
        [Range(0f, 1f)] public float WinGrowthPercentage = 0.20f;
        [Range(0f, 1f)] public float LossPenaltyPercentage = 0.10f;

        [Header("📜 Dalga Kuralları (Sıralı Liste)")]
        public List<WaveRule> WaveRules = new List<WaveRule>();

        // --- YARDIMCI METOT ---
        public WaveRule GetRuleForWave(int currentLevelReached)
        {
            // Elimizdeki seviyeye (currentLevelReached) eşit veya küçük olan EN BÜYÜK kuralı bul.
            // Örnek: Seviye 7 ise ve kurallar 1, 5, 10 ise -> 5. seviye kuralını getirir.
            return WaveRules
                .Where(r => r.MinWinWave <= currentLevelReached)
                .OrderByDescending(r => r.MinWinWave)
                .FirstOrDefault();
        }
    }
}