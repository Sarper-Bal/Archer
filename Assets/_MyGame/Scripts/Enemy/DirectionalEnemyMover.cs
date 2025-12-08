using UnityEngine;

namespace IndianOceanAssets.Engine2_5D
{
    /// <summary>
    /// [TR] Düşmanları tek bir yönde, fizik tabanlı ve CPU dostu şekilde hareket ettirir.
    /// [EN] Moves enemies in a single direction with physics-based, CPU-friendly logic.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))]
    public class DirectionalEnemyMover : MonoBehaviour
    {
        private Rigidbody _rb;
        private EnemyStats _stats;
        
        // [OPTIMIZATION] Hızı her karede çarpmak yerine, sonucu saklıyoruz.
        private Vector3 _cachedGroundVelocity; 
        private bool _isInitialized = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();
            
            // Fizik Motoru Ayarları (Performans için kritik)
            _rb.useGravity = true; 
            _rb.isKinematic = false; 
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.constraints = RigidbodyConstraints.FreezeRotation; 
        }

        private void OnEnable()
        {
            // Her doğuşta durumu sıfırla
            _isInitialized = false;
            _cachedGroundVelocity = Vector3.zero;
        }

        private void FixedUpdate()
        {
            // 1. Veri Hazır mı? (Veri gelmediyse işlem yapma)
            if (!_isInitialized)
            {
                // Veri (Stats) yüklendi mi kontrol et
                if (_stats != null && _stats.Definition != null)
                {
                    InitializeMovement();
                }
                return;
            }

            // 2. Fizik Hareketi (Matematik işlemi yok, sadece atama var)
            ApplyMovement();
        }

        /// <summary>
        /// [TR] Tüm matematiksel hesaplamalar burada sadece 1 kere yapılır.
        /// </summary>
        private void InitializeMovement()
        {
            // Yön ve Hız verisini al
            Vector3 direction = _stats.Definition.FixedDirection.normalized;
            float speed = _stats.Definition.MoveSpeed;

            // Y eksenini temizle (Yere paralel gitmesi için)
            direction.y = 0;

            // Yönü cache'le (Yüzünü dönme)
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
                // [OPTIMIZATION] Hız vektörünü hesapla ve sakla (Her frame çarpma yapmamak için)
                _cachedGroundVelocity = direction * speed;
                _isInitialized = true;
            }
        }

        private void ApplyMovement()
        {
            // [OPTIMIZATION] Yerçekimini korumak için Y hızını rigidbody'den alıyoruz.
            // X ve Z hızını ise önceden hesaplanmış cache'den çekiyoruz.
            
            Vector3 currentVelocity;
            
            #if UNITY_6000_0_OR_NEWER
            currentVelocity.x = _cachedGroundVelocity.x;
            currentVelocity.y = _rb.linearVelocity.y; // Yerçekimi etkisi
            currentVelocity.z = _cachedGroundVelocity.z;
            _rb.linearVelocity = currentVelocity;
            #else
            currentVelocity.x = _cachedGroundVelocity.x;
            currentVelocity.y = _rb.velocity.y; // Yerçekimi etkisi
            currentVelocity.z = _cachedGroundVelocity.z;
            _rb.velocity = currentVelocity;
            #endif
        }
    }
}