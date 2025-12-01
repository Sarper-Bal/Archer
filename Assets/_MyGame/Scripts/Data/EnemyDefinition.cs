using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using ArcadeBridge.ArcadeIdleEngine.Enemy;

namespace IndianOceanAssets.Engine2_5D
{
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
        [Header("📊 İstatistikler (Değiştirince Puan Hesaplanır)")]
        [Tooltip("Düşmanın hareket hızı.")]
        public float MoveSpeed = 5f;
        
        [Tooltip("Düşmanın maksimum canı.")]
        public float MaxHealth = 100f;
        
        [Tooltip("Dokunduğunda verdiği hasar.")]
        public float ContactDamage = 10f;

        [Header("💀 Tehdit Puanı (Otomatik)")]
        [Tooltip("Otomatik hesaplanan zorluk derecesi.")]
        public float ThreatScore = 0f; // Hesaplanan değer burada tutulur

        [Header("🧠 Yapay Zeka")]
        [SerializeField] private EnemyBehaviorType _defaultBehavior = EnemyBehaviorType.SimpleChaser;
        public RouteID PatrolRouteID; 

        [Header("✨ Görsel & Efekt")]
        [Tooltip("Düşmanın fiziksel Prefab'ı (WaveSpawner bunu kullanacak)")]
        public GameObject EnemyPrefab; // [YENİ] Prefab referansını buraya ekledik
        public DeathEffectPool DeathEffectPool; 

        public EnemyBehaviorType DefaultBehavior => _defaultBehavior;

        // --- OTOMATİK HESAPLAMA ---
        // Inspector'da bir değer değiştiği an çalışır.
        private void OnValidate()
        {
            CalculateThreat();
        }

        private void CalculateThreat()
        {
            // Formül: (Can + (Hasar x 2)) * (Hız / 3)
            float rawScore = (MaxHealth + (ContactDamage * 2f)) * (MoveSpeed / 3f);
            
            // Okunabilir olması için yuvarla (Örn: 12.5)
            ThreatScore = Mathf.Round(rawScore * 10f) / 10f;
        }
    }
}