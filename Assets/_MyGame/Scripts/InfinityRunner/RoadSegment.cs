using UnityEngine;

namespace IndianOceanAssets.Engine2_5D.World
{
    public class RoadSegment : MonoBehaviour
    {
        [Tooltip("Bu yol parçasının bittiği nokta. Bir sonraki yol buraya yapışacak.")]
        [SerializeField] private Transform _connectPoint;

        /// <summary>
        /// Bu yolun ucunun dünya üzerindeki (World Space) pozisyonunu verir.
        /// </summary>
        public Vector3 GetEndPosition()
        {
            if (_connectPoint == null)
            {
                // Güvenlik önlemi: Eğer nokta atanmamışsa kendi uzunluğunu varsay (Örn: 20m)
                return transform.position + Vector3.forward * 20f;
            }
            return _connectPoint.position;
        }

        // Editörde ConnectPoint'i görmeyi kolaylaştıran gizmo
        private void OnDrawGizmos()
        {
            if (_connectPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_connectPoint.position, 0.5f);
                Gizmos.DrawLine(transform.position, _connectPoint.position);
            }
        }
    }
}