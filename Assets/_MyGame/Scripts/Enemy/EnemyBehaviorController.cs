using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Experimental; 
using ArcadeBridge.ArcadeIdleEngine.Enemy; 
using ArcadeBridge.ArcadeIdleEngine.Spawners; 

namespace IndianOceanAssets.Engine2_5D
{
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyBehaviorController : MonoBehaviour
    {
        [Header("Debug Bilgisi")]
        [SerializeField] private EnemyBehaviorType _currentBehavior;

        // Script Referansları
        private EnemyStats _stats;
        private SimpleEnemyMover _simpleMover;
        private StalkerEnemyMover _stalkerMover;
        private WaypointEnemyMover _waypointMover;

        // Düşmanın hangi havuzdan geldiğini hatırlaması gerek
        private EnemyPool _originPool; 

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            _simpleMover = GetComponent<SimpleEnemyMover>();
            _stalkerMover = GetComponent<StalkerEnemyMover>();
            _waypointMover = GetComponent<WaypointEnemyMover>();
        }

        public void InitializePool(EnemyPool pool)
        {
            _originPool = pool;
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

            // Eğer bir havuzum varsa, beni o havuza geri iade et!
            if (_originPool != null)
            {
                // [DÜZELTME] "Return" yerine "Release" yazdık.
                _originPool.Release(this); 
            }
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