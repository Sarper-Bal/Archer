using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using ArcadeBridge.ArcadeIdleEngine.Enemy;

namespace IndianOceanAssets.Engine2_5D
{
    // [YENİ] Düşman Sınıfları
    public enum EnemyCategory
    {
        Swarm,  // Sürü (Slime, Böcek - Kalabalık yapar)
        Rusher, // Baskıncı (Yarasa, Kurt - Hızlı dırlar)
        Tank    // Zırhlı (Golem, Şövalye - Zor ölür)
    }

    public enum EnemyBehaviorType
    {
        None,
        SimpleChaser,
        Stalker,
        Patrol
    }

    [CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "MyGame/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("🏷️ Sınıflandırma")]
        public EnemyCategory Category = EnemyCategory.Swarm; // [YENİ]

        [Header("📊 İstatistikler")]
        [Tooltip("Değeri değiştirdiğinde puan otomatik güncellenir.")]
        public float MoveSpeed = 5f;
        public float MaxHealth = 100f;
        public float ContactDamage = 10f;

        [Header("💀 Tehdit Puanı (Otomatik)")]
        [Tooltip("Bu düşmanın maliyeti.")]
        public float ThreatScore = 0f; 

        [Header("🧠 Yapay Zeka")]
        [SerializeField] private EnemyBehaviorType _defaultBehavior = EnemyBehaviorType.SimpleChaser;
        public RouteID PatrolRouteID; 

        [Header("✨ Görsel & Efekt")]
        public GameObject EnemyPrefab; 
        public DeathEffectPool DeathEffectPool; 

        public EnemyBehaviorType DefaultBehavior => _defaultBehavior;

        // --- OTOMATİK HESAPLAMA ---
        private void OnValidate()
        {
            // Formül: (Can + (Hasar x 2)) * (Hız / 3)
            float rawScore = (MaxHealth + (ContactDamage * 2f)) * (MoveSpeed / 3f);
            ThreatScore = Mathf.Round(rawScore * 10f) / 10f;
        }
    }
}