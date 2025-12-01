using UnityEngine;

namespace IndianOceanAssets.Engine2_5D.Data
{
    [CreateAssetMenu(fileName = "AI_Director_Config", menuName = "MyGame/AI Director Config")]
    public class DirectorConfig : ScriptableObject
    {
        // -----------------------------------------------------------------------
        // BÖLÜM 1: TEMEL EKONOMİ
        // -----------------------------------------------------------------------
        [Header("💰 Ekonomi ve Enflasyon")]
        [Tooltip("Oyunun en başında (Dalga 1) AI'nın cebinde kaç kredi olsun? \n(Örn: 1 Slime = 1 Kredi)")]
        public float BaseCredit = 15f;

        [Tooltip("Her dalgada AI'nın parası yüzde kaç artsın? \n1.2 = %20 Artış (Agresif) \n1.1 = %10 Artış (Dengeli)")]
        [Range(1.0f, 2.0f)] 
        public float WaveGrowthMultiplier = 1.2f;

        // -----------------------------------------------------------------------
        // BÖLÜM 2: DİNAMİK ZORLUK (OYUNCUYA GÖRE)
        // -----------------------------------------------------------------------
        [Header("⚖️ Dinamik Zorluk Dengesi")]
        [Tooltip("Oyuncu bir dalgayı geçtiğinde oyun ne kadar zorlaşsın? (+0.1 = %10 Ekstra Bütçe)")]
        [Range(0f, 1f)]
        public float DifficultyIncreaseOnWin = 0.1f;

        [Tooltip("Oyuncu ÖLDÜĞÜNDE oyun ne kadar kolaylaşsın? (-0.1 = %10 İndirim)")]
        [Range(0f, 1f)]
        public float DifficultyDecreaseOnLoss = 0.1f;

        [Tooltip("Oyunun düşebileceği en kolay seviye çarpanı. \n(0.5 yaparsan bütçe asla yarı fiyatının altına düşmez)")]
        public float MinDifficultyMultiplier = 0.5f;

        [Tooltip("Oyunun çıkabileceği en zor seviye çarpanı. \n(3.0 yaparsan bütçe normalin 3 katına kadar çıkabilir)")]
        public float MaxDifficultyMultiplier = 3.0f;

        // -----------------------------------------------------------------------
        // BÖLÜM 3: SİMÜLASYON (GELECEĞİ GÖR)
        // -----------------------------------------------------------------------
        [Header("🔮 Geleceği Gör (Simülasyon)")]
        [Tooltip("Merak ettiğin dalga numarasını buraya yaz.")]
        [SerializeField] private int _testWaveNumber = 5;

        [Tooltip("Varsayılan zorluk çarpanı (1.0 = Normal, 1.5 = Zor)")]
        [SerializeField] private float _testDifficulty = 1.0f;

        [Space(10)]
        [Tooltip("Bu dalgada AI'nın harcayacağı toplam para (Tahmini)")]
        [SerializeField] private float _predictedBudget = 0f; // ReadOnly (Bilgi amaçlı)

        [Tooltip("Bu parayla yaklaşık kaç tane 'Slime' (1 Puanlık) alabilir?")]
        [SerializeField] private int _buyingPowerSlime = 0;

        [Tooltip("Bu parayla yaklaşık kaç tane 'Golem' (15 Puanlık) alabilir?")]
        [SerializeField] private int _buyingPowerGolem = 0;

        // --- EDİTÖR HESAPLAMASI ---
        private void OnValidate()
        {
            CalculatePrediction();
        }

        private void CalculatePrediction()
        {
            if (_testWaveNumber < 1) _testWaveNumber = 1;

            // Formül: Başlangıç * (Büyüme ^ (Dalga-1)) * Zorluk
            float waveFactor = Mathf.Pow(WaveGrowthMultiplier, _testWaveNumber - 1);
            float total = BaseCredit * waveFactor * _testDifficulty;

            _predictedBudget = Mathf.Round(total * 10f) / 10f; // Yuvarla

            // Alım gücü örnekleri (Slime=1, Golem=15 varsayıyoruz)
            _buyingPowerSlime = Mathf.FloorToInt(_predictedBudget / 1f);
            _buyingPowerGolem = Mathf.FloorToInt(_predictedBudget / 15f);
        }
    }
}