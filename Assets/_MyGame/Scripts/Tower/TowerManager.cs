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
        [Tooltip("Kule seviyesini tutan IntVariable.")]
        [SerializeField] private IntVariable _levelVariable;

        [Header("💰 Ekonomi")]
        [SerializeField] private Unlocker _upgradeZone;

        [Header("🚀 Kule Gelişimi (Sadece Silah)")]
        [SerializeField] private List<WeaponDefinition> _weaponLevels;
        
        [Tooltip("Yükseltme ücretleri. (Örn: 0. eleman = Lvl 1'den 2'ye geçiş ücreti)")]
        [SerializeField] private List<int> _upgradeCosts;

        private TowerAttacker _attacker;
        
        // Kayıtlı veriyi okuma/yazma yardımcısı
        private int CurrentLevelIndex
        {
            get => _levelVariable != null ? _levelVariable.RuntimeValue : 0;
            set { if (_levelVariable != null) _levelVariable.RuntimeValue = value; }
        }

        private void Awake()
        {
            _attacker = GetComponent<TowerAttacker>();
        }

        private void Start()
        {
            // Verilerin yüklenmesi için 1 kare bekle (Emin olmak için)
            StartCoroutine(InitializeRoutine());
        }
        
        private IEnumerator InitializeRoutine()
        {
            yield return null;
            
            // Kayıtlı seviyeyi kontrol et, sınırı aşmışsa düzelt
            if (CurrentLevelIndex >= _weaponLevels.Count) 
                CurrentLevelIndex = _weaponLevels.Count - 1;

            // Silahı yükle ve fiyatı ayarla
            UpdateTowerWeapon();
            InitializeUnlocker();
            
            Debug.Log($"🏰 Kule Hazır! Seviye: {CurrentLevelIndex + 1}");
        }

        private void InitializeUnlocker()
        {
            if (_upgradeZone == null) return;

            // Eğer daha yükselecek seviye varsa fiyatı Unlocker'a bildir
            if (CurrentLevelIndex < _upgradeCosts.Count)
            {
                _upgradeZone.SetRequiredResource(_upgradeCosts[CurrentLevelIndex]);
            }
            else
            {
                // Zaten son seviyedeyiz, kutuyu kapat
                _upgradeZone.gameObject.SetActive(false);
            }
        }

        // --- UNLOCKER BU FONKSİYONU ÇAĞIRIR ---
        public void OnUpgradePaid()
        {
            // 1. Seviyeyi Artır (Kaydedilir)
            CurrentLevelIndex++;

            // 2. Silahı Güçlendir
            UpdateTowerWeapon();

            // 3. Sıradaki Fiyatı Belirle veya Kapat
            if (CurrentLevelIndex < _upgradeCosts.Count)
            {
                int nextCost = _upgradeCosts[CurrentLevelIndex];
                _upgradeZone.SetRequiredResource(nextCost);
                Debug.Log($"✅ Kule Yükseldi! Yeni Seviye: {CurrentLevelIndex + 1}. Sonraki Maliyet: {nextCost}");
            }
            else
            {
                Debug.Log("🔥 Kule MAKSİMUM Seviyeye Ulaştı!");
                // Unlocker hatasını önlemek için 1 kare sonra kapat
                StartCoroutine(DisableUpgradeZoneRoutine());
            }
        }

        private IEnumerator DisableUpgradeZoneRoutine()
        {
            yield return null; 
            if (_upgradeZone != null) _upgradeZone.gameObject.SetActive(false);
        }

        private void UpdateTowerWeapon()
        {
            if (CurrentLevelIndex < _weaponLevels.Count)
            {
                _attacker.SetWeapon(_weaponLevels[CurrentLevelIndex]);
            }
        }
        
        // Test Amaçlı Sıfırlama
        [ContextMenu("🔄 Reset Tower Level")]
        public void ResetTower()
        {
            CurrentLevelIndex = 0;
            UpdateTowerWeapon();
            InitializeUnlocker();
            if (_upgradeZone) _upgradeZone.gameObject.SetActive(true);
            Debug.Log("🔄 Kule Sıfırlandı.");
        }
    }
}