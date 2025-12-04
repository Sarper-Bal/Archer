using UnityEngine;
using DG.Tweening; // DOTween Kütüphanesi

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
        [Tooltip("Sallanacak olan görsel 3D obje.")]
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
            // Awake'te hemen kaydet ki Start gecikirse veri kaybolmasın
            InitializeBaseline();
        }

        private void Start()
        {
            // Start'ta tekrar kontrol et (Spawner sonradan boyut değiştirmiş olabilir)
            if (_visualModel != null)
            {
                // Eğer Awake'te aldığımız scale 0 ise (Hata), şimdi tekrar al
                if (_baseScale.sqrMagnitude < 0.001f)
                {
                    _baseScale = _visualModel.localScale;
                    _baseRotation = _visualModel.localRotation;
                }
            }
        }

        private void OnEnable()
        {
            if (_barrier != null)
                _barrier.OnDamageTaken += PlayHitAnimation;
            
            // [KRİTİK DÜZELTME] Obje açılır açılmaz DOTween kalıntılarını temizle ve boyutu düzelt
            ForceResetVisuals();
        }

        private void OnDisable()
        {
            if (_barrier != null)
                _barrier.OnDamageTaken -= PlayHitAnimation;
            
            // [DOTWEEN HATASI ÇÖZÜMÜ] Kapanırken tween'i nazikçe değil, sertçe öldür.
            if (_visualModel != null)
            {
                _visualModel.DOKill(); // Bu objeye bağlı tüm tweenleri siler
            }
            
            ForceResetVisuals();
        }

        private void InitializeBaseline()
        {
            if (_visualModel != null && !_initialized)
            {
                _baseScale = _visualModel.localScale;
                _baseRotation = _visualModel.localRotation;

                // Eğer şans eseri 0 yakaladıysak, 1 olarak düzelt (Güvenlik)
                if (_baseScale.sqrMagnitude < 0.001f) _baseScale = Vector3.one;

                _initialized = true;
            }
        }

        private void PlayHitAnimation()
        {
            if (_visualModel == null || _mode == BarrierAnimMode.None) return;
            if (!_initialized) InitializeBaseline();

            // Önceki animasyonu öldür ve objeyi temizle
            _visualModel.DOKill(true); // true = Complete etmeden direkt öldür
            _visualModel.localScale = _baseScale;
            _visualModel.localRotation = _baseRotation;

            // Yeni animasyonu başlat
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
                // Tween kalıntısı varsa sil
                _visualModel.DOKill();

                // Scale 0 sorununu çözmek için orijinal boyuta zorla
                if (_initialized && _baseScale.sqrMagnitude > 0.001f)
                {
                    _visualModel.localScale = _baseScale;
                    _visualModel.localRotation = _baseRotation;
                }
                else
                {
                    // Eğer data yoksa en azından görünür yap (1,1,1)
                    _visualModel.localScale = Vector3.one;
                }
            }
        }
    }
}