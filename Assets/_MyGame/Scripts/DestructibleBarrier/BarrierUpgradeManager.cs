using UnityEngine;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D.Managers; 
using ArcadeBridge.ArcadeIdleEngine.Data.Variables; 

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
    [RequireComponent(typeof(DestructibleBarrier))]
    public class BarrierUpgradeManager : MonoBehaviour
    {
        [Header("💾 Kayıt Sistemi")]
        [SerializeField] private IntVariable _levelVariable;

        [Header("💰 Ekonomi & UI")]
        [SerializeField] private Unlocker _upgradeZone;

        [Header("🚀 Gelişim Ayarları")]
        [SerializeField] private List<float> _healthPerLevel;
        [SerializeField] private List<int> _upgradeCosts;

        [Header("🎨 Görsel Kontrol")]
        [SerializeField] private BarrierVisualController _visualController;

        // Animasyon kontrolcüsünü de yönetmek için
        private BarrierAnimationController _animController;

        private DestructibleBarrier _barrier;
        private SmartWaveManager _waveManager;
        
        private int CurrentLevelIndex
        {
            get => _levelVariable != null ? _levelVariable.RuntimeValue : 0;
            set { if (_levelVariable != null) _levelVariable.RuntimeValue = value; }
        }

        private void Awake()
        {
            _barrier = GetComponent<DestructibleBarrier>();
            _waveManager = FindObjectOfType<SmartWaveManager>();
            
            if (_visualController == null) _visualController = GetComponent<BarrierVisualController>();
            _animController = GetComponent<BarrierAnimationController>();
        }

        private void Start()
        {
            if (_waveManager != null)
            {
                _waveManager.OnWaveStarted += HideUnlocker;
                _waveManager.OnWaveCompleted += RefreshUnlockerState;
                _waveManager.OnGameReset += RefreshUnlockerState;
            }
            
            InitializeBarrierState();
        }
        
        private void OnDestroy()
        {
            if (_waveManager != null)
            {
                _waveManager.OnWaveStarted -= HideUnlocker;
                _waveManager.OnWaveCompleted -= RefreshUnlockerState;
                _waveManager.OnGameReset -= RefreshUnlockerState;
            }
        }

        private void InitializeBarrierState()
        {
            if (CurrentLevelIndex >= _healthPerLevel.Count) CurrentLevelIndex = _healthPerLevel.Count - 1;
            UpdateBarrierStats();
            RefreshUnlockerState();
        }

        public void OnUpgradePaid()
        {
            CurrentLevelIndex++;
            UpdateBarrierStats();
            RefreshUnlockerState();
        }

        private void UpdateBarrierStats()
        {
            // 1. Görseli Değiştir ve Yeni Objeyi Al
            GameObject activeModel = null;
            if (_visualController) 
            {
                activeModel = _visualController.UpdateVisuals(CurrentLevelIndex);
            }

            // 2. Yeni Modeli Diğer Scriptlere Tanıt (Referans Kopukluğunu Çözer)
            if (activeModel != null)
            {
                _barrier.UpdateVisualModel(activeModel); // Bariyer artık bunu yok edecek
                
                if (_animController) 
                    _animController.UpdateVisualTarget(activeModel.transform); // Animasyon artık bunu sallayacak
            }

            // 3. Canı Güncelle
            if (CurrentLevelIndex < _healthPerLevel.Count)
            {
                float newMax = _healthPerLevel[CurrentLevelIndex];
                _barrier.InitializeHealth(newMax, null); 
            }
        }

        private void HideUnlocker()
        {
            if (_upgradeZone != null) _upgradeZone.gameObject.SetActive(false);
        }

        private void RefreshUnlockerState()
        {
            if (_upgradeZone == null) return;

            if (CurrentLevelIndex < _upgradeCosts.Count)
            {
                _upgradeZone.SetRequiredResource(_upgradeCosts[CurrentLevelIndex]);
                _upgradeZone.gameObject.SetActive(true);
            }
            else
            {
                _upgradeZone.gameObject.SetActive(false);
            }
        }

        // [YENİ] TEST İÇİN RESET FONKSİYONU
        // Inspector'da script ismine sağ tıklayıp çalıştırabilirsin.
        [ContextMenu("🔄 Reset Barrier Level")]
        public void ResetBarrierLevel()
        {
            CurrentLevelIndex = 0;
            InitializeBarrierState();
            
            // Resetlendiğinde Unlocker'ı da zorla aç (Eğer kapalıysa)
            if (_upgradeZone != null) _upgradeZone.gameObject.SetActive(true);
            
            Debug.Log("🔄 Bariyer Seviyesi 0'a Sıfırlandı.");
        }
    }
}