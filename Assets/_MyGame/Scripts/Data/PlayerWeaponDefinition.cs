using UnityEngine;
using IndianOceanAssets.Engine2_5D;

namespace ArcadeBridge.ArcadeIdleEngine.Weapon
{
    [CreateAssetMenu(fileName = "NewPlayerWeapon", menuName = "MyGame/Player Weapon Definition")]
    public class PlayerWeaponDefinition : ScriptableObject
    {
        [Header("🔫 Mühimmat (Ammo)")]
        [Tooltip("Bu silahtan hangi asker/birim fırlatılacak?")]
        public EnemyDefinition UnitToSpawn; 
        
        [Header("⚙️ Atış Ayarları (Firing Stats)")]
        [Tooltip("Saniyede kaç atış yapılacak?")]
        [Min(0.1f)]
        public float FireRate = 5f;      

        [Tooltip("Tek seferde namludan kaç birim çıkacak?")]
        [Min(1)]
        public int ProjectilesPerShot = 1; 

        [Tooltip("Çoklu atışlarda birimlerin saçılma açısı.")]
        [Range(0f, 45f)]
        public float SpreadAngle = 10f;

        [Header("🏃 Hareket Ayarları (Swerve)")]
        [Tooltip("Topun sağa sola kayma hızı.")]
        public float SwerveSpeed = 10f; // [YENİ] Hız dataya taşındı

        [Tooltip("Başlangıç noktasından sağa ve sola maksimum kaç birim gidebilir? (Örn: 4.5 ise toplam genişlik 9 olur)")]
        public float MaxSwerveOffset = 4.5f; // [YENİ] Limit ayarı
    }
}