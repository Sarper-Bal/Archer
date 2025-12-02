using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D;
using ArcadeBridge.ArcadeIdleEngine.Interactables;
using ArcadeBridge.ArcadeIdleEngine.Data.Variables;

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    [RequireComponent(typeof(TowerAttacker))]
    public class TowerManager : MonoBehaviour
    {
        [Header("💾 Kayıt Sistemi")]
        [SerializeField] private IntVariable _levelVariable;

        [Header("💰 Ekonomi")]
        [SerializeField] private Unlocker _upgradeZone;

        [Header("🚀 Kule Gelişimi")]
        [SerializeField] private List<WeaponDefinition> _weaponLevels;
        [SerializeField] private List<int> _upgradeCosts;

        [Header("🎨 Görsel Kontrol")]
        [SerializeField] private TowerVisualController _visualController;

        private TowerAttacker _attacker;
        
        private int CurrentLevelIndex
        {
            get => _levelVariable != null ? _levelVariable.RuntimeValue : 0;
            set { if (_levelVariable != null) _levelVariable.RuntimeValue = value; }
        }

        private void Awake()
        {
            _attacker = GetComponent<TowerAttacker>();
            if (_visualController == null) _visualController = GetComponent<TowerVisualController>();
        }

        private void Start()
        {
            StartCoroutine(InitializeRoutine());
        }
        
        private IEnumerator InitializeRoutine()
        {
            yield return null;
            
            if (CurrentLevelIndex >= _weaponLevels.Count) 
                CurrentLevelIndex = _weaponLevels.Count - 1;

            UpdateTowerState();
            InitializeUnlocker();
            
            Debug.Log($"🏰 Kule Hazır! Seviye: {CurrentLevelIndex + 1}");
        }

        private void InitializeUnlocker()
        {
            if (_upgradeZone == null) return;

            if (CurrentLevelIndex < _upgradeCosts.Count)
            {
                _upgradeZone.SetRequiredResource(_upgradeCosts[CurrentLevelIndex]);
            }
            else
            {
                _upgradeZone.gameObject.SetActive(false);
            }
        }

        public void OnUpgradePaid()
        {
            CurrentLevelIndex++;

            UpdateTowerState();

            if (CurrentLevelIndex < _upgradeCosts.Count)
            {
                int nextCost = _upgradeCosts[CurrentLevelIndex];
                _upgradeZone.SetRequiredResource(nextCost);
            }
            else
            {
                StartCoroutine(DisableUpgradeZoneRoutine());
            }
        }

        private IEnumerator DisableUpgradeZoneRoutine()
        {
            yield return null; 
            if (_upgradeZone != null) _upgradeZone.gameObject.SetActive(false);
        }

        private void UpdateTowerState()
        {
            // 1. Silahı Güncelle
            if (CurrentLevelIndex < _weaponLevels.Count)
            {
                _attacker.SetWeapon(_weaponLevels[CurrentLevelIndex]);
            }

            // 2. Görseli Güncelle
            if (_visualController != null)
            {
                _visualController.UpdateVisuals(CurrentLevelIndex, _attacker);
            }
        }
        
        [ContextMenu("🔄 Reset Tower Level")]
        public void ResetTower()
        {
            CurrentLevelIndex = 0;
            UpdateTowerState();
            InitializeUnlocker();
            if (_upgradeZone) _upgradeZone.gameObject.SetActive(true);
            Debug.Log("🔄 Kule Sıfırlandı.");
        }
    }
}