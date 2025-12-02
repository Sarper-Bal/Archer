using UnityEngine;
using System.Collections.Generic;

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    /// <summary>
    /// Kulenin seviyesine göre görsel modelini değiştiren script.
    /// TowerManager bu scripti tetikler.
    /// </summary>
    public class TowerVisualController : MonoBehaviour
    {
        [Header("🎨 Görsel Listesi")]
        [Tooltip("Sırasıyla Level 1, Level 2... modelleri buraya sürükleyin.")]
        [SerializeField] private List<TowerVisual> _levelVisuals;

        /// <summary>
        /// İlgili seviyenin modelini açar, diğerlerini kapatır.
        /// </summary>
        public void UpdateVisuals(int levelIndex, TowerAttacker attacker)
        {
            if (_levelVisuals == null || _levelVisuals.Count == 0) return;

            // Seviye sayısı model sayısını geçerse son modeli kullan
            int visualIndex = Mathf.Clamp(levelIndex, 0, _levelVisuals.Count - 1);

            for (int i = 0; i < _levelVisuals.Count; i++)
            {
                if (_levelVisuals[i] == null) continue;

                bool isActive = (i == visualIndex);
                
                // Sadece gerekli modeli aktif et
                if (_levelVisuals[i].gameObject.activeSelf != isActive)
                {
                    _levelVisuals[i].gameObject.SetActive(isActive);
                }

                // Aktif olan modelin ateş etme noktalarını Attacker scriptine gönder
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