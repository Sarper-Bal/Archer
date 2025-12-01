using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Experimental; 
using ArcadeBridge.ArcadeIdleEngine.Enemy; 
using ArcadeBridge.ArcadeIdleEngine.Spawners; 
using DG.Tweening; // [EKLENDI] DOTween kütüphanesi

namespace IndianOceanAssets.Engine2_5D
{
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyBehaviorController : MonoBehaviour
    {
        [Header("Debug Info / Debug Bilgisi")]
        [SerializeField] private EnemyBehaviorType _currentBehavior;

        // Script References / Script Referansları
        private EnemyStats _stats;
        private SimpleEnemyMover _simpleMover;
        private StalkerEnemyMover _stalkerMover;
        private WaypointEnemyMover _waypointMover;

        // Needs to remember origin pool / Düşmanın hangi havuzdan geldiğini hatırlaması gerek
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
            // [EKLENDI] Spawn Animation (Pop-up Effect) / Doğma Animasyonu
            PlaySpawnAnimation();

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

            // [EKLENDI] Kill active tweens / Aktif tweenleri kapat (Havuzda sorun olmaması için)
            transform.DOKill();

            // Return to pool / Eğer bir havuzum varsa, beni o havuza geri iade et!
            if (_originPool != null)
            {
                // [FIX] Used "Release" instead of "Return" / "Return" yerine "Release" yazdık.
                _originPool.Release(this); 
            }
        }

        // [EKLENDI] Cozy/Cartoon Spawn Effect
        private void PlaySpawnAnimation()
        {
            // Önce scale'i sıfıra indiriyoruz (Görünmez oluyor)
            transform.localScale = Vector3.zero;

            // Sonra "OutBack" ease tipi ile (hafif taşarak) büyütüyoruz.
            // Bu "jelibon" gibi bir çıkış hissi verir.
            transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
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
                // Unity 6000 compatibility check / Unity 6 uyumluluğu
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