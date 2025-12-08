using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using ArcadeBridge.ArcadeIdleEngine.Enemy;

namespace IndianOceanAssets.Engine2_5D
{
    public enum EnemyCategory
    {
        Swarm,
        Rusher,
        Tank
    }

    public enum EnemyBehaviorType
    {
        None,
        SimpleChaser,
        Stalker,
        Patrol,
        Directional // [YENİ] Yeni hareket tipi eklendi
    }

    [CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "MyGame/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("🏷️ Sınıflandırma")]
        public EnemyCategory Category = EnemyCategory.Swarm;

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
        
        // [YENİ] Doğrusal hareket yönü (X, Y, Z). Genelde Z= -1 (Aşağı) veya Z= 1 (Yukarı) olur.
        [Tooltip("Sadece 'Directional' davranışı seçiliyse kullanılır.")]
        public Vector3 FixedDirection = new Vector3(0, 0, -1);

        [Header("✨ Görsel & Efekt")]
        public GameObject EnemyPrefab; 
        public DeathEffectPool DeathEffectPool; 

        public EnemyBehaviorType DefaultBehavior => _defaultBehavior;

        private void OnValidate()
        {
            float rawScore = (MaxHealth + (ContactDamage * 2f)) * (MoveSpeed / 3f);
            ThreatScore = Mathf.Round(rawScore * 10f) / 10f;
        }
    }
}