using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IndianOceanAssets.Engine2_5D; 
using IndianOceanAssets.Engine2_5D.Managers; 
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
        private SmartWaveManager _waveManager; 

        public bool IsDead => _isDestroyed;
        public float CurrentHealth => _currentHealth;

        public event System.Action<float> OnHealthChanged;
        public event System.Action OnDeath;
        public event System.Action OnDamageTaken;

        private void Awake()
        {
            _waveManager = FindObjectOfType<SmartWaveManager>();
        }

        private void Start()
        {
            if (_waveManager == null) _waveManager = FindObjectOfType<SmartWaveManager>();

            if (_waveManager != null)
            {
                _waveManager.OnGameReset += ResetBarrier;
            }
            
            ResetBarrier();
        }

        private void OnDestroy()
        {
            if (_waveManager != null) _waveManager.OnGameReset -= ResetBarrier;
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
        public void ResetHealth() => ResetBarrier();

        private void BreakBarrier()
        {
            _isDestroyed = true;
            OnDeath?.Invoke();

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

    if (_barrierModel) 
    {
        _barrierModel.SetActive(true);
        // [EKLEME] Modelin transformunu da sıfırla (AnimationController yapamzsa diye yedek)
        // _barrierModel.transform.localScale = Vector3.one; // (Gerekirse bu yorumu aç)
    }

    if (_uiCanvas) 
    {
        _uiCanvas.gameObject.SetActive(true);
        // [EKLEME] Canvas boyutunu zorla düzelt. DOTween bazen burayı da bozabilir.
        _uiCanvas.transform.localScale = Vector3.one; 
    }
    
    var navObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
    if (navObstacle) navObstacle.enabled = true;

    var col = GetComponent<Collider>();
    if (col) col.enabled = true;

    UpdateUI();
    
    Debug.Log($"♻️ {gameObject.name}: Tamir edildi ve görseller resetlendi.");
}
        private void UpdateUI()
        {
            if (_healthText != null) _healthText.text = Mathf.Max(0, _currentHealth).ToString("F0");
            if (_fillBar != null) _fillBar.fillAmount = _currentHealth / _maxHealth;
        }
    }
}