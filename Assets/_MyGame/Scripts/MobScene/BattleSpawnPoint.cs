using UnityEngine;

namespace IndianOceanAssets.Engine2_5D.Spawners
{
    /// <summary>
    /// [TR] Düşmanların doğacağı fiziksel noktayı temsil eder.
    /// [EN] Represents the physical point where enemies will spawn.
    /// </summary>
    public class BattleSpawnPoint : MonoBehaviour
    {
        [Tooltip("Bu noktanın kimliği (Örn: 'MainGate', 'LeftLane'). Spawner bu ID ile burayı bulur.")]
        public string PointID = "MainGate";

        private void OnDrawGizmos()
        {
            // Editörde görmek için kırmızı bir küre ve ok çiz
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            
            // Baktığı yönü göster (Düşmanlar bu yöne koşacak)
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}