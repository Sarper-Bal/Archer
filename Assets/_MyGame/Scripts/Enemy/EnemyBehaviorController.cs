using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Experimental; // Stalker için
using ArcadeBridge.ArcadeIdleEngine.Enemy; // Waypoint için

namespace IndianOceanAssets.Engine2_5D
{
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyBehaviorController : MonoBehaviour
    {
        // [DEĞİŞİKLİK] Artık burada "Starting Behavior" ayarı yok.
        // Veri tamamen EnemyStats -> EnemyDefinition'dan geliyor.

        [Header("Debug Bilgisi (Sadece Okunabilir)")]
        [SerializeField] private EnemyBehaviorType _currentBehavior; // Test ederken görmen için

        // Script Referansları
        private EnemyStats _stats;
        private SimpleEnemyMover _simpleMover;
        private StalkerEnemyMover _stalkerMover;
        private WaypointEnemyMover _waypointMover;

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            
            _simpleMover = GetComponent<SimpleEnemyMover>();
            _stalkerMover = GetComponent<StalkerEnemyMover>();
            _waypointMover = GetComponent<WaypointEnemyMover>();

            // Güvenlik kontrolü
            if (_simpleMover == null) Debug.LogWarning($"{name}: SimpleMover eksik!");
        }

        private void OnEnable()
        {
            // Düşman doğduğunda (veya havuzdan çıktığında)
            // Veri dosyasındaki "DefaultBehavior" ne ise onu uygula.
            if (_stats != null && _stats.Definition != null)
            {
                SetBehavior(_stats.Definition.DefaultBehavior);
            }
            else
            {
                // Veri yoksa güvenli modda aç (Hata vermesin)
                SetBehavior(EnemyBehaviorType.SimpleChaser);
                Debug.LogWarning($"{name}: EnemyDefinition bulunamadı, varsayılan SimpleChaser açıldı.");
            }
        }

        public void SetBehavior(EnemyBehaviorType newBehavior)
        {
            _currentBehavior = newBehavior; // Debug için güncelle

            // 1. Temiz Sayfa: Hepsini kapat
            DisableAllBehaviors();

            // 2. İstenileni Aç
            switch (newBehavior)
            {
                case EnemyBehaviorType.SimpleChaser:
                    if (_simpleMover) _simpleMover.enabled = true;
                    break;

                case EnemyBehaviorType.Stalker:
                    if (_stalkerMover) _stalkerMover.enabled = true;
                    break;

                case EnemyBehaviorType.Patrol:
                    if (_waypointMover) _waypointMover.enabled = true;
                    break;
                
                case EnemyBehaviorType.None:
                    // Hepsi kapalı kalır
                    break;
            }
        }

        private void DisableAllBehaviors()
        {
            if (_simpleMover) _simpleMover.enabled = false;
            if (_stalkerMover) _stalkerMover.enabled = false;
            if (_waypointMover) _waypointMover.enabled = false;

            // Hızı sıfırla (Kaymayı önle)
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                #if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector3.zero;
                #else
                rb.velocity = Vector3.zero;
                #endif
            }
        }
        
        // Spawner'dan devriye yolu atandığında otomatik geçiş için yardımcı metod
        public void SetPatrolRoute(WaypointRoute route)
        {
            if (_waypointMover != null)
            {
                _waypointMover.SetRoute(route);
                SetBehavior(EnemyBehaviorType.Patrol);
            }
        }
    }
}