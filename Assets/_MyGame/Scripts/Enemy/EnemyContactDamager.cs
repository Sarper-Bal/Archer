using UnityEngine;
using IndianOceanAssets.Engine2_5D; // IDamageable ve EnemyStats için

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyContactDamager : MonoBehaviour
    {
        [Header("Ayarlar")]
        [SerializeField] private string _targetTag = "Player";
        [SerializeField] private bool _destroyOnHit = true; // Kamikaze mi? (Vurunca ölsün mü)

        private EnemyStats _stats;
        private bool _hasHit = false; // [KRİTİK] Çifte hasarı önleyen emniyet kilidi

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
        }

        private void OnEnable()
        {
            _hasHit = false; // Havuzdan çıkınca kilidi aç
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Eğer zaten vurduysam veya hedef o değilse işlem yapma
            if (_hasHit || !collision.gameObject.CompareTag(_targetTag)) return;

            // Kilidi hemen kapat (Bu karede başka çarpışma olursa reddet)
            _hasHit = true;

            TryDealDamage(collision.gameObject);
        }

        private void TryDealDamage(GameObject targetObj)
        {
            if (_stats.Definition == null) return;

            float damageAmount = _stats.Definition.ContactDamage;

            // Hedefin canı var mı?
            if (targetObj.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damageAmount);
                if (_destroyOnHit) SelfDestruct();
            }
            // Belki collider çocuğundadır, ebeveyni kontrol et
            else
            {
                var parentDamageable = targetObj.GetComponentInParent<IDamageable>();
                if (parentDamageable != null)
                {
                    parentDamageable.TakeDamage(damageAmount);
                    if (_destroyOnHit) SelfDestruct();
                }
            }
        }

        private void SelfDestruct()
        {
            // Ölüm efekti
            if (_stats.Definition.DeathEffectPool != null)
            {
                var effect = _stats.Definition.DeathEffectPool.Get();
                effect.transform.position = transform.position;
                effect.Initialize(_stats.Definition.DeathEffectPool);
            }

            // Düşmanı kapat
            gameObject.SetActive(false);
        }
    }
}