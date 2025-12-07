using UnityEngine;
using DG.Tweening; 
using IndianOceanAssets.Engine2_5D;

namespace IndianOceanAssets.Engine2_5D.Visuals
{
    public enum VisualStyle 
    { 
        None, Custom, Jelly, Hard, Cartoon 
    }

    [RequireComponent(typeof(Health))]
    public class EnemyVisuals : MonoBehaviour
    {
        [Header("🎯 Hedef Model")]
        [SerializeField] private Transform _modelTransform; 

        [Header("🐣 Doğma Animasyonu (Spawn)")]
        [SerializeField] private VisualStyle _spawnStyle = VisualStyle.Jelly;
        [SerializeField] private float _spawnDuration = 0.6f;
        [SerializeField] private Ease _spawnEase = Ease.OutElastic;

        [Header("🤕 Vurulma Animasyonu (Hit)")]
        [SerializeField] private VisualStyle _hitStyle = VisualStyle.Jelly;
        [SerializeField] private Vector3 _punchScale = new Vector3(-0.3f, -0.3f, -0.3f);
        [SerializeField] private float _hitDuration = 0.4f;
        [SerializeField] private int _vibrato = 10; 
        [SerializeField] [Range(0,1)] private float _elasticity = 1f;

        private Health _health;
        private Vector3 _initialScale; // [YENİ] Başlangıç boyutunu saklamak için

        private void Awake()
        {
            _health = GetComponent<Health>();
            if (_modelTransform == null) _modelTransform = transform;

            // [YENİ] Prefab'in orijinal boyutunu hafızaya alıyoruz
            _initialScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (_health != null) _health.OnDamageTaken += PlayHitAnimation;
            PlaySpawnAnimation();
        }

        private void OnDisable()
        {
            if (_health != null) _health.OnDamageTaken -= PlayHitAnimation;
            
            _modelTransform.DOKill();
            
            // [DÜZELTME] 1,1,1 yerine orijinal boyuta dönüyoruz
            transform.localScale = _initialScale; 
        }

        private void OnValidate()
        {
            ApplySpawnPreset();
            ApplyHitPreset();
        }

        private void ApplySpawnPreset()
        {
            // (Burası aynı kalıyor, kod kalabalığı olmasın diye kısalttım)
            switch (_spawnStyle)
            {
                case VisualStyle.Jelly: _spawnDuration = 0.6f; _spawnEase = Ease.OutElastic; break;
                case VisualStyle.Hard: _spawnDuration = 0.3f; _spawnEase = Ease.OutBack; break;
                case VisualStyle.Cartoon: _spawnDuration = 0.7f; _spawnEase = Ease.OutBounce; break;
            }
        }

        private void ApplyHitPreset()
        {
            // (Burası aynı kalıyor)
            switch (_hitStyle)
            {
                case VisualStyle.Jelly: _punchScale = new Vector3(-0.3f, -0.3f, -0.3f); _hitDuration = 0.4f; _vibrato = 10; _elasticity = 1f; break;
                case VisualStyle.Hard: _punchScale = new Vector3(-0.15f, -0.15f, -0.15f); _hitDuration = 0.15f; _vibrato = 5; _elasticity = 0.5f; break;
                case VisualStyle.Cartoon: _punchScale = new Vector3(0.4f, -0.4f, 0.4f); _hitDuration = 0.3f; _vibrato = 8; _elasticity = 1f; break;
            }
        }

        [ContextMenu("Test Spawn")]
        private void PlaySpawnAnimation()
        {
            // [DÜZELTME] Vector3.one yerine _initialScale kullanıyoruz
            if (_spawnStyle == VisualStyle.None) 
            {
                transform.localScale = _initialScale;
                return;
            }

            transform.localScale = Vector3.zero;
            // DOTween hedefi artık senin belirlediğin boyut
            transform.DOScale(_initialScale, _spawnDuration).SetEase(_spawnEase);
        }

        [ContextMenu("Test Hit")]
        private void PlayHitAnimation()
        {
            if (_hitStyle == VisualStyle.None) return;

            _modelTransform.DOKill(true);
            _modelTransform.DOPunchScale(_punchScale, _hitDuration, _vibrato, _elasticity);
        }
    }
}