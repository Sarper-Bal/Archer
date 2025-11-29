using UnityEngine;
using System.Collections.Generic;

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    public class WaypointRoute : MonoBehaviour
    {
        [Header("Yol Noktaları")]
        [Tooltip("Düşmanın sırayla gideceği noktalar.")]
        [SerializeField] private List<Transform> _waypoints = new List<Transform>();

        [Header("Görselleştirme")]
        [SerializeField] private Color _pathColor = Color.yellow;
        [SerializeField] private float _sphereRadius = 0.3f;

        // Dışarıdan erişim için Property
        public List<Transform> Waypoints => _waypoints;

        // Editörde kolayca nokta eklemek için bağlam menüsü
        [ContextMenu("Find Child Points")]
        private void FindChildPoints()
        {
            _waypoints.Clear();
            foreach (Transform child in transform)
            {
                _waypoints.Add(child);
            }
        }

        // Editörde yolu görebilmemiz için çizim fonksiyonu
        private void OnDrawGizmos()
        {
            if (_waypoints == null || _waypoints.Count < 2) return;

            Gizmos.color = _pathColor;

            for (int i = 0; i < _waypoints.Count; i++)
            {
                if (_waypoints[i] == null) continue;

                // Noktayı çiz
                Gizmos.DrawSphere(_waypoints[i].position, _sphereRadius);

                // Çizgiyi çiz (Bir sonraki noktaya)
                if (i < _waypoints.Count - 1 && _waypoints[i+1] != null)
                {
                    Gizmos.DrawLine(_waypoints[i].position, _waypoints[i + 1].position);
                }
            }
        }
    }
}