using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Experimental; 
using ArcadeBridge.ArcadeIdleEngine.Enemy; 
using ArcadeBridge.ArcadeIdleEngine.Spawners; 
using IndianOceanAssets.Engine2_5D.Managers;

namespace IndianOceanAssets.Engine2_5D
{
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(Health))] 
    public class EnemyBehaviorController : MonoBehaviour
    {
        [Header("Debug Info")]
        [SerializeField] private EnemyBehaviorType _currentBehavior;

        // --- Cache Referansları ---
        private EnemyStats _stats;
        private Health _health;
        private Rigidbody _rb;

        // Hareket Scriptleri
        private SimpleEnemyMover _simpleMover;
        private StalkerEnemyMover _stalkerMover;
        private WaypointEnemyMover _waypointMover;
        private DirectionalEnemyMover _directionalMover;
        
        private SmartWaveManager _cachedWaveManager;
        public System.Action<EnemyBehaviorController> OnReturnToPool;

        // [YENİ] Ganimet Çakışmasını Önleyen Bayrak
        // True ise: Kule bu düşmanı öldürüp ganimetini almıştır, Spawner oyuncuya vermez.
        public bool LootDropped { get; set; }

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            _health = GetComponent<Health>();
            _rb = GetComponent<Rigidbody>();

            _simpleMover = GetComponent<SimpleEnemyMover>();
            _stalkerMover = GetComponent<StalkerEnemyMover>();
            _waypointMover = GetComponent<WaypointEnemyMover>();
            _directionalMover = GetComponent<DirectionalEnemyMover>();
            
            _cachedWaveManager = FindObjectOfType<SmartWaveManager>();
        }

        private void OnEnable()
        {
            // Spawner ile çalışırken burası boş kalabilir, InitializeEnemy her şeyi yapar.
        }

        public void InitializeEnemy(EnemyDefinition data)
        {
            gameObject.SetActive(true);

            // [KRİTİK] Yeni doğduğunda bayrağı indir. Henüz kimse ganimetini almadı.
            LootDropped = false;

            if (_stats != null) _stats.InitializeRuntime(data);
            if (_health != null) _health.ResetHealth();

            ResetPhysics();

            if (data != null)
            {
                SetBehavior(data.DefaultBehavior);
            }
            else
            {
                Debug.LogWarning($"⚠️ {name} için veri (data) boş geldi! SimpleChaser yapılıyor.");
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
                case EnemyBehaviorType.Directional:
                    if (_directionalMover != null) _directionalMover.enabled = true;
                    else Debug.LogError($"❌ {name}: 'DirectionalEnemyMover' scripti eksik!");
                    break;
                default:
                    if (_simpleMover) _simpleMover.enabled = true;
                    break;
            }
        }

        private void DisableAllBehaviors()
        {
            if (_simpleMover) _simpleMover.enabled = false;
            if (_stalkerMover) _stalkerMover.enabled = false;
            if (_waypointMover) _waypointMover.enabled = false;
            if (_directionalMover) _directionalMover.enabled = false;
            
            ResetPhysics();
        }

        private void ResetPhysics()
        {
            if (_rb != null)
            {
                #if UNITY_6000_0_OR_NEWER
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                #else
                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
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