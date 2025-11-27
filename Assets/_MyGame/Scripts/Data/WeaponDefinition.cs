using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools; 

namespace IndianOceanAssets.Engine2_5D
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Arcade Idle/Combat/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField] private string _weaponName;

        [Header("Saldırı İstatistikleri")]
        [SerializeField] private float _range = 10f;           // Menzil
        [SerializeField] private float _attacksPerSecond = 1f; // Saldırı Hızı
        [SerializeField] private float _damage = 10f;          // Hasar
        [SerializeField] private float _rotationSpeed = 15f;   // Dönüş Hızı

        [Header("Mermi Ayarları")]
        [Tooltip("Bu silahın kullanacağı mermi havuzu")]
        [SerializeField] private BasicProjectilePool _projectilePool;

        // --- Property'ler ---
        public string WeaponName => _weaponName;
        public float Range => _range;
        // Saniyedeki saldırı sayısını bekleme süresine çevirir
        public float AttackInterval => _attacksPerSecond > 0 ? 1f / _attacksPerSecond : 1f;
        public float Damage => _damage;
        public float RotationSpeed => _rotationSpeed;
        public BasicProjectilePool ProjectilePool => _projectilePool;
    }
}