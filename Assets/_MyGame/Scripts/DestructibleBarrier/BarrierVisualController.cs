using UnityEngine;
using System.Collections.Generic;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
    public class BarrierVisualController : MonoBehaviour
    {
        [Header("🎨 Görsel Listesi")]
        [Tooltip("Level 1, Level 2... modellerini sırasıyla buraya sürükle.")]
        [SerializeField] private List<GameObject> _levelModels;

        /// <summary>
        /// İstenen seviyenin modelini açar ve O MODELİ GERİ DÖNDÜRÜR.
        /// </summary>
        public GameObject UpdateVisuals(int levelIndex)
        {
            if (_levelModels == null || _levelModels.Count == 0) return null;

            int visualIndex = Mathf.Clamp(levelIndex, 0, _levelModels.Count - 1);
            GameObject activeModel = null;

            for (int i = 0; i < _levelModels.Count; i++)
            {
                if (_levelModels[i] == null) continue;

                bool shouldBeActive = (i == visualIndex);
                
                if (_levelModels[i].activeSelf != shouldBeActive)
                    _levelModels[i].SetActive(shouldBeActive);

                if (shouldBeActive) activeModel = _levelModels[i];
            }
            
            return activeModel; // [YENİ] Aktif olan objeyi paketle gönder
        }
    }
}