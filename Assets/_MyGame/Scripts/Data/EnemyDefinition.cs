using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using ArcadeBridge.ArcadeIdleEngine.Enemy; // RouteID için

namespace IndianOceanAssets.Engine2_5D
{
    public enum EnemyBehaviorType
    {
        None,
        SimpleChaser,
        Stalker,
        Patrol // Waypoint
    }

    [CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "MyGame/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Temel İstatistikler")]
        public float MoveSpeed = 5f;
        public float MaxHealth = 100f;
        public float ContactDamage = 10f;

        [Header("Yapay Zeka (AI)")]
        [SerializeField] private EnemyBehaviorType _defaultBehavior = EnemyBehaviorType.SimpleChaser;
        
        // [GÜNCELLEME] Yeni özellik: Hangi yolu izleyecek?
        [Tooltip("Eğer davranış 'Patrol' ise, sahnede bu ID'ye sahip yolu arar.")]
        public RouteID PatrolRouteID; 

        [Header("Görsel & Efekt")]
        public DeathEffectPool DeathEffectPool; 

        public EnemyBehaviorType DefaultBehavior => _defaultBehavior;
    }
}