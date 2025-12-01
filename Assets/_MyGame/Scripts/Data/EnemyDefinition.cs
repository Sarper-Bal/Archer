using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using ArcadeBridge.ArcadeIdleEngine.Enemy;

namespace IndianOceanAssets.Engine2_5D
{
    // [NOT] Bu enum dosyanın en üstünde veya ayrı bir dosyada durabilir.
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
        [Header("📊 Temel İstatistikler")]
        [Tooltip("Değeri değiştirdiğinde puan otomatik güncellenir.")]
        public float MoveSpeed = 5f;
        public float MaxHealth = 100f;
        public float ContactDamage = 10f;

        [Header("💀 Tehdit Analizi")]
        public bool ManualOverride = false; // Elle girmek istersen bunu işaretle
        public float ManualThreatValue = 0f;

        // Bunu Inspector'da gri yapmak için CustomEditor gerekir ama şimdilik sadece gösteriyoruz.
        // Elle değiştirsen bile oyun tekrar hesaplayıp üzerine yazar.
        [Tooltip("Bu değer otomatiktir. Elle değiştirsen bile geri düzelir.")]
        public float CalculatedThreat = 0f;

        // Dışarıdan okumak için Property
        public float ThreatScore => ManualOverride ? ManualThreatValue : CalculatedThreat;

        [Header("🧠 Yapay Zeka")]
        [SerializeField] private EnemyBehaviorType _defaultBehavior = EnemyBehaviorType.SimpleChaser;
        public RouteID PatrolRouteID; 

        [Header("✨ Görsel & Efekt")]
        public DeathEffectPool DeathEffectPool; 

        public EnemyBehaviorType DefaultBehavior => _defaultBehavior;

        // --- OTOMATİK HESAPLAMA MANTIĞI ---
        
        // Bu fonksiyon Unity'nin kendi özelliğidir.
        // Inspector'da bir şeye dokunduğun an çalışır. Eklentiye gerek yoktur.
        private void OnValidate()
        {
            CalculateThreat();
        }

        private void CalculateThreat()
        {
            // Formül: (Can + (Hasar x 2)) * (Hız / 3)
            float rawScore = (MaxHealth + (ContactDamage * 2f)) * (MoveSpeed / 3f);
            
            // Okunabilir olması için virgülden sonra 1 basamak yuvarla
            CalculatedThreat = Mathf.Round(rawScore * 10f) / 10f;
        }
    }
}