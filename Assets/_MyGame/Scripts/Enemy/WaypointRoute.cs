using UnityEngine;
using System.Collections.Generic;
// [ÖNEMLİ] RouteManager ve RouteID muhtemelen bu namespace'lerin içinde.
// Eğer RouteID scriptine bakarsan namespace'i neyse onu buraya eklemelisin.
using IndianOceanAssets.Engine2_5D; 
using ArcadeBridge.ArcadeIdleEngine.Managers; 

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    public class WaypointRoute : MonoBehaviour
    {
        [Header("Identity (Required) / Kimlik (Zorunlu)")]
        [Tooltip("This ID is required for access via Data. / Data üzerinden erişim için bu ID şarttır.")]
        [SerializeField] private RouteID _routeID; // Bu tipin tanınması için yukarıdaki 'using' gereklidir.

        [Header("Waypoints / Yol Noktaları")]
        [SerializeField] private List<Transform> _waypoints = new List<Transform>();
        
        [Header("Visualization / Görselleştirme")]
        [SerializeField] private Color _pathColor = Color.yellow;
        [SerializeField] private float _sphereRadius = 0.3f;

        private bool _isRegistered = false;

        public List<Transform> Waypoints => _waypoints;

        private void OnEnable()
        {
            _isRegistered = false; 
            TryRegister();
        }

        private void Start()
        {
            if (!_isRegistered)
            {
                TryRegister();
            }
        }

        private void TryRegister()
        {
            if (_isRegistered || _routeID == null) return;

            // RouteManager'a güvenli erişim
            if (RouteManager.Instance != null)
            {
                RouteManager.Instance.RegisterRoute(_routeID, this);
                _isRegistered = true;
            }
        }
        
        private void OnDisable()
        {
            _isRegistered = false;
        }

        [ContextMenu("Find Child Points")]
        private void FindChildPoints()
        {
            _waypoints.Clear();
            foreach (Transform child in transform)
            {
                _waypoints.Add(child);
            }
        }

        private void OnDrawGizmos()
        {
            if (_waypoints == null || _waypoints.Count < 2) return;
            Gizmos.color = _pathColor;
            for (int i = 0; i < _waypoints.Count; i++)
            {
                if (_waypoints[i] == null) continue;
                Gizmos.DrawSphere(_waypoints[i].position, _sphereRadius);
                if (i < _waypoints.Count - 1 && _waypoints[i+1] != null)
                    Gizmos.DrawLine(_waypoints[i].position, _waypoints[i + 1].position);
            }
        }
    }
}