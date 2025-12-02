using UnityEngine;
using System.Collections.Generic;

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    /// <summary>
    /// Kulenin görsel evrimini yöneten kontrolcüdür.
    /// Seviyeye göre ilgili Child objeyi açar/kapatır ve Attacker'a yeni referansları (namlu ucu vb.) bildirir.
    /// </summary>
    public class TowerVisualController : MonoBehaviour
    {
        [Header("🎨 Görsel Ayarlar")]
        [Tooltip("Her seviye için hazırlanmış kule görselleri listesi. Sıralama: Level 1, Level 2...")]
        [SerializeField] private List<TowerVisual> _levelVisuals;

        /// <summary>
        /// Belirtilen seviye indeksine göre kule modelini günceller.
        /// </summary>
        /// <param name="levelIndex">Aktif olacak seviye indeksi (0 tabanlı).</param>
        /// <param name="attacker">Referansların (FirePoint) atanacağı saldırı scripti.</param>
        public void UpdateVisuals(int levelIndex, TowerAttacker attacker)
        {
            // Liste boşsa hata vermemesi için kontrol
            if (_levelVisuals == null || _levelVisuals.Count == 0) return;

            // Eğer seviye sayısı model sayısını aşarsa, son modeli kullan (Clamp)
            int visualIndex = Mathf.Clamp(levelIndex, 0, _levelVisuals.Count - 1);

            for (int i = 0; i < _levelVisuals.Count; i++)
            {
                if (_levelVisuals[i] == null) continue;

                bool isActive = (i == visualIndex);
                
                // İlgili modeli aç, diğerlerini kapat
                _levelVisuals[i].gameObject.SetActive(isActive);

                // Eğer bu model aktif edildiyse, Attacker'a yeni namlu ve kafa bilgilerini gönder
                if (isActive)
                {
                    attacker.UpdateVisualReferences(
                        _levelVisuals[i].FirePoint,
                        _levelVisuals[i].RotatingPart
                    );
                }
            }
        }
    }
}