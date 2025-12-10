using UnityEngine;
using IndianOceanAssets.Engine2_5D;

namespace ArcadeBridge.ArcadeIdleEngine.Experimental
{
    /// <summary>
    /// [TR] Stalker (Sinsi) Hareket Modu:
    /// Düşman oyuncuyu sürekli takip etmez. Oyuncunun o anki konumunu "En Son Bilinen Konum" olarak kaydeder,
    /// o noktaya kadar yürür. Oraya varınca durur, etrafına bakar ve oyuncunun yeni yerini tespit edip oraya yürür.
    /// Bu sayede oyuncu sürekli hareket ederek düşmanı "kiting" (peşinden koşturma) yapabilir.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))]
    public class StalkerEnemyMover : MonoBehaviour
    {
        private enum StalkerState 
        { 
            Searching,      // Hedefi arıyor veya yeni hedef belirlemeye çalışıyor
            MovingToLastPos // Belirlenen son noktaya doğru yürüyor
        }

        [Header("Hareket Ayarları")]
        [SerializeField] private string _targetTag = "Player";
        [SerializeField] private float _rotationSpeed = 8f;
        [Tooltip("Hedefe ne kadar yaklaşınca varmış sayılsın?")]
        [SerializeField] private float _arrivalDistance = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugGizmos = true;

        // --- Referanslar ---
        private Rigidbody _rb;
        private EnemyStats _stats;
        private Transform _cachedTarget; // [OPTIMIZASYON] Hedefi bir kez bulunca burada saklarız.

        // --- Durum Değişkenleri ---
        private StalkerState _currentState;
        private Vector3 _lastKnownPosition; // Düşmanın gitmeye çalıştığı sabit nokta
        private float _arrivalDistanceSqr;  // [OPTIMIZASYON] Mesafe karesi (Karekök almamak için)
        private float _nextSearchTime;
        private const float SEARCH_INTERVAL = 1.0f; // Saniyede 1 kez hedef ara (Eğer kayıpsa)

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();

            // Rigidbody Ayarları (Fizik motorunu yormamak için)
            _rb.useGravity = true;
            _rb.isKinematic = false;
            // Sadece Y ekseninde dön, devrilme.
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            // [OPTIMIZASYON] Mesafeyi her karede çarpmamak için başta karesini alıyoruz.
            _arrivalDistanceSqr = _arrivalDistance * _arrivalDistance;
        }

        private void OnEnable()
        {
            _currentState = StalkerState.Searching;
            _nextSearchTime = Time.time + Random.Range(0f, 0.5f); // Hepsi aynı anda başlamasın (Yük dengeleme)
            
            ResetPhysics();
        }

        private void Update()
        {
            // Duruma göre mantık çalıştır
            switch (_currentState)
            {
                case StalkerState.Searching: 
                    HandleSearchingState(); 
                    break;
                case StalkerState.MovingToLastPos: 
                    CheckArrival(); 
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
        /// Hedef arama veya hedef varsa pozisyon kilitleme mantığı.
        /// </summary>
        private void HandleSearchingState()
        {
            // 1. Elimizde zaten geçerli bir hedef var mı?
            if (IsTargetValid())
            {
                // Hedef geçerliyse hemen onun pozisyonunu kilitle ve yola koyul.
                LockNewDestination();
                return;
            }

            // 2. Hedef yoksa, belirli aralıklarla sahneyi tara.
            if (Time.time >= _nextSearchTime)
            {
                FindTargetInScene();
                _nextSearchTime = Time.time + SEARCH_INTERVAL;
            }
        }

        /// <summary>
        /// Belirlenen noktaya vardık mı kontrolü.
        /// </summary>
        private void CheckArrival()
        {
            // [OPTIMIZASYON] sqrMagnitude kullanarak karekök işleminden kaçınıyoruz.
            // (hedef - ben).sqrMagnitude
            float distSqr = (transform.position - _lastKnownPosition).sqrMagnitude;

            // Yüksekliği (Y ekseni) ihmal etmek istersen şu yöntemi kullan:
            float dx = transform.position.x - _lastKnownPosition.x;
            float dz = transform.position.z - _lastKnownPosition.z;
            float flatDistSqr = (dx * dx) + (dz * dz);

            if (flatDistSqr <= _arrivalDistanceSqr)
            {
                // Vardık! Dur ve tekrar arama moduna geç.
                ResetPhysics();
                _currentState = StalkerState.Searching;
            }
        }

        private void LockNewDestination()
        {
            // O an hedef neredeyse orayı hafızaya al.
            // Hedef sonradan hareket etse bile düşman BU noktaya gidecek (Stalker mantığı).
            _lastKnownPosition = _cachedTarget.position;
            _currentState = StalkerState.MovingToLastPos;
        }

        private bool IsTargetValid()
        {
            return _cachedTarget != null && _cachedTarget.gameObject.activeInHierarchy;
        }

        private void FindTargetInScene()
        {
            // [OPTIMIZASYON] Bu işlem ağırdır, sadece hedef kayıpsa çalışır.
            GameObject targetObj = GameObject.FindGameObjectWithTag(_targetTag);
            if (targetObj != null)
            {
                _cachedTarget = targetObj.transform;
                LockNewDestination(); // Bulur bulmaz yola çık
            }
        }

        private void MoveToPosition(Vector3 destination)
        {
            if (_stats.Definition == null) return;

            Vector3 direction = (destination - transform.position).normalized;
            direction.y = 0; // Havaya/Yere bakmasın

            // Hareket vektörü çok küçükse işlem yapma
            if (direction.sqrMagnitude > 0.001f)
            {
                // Rotasyon (Yumuşak Dönüş)
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                // [OPTIMIZASYON] Zaten bakıyorsa Slerp hesabı yapma
                if (Quaternion.Angle(_rb.rotation, lookRotation) > 0.5f)
                {
                    _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, lookRotation, _rotationSpeed * Time.fixedDeltaTime));
                }

                // Hareket (Hız)
                Vector3 moveVelocity = direction * _stats.Definition.MoveSpeed;
                
                // Yerçekimini (Y eksenindeki hızı) koru, diğer eksenleri değiştir.
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

        // Editörde ne yaptığını görmek için çizgiler çizer
        private void OnDrawGizmosSelected()
        {
            if (!_showDebugGizmos) return;

            if (_currentState == StalkerState.MovingToLastPos)
            {
                Gizmos.color = Color.red; // Gittiği hedef (Kırmızı)
                Gizmos.DrawWireSphere(_lastKnownPosition, _arrivalDistance);
                Gizmos.DrawLine(transform.position, _lastKnownPosition);
            }
            else
            {
                Gizmos.color = Color.yellow; // Arıyor (Sarı)
                Gizmos.DrawWireSphere(transform.position, 1f);
            }
        }
        
        private void OnValidate()
        {
            // Editörde değeri değiştirince karesini otomatik güncelle
            _arrivalDistanceSqr = _arrivalDistance * _arrivalDistance;
        }
    }
}