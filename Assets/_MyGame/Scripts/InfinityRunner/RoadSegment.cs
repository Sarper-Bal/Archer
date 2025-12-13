using UnityEngine;
using System.Collections.Generic;

namespace IndianOceanAssets.Engine2_5D.World
{
    public class RoadSegment : MonoBehaviour
    {
        [Tooltip("Bu yol parçasının bittiği nokta.")]
        [SerializeField] private Transform _connectPoint;

        [Header("🔴 Engel Yönetimi")]
        [Tooltip("Yol üzerindeki engellerin listesi. Oyun başlayınca otomatik dolar.")]
        [SerializeField] private List<SimpleObstacle> _obstacles = new List<SimpleObstacle>();

        private void Awake()
        {
            // OYUN BAŞLARKEN OTOMATİK TARAMA
            // Eğer listeyi elle doldurmadıysan, kod kendisi bulur.
            if (_obstacles.Count == 0)
            {
                // Kendisinin ve altındaki tüm objelerin içindeki SimpleObstacle scriptlerini bulur
                GetComponentsInChildren(true, _obstacles);
            }
        }

        public Vector3 GetEndPosition()
        {
            if (_connectPoint == null)
                return transform.position + Vector3.forward * 20f;
            return _connectPoint.position;
        }

        /// <summary>
        /// Yol en başa taşındığında çağrılır. Tüm engelleri sıfırlar.
        /// </summary>
        public void ResetObstacles()
        {
            // Şimdilik test için HEPSİNİ açıyoruz.
            // İleride buraya "Rastgele %50'sini aç" gibi mantıklar ekleyeceğiz.
            for (int i = 0; i < _obstacles.Count; i++)
            {
                if (_obstacles[i] != null)
                {
                    _obstacles[i].gameObject.SetActive(true);
                    // İleride: _obstacles[i].ResetHealth();
                }
            }
        }

        // Editörde kolaylık sağlamak için sağ tık menüsü
        [ContextMenu("Engelleri Bul (Editör)")]
        private void FindObstaclesInEditor()
        {
            _obstacles.Clear();
            GetComponentsInChildren(true, _obstacles);
            Debug.Log($"✅ {_obstacles.Count} adet engel bulundu ve listeye eklendi!");
        }

        private void OnDrawGizmos()
        {
            if (_connectPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_connectPoint.position, 0.5f);
            }
        }
    }
}