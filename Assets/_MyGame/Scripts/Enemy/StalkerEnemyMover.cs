using UnityEngine;
using IndianOceanAssets.Engine2_5D;

namespace ArcadeBridge.ArcadeIdleEngine.Experimental
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))]
    public class StalkerEnemyMover : MonoBehaviour
    {
        // Düşman Davranış Durumları
        private enum StalkerState
        {
            Searching,      // Hedef arıyor veya bekliyor
            MovingToLastPos // Son bilinen konuma fiziksel olarak gidiyor
        }

        [Header("Hareket Ayarları")]
        [SerializeField] private string _targetTag = "Player";
        [SerializeField] private float _rotationSpeed = 8f;
        [SerializeField] private float _arrivalDistance = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugGizmos = true;

        // Bileşenler
        private Rigidbody _rb;
        private EnemyStats _stats;
        private Transform _realTargetTransform;

        // Durum Değişkenleri
        private StalkerState _currentState;
        private Vector3 _lastKnownPosition;
        
        // [OPTİMİZASYON] Karekök işleminden kaçınmak için mesafenin karesini tutuyoruz.
        private float _arrivalDistanceSqr; 

        // Zamanlayıcılar
        private float _nextSearchTime;
        private const float SEARCH_INTERVAL = 1.0f; // Saniyede 1 kez hedef kontrolü

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();

            // Fizik Optimizasyonu
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            // [OPTİMİZASYON] Mesafenin karesini bir kez hesapla, her karede tekrar hesaplama
            _arrivalDistanceSqr = _arrivalDistance * _arrivalDistance;
        }

        private void OnEnable()
        {
            // Resetleme işlemleri
            _currentState = StalkerState.Searching;
            _nextSearchTime = Time.time;
            _rb.linearVelocity = Vector3.zero; // Unity 6 için linearVelocity
        }

        private void Update()
        {
            // Mantıksal kararlar Update içinde verilir
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
            // Fiziksel hareket SADECE FixedUpdate içinde yapılmalıdır
            if (_currentState == StalkerState.MovingToLastPos)
            {
                MoveToPosition(_lastKnownPosition);
            }
        }

        private void HandleSearchingState()
        {
            // Eğer elimde canlı bir hedef referansı varsa, hemen konumunu kilitle
            if (_realTargetTransform != null && _realTargetTransform.gameObject.activeInHierarchy)
            {
                LockNewDestination();
                return;
            }

            // Hedefim yoksa, belirli aralıklarla sahneyi tara (Performans koruması)
            if (Time.time >= _nextSearchTime)
            {
                FindTargetInScene();
                _nextSearchTime = Time.time + SEARCH_INTERVAL;
            }
        }

        private void CheckArrival()
        {
            // [KRİTİK OPTİMİZASYON] Vector3.Distance yerine saf matematiksel kare hesabı.
            // Y eksenini yok sayarak (2D düzlemde) mesafe kontrolü yapıyoruz.
            
            float dx = transform.position.x - _lastKnownPosition.x;
            float dz = transform.position.z - _lastKnownPosition.z;
            float distSqr = (dx * dx) + (dz * dz); // Karekök almadan en hızlı mesafe hesabı

            // Eğer kalan mesafe karesi, hedef mesafenin karesinden küçükse varmışızdır.
            if (distSqr <= _arrivalDistanceSqr)
            {
                _rb.linearVelocity = Vector3.zero; // Unity 6: Dur
                _currentState = StalkerState.Searching; // Yeni hedef için dur ve düşün
            }
        }

        private void LockNewDestination()
        {
            // Player'ın o anki "snapshot" (anlık) konumunu alıyoruz.
            _lastKnownPosition = _realTargetTransform.position;
            _currentState = StalkerState.MovingToLastPos;
        }

        private void FindTargetInScene()
        {
            // GameObject.Find pahalıdır ama saniyede en fazla 1 kez çalışacağı için güvenli.
            GameObject targetObj = GameObject.FindGameObjectWithTag(_targetTag);
            if (targetObj != null)
            {
                _realTargetTransform = targetObj.transform;
                LockNewDestination();
            }
        }

        private void MoveToPosition(Vector3 destination)
        {
            if (_stats.Definition == null) return;

            // Yön Hesaplama (Y eksenini sıfırlayarak)
            Vector3 direction = (destination - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                // Yumuşak Dönüş
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, lookRotation, _rotationSpeed * Time.fixedDeltaTime));

                // Hareket Hızı
                Vector3 moveVelocity = direction * _stats.Definition.MoveSpeed;
                
                // [UNITY 6 GÜNCELLEMESİ]
                // linearVelocity kullanımı (Eski 'velocity' yerine)
                moveVelocity.y = _rb.linearVelocity.y; // Yerçekimini korumak için Y hızını ellemiyoruz
                _rb.linearVelocity = moveVelocity;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(_targetTag))
            {
                ExplodeAndDie(collision.gameObject);
            }
        }

        private void ExplodeAndDie(GameObject targetObj)
        {
            if (_stats.Definition == null) return;

            // Hasar ver
            if (targetObj.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_stats.Definition.ContactDamage);
            }

            // Efekt oynat (Pooling sistemi ile)
            if (_stats.Definition.DeathEffectPool != null)
            {
                var effect = _stats.Definition.DeathEffectPool.Get();
                effect.transform.position = transform.position;
                effect.transform.rotation = Quaternion.identity;
                effect.Initialize(_stats.Definition.DeathEffectPool);
            }

            // Kendini havuza geri gönder / Kapat
            gameObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showDebugGizmos) return;

            if (_currentState == StalkerState.MovingToLastPos)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_lastKnownPosition, _arrivalDistance);
                Gizmos.DrawLine(transform.position, _lastKnownPosition);
            }
        }
        
        // Editörde değer değişirse square değerini güncelle
        private void OnValidate()
        {
            _arrivalDistanceSqr = _arrivalDistance * _arrivalDistance;
        }
    }
}