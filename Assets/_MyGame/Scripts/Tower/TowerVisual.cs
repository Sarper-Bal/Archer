using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    public class TowerVisual : MonoBehaviour
    {
        [Header("Bu Modele Ait Parçalar")]
        [Tooltip("Bu modelin ateş etme noktası (Namlunun ucu)")]
        public Transform FirePoint;

        [Tooltip("Bu modelin dönen parçası (Kafa). Yoksa boş bırak.")]
        public Transform RotatingPart;
    }
}