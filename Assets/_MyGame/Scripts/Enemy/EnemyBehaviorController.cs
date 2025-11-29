using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Experimental; // Stalker için
using ArcadeBridge.ArcadeIdleEngine.Enemy; // Simple ve Waypoint için

namespace IndianOceanAssets.Engine2_5D // Veya senin ana namespace'in
{
    public class EnemyBehaviorController : MonoBehaviour
    {
        // Davranış Seçenekleri
        public enum EnemyBehaviorType
        {
            None,           // Hareketsiz (Dummy)
            SimpleChaser,   // Basit Takip (SimpleEnemyMover)
            Stalker,        // Zeki Takip (StalkerEnemyMover)
            Patrol          // Devriye (WaypointEnemyMover)
        }

        [Header("Davranış Ayarları")]
        [Tooltip("Bu düşman hangi zekayı kullanacak?")]
        [SerializeField] private EnemyBehaviorType _startingBehavior = EnemyBehaviorType.SimpleChaser;

        // Script Referansları (Otomatik Bulunacak)
        private SimpleEnemyMover _simpleMover;
        private StalkerEnemyMover _stalkerMover;
        private WaypointEnemyMover _waypointMover;

        private void Awake()
        {
            // Tüm beyinleri üzerimden topla
            _simpleMover = GetComponent<SimpleEnemyMover>();
            _stalkerMover = GetComponent<StalkerEnemyMover>();
            _waypointMover = GetComponent<WaypointEnemyMover>();

            // Hata Kontrolü: Scriptler eksikse uyar
            if (_simpleMover == null) Debug.LogWarning($"{name}: SimpleEnemyMover eksik!");
            if (_stalkerMover == null) Debug.LogWarning($"{name}: StalkerEnemyMover eksik!");
            if (_waypointMover == null) Debug.LogWarning($"{name}: WaypointEnemyMover eksik!");
        }

        private void OnEnable()
        {
            // Düşman her doğduğunda (Pool'dan çıktığında) seçili davranışı uygula
            SetBehavior(_startingBehavior);
        }

        /// <summary>
        /// Dışarıdan (Spawner veya Kod ile) davranışı değiştirmek için kullanılır.
        /// </summary>
        public void SetBehavior(EnemyBehaviorType newBehavior)
        {
            _startingBehavior = newBehavior;

            // 1. Önce hepsini kapat (Temiz sayfa)
            DisableAllBehaviors();

            // 2. Seçili olanı aç
            switch (newBehavior)
            {
                case EnemyBehaviorType.SimpleChaser:
                    if (_simpleMover != null) _simpleMover.enabled = true;
                    break;

                case EnemyBehaviorType.Stalker:
                    if (_stalkerMover != null) _stalkerMover.enabled = true;
                    break;

                case EnemyBehaviorType.Patrol:
                    if (_waypointMover != null) _waypointMover.enabled = true;
                    break;
                
                case EnemyBehaviorType.None:
                    // Hepsi kapalı kalsın
                    break;
            }
        }

        /// <summary>
        /// Tüm hareket scriptlerini devre dışı bırakır.
        /// </summary>
        private void DisableAllBehaviors()
        {
            if (_simpleMover != null) _simpleMover.enabled = false;
            if (_stalkerMover != null) _stalkerMover.enabled = false;
            if (_waypointMover != null) _waypointMover.enabled = false;
            
            // Eğer fiziksel bir hız kaldıysa sıfırla (Kaymayı önle)
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
        
        // Waypoint sistemi için özel bir yardımcı fonksiyon
        // Spawner'dan yol atamak istersek bu köprüyü kullanacağız.
        public void SetPatrolRoute(WaypointRoute route)
        {
            if (_waypointMover != null)
            {
                _waypointMover.SetRoute(route);
                SetBehavior(EnemyBehaviorType.Patrol); // Otomatik olarak Patrol moduna geçir
            }
        }
        
#if UNITY_EDITOR
        // Editörde değer değiştirdiğimizde anlık tepki versin (Debug için)
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                SetBehavior(_startingBehavior);
            }
        }
#endif
    }
}