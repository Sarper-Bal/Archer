using UnityEngine;

namespace IndianOceanAssets.Engine2_5D
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))] // Veri dosyasını taşıyan scripti zorunlu kıl
    public class SimpleEnemyMover : MonoBehaviour
    {
        [Header("Hedef Ayarları")]
        [Tooltip("Takip edilecek etiketi girin (Genellikle 'Player')")]
        [SerializeField] private string _targetTag = "Player";
        [SerializeField] private float _rotationSpeed = 5f;

        // Private Değişkenler
        private Transform _target;
        private Rigidbody _rb;
        private EnemyStats _stats; // Veriye ulaşacağımız köprü
        
        private float _lastAttackTime;
        private const float DAMAGE_INTERVAL = 1.0f; // Kaç saniyede bir vursun

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();

            // --- FİZİK AYARLARI (Test Scriptindeki Gibi) ---
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.mass = 100f; // İtilmeyi zorlaştırmak için
        }

        private void OnEnable()
        {
            // Düşman her doğduğunda hedefi tazele
            FindTarget();
        }

        private void Update()
        {
            // Hedef yoksa sürekli aramaya devam et (Player geç doğarsa diye)
            if (_target == null)
            {
                FindTarget();
            }
        }

        private void FixedUpdate()
        {
            // Hedef yoksa veya Veri dosyası (ScriptableObject) atanmadıysa dur
            if (_target == null) return;
            if (_stats.Definition == null) return;

            MoveLogic();
        }

        private void FindTarget()
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(_targetTag);
            if (targetObj != null)
            {
                _target = targetObj.transform;
            }
        }

        private void MoveLogic()
        {
            // 1. Yönü Bul (Player nerede?)
            Vector3 direction = (_target.position - transform.position).normalized;
            direction.y = 0; // Yere paralel kalsın

            if (direction != Vector3.zero)
            {
                // 2. Dönüş (Yüzünü dön)
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, lookRotation, _rotationSpeed * Time.fixedDeltaTime));

                // 3. Hareket (Veri dosyasındaki hızı kullan)
                float currentSpeed = _stats.Definition.MoveSpeed;
                Vector3 velocity = direction * currentSpeed;

                // Yerçekimini koru (Y ekseni hızını bozma)
#if UNITY_6000_0_OR_NEWER
                velocity.y = _rb.linearVelocity.y;
                _rb.linearVelocity = velocity;
#else
                velocity.y = _rb.velocity.y;
                _rb.velocity = velocity;
#endif
            }
        }

        // --- HASAR BÖLÜMÜ ---
        private void OnCollisionStay(Collision collision)
        {
            // Çarpan şey Player mı?
            if (collision.gameObject.CompareTag(_targetTag))
            {
                // Saldırı zamanı geldi mi?
                if (Time.time >= _lastAttackTime + DAMAGE_INTERVAL)
                {
                    DealDamage(collision.gameObject);
                    _lastAttackTime = Time.time;
                }
            }
        }

        private void DealDamage(GameObject targetObj)
        {
            if (_stats.Definition == null) return;

            // Veri dosyasındaki hasar miktarını al
            float damageAmount = _stats.Definition.ContactDamage;

            // Hasar verilebilir (IDamageable) bir şey mi?
            if (targetObj.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damageAmount);
            }
            else
            {
                // Belki Health scripti parent objededir
                var parentDamageable = targetObj.GetComponentInParent<IDamageable>();
                if (parentDamageable != null)
                {
                    parentDamageable.TakeDamage(damageAmount);
                }
            }
        }
    }
}