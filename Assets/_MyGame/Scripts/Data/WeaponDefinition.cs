using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;

namespace IndianOceanAssets.Engine2_5D
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Arcade Idle/Combat/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField] private string _weaponName;

        [Header("Oyuncu İstatistikleri")]
        [SerializeField] private float _playerMaxHealth = 100f;

        [Header("Silah Davranışı & Kısıtlamalar")]
        [Tooltip("True: Koşarken ateş edebilir. False: Ateş etmek için durması gerekir.")]
        [SerializeField] private bool _canFireWhileMoving = true; 

        [Header("Saldırı İstatistikleri")]
        [SerializeField] private float _range = 10f;           
        [SerializeField] private float _attacksPerSecond = 1f; 
        [SerializeField] private float _damage = 10f;          
        [SerializeField] private float _rotationSpeed = 15f;   

        [Header("Mermi Davranışı")]
        [SerializeField] private ProjectileTrackingMode _trackingMode = ProjectileTrackingMode.Guided;
        [SerializeField] private float _projectileSpeed = 15f;
        [SerializeField] private float _projectileLifeTime = 5f;
        
        [Header("Patlama Ayarları")]
        [SerializeField] private BasicProjectilePool _projectilePool; 
        [SerializeField] private ExplosionPool _explosionPool;        
        [SerializeField] private float _explosionRadius = 2f;         
        [SerializeField] private LayerMask _damageLayer;              

        // --- Property'ler ---
        public string WeaponName => _weaponName;
        public float PlayerMaxHealth => _playerMaxHealth;
        public bool CanFireWhileMoving => _canFireWhileMoving; // Yeni Özellik
        
        public float Range => _range;
        public float AttackInterval => _attacksPerSecond > 0 ? 1f / _attacksPerSecond : 1f;
        public float Damage => _damage;
        public float RotationSpeed => _rotationSpeed;
        
        public ProjectileTrackingMode TrackingMode => _trackingMode;
        public float ProjectileSpeed => _projectileSpeed;
        public float ProjectileLifeTime => _projectileLifeTime;
        public BasicProjectilePool ProjectilePool => _projectilePool;
        public ExplosionPool ExplosionPool => _explosionPool;
        public float ExplosionRadius => _explosionRadius;
        public LayerMask DamageLayer => _damageLayer;
    }
}