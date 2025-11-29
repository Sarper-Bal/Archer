using UnityEngine;
using System.Collections.Generic;
using ArcadeBridge.ArcadeIdleEngine.Managers;

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    public class WaypointRoute : MonoBehaviour
    {
        [Header("Kimlik (Zorunlu)")]
        [Tooltip("Data üzerinden erişim için bu ID şarttır.")]
        [SerializeField] private RouteID _routeID;

        [Header("Yol Noktaları")]
        [SerializeField] private List<Transform> _waypoints = new List<Transform>();
        
        [Header("Görselleştirme")]
        [SerializeField] private Color _pathColor = Color.yellow;
        [SerializeField] private float _sphereRadius = 0.3f;

        // [DÜZELTME] Kayıt durumunu takip eden değişken
        private bool _isRegistered = false;

        public List<Transform> Waypoints => _waypoints;

        private void OnEnable()
        {
            // Obje her açıldığında sıfırla ve kaydetmeyi dene
            _isRegistered = false; 
            TryRegister();
        }

        private void Start()
        {
            // Eğer OnEnable'da yönetici hazır değildiyse ve kayıt olamadıysak, şimdi tekrar dene.
            if (!_isRegistered)
            {
                TryRegister();
            }
        }

        private void TryRegister()
        {
            // Eğer zaten kayıtlıysam veya ID yoksa dur.
            if (_isRegistered || _routeID == null) return;

            if (RouteManager.Instance != null)
            {
                RouteManager.Instance.RegisterRoute(_routeID, this);
                _isRegistered = true; // [ÖNEMLİ] Artık kayıtlıyım, tekrar deneme.
            }
        }
        
        private void OnDisable()
        {
            // İsteğe bağlı: Obje kapanırsa kaydı silebiliriz ama
            // Manager şimdilik buna destek vermiyor, o yüzden sadece flag'i sıfırlıyoruz.
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