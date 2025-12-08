using UnityEngine;

namespace IndianOceanAssets.Engine2_5D
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))]
    public class DirectionalEnemyMover : MonoBehaviour
    {
        private Rigidbody _rb;
        private EnemyStats _stats;
        
        private Vector3 _moveVelocity;
        private Vector3 _cachedDirection;
        private bool _isInitialized = false; // [DÜZELTME] Başlatma kontrolü eklendi

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();
            
            _rb.useGravity = true;
            _rb.isKinematic = false; 
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.constraints = RigidbodyConstraints.FreezeRotation; 
        }

        private void OnEnable()
        {
            // Script her açıldığında sıfırla, veriyi bekle.
            _isInitialized = false;
            _cachedDirection = Vector3.zero;
        }

        private void FixedUpdate()
        {
            // 1. Veri henüz gelmediyse hiçbir şey yapma (Hata vermesini engeller)
            if (_stats == null || _stats.Definition == null) return;

            // 2. Veri geldi ama yönü henüz hesaplamadıysak hesapla (Sadece 1 kez çalışır)
            if (!_isInitialized)
            {
                InitializeMovement();
            }

            // 3. Eğer hala yön yoksa (0,0,0 ise) hareket etme
            if (_cachedDirection == Vector3.zero) return;

            Move();
        }

        private void InitializeMovement()
        {
            // Yön verisini al
            Vector3 rawDirection = _stats.Definition.FixedDirection;

            // Eğer Inspector'da (0,0,0) unutulmuşsa hata verme, sadece çalışma
            if (rawDirection == Vector3.zero) 
            {
                // Debug.LogWarning($"{gameObject.name} için FixedDirection (0,0,0) girilmiş! Hareket etmeyecek.");
                return;
            }

            _cachedDirection = rawDirection.normalized;
            _cachedDirection.y = 0; // Yüksekliği koru

            // Yüzünü dön
            if (_cachedDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(_cachedDirection);
            }

            _isInitialized = true; // Artık hazırsın
        }

        private void Move()
        {
            // Hızı hesapla
            _moveVelocity = _cachedDirection * _stats.Definition.MoveSpeed;

            // Yerçekimini koruyarak hızı uygula
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