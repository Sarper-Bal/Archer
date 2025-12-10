using UnityEngine;
using IndianOceanAssets.Engine2_5D;

namespace ArcadeBridge.ArcadeIdleEngine.Experimental
{
    /// <summary>
    /// [TR] Stalker (Sinsi) Hareket Modu - Alan Taramalı:
    /// Düşman "Idle" modunda bekler. Oyuncu belirlenen alana (_detectionRadius) girerse takip başlar.
    /// Takip, oyuncunun son görüldüğü konuma gitme (Stalking) mantığıyla çalışır.
    /// Oyuncu çok uzaklaşırsa düşman tekrar Idle moduna döner.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))]
    public class StalkerEnemyMover : MonoBehaviour
    {
        private enum StalkerState 
        { 
            Idle,           // Oyuncuyu bekliyor, hareket etmiyor
            Searching,      // Oyuncu menzilde, yeni konumunu tespit etmeye çalışıyor
            MovingToLastPos // Oyuncunun en son görüldüğü noktaya yürüyor
        }

        [Header("🎯 Hedef ve Alan Ayarları")]
        [SerializeField] private string _targetTag = "Player";
        
        [Tooltip("Düşman oyuncuyu kaç metre öteden fark etsin?")]
        [SerializeField] private float _detectionRadius = 8f;
        
        [Tooltip("Oyuncu bu mesafeden daha uzağa kaçarsa takip bırakılır.")]
        [SerializeField] private float _loseRadius = 12f;

        [Header("⚙️ Hareket Ayarları")]
        [SerializeField] private float _rotationSpeed = 8f;
        [Tooltip("Hedefe ne kadar yaklaşınca varmış sayılsın?")]
        [SerializeField] private float _arrivalDistance = 0.5f;

        [Header("👀 Debug")]
        [SerializeField] private bool _showDebugGizmos = true;

        // --- Referanslar ---
        private Rigidbody _rb;
        private EnemyStats _stats;
        private Transform _cachedTarget; // Oyuncuyu bir kez bulup hafızada tutuyoruz

        // --- Durum Değişkenleri ---
        private StalkerState _currentState;
        private Vector3 _lastKnownPosition;
        
        // --- Optimizasyon (Kare Alma İşlemleri) ---
        private float _arrivalDistanceSqr;
        private float _detectionRadiusSqr;
        private float _loseRadiusSqr;
        
        // --- Zamanlayıcılar ---
        private float _nextScanTime;
        private const float SCAN_INTERVAL_IDLE = 0.5f;   // Idle iken saniyede 2 kez mesafe ölç
        private const float SCAN_INTERVAL_ACTIVE = 0.2f; // Takipte iken saniyede 5 kez kontrol et

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();

            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            // [OPTIMIZASYON] Karekök işlemi yapmamak için mesafelerin karesini sakla
            UpdateSqrDistances();
        }

        private void OnEnable()
        {
            _currentState = StalkerState.Idle;
            _nextScanTime = Time.time + Random.Range(0f, 0.5f); // Yük dengeleme
            ResetPhysics();
            
            // Eğer hedef daha önce bulunmadıysa bul (Sahne başında)
            if (_cachedTarget == null) FindTargetInScene();
        }

        private void Update()
        {
            // Durum Makinesi
            switch (_currentState)
            {
                case StalkerState.Idle:
                    HandleIdleState();
                    break;
                    
                case StalkerState.Searching: 
                    HandleSearchingState(); 
                    break;
                    
                case StalkerState.MovingToLastPos: 
                    CheckArrivalAndDistance(); 
                    break;
            }
        }

        private void FixedUpdate()
        {
            // Sadece hareket modundaysak fizik uygula
            if (_currentState == StalkerState.MovingToLastPos) 
            {
                MoveToPosition(_lastKnownPosition);
            }
        }

        /// <summary>
        /// Düşman bekleme modundadır. Sadece oyuncu yaklaştı mı diye bakar.
        /// </summary>
        private void HandleIdleState()
        {
            if (Time.time < _nextScanTime) return;
            _nextScanTime = Time.time + SCAN_INTERVAL_IDLE;

            if (!IsTargetValid())
            {
                FindTargetInScene(); // Hedef kayıpsa (ölmüş veya yok olmuşsa) tekrar ara
                return;
            }

            // Mesafe Kontrolü (Kareli işlem - Çok hızlı)
            float distSqr = (transform.position - _cachedTarget.position).sqrMagnitude;
            
            // Eğer oyuncu algılama alanına girdiyse -> AV BAŞLASIN
            if (distSqr < _detectionRadiusSqr)
            {
                LockNewDestination(); // Hemen konumu kilitle ve harekete geç
            }
        }

        /// <summary>
        /// Düşman aktif ama durmuş, oyuncunun yerini tespit etmeye çalışıyor.
        /// </summary>
        private void HandleSearchingState()
        {
            // Hedef hala geçerli mi?
            if (IsTargetValid())
            {
                // Geçerliyse konumu kilitle ve yürü
                LockNewDestination();
                return;
            }
            
            // Değilse ara (Çok nadir çalışır)
            if (Time.time > _nextScanTime)
            {
                FindTargetInScene();
                _nextScanTime = Time.time + SCAN_INTERVAL_IDLE;
            }
        }

        /// <summary>
        /// Yürürken yapılan kontroller: Vardık mı? Oyuncu çok uzaklaştı mı?
        /// </summary>
        private void CheckArrivalAndDistance()
        {
            // 1. Hedef çok uzaklaştı mı kontrolü (Ara sıra yap, her kare değil)
            if (Time.time >= _nextScanTime)
            {
                _nextScanTime = Time.time + SCAN_INTERVAL_ACTIVE;
                
                if (IsTargetValid())
                {
                    float distToRealTargetSqr = (transform.position - _cachedTarget.position).sqrMagnitude;
                    if (distToRealTargetSqr > _loseRadiusSqr)
                    {
                        // Oyuncu kaçtı, takibi bırak
                        StopMovingAndIdle();
                        return;
                    }
                }
            }

            // 2. Belirlenen noktaya vardık mı?
            float distToDestSqr = (transform.position - _lastKnownPosition).sqrMagnitude;
            if (distToDestSqr <= _arrivalDistanceSqr)
            {
                // Vardık! Dur ve tekrar Searching moduna geç (Yeni konum alacak)
                ResetPhysics();
                _currentState = StalkerState.Searching;
            }
        }

        private void LockNewDestination()
        {
            if (_cachedTarget != null)
            {
                _lastKnownPosition = _cachedTarget.position;
                _currentState = StalkerState.MovingToLastPos;
            }
        }

        private void StopMovingAndIdle()
        {
            ResetPhysics();
            _currentState = StalkerState.Idle;
        }

        private bool IsTargetValid()
        {
            return _cachedTarget != null && _cachedTarget.gameObject.activeInHierarchy;
        }

        private void FindTargetInScene()
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(_targetTag);
            if (targetObj != null)
            {
                _cachedTarget = targetObj.transform;
            }
        }

        private void MoveToPosition(Vector3 destination)
        {
            if (_stats.Definition == null) return;

            Vector3 direction = (destination - transform.position).normalized;
            direction.y = 0; 

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                if (Quaternion.Angle(_rb.rotation, lookRotation) > 0.5f)
                {
                    _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, lookRotation, _rotationSpeed * Time.fixedDeltaTime));
                }

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

        private void ResetPhysics()
        {
            #if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            #else
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            #endif
        }

        private void UpdateSqrDistances()
        {
            _arrivalDistanceSqr = _arrivalDistance * _arrivalDistance;
            _detectionRadiusSqr = _detectionRadius * _detectionRadius;
            _loseRadiusSqr = _loseRadius * _loseRadius;
        }

        // Editörde ne yaptığını görmek için
        private void OnDrawGizmosSelected()
        {
            if (!_showDebugGizmos) return;

            // Alanları çiz
            Gizmos.color = Color.yellow; // Algılama alanı
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);

            Gizmos.color = new Color(1, 0.5f, 0, 0.5f); // Kaybetme alanı (Turuncu)
            Gizmos.DrawWireSphere(transform.position, _loseRadius);

            if (_currentState == StalkerState.MovingToLastPos)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _lastKnownPosition);
                Gizmos.DrawWireSphere(_lastKnownPosition, 0.5f);
            }
        }
        
        private void OnValidate()
        {
            if (_loseRadius < _detectionRadius) _loseRadius = _detectionRadius + 2f;
            UpdateSqrDistances();
        }
    }
}