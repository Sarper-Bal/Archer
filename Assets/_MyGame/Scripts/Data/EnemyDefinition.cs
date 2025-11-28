using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;

namespace IndianOceanAssets.Engine2_5D
{
    // Düşman verilerini (Can, Hız, Hasar) tutan dosya
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Arcade Idle/Combat/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Temel Özellikler")]
        [SerializeField] private string _enemyName;
        [SerializeField] private float _maxHealth = 50f; // Düşmanın Canı
        [SerializeField] private float _moveSpeed = 3f;  // Düşmanın Hızı
        
        [Header("Saldırı")]
        [SerializeField] private float _contactDamage = 5f; // Oyuncuya çarpınca vereceği hasar

        [Header("Efektler")]
        [Tooltip("Bu düşman ölünce kullanılacak efekt havuzu (Opsiyonel)")]
        [SerializeField] private DeathEffectPool _deathEffectPool;

        // --- Erişimciler ---
        public float MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float ContactDamage => _contactDamage;
        public DeathEffectPool DeathEffectPool => _deathEffectPool;
    }
}