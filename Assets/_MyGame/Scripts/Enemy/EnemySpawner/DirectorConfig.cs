using UnityEngine;

namespace IndianOceanAssets.Engine2_5D.Data
{
    [CreateAssetMenu(fileName = "AI_Director_Config", menuName = "MyGame/AI Director Config")]
    public class DirectorConfig : ScriptableObject
    {
        [Header("💰 Bütçe Ayarları")]
        [Tooltip("Oyunun en başında AI kaç krediyle başlasın? (Örn: 10 puanlık düşman)")]
        public float BaseCredit = 15f;

        [Tooltip("Her dalgada bütçe ne kadar katlansın? (1.2 = %20 artış)")]
        public float WaveGrowthMultiplier = 1.2f;

        [Header("⚖️ Dinamik Zorluk (Adaptive Difficulty)")]
        [Tooltip("Oyuncu kazandığında zorluk çarpanı ne kadar artsın? (+0.1 = %10 daha zor)")]
        public float DifficultyIncreaseOnWin = 0.1f;

        [Tooltip("Oyuncu kaybettiğinde zorluk çarpanı ne kadar azalsın? (-0.1 = %10 daha kolay)")]
        public float DifficultyDecreaseOnLoss = 0.1f;

        [Tooltip("Zorluk çarpanı en az kaç olabilir? (0.5 altına düşmesin ki oyun çok basitleşmesin)")]
        public float MinDifficultyMultiplier = 0.5f;
    }
}