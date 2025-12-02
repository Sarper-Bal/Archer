using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Experimental; 
using ArcadeBridge.ArcadeIdleEngine.Enemy; 
using ArcadeBridge.ArcadeIdleEngine.Spawners; 
using IndianOceanAssets.Engine2_5D.Managers; // SmartWaveManager için

namespace IndianOceanAssets.Engine2_5D
{
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyBehaviorController : MonoBehaviour
    {
        [Header("Debug Info")]
        [SerializeField] private EnemyBehaviorType _currentBehavior;

        // Script Referansları
        private EnemyStats _stats;
        private SimpleEnemyMover _simpleMover;
        private StalkerEnemyMover _stalkerMover;
        private WaypointEnemyMover _waypointMover;
        
        // Cachelenmiş Manager (Performans için her karede Find yapmamak adına)
        private SmartWaveManager _cachedWaveManager;

        // Eve Dönüş Bileti
        public System.Action<EnemyBehaviorController> OnReturnToPool;

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            _simpleMover = GetComponent<SimpleEnemyMover>();
            _stalkerMover = GetComponent<StalkerEnemyMover>();
            _waypointMover = GetComponent<WaypointEnemyMover>();
            
            // Sahnedeki manager'ı bul ve sakla (Bunu sadece 1 kere yapar)
            _cachedWaveManager = FindObjectOfType<SmartWaveManager>();
        }

        private void OnEnable()
        {
            if (_stats != null && _stats.Definition != null)
            {
                SetBehavior(_stats.Definition.DefaultBehavior);
            }
            else
            {
                SetBehavior(EnemyBehaviorType.SimpleChaser);
            }
            
            // [NOT] Register işlemi Spawner tarafından yapılıyor, burada gerek yok.
            // Ama çift dikiş olsun derseniz buraya da _cachedWaveManager.RegisterEnemy(this) eklenebilir.
        }

        private void OnDisable()
        {
            DisableAllBehaviors();
            
            // [KRİTİK] Sahneden çıkarken (ölüm veya pool) kaydını sildir!
            if (_cachedWaveManager != null)
            {
                _cachedWaveManager.UnregisterEnemy(this);
            }
            
            // Spawner'a beni geri alması için haber ver (Pool sistemi)
            OnReturnToPool?.Invoke(this);
        }

        public void SetBehavior(EnemyBehaviorType newBehavior)
        {
            _currentBehavior = newBehavior; 
            DisableAllBehaviors();

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
            }
        }

        private void DisableAllBehaviors()
        {
            if (_simpleMover) _simpleMover.enabled = false;
            if (_stalkerMover) _stalkerMover.enabled = false;
            if (_waypointMover) _waypointMover.enabled = false;

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