using UnityEngine;
using IndianOceanAssets.Engine2_5D;
using ArcadeBridge.ArcadeIdleEngine.Managers;

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    /// <summary>
    /// [TR] Düşmanların belirli noktaları (Waypoint) takip etmesini sağlayan optimize edilmiş hareket sınıfı.
    /// [EN] Optimized movement class allowing enemies to follow specific waypoints.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))]
    public class WaypointEnemyMover : MonoBehaviour
    {
        #region Settings
        [Header("Path Settings / Yol Ayarları")]
        [SerializeField] private WaypointRoute _manualRoute; 
        [SerializeField] private float _rotationSpeed = 8f;
        [SerializeField] private float _arrivalDistance = 0.5f;
        [SerializeField] private bool _loopPath = true;
        #endregion

        #region Private Variables
        private Rigidbody _rb;
        private EnemyStats _stats;
        private WaypointRoute _activeRoute;

        private int _currentWaypointIndex = 0;
        private Transform _currentTargetPoint;
        private float _arrivalDistanceSqr;
        private bool _isPathComplete = false;
        
        // [OPTIMIZATION] Çöp (Garbage) oluşumunu engellemek için önbelleklenmiş değişkenler
        private Vector3 _moveVelocity;
        private Vector3 _direction;
        #endregion

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();
            
            // Fizik motoru ayarları
            _rb.useGravity = true;
            _rb.isKinematic = false; // Fizik tabanlı hareket için false
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.interpolation = RigidbodyInterpolation.Interpolate; // Akıcı hareket için

            // Mesafe kontrolü için karesini alıp saklıyoruz (Her karede karekök almamak için)
            _arrivalDistanceSqr = _arrivalDistance * _arrivalDistance;
        }

        private void OnEnable()
        {
            ResolveRoute(); 
            ResetPath();
        }

        private void Start()
        {
            if (_activeRoute == null)
            {
                ResolveRoute();
                ResetPath();
            }
        }

        /// <summary>
        /// Rota bulma işlemini yönetir. Önce manuel, yoksa veritabanı ID'sine bakar.
        /// </summary>
        private void ResolveRoute()
        {
            if (_activeRoute != null) return;
            
            // 1. Manuel rota varsa onu kullan (Inspector'dan atanan)
            if (_manualRoute != null)
            {
                _activeRoute = _manualRoute;
                return;
            }
            
            // 2. Data'dan gelen ID ile rota bul
            if (_stats.Definition != null && _stats.Definition.PatrolRouteID != null)
            {
                // RouteManager singleton kontrolü
                if (RouteManager.Instance != null)
                {
                    _activeRoute = RouteManager.Instance.GetRoute(_stats.Definition.PatrolRouteID);
                }
            }
        }

        private void FixedUpdate()
        {
            // Rota yoksa veya yol bittiyse dur.
            if (_activeRoute == null) return;

            if (_isPathComplete || _currentTargetPoint == null || _stats.Definition == null) 
            {
                StopMovement();
                return;
            }

            MoveAlongPath();
            CheckArrival();
        }

        private void StopMovement()
        {
            // Hızı sıfırla ama düşmeyi engelleme (Y eksenini koru)
            #if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
            #else
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
            #endif
        }

        private void MoveAlongPath()
        {
            // Y eksenini yoksayarak yön bul
            _direction = _currentTargetPoint.position - transform.position;
            _direction.y = 0;
            _direction.Normalize(); // Normalized vektör

            if (_direction != Vector3.zero)
            {
                // [OPTIMIZATION] Sadece açı farkı 1 dereceden büyükse döndür.
                // Sürekli Slerp hesaplamak CPU'yu yorar.
                Quaternion targetRotation = Quaternion.LookRotation(_direction);
                if (Quaternion.Angle(_rb.rotation, targetRotation) > 1f)
                {
                    _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime));
                }

                // Hareketi uygula
                if (_stats.Definition != null)
                {
                    _moveVelocity = _direction * _stats.Definition.MoveSpeed;
                    
                    // Y ekseni hızını (yerçekimi) koru
                    #if UNITY_6000_0_OR_NEWER
                    _moveVelocity.y = _rb.linearVelocity.y;
                    _rb.linearVelocity = _moveVelocity;
                    #else
                    _moveVelocity.y = _rb.velocity.y;
                    _rb.velocity = _moveVelocity;
                    #endif
                }
            }
        }

        private void CheckArrival()
        {
            // Vector3.Distance yerine manuel matematik (CPU dostu)
            float dx = transform.position.x - _currentTargetPoint.position.x;
            float dz = transform.position.z - _currentTargetPoint.position.z;
            float distSqr = (dx * dx) + (dz * dz);

            // Karesi alınmış mesafe ile karşılaştır
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
            _activeRoute = null;
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
    }
}