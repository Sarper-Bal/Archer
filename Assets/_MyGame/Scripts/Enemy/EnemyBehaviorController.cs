using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Experimental; 
using ArcadeBridge.ArcadeIdleEngine.Enemy; // [ÖNEMLİ] Simple ve Waypoint Moverlar burada
using ArcadeBridge.ArcadeIdleEngine.Spawners; 
using DG.Tweening; // Animasyonlar için

namespace IndianOceanAssets.Engine2_5D
{
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyBehaviorController : MonoBehaviour
    {
        [Header("Debug Info")]
        [SerializeField] private EnemyBehaviorType _currentBehavior;

        // Script Referansları
        private EnemyStats _stats;
        private SimpleEnemyMover _simpleMover;   // Hata veren satır burasıydı
        private StalkerEnemyMover _stalkerMover;
        private WaypointEnemyMover _waypointMover; // Hata veren diğer satır

        // Düşmanın dönüş bileti (Spawner'a geri dönmesi için)
        public System.Action<EnemyBehaviorController> OnReturnToPool;

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
            _simpleMover = GetComponent<SimpleEnemyMover>();
            _stalkerMover = GetComponent<StalkerEnemyMover>();
            _waypointMover = GetComponent<WaypointEnemyMover>();
        }

        private void OnEnable()
        {
            // [JUICE] Doğma Animasyonu
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);

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
            transform.DOKill(); // Animasyonları temizle

            // [POOL] Spawner'a beni geri alması için haber ver
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