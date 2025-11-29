using UnityEngine;
using IndianOceanAssets.Engine2_5D;
using ArcadeBridge.ArcadeIdleEngine.Managers;

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))]
    public class WaypointEnemyMover : MonoBehaviour
    {
        [Header("Yol Ayarları")]
        [SerializeField] private WaypointRoute _manualRoute; 
        [SerializeField] private float _rotationSpeed = 8f;
        [SerializeField] private float _arrivalDistance = 0.5f;
        [SerializeField] private bool _loopPath = true;

        private Rigidbody _rb;
        private EnemyStats _stats;
        private WaypointRoute _activeRoute;

        private int _currentWaypointIndex = 0;
        private Transform _currentTargetPoint;
        private float _arrivalDistanceSqr;
        private bool _isPathComplete = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();
            
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            _arrivalDistanceSqr = _arrivalDistance * _arrivalDistance;
        }

        private void OnEnable()
        {
            // Havuzdan çıktığında hemen bulmaya çalış
            ResolveRoute(); 
            ResetPath();
        }

        // [DÜZELTME] Start ekledik.
        // Eğer oyunun en başında OnEnable çalıştığında "RouteManager" henüz hazır değilse,
        // Yol bulunamamış olabilir. Start fonksiyonu her şey yüklendikten sonra çalışır.
        private void Start()
        {
            if (_activeRoute == null)
            {
                ResolveRoute();
                ResetPath();
            }
        }

        private void ResolveRoute()
        {
            // Eğer zaten bulduysam tekrar arama (Optimizasyon)
            if (_activeRoute != null) return;

            // 1. Manuel atama (Inspector)
            if (_manualRoute != null)
            {
                _activeRoute = _manualRoute;
                return;
            }

            // 2. Data-Driven ID ile arama
            if (_stats.Definition != null && _stats.Definition.PatrolRouteID != null)
            {
                if (RouteManager.Instance != null)
                {
                    _activeRoute = RouteManager.Instance.GetRoute(_stats.Definition.PatrolRouteID);
                }
            }
        }

        private void FixedUpdate()
        {
            // Hala yol yoksa yapacak bir şey yok, bekle
            if (_activeRoute == null)
            {
                // [EKSTRA KORUMA] Belki yönetici geç yüklendi, arada bir tekrar sor?
                // Performans için burayı her karede çağırmıyoruz, Start'ta halledilmiş olmalı.
                return;
            }

            if (_isPathComplete || _currentTargetPoint == null || _stats.Definition == null) 
            {
                #if UNITY_6000_0_OR_NEWER
                _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
                #else
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
                #endif
                return;
            }

            MoveAlongPath();
            CheckArrival();
        }

        private void MoveAlongPath()
        {
            Vector3 direction = (_currentTargetPoint.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, lookRotation, _rotationSpeed * Time.fixedDeltaTime));

                if (_stats.Definition != null)
                {
                    Vector3 moveVelocity = direction * _stats.Definition.MoveSpeed;
                    #if UNITY_6000_0_OR_NEWER
                    moveVelocity.y = _rb.linearVelocity.y;
                    _rb.linearVelocity = moveVelocity;
                    #else
                    moveVelocity.y = _rb.velocity.y;
                    _rb.velocity = moveVelocity;
                    #endif
                }
            }
        }

        private void CheckArrival()
        {
            float dx = transform.position.x - _currentTargetPoint.position.x;
            float dz = transform.position.z - _currentTargetPoint.position.z;
            float distSqr = (dx * dx) + (dz * dz);

            if (distSqr <= _arrivalDistanceSqr)
            {
                AdvanceToNextWaypoint();
            }
        }

        private void AdvanceToNextWaypoint()
        {
            _currentWaypointIndex++;

            if (_activeRoute != null && _currentWaypointIndex >= _activeRoute.Waypoints.Count)
            {
                if (_loopPath)
                {
                    _currentWaypointIndex = 0;
                }
                else
                {
                    _isPathComplete = true;
                    _currentTargetPoint = null;
                    return;
                }
            }

            if (_activeRoute != null && _activeRoute.Waypoints.Count > 0)
                _currentTargetPoint = _activeRoute.Waypoints[_currentWaypointIndex];
        }

        public void SetRoute(WaypointRoute newRoute)
        {
            _manualRoute = newRoute;
            _activeRoute = null; // Sıfırla ki Resolve tekrar çalışsın
            ResolveRoute();
            ResetPath();
        }

        private void ResetPath()
        {
            _isPathComplete = false;
            _currentWaypointIndex = 0;

            if (_activeRoute != null && _activeRoute.Waypoints.Count > 0)
            {
                _currentTargetPoint = _activeRoute.Waypoints[0];
            }
            else
            {
                _isPathComplete = true;
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
             if (collision.gameObject.CompareTag("Player") && _stats.Definition != null)
             {
                 if (collision.gameObject.TryGetComponent(out IDamageable damageable))
                 {
                     damageable.TakeDamage(_stats.Definition.ContactDamage);
                     if (_stats.Definition.DeathEffectPool != null)
                     {
                         var effect = _stats.Definition.DeathEffectPool.Get();
                         effect.transform.position = transform.position;
                         effect.Initialize(_stats.Definition.DeathEffectPool);
                     }
                     gameObject.SetActive(false);
                 }
             }
        }
    }
}