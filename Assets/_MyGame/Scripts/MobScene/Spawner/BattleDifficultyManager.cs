using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Data.Variables; // Eğer değişken kullanmak istersen

namespace IndianOceanAssets.Engine2_5D.Managers
{
    /// <summary>
    /// [TR] Oyunun dinamik zorluk seviyesini yöneten ve kaydeden sınıf.
    /// [EN] Class that manages and saves the dynamic difficulty level of the game.
    /// </summary>
    public class BattleDifficultyManager : MonoBehaviour
    {
        public static BattleDifficultyManager Instance { get; private set; }

        [Header("⚙️ Ayarlar")]
        [Tooltip("Başlangıç zorluk çarpanı (1.0 = Normal Bütçe)")]
        [SerializeField] private float _startingMultiplier = 1.0f;
        
        [Tooltip("Zorluk asla bu değerin altına düşmez.")]
        [SerializeField] private float _minMultiplier = 0.8f;

        [Header("📈 Değişim Oranları")]
        [Tooltip("Oyuncu kazandığında zorluk ne kadar artsın? (0.1 = %10 Artış)")]
        [SerializeField] private float _winDifficultyIncrease = 0.1f;
        
        [Tooltip("Oyuncu kaybettiğinde zorluk ne kadar düşsün? (0.05 = %5 Düşüş)")]
        [SerializeField] private float _lossDifficultyDecrease = 0.05f;

        // Anlık çarpan değeri (PlayerPrefs ile kaydedilir)
        public float CurrentMultiplier { get; private set; }

        private const string SAVE_KEY = "Battle_Difficulty_Multiplier";

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            LoadDifficulty();
        }

        /// <summary>
        /// [TR] Savaş bittiğinde çağrılır. Sonuca göre yeni zorluğu hesaplar.
        /// </summary>
        public void OnBattleComplete(bool playerWon)
        {
            if (playerWon)
            {
                // Kazandıysa zorlaştır
                CurrentMultiplier += _winDifficultyIncrease;
                Debug.Log($"👑 Savaş Kazanıldı! Zorluk Arttı: {CurrentMultiplier}");
            }
            else
            {
                // Kaybettiyse kolaylaştır (Ama taban sınırın altına inme)
                CurrentMultiplier -= _lossDifficultyDecrease;
                if (CurrentMultiplier < _minMultiplier) CurrentMultiplier = _minMultiplier;
                
                Debug.Log($"💀 Savaş Kaybedildi. Zorluk Düştü: {CurrentMultiplier}");
            }

            SaveDifficulty();
        }

        private void SaveDifficulty()
        {
            PlayerPrefs.SetFloat(SAVE_KEY, CurrentMultiplier);
            PlayerPrefs.Save();
        }

        private void LoadDifficulty()
        {
            CurrentMultiplier = PlayerPrefs.GetFloat(SAVE_KEY, _startingMultiplier);
        }
        
        // [DEBUG] Test için zorluğu sıfırlama butonu
        [ContextMenu("Reset Difficulty")]
        public void ResetDifficulty()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            CurrentMultiplier = _startingMultiplier;
            Debug.Log("Zorluk Sıfırlandı.");
        }
    }
}