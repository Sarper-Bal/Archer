using UnityEngine;
using System.Collections.Generic;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
    /// <summary>
    /// Bariyer seviyesine göre ilgili 3D modeli açar/kapatır.
    /// </summary>
    public class BarrierVisualController : MonoBehaviour
    {
        [Header("🎨 Görsel Listesi")]
        [Tooltip("Level 1, Level 2... modellerini sırasıyla buraya sürükle.")]
        [SerializeField] private List<GameObject> _levelModels;

        /// <summary>
        /// İstenen seviyenin modelini açar, diğerlerini kapatır.
        /// </summary>
        public void UpdateVisuals(int levelIndex)
        {
            if (_levelModels == null || _levelModels.Count == 0) return;

            // Seviye, model sayısını aşarsa son modeli kullan (Clamp)
            int visualIndex = Mathf.Clamp(levelIndex, 0, _levelModels.Count - 1);

            for (int i = 0; i < _levelModels.Count; i++)
            {
                if (_levelModels[i] == null) continue;

                bool shouldBeActive = (i == visualIndex);
                
                if (_levelModels[i].activeSelf != shouldBeActive)
                {
                    _levelModels[i].SetActive(shouldBeActive);
                }
            }
            
            // Debug.Log($"🎨 Bariyer görseli güncellendi. Index: {visualIndex}");
        }
    }
}