using UnityEngine;

namespace IndianOceanAssets.Engine2_5D
{
    public class BasicProjectile : MonoBehaviour
    {
        [Header("Ayarlar")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifeTime = 3f; // Hedefi bulamazsa kaç saniyede yok olsun
        
        [Header("Efektler")]
        [SerializeField] private GameObject explosionPrefab; // Patlama efekti

        private Transform target;
        private Vector3 lastTargetPos; // Hedef ölürse son konumuna gitmeye devam etsin

        private void Start()
        {
            // Hedef bulamazsa sonsuza kadar gitmesin, kendini yok etsin
            Destroy(gameObject, lifeTime);
        }

        // Shooter tarafından çağrılır
        public void Initialize(Transform targetTransform)
        {
            this.target = targetTransform;
            if (target != null) lastTargetPos = target.position;
        }

        private void Update()
        {
            // Eğer hedef hala varsa pozisyonunu güncelle, yoksa son bilinen konuma git
            if (target != null)
            {
                lastTargetPos = target.position;
            }

            // Hedefe doğru hareket
            Vector3 direction = (lastTargetPos - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // Hedefe bakması için (Opsiyonel)
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // Hedefe çok yaklaştıysa çarpışma kontrolü (Manuel Distance Check)
            if (Vector3.Distance(transform.position, lastTargetPos) < 0.5f)
            {
                HitTarget();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Eğer Collider ile çarpışırsa (Enemy tag'li)
            if (other.CompareTag("Enemy"))
            {
                HitTarget();
            }
        }

        private void HitTarget()
        {
            // --- HASAR KISMI KALDIRILDI ---
            // Buraya ileride kendi hasar sistemini veya sadece debug log ekleyebilirsin.
            // Debug.Log("Hedef vuruldu!");

            // Patlama efekti oluştur
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // Mermiyi yok et
            Destroy(gameObject);
        }
    }
}