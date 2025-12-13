using UnityEngine;

namespace IndianOceanAssets.Engine2_5D.World
{
    public class SimpleObstacle : MonoBehaviour
    {
        [Header("🔀 Karıştırma Ayarı")]
        [Tooltip("Eğer bu kutu işaretliyse, yol yenilendiğinde bu küp, diğer işaretli küplerle yer değiştirir.")]
        public bool AllowShuffle = false;

        // Orijinal pozisyonu hafızada tutmak için (İsteğe bağlı, resetlerde kayma olmaması için)
        [HideInInspector] public Vector3 OriginalLocalPosition;

        private void Awake()
        {
            // Oyun başlarken nerede olduğunu kaydet
            OriginalLocalPosition = transform.localPosition;
        }
    }
}