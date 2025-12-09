using UnityEngine;
using UnityEngine.SceneManagement;
using IndianOceanAssets.Engine2_5D.Spawners; // Spawner namespace'in
using System.Collections;

namespace IndianOceanAssets.Engine2_5D.Managers
{
    /// <summary>
    /// [TR] Sahneler arası geçişi ve savaş verilerinin taşınmasını yöneten kalıcı (Persistent) yönetici.
    /// [EN] Persistent manager handling scene transitions and carrying battle data.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Scene Names")]
        [Tooltip("Köy sahnesinin adı (Build Settings'teki ile birebir aynı olmalı)")]
        [SerializeField] private string _villageSceneName = "S_1";
        
        [Tooltip("Savaş sahnesinin adı")]
        [SerializeField] private string _battleSceneName = "BattleScene";

        // --- Veri Taşıma Çantası ---
        // Savaş sahnesi açıldığında "Hangi level oynanacak?" sorusunun cevabı burada saklanır.
        public EnemyDefinition CurrentEnemyData { get; private set; } 
        public int CurrentLevelDifficulty { get; private set; }

        private void Awake()
        {
            // Singleton + DontDestroyOnLoad (Sahne değişse de yok olma)
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// [TR] Köyden savaşa gitmek için bu metodu çağırın.
        /// </summary>
        /// <param name="enemyToFight">Savaşılacak düşman türü (İleride LevelData ile değişecek)</param>
        public void LoadBattleScene(EnemyDefinition enemyToFight, int difficulty)
        {
            CurrentEnemyData = enemyToFight;
            CurrentLevelDifficulty = difficulty;

            Debug.Log($"⚔️ Savaş Yükleniyor: {enemyToFight.name} (Zorluk: {difficulty})");
            
            // Sahne yüklemeyi başlat
            SceneManager.LoadScene(_battleSceneName);
        }

        /// <summary>
        /// [TR] Savaş bittiğinde köye dönmek için çağırın.
        /// </summary>
        public void ReturnToVillage()
        {
            Debug.Log("🏡 Köye Dönülüyor...");
            SceneManager.LoadScene(_villageSceneName);
        }

        // --- TEST MENÜSÜ ---
        // Inspector'dan sağ tıklayıp test edebilirsin.
        
        [Header("Test Data")]
        public EnemyDefinition TestEnemy; // Test için bir düşman ata

        [ContextMenu("🚀 TEST: Go to Battle Scene")]
        public void TestLoadBattle()
        {
            if (TestEnemy != null)
                LoadBattleScene(TestEnemy, 1);
            else
                Debug.LogError("TestEnemy boş! Lütfen Inspector'dan bir EnemyDefinition atayın.");
        }
    }
}