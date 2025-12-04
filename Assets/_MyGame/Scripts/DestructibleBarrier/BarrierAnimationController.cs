using UnityEngine;
using DG.Tweening; 

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
    public enum BarrierAnimMode
    {
        None,           
        PunchScale,     
        ShakeRotation,  
        ElasticJelly    
    }

    [RequireComponent(typeof(DestructibleBarrier))]
    public class BarrierAnimationController : MonoBehaviour
    {
        [Header("⚙️ Animasyon Seçimi")]
        [SerializeField] private BarrierAnimMode _mode = BarrierAnimMode.PunchScale;

        [Header("🎯 Hedef")]
        [SerializeField] private Transform _visualModel;

        [Header("Ayarlar")]
        [SerializeField] private Vector3 _punchStrength = new Vector3(0.1f, 0.1f, 0.1f);
        [SerializeField] private float _punchDuration = 0.15f;
        [SerializeField] private int _punchVibrato = 10;
        
        [SerializeField] private Vector3 _shakeStrength = new Vector3(0f, 0f, 5f);
        [SerializeField] private float _shakeDuration = 0.2f;
        [SerializeField] private int _shakeVibrato = 10;

        [SerializeField] private Vector3 _jellyStrength = new Vector3(0.1f, -0.1f, 0.1f);
        [SerializeField] private float _jellyDuration = 0.4f;

        private DestructibleBarrier _barrier;
        private Tween _currentTween;
        
        private Vector3 _baseScale;
        private Quaternion _baseRotation;
        private bool _initialized = false;

        private void Awake()
        {
            _barrier = GetComponent<DestructibleBarrier>();
            // Awake anındaki boyutu "Kutsal Boyut" olarak kabul et
            InitializeBaseline();
        }

        private void OnEnable()
        {
            if (_barrier != null) _barrier.OnDamageTaken += PlayHitAnimation;
            ForceResetVisuals();
        }

        private void OnDisable()
        {
            if (_barrier != null) _barrier.OnDamageTaken -= PlayHitAnimation;
            
            if (_visualModel != null) _visualModel.DOKill(); 
            
            ForceResetVisuals();
        }

        private void InitializeBaseline()
        {
            if (_visualModel != null && !_initialized)
            {
                _baseScale = _visualModel.localScale;
                _baseRotation = _visualModel.localRotation;
                _initialized = true;
            }
        }

        private void PlayHitAnimation()
        {
            if (_visualModel == null || _mode == BarrierAnimMode.None) return;
            if (!_initialized) InitializeBaseline();

            _visualModel.DOKill(true); 
            _visualModel.localScale = _baseScale;
            _visualModel.localRotation = _baseRotation;

            switch (_mode)
            {
                case BarrierAnimMode.PunchScale:
                    _currentTween = _visualModel.DOPunchScale(_punchStrength, _punchDuration, _punchVibrato, 1f);
                    break;

                case BarrierAnimMode.ShakeRotation:
                    _currentTween = _visualModel.DOShakeRotation(_shakeDuration, _shakeStrength, _shakeVibrato, 90f);
                    break;

                case BarrierAnimMode.ElasticJelly:
                    _currentTween = _visualModel.DOPunchScale(_jellyStrength, _jellyDuration, 4, 0.5f)
                        .SetEase(Ease.OutElastic); 
                    break;
            }
        }

        private void ForceResetVisuals()
        {
            if (_visualModel != null)
            {
                _visualModel.DOKill();

                if (_initialized)
                {
                    // [DÜZELTME] Asla Vector3.one kullanma, kaydettiğin boyutu kullan
                    _visualModel.localScale = _baseScale;
                    _visualModel.localRotation = _baseRotation;
                }
            }
        }
    }
}