using System.Collections.Generic;
using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Enemy; // WaypointRoute ve RouteID için

namespace ArcadeBridge.ArcadeIdleEngine.Managers
{
    public class RouteManager : MonoBehaviour
    {
        // Singleton Deseni (En basit ve hızlı erişim için)
        public static RouteManager Instance { get; private set; }

        // Rehberimiz: ID -> Yol
        private Dictionary<RouteID, WaypointRoute> _routeMap = new Dictionary<RouteID, WaypointRoute>();

        private void Awake()
        {
            // Singleton Ayarı
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Yollar (Route) kendilerini buraya kaydettirir.
        /// </summary>
        public void RegisterRoute(RouteID id, WaypointRoute route)
        {
            if (id == null) return;

            if (!_routeMap.ContainsKey(id))
            {
                _routeMap.Add(id, route);
            }
            else
            {
                Debug.LogWarning($"[RouteManager] '{id.name}' ID'si ile birden fazla yol var! İkincisi yok sayıldı.");
            }
        }

        /// <summary>
        /// Düşmanlar ID verip yolu buradan alır.
        /// </summary>
        public WaypointRoute GetRoute(RouteID id)
        {
            if (id == null) return null;

            if (_routeMap.TryGetValue(id, out WaypointRoute route))
            {
                return route;
            }
            
            Debug.LogWarning($"[RouteManager] '{id.name}' ID'li bir yol sahnede bulunamadı!");
            return null;
        }
    }
}