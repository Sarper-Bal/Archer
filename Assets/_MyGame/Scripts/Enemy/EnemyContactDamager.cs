using UnityEngine;
using IndianOceanAssets.Engine2_5D; // IDamageable ve EnemyStats için

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyContactDamager : MonoBehaviour
    {
        [Header("🎯 Hedef Ayarları")]
        [Tooltip("Bu birim kime çarparsa patlasın/hasar versin? (Çoklu seçim yapabilirsin)")]
        [SerializeField] private LayerMask _targetLayers;

        [Header("💥 Davranış")]
        [SerializeField] private bool _destroyOnHit = true; // Kamikaze modu

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
            // 1. KİLİT: Zaten vurduysam çık
            if (_hasHit) return;

            GameObject obj = collision.gameObject;

            // 2. [KRİTİK OPTİMİZASYON] Katman Kontrolü
            // Çarptığım objenin katmanı, hedef listemde var mı?
            // (Yoksa GetComponent yapmadan hemen çık, işlemciyi yorma)
            if (!IsInLayerMask(obj.layer, _targetLayers)) return;

            // 3. Hasar Verme
            if (obj.TryGetComponent(out IDamageable damageable))
            {
                DealDamage(damageable);
            }
            else
            {
                // Belki canı olan parça parent'tadır
                var parentDamageable = obj.GetComponentInParent<IDamageable>();
                if (parentDamageable != null)
                {
                    DealDamage(parentDamageable);
                }
            }
        }

        // Katman kontrolü yapan yardımcı matematiksel fonksiyon
        private bool IsInLayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        private void DealDamage(IDamageable target)
        {
            if (_stats.Definition == null) return;

            _hasHit = true; // Kilidi kapat

            float damageAmount = _stats.Definition.ContactDamage;
            target.TakeDamage(damageAmount);

            // Kendini yok et (Kamikaze)
            if (_destroyOnHit) SelfDestruct();
        }

        private void SelfDestruct()
        {
            // Ölüm Efekti
            if (_stats.Definition != null && _stats.Definition.DeathEffectPool != null)
            {
                var effect = _stats.Definition.DeathEffectPool.Get();
                effect.transform.position = transform.position + Vector3.up; 
                effect.Initialize(_stats.Definition.DeathEffectPool);
            }

            // Objeyi kapat
            gameObject.SetActive(false);
        }
    }
}