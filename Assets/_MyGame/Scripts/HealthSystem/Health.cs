using UnityEngine;
using System;
using ArcadeBridge.ArcadeIdleEngine.Pools;

namespace IndianOceanAssets.Engine2_5D
{
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Default Settings")]
        [Tooltip("Eğer dışarıdan bir veri dosyası atanmazsa bu değer kullanılır.")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private bool _isInvincible = false;

        [Header("Effects")]
        [SerializeField] private DeathEffectPool _defaultDeathEffectPool;
        
        private float _currentHealth;
        private bool _isDead;
        private DeathEffectPool _currentDeathPool; 

        // Visuals scripti bu eventleri dinleyecek
        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        public event Action OnDamageTaken; // EnemyVisuals bunu dinliyor!

        public bool IsDead => _isDead;
        public float CurrentHealth => _currentHealth;

        private void Awake()
        {
            _currentDeathPool = _defaultDeathEffectPool;
        }

        private void OnEnable()
        {
            ResetHealth();
        }

        public void InitializeHealth(float newMaxHealth, DeathEffectPool deathPool = null)
        {
            _maxHealth = newMaxHealth; 
            
            if (deathPool != null)
            {
                _currentDeathPool = deathPool;
            }

            ResetHealth(); 
        }

        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
            OnHealthChanged?.Invoke(1f);
        }

        public void TakeDamage(float amount)
        {
            if (_isDead || _isInvincible) return;

            _currentHealth -= amount;
            
            // [ÖNEMLİ] Burası tetiklendiğinde EnemyVisuals otomatik olarak animasyonu oynatacak.
            OnDamageTaken?.Invoke(); 
            OnHealthChanged?.Invoke(_currentHealth / _maxHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (_isDead) return;
            _currentHealth += amount;
            if (_currentHealth > _maxHealth) _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth / _maxHealth);
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;
            OnDeath?.Invoke();
            PlayDeathEffect();

            if (gameObject.CompareTag("Player"))
            {
                Debug.Log("GAME OVER");
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void PlayDeathEffect()
        {
            if (_currentDeathPool != null)
            {
                var effect = _currentDeathPool.Get();
                effect.transform.position = transform.position + Vector3.up;
                effect.transform.rotation = Quaternion.identity;
                effect.Initialize(_currentDeathPool);
            }
        }
    }
}