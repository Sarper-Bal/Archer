using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IndianOceanAssets.Engine2_5D; 
using IndianOceanAssets.Engine2_5D.Managers; // SmartWaveManager'a erişim için
using DG.Tweening;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
    public class DestructibleBarrier : MonoBehaviour, IDamageable
    {
        [Header("Dayanıklılık")]
        [SerializeField] private float _maxHealth = 1000f;

        [Header("Görsel Parçalar")]
        [SerializeField] private GameObject _barrierModel; 
        [SerializeField] private Canvas _uiCanvas;         
        [SerializeField] private Image _fillBar;           
        [SerializeField] private TextMeshProUGUI _healthText; 

        [Header("Efektler")]
        [SerializeField] private ParticleSystem _destructionParticles; 
        
        private float _currentHealth;
        private bool _isDestroyed = false;
        private SmartWaveManager _waveManager; // [YENİ] Manager Referansı

        public bool IsDead => _isDestroyed;
        public float CurrentHealth => _currentHealth;

        public event System.Action<float> OnHealthChanged;
        public event System.Action OnDeath;
        public event System.Action OnDamageTaken;

        private void Awake()
        {
            // [YENİ] Sahnedeki Manager'ı bul
            _waveManager = FindObjectOfType<SmartWaveManager>();
        }

        private void OnEnable()
        {
            ResetBarrier();
            
            // [YENİ] Reset olayına abone ol
            if (_waveManager != null)
                _waveManager.OnGameReset += ResetBarrier;
        }

        private void OnDisable()
        {
            // [YENİ] Abonelikten çık (Memory Leak önlemi)
            if (_waveManager != null)
                _waveManager.OnGameReset -= ResetBarrier;
        }

        public void InitializeHealth(float maxHealth, DeathEffectPool deathPool) 
        {
            _maxHealth = maxHealth;
            ResetBarrier();
        }

        public void TakeDamage(float amount)
        {
            if (_isDestroyed) return;

            _currentHealth -= amount;
            OnDamageTaken?.Invoke();
            UpdateUI();

            if (_currentHealth <= 0)
            {
                BreakBarrier();
            }
        }

        public void Heal(float amount) { } 

        // [GÜNCELLEME] Bu fonksiyon artık public değilse de Interface gereği public kalmalı
        // Ama asıl çağrıyı Event üzerinden alıyor.
        public void ResetHealth() => ResetBarrier();

        private void BreakBarrier()
        {
            _isDestroyed = true;
            OnDeath?.Invoke(); // BaseObjectiveController burayı dinleyecek

            if (_barrierModel) _barrierModel.SetActive(false);
            if (_uiCanvas) _uiCanvas.gameObject.SetActive(false);
            if (_destructionParticles) _destructionParticles.Play();

            var navObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
            if (navObstacle) navObstacle.enabled = false;
            
            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
        }

        private void ResetBarrier()
        {
            _currentHealth = _maxHealth;
            _isDestroyed = false;

            if (_barrierModel) _barrierModel.SetActive(true);
            if (_uiCanvas) _uiCanvas.gameObject.SetActive(true);
            
            var navObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
            if (navObstacle) navObstacle.enabled = true;

            var col = GetComponent<Collider>();
            if (col) col.enabled = true;

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_healthText != null)
                _healthText.text = Mathf.Max(0, _currentHealth).ToString("F0");

            if (_fillBar != null)
                _fillBar.fillAmount = _currentHealth / _maxHealth;
        }
    }
}