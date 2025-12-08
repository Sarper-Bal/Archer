using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Experimental; 
using ArcadeBridge.ArcadeIdleEngine.Enemy; 
using ArcadeBridge.ArcadeIdleEngine.Spawners; 
using IndianOceanAssets.Engine2_5D.Managers;

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
        private DirectionalEnemyMover _directionalMover; // Yeni Referans
        
        private SmartWaveManager _cachedWaveManager;
        public System.Action<EnemyBehaviorController> OnReturnToPool;

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            _simpleMover = GetComponent<SimpleEnemyMover>();
            _stalkerMover = GetComponent<StalkerEnemyMover>();
            _waypointMover = GetComponent<WaypointEnemyMover>();
            _directionalMover = GetComponent<DirectionalEnemyMover>(); // Bileşeni almaya çalış
            
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
        }

        private void OnDisable()
        {
            DisableAllBehaviors();
            if (_cachedWaveManager != null)
            {
                _cachedWaveManager.UnregisterEnemy(this);
            }
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
                
                // [YENİ SİSTEM BURADA DEVREYE GİRİYOR]
                case EnemyBehaviorType.Directional:
                    if (_directionalMover != null)
                    {
                        _directionalMover.enabled = true;
                    }
                    else
                    {
                        // EĞER BU HATAYI GÖRÜYORSAN PREFAB'A SCRİPT EKLEMEMİŞSİN DEMEKTİR
                        Debug.LogError($"❌ HATA: {gameObject.name} üzerinde 'DirectionalEnemyMover' scripti YOK! Lütfen Prefab'a ekleyin.");
                    }
                    break;
            }
        }

        private void DisableAllBehaviors()
        {
            if (_simpleMover) _simpleMover.enabled = false;
            if (_stalkerMover) _stalkerMover.enabled = false;
            if (_waypointMover) _waypointMover.enabled = false;
            if (_directionalMover) _directionalMover.enabled = false;

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