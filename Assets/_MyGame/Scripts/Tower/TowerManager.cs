using UnityEngine;
using System.Collections; // IEnumerator için gerekli
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D;
using ArcadeBridge.ArcadeIdleEngine.Interactables;

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    [RequireComponent(typeof(TowerAttacker))]
    public class TowerManager : MonoBehaviour
    {
        [Header("💰 Ekonomi Bağlantısı")]
        [Tooltip("Kulenin dibindeki para toplama alanı (Child Obje).")]
        [SerializeField] private Unlocker _upgradeZone;

        [Header("🚀 Kule Seviyeleri")]
        [Tooltip("Seviye 1'den başlayarak silah listesi.")]
        [SerializeField] private List<WeaponDefinition> _weaponLevels;

        [Tooltip("Her seviye için yükseltme maliyeti. (Örn: Lvl1->Lvl2 için 100 altın)")]
        [SerializeField] private List<int> _upgradeCosts;

        [Header("🎨 Görsel Modeller")]
        [Tooltip("Seviye değiştikçe açılacak modeller.")]
        [SerializeField] private List<GameObject> _levelModels;

        [Header("Durum (Debug)")]
        [SerializeField] private int _currentLevelIndex = 0;

        private TowerAttacker _attacker;

        private void Awake()
        {
            _attacker = GetComponent<TowerAttacker>();
        }

        private void Start()
        {
            UpdateTowerState();
            InitializeUnlocker();
        }

        private void InitializeUnlocker()
        {
            if (_upgradeZone == null) return;

            if (_currentLevelIndex < _upgradeCosts.Count)
            {
                int cost = _upgradeCosts[_currentLevelIndex];
                _upgradeZone.SetRequiredResource(cost);
            }
            else
            {
                _upgradeZone.gameObject.SetActive(false);
            }
        }

        // --- GÜNCELLENEN KISIM ---
        public void OnUpgradePaid()
        {
            _currentLevelIndex++;
            UpdateTowerState();

            if (_currentLevelIndex < _upgradeCosts.Count)
            {
                // Bir sonraki seviye varsa fiyatı güncelle
                int nextCost = _upgradeCosts[_currentLevelIndex];
                _upgradeZone.SetRequiredResource(nextCost);
                Debug.Log($"✅ Kule Lvl {_currentLevelIndex + 1} oldu! Sıradaki Maliyet: {nextCost}");
            }
            else
            {
                // [DÜZELTME] Maksimum seviyeye ulaştık.
                // Unlocker'ı hemen kapatırsak "Coroutine Error" verir çünkü Unlocker hala kendi kodunu bitirmedi.
                // O yüzden "1 Frame Sonra Kapat" diyoruz.
                Debug.Log("🔥 Kule MAKSİMUM seviyeye ulaştı!");
                StartCoroutine(DisableUpgradeZoneRoutine());
            }
        }

        // Güvenli kapatma için küçük bir zamanlayıcı
        private IEnumerator DisableUpgradeZoneRoutine()
        {
            // Bu karenin bitmesini bekle (Unlocker işini bitirsin)
            yield return null; 
            
            // Şimdi güvenle kapatabiliriz
            if (_upgradeZone != null)
            {
                _upgradeZone.gameObject.SetActive(false);
            }
        }

        private void UpdateTowerState()
        {
            if (_currentLevelIndex < _weaponLevels.Count)
            {
                _attacker.SetWeapon(_weaponLevels[_currentLevelIndex]);
            }

            if (_levelModels != null && _levelModels.Count > 0)
            {
                for (int i = 0; i < _levelModels.Count; i++)
                {
                    if (_levelModels[i]) 
                        _levelModels[i].SetActive(i == _currentLevelIndex);
                }
            }
        }
    }
}