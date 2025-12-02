using UnityEngine;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; // WeaponDefinition için

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    [RequireComponent(typeof(TowerAttacker))]
    public class TowerManager : MonoBehaviour
    {
        [Header("Kule Seviyeleri (Silahlar)")]
        [Tooltip("Her bir eleman bir seviyedir. 0. eleman = Level 1")]
        [SerializeField] private List<WeaponDefinition> _weaponLevels;

        [Header("Görsel Modeller (Opsiyonel)")]
        [Tooltip("Seviyeye göre değişecek kule modelleri (Mesh). Listeyi boş bırakırsan model değişmez.")]
        [SerializeField] private List<GameObject> _levelModels;

        [Header("Durum")]
        [SerializeField] private int _currentLevelIndex = 0;

        private TowerAttacker _attacker;

        private void Awake()
        {
            _attacker = GetComponent<TowerAttacker>();
        }

        private void Start()
        {
            // Oyuna başlarken ilk seviye silahı yükle
            UpdateTowerState();
        }

        // --- INSPECTOR TEST BUTONU ---
        [ContextMenu("⚡ Upgrade Tower (Test)")]
        public void Upgrade()
        {
            // Son seviyede miyiz?
            if (_currentLevelIndex >= _weaponLevels.Count - 1)
            {
                Debug.Log("🚫 Kule zaten maksimum seviyede!");
                return;
            }

            // Seviye atla
            _currentLevelIndex++;
            UpdateTowerState();
            
            Debug.Log($"✅ Kule Yükseltildi! Yeni Seviye: {_currentLevelIndex + 1}");
        }

        // --- KULEYİ GÜNCELLE ---
        private void UpdateTowerState()
        {
            // 1. Silahı Değiştir
            if (_currentLevelIndex < _weaponLevels.Count)
            {
                WeaponDefinition newWeapon = _weaponLevels[_currentLevelIndex];
                _attacker.SetWeapon(newWeapon);
            }

            // 2. Modeli Değiştir (Eğer liste doluysa)
            if (_levelModels != null && _levelModels.Count > _currentLevelIndex)
            {
                // Hepsini kapat
                foreach (var model in _levelModels)
                {
                    if(model) model.SetActive(false);
                }
                // Sadece yeniyi aç
                if (_levelModels[_currentLevelIndex]) 
                    _levelModels[_currentLevelIndex].SetActive(true);
            }
        }
        
        // Seviyeyi sıfırlamak için (Test amaçlı)
        [ContextMenu("🔄 Reset Tower")]
        public void ResetTower()
        {
            _currentLevelIndex = 0;
            UpdateTowerState();
            Debug.Log("🔄 Kule Sıfırlandı.");
        }
    }
}