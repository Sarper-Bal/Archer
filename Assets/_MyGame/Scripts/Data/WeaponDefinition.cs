using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;

namespace IndianOceanAssets.Engine2_5D
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Arcade Idle/Combat/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField] private string _weaponName;

        [Header("Oyuncu İstatistikleri (Player Stats)")]
        // YENİ: Bu silahı takan oyuncunun Maksimum Canı
        [SerializeField] private float _playerMaxHealth = 100f; 

        [Header("Saldırı İstatistikleri")]
        [SerializeField] private float _range = 10f;
        [SerializeField] private float _attacksPerSecond = 1f;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private float _rotationSpeed = 15f;

        [Header("Mermi Ayarları")]
        [SerializeField] private BasicProjectilePool _projectilePool;

        // --- Property'ler ---
        public string WeaponName => _weaponName;
        public float PlayerMaxHealth => _playerMaxHealth; // Dışarıya açıyoruz
        public float Range => _range;
        public float AttackInterval => _attacksPerSecond > 0 ? 1f / _attacksPerSecond : 1f;
        public float Damage => _damage;
        public float RotationSpeed => _rotationSpeed;
        public BasicProjectilePool ProjectilePool => _projectilePool;
    }
}