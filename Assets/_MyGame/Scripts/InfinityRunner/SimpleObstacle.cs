using UnityEngine;

namespace IndianOceanAssets.Engine2_5D.World
{
    public class SimpleObstacle : MonoBehaviour
    {
        [HideInInspector] public Vector3 OriginalLocalPosition;

        private void Awake()
        {
            OriginalLocalPosition = transform.localPosition;
        }
    }
}