using UnityEngine;
using System.Collections.Generic;
using ArcadeBridge.ArcadeIdleEngine.Spawners; // EnemyPool için

namespace IndianOceanAssets.Engine2_5D.Spawners
{
    // Tek bir dalganın içindeki bir düşman grubunu tanımlar
    // Örn: "5 tane Orc, her biri 0.5 saniye arayla"
    [System.Serializable]
    public class WaveGroup
    {
        [Tooltip("Hangi düşman havuzundan üretilecek?")]
        public EnemyPool EnemyPool; 
        
        [Tooltip("Kaç tane üretilecek?")]
        public int Count = 5;
        
        [Tooltip("İki düşman üretimi arasındaki bekleme süresi (sn)")]
        public float DelayBetweenSpawns = 0.5f;
    }

    // Bir Dalganın kendisi (Örn: Wave 1)
    [System.Serializable]
    public class WaveDefinition
    {
        [Tooltip("Bu dalganın adı (Editörde kolay okumak için)")]
        public string WaveName = "Wave 1";

        [Tooltip("Bu dalgada oluşacak düşman grupları")]
        public List<WaveGroup> Groups = new List<WaveGroup>();

        [Header("Bitiş Koşulu")]
        [Tooltip("Tüm düşmanlar üretildikten sonra, bir sonraki dalgaya geçmek için oyuncunun hepsini öldürmesini bekleyelim mi?")]
        public bool WaitForAllDead = true;

        [Tooltip("Dalga bittikten (veya hepsi öldükten) sonraki dalgaya geçmeden önceki mola süresi")]
        public float TimeToNextWave = 3.0f;
    }

    // Bütün bölümün senaryosu (Level 1 Config)
    [CreateAssetMenu(fileName = "NewWaveConfig", menuName = "MyGame/Spawners/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        [Header("Bölüm Ayarları")]
        public List<WaveDefinition> Waves = new List<WaveDefinition>();
        public bool LoopWaves = false; // Dalgalar bitince başa dönsün mü?
    }
}