using UnityEngine;

namespace IndianOceanAssets.Engine2_5D
{
    public class BasicShooter : MonoBehaviour
    {
        [Header("Mermi Ayarları")]
        [SerializeField] private GameObject projectilePrefab; // BasicProjectile scripti olan prefab
        [SerializeField] private Transform firePoint; // Merminin çıkış noktası
        
        [Header("Saldırı Ayarları")]
        [SerializeField] private float fireRate = 1f; // Saniyede kaç atış
        [SerializeField] private float range = 10f;   // Menzil
        [SerializeField] private LayerMask enemyLayer; // Düşman katmanı

        private float nextFireTime = 0f;

        private void Update()
        {
            if (Time.time >= nextFireTime)
            {
                Transform target = FindClosestEnemy();
                if (target != null)
                {
                    Shoot(target);
                    nextFireTime = Time.time + (1f / fireRate);
                }
            }
        }

        private void Shoot(Transform target)
        {
            if (projectilePrefab == null) return;

            // Mermiyi Yarat (Instantiate)
            GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            
            // Mermiyi Başlat
            BasicProjectile projectile = projObj.GetComponent<BasicProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(target);
            }
        }

        // En yakın düşmanı bulan basit fonksiyon
        private Transform FindClosestEnemy()
        {
            // Physics.OverlapSphere en optimize yöntemlerden biridir
            Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);
            
            Transform bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            foreach (Collider hit in hits)
            {
                // Sadece "Enemy" tag'i olanlara bak
                if (hit.CompareTag("Enemy"))
                {
                    Vector3 directionToTarget = hit.transform.position - currentPos;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        bestTarget = hit.transform;
                    }
                }
            }
            return bestTarget;
        }

        // Menzili Editörde görmek için Gizmo
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}