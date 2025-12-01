using UnityEngine;
using DG.Tweening; 
using IndianOceanAssets.Engine2_5D;

namespace IndianOceanAssets.Engine2_5D.Visuals
{
    public enum VisualStyle 
    { 
        None,   // 🚫 İptal (Animasyon Oynama)
        Custom, // 🔧 Özel (Senin girdiğin değerler)
        Jelly,  // 🍬 Yumuşak Jelibon
        Hard,   // 🛡️ Sert ve Tok
        Cartoon // 🤪 Çizgi Film
    }

    [RequireComponent(typeof(Health))]
    public class EnemyVisuals : MonoBehaviour
    {
        [Header("🎯 Hedef Model")]
        [Tooltip("Animasyonun uygulanacağı model. Boş bırakırsan otomatik bulur.")]
        [SerializeField] private Transform _modelTransform; 

        // -----------------------------------------------------------
        // 🐣 DOĞMA AYARLARI (SPAWN)
        // -----------------------------------------------------------
        [Header("🐣 Doğma Animasyonu (Spawn)")]
        [SerializeField] private VisualStyle _spawnStyle = VisualStyle.Jelly;
        
        [Tooltip("Büyüme süresi")]
        [SerializeField] private float _spawnDuration = 0.6f;
        [SerializeField] private Ease _spawnEase = Ease.OutElastic;

        // -----------------------------------------------------------
        // 🤕 VURULMA AYARLARI (HIT)
        // -----------------------------------------------------------
        [Header("🤕 Vurulma Animasyonu (Hit)")]
        [SerializeField] private VisualStyle _hitStyle = VisualStyle.Jelly;

        [Tooltip("Vurulma şiddeti (Eksi değer içe büzer)")]
        [SerializeField] private Vector3 _punchScale = new Vector3(-0.3f, -0.3f, -0.3f);
        [SerializeField] private float _hitDuration = 0.4f;
        [SerializeField] private int _vibrato = 10; 
        [SerializeField] [Range(0,1)] private float _elasticity = 1f;

        private Health _health;

        // --- UNITY METHODS ---

        private void Awake()
        {
            _health = GetComponent<Health>();
            // Model atanmadıysa kendini model say
            if (_modelTransform == null) _modelTransform = transform;
        }

        private void OnEnable()
        {
            if (_health != null) _health.OnDamageTaken += PlayHitAnimation;
            
            // Doğma animasyonunu başlat
            PlaySpawnAnimation();
        }

        private void OnDisable()
        {
            if (_health != null) _health.OnDamageTaken -= PlayHitAnimation;
            
            // Havuza girerken temizlik yap
            _modelTransform.DOKill();
            transform.localScale = Vector3.one; 
        }

        // Editörde değer değiştiğinde presetleri uygula
        private void OnValidate()
        {
            ApplySpawnPreset();
            ApplyHitPreset();
        }

        // --- PRESET SİSTEMİ (OTOMATİK AYARLAR) ---

        private void ApplySpawnPreset()
        {
            switch (_spawnStyle)
            {
                case VisualStyle.None:   break; // Kapalıysa dokunma
                case VisualStyle.Custom: break; // Özelse dokunma
                
                case VisualStyle.Jelly:
                    _spawnDuration = 0.6f;
                    _spawnEase = Ease.OutElastic;
                    break;
                case VisualStyle.Hard:
                    _spawnDuration = 0.3f;
                    _spawnEase = Ease.OutBack;
                    break;
                case VisualStyle.Cartoon:
                    _spawnDuration = 0.7f;
                    _spawnEase = Ease.OutBounce;
                    break;
            }
        }

        private void ApplyHitPreset()
        {
            switch (_hitStyle)
            {
                case VisualStyle.None:   break;
                case VisualStyle.Custom: break;

                case VisualStyle.Jelly:
                    _punchScale = new Vector3(-0.3f, -0.3f, -0.3f);
                    _hitDuration = 0.4f;
                    _vibrato = 10;
                    _elasticity = 1f;
                    break;
                case VisualStyle.Hard:
                    _punchScale = new Vector3(-0.15f, -0.15f, -0.15f);
                    _hitDuration = 0.15f;
                    _vibrato = 5;
                    _elasticity = 0.5f;
                    break;
                case VisualStyle.Cartoon:
                    _punchScale = new Vector3(0.4f, -0.4f, 0.4f);
                    _hitDuration = 0.3f;
                    _vibrato = 8;
                    _elasticity = 1f;
                    break;
            }
        }

        // --- ANİMASYON OYNATICILAR ---

        [ContextMenu("Test Spawn")]
        private void PlaySpawnAnimation()
        {
            // Eğer "None" seçiliyse animasyon yapma, normal boyutta başlat.
            if (_spawnStyle == VisualStyle.None) 
            {
                transform.localScale = Vector3.one;
                return;
            }

            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, _spawnDuration).SetEase(_spawnEase);
        }

        [ContextMenu("Test Hit")]
        private void PlayHitAnimation()
        {
            // "None" seçiliyse tepki verme.
            if (_hitStyle == VisualStyle.None) return;

            _modelTransform.DOKill(true); // Önceki titremeyi bitir
            _modelTransform.DOPunchScale(_punchScale, _hitDuration, _vibrato, _elasticity);
        }
    }
}