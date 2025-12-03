using UnityEngine;
using IndianOceanAssets.Engine2_5D; // IDamageable ve EnemyStats için

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyContactDamager : MonoBehaviour
    {
        [Header("Ayarlar")]
        [SerializeField] private bool _destroyOnHit = true; // Kamikaze modu (Vurunca kendini yok et)

        private EnemyStats _stats;
        private bool _hasHit = false; // Çifte hasar kilidi

        private void Awake()
        {
            _stats = GetComponent<EnemyStats>();
        }

        private void OnEnable()
        {
            _hasHit = false; 
        }

        private void OnCollisionEnter(Collision collision)
        {
            // 1. KİLİT: Bu karede zaten birine vurduysam dur.
            if (_hasHit) return;

            GameObject targetObj = collision.gameObject;

            // 2. DOST ATEŞİ KORUMASI: Çarptığım şey kendi arkadaşımsa (Enemy) dur.
            if (targetObj.CompareTag("Enemy")) return;

            // 3. HEDEF KONTROLÜ: Etikete DEĞİL, canı olup olmadığına bakıyoruz.
            // (Player, Barrier, Kutu... Hepsi çalışır)
            if (targetObj.TryGetComponent(out IDamageable damageable))
            {
                // Buldum! Hasar ver.
                DealDamage(damageable);
            }
            else
            {
                // Belki collider çocuğundadır, ana objeye bakalım.
                var parentDamageable = targetObj.GetComponentInParent<IDamageable>();
                if (parentDamageable != null)
                {
                    DealDamage(parentDamageable);
                }
            }
        }

        private void DealDamage(IDamageable target)
        {
            if (_stats.Definition == null) return;

            _hasHit = true; // Kilidi kapat, bu karede başkasına vurma.

            float damageAmount = _stats.Definition.ContactDamage;
            
            // Hasarı arayüz üzerinden ver
            target.TakeDamage(damageAmount);

            // Eğer ayarlıysa kendini yok et (Kamikaze)
            if (_destroyOnHit) SelfDestruct();
        }

        private void SelfDestruct()
        {
            // Ölüm Efekti Oynat (Varsa)
            if (_stats.Definition != null && _stats.Definition.DeathEffectPool != null)
            {
                var effect = _stats.Definition.DeathEffectPool.Get();
                effect.transform.position = transform.position + Vector3.up; // Efekt biraz yukarıda çıksın
                effect.Initialize(_stats.Definition.DeathEffectPool);
            }

            // Düşmanı havuza geri gönder
            gameObject.SetActive(false);
        }
    }
}