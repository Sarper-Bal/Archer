using UnityEngine;
using DG.Tweening; // DOTween Kütüphanesi

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
    public enum BarrierAnimMode
    {
        None,           // Animasyon Yok
        PunchScale,     // Büyü/Küçül (Tok Vuruş)
        ShakeRotation,  // Sağa Sola Salla (Deprem/Darbe)
        ElasticJelly    // Lastik Gibi Esne (Yumuşak)
    }

    [RequireComponent(typeof(DestructibleBarrier))]
    public class BarrierAnimationController : MonoBehaviour
    {
        [Header("⚙️ Animasyon Seçimi")]
        [SerializeField] private BarrierAnimMode _mode = BarrierAnimMode.PunchScale;

        [Header("🎯 Hedef")]
        [Tooltip("Sallanacak olan görsel 3D obje.")]
        [SerializeField] private Transform _visualModel;

        [Header("1. Punch Scale Ayarları")]
        [SerializeField] private Vector3 _punchStrength = new Vector3(0.1f, 0.1f, 0.1f);
        [SerializeField] private float _punchDuration = 0.15f;
        [SerializeField] private int _punchVibrato = 10;

        [Header("2. Shake Rotation Ayarları")]
        [SerializeField] private Vector3 _shakeStrength = new Vector3(0f, 0f, 5f);
        [SerializeField] private float _shakeDuration = 0.2f;
        [SerializeField] private int _shakeVibrato = 10;

        [Header("3. Elastic Jelly Ayarları")]
        [SerializeField] private Vector3 _jellyStrength = new Vector3(0.1f, -0.1f, 0.1f);
        [SerializeField] private float _jellyDuration = 0.4f;

        private DestructibleBarrier _barrier;
        private Tween _currentTween;
        
        // Orijinal değerleri saklamak için
        private Vector3 _baseScale;
        private Quaternion _baseRotation;
        private bool _initialized = false;

        private void Awake()
        {
            _barrier = GetComponent<DestructibleBarrier>();
        }

        private void Start()
        {
            // Start, objenin boyutu dışarıdan (Spawner vb.) ayarlandıktan sonra çalışır.
            // Bu yüzden orijinal boyutu yakalamak için en güvenli yerdir.
            InitializeBaseline();
        }

        private void OnEnable()
        {
            if (_barrier != null)
                _barrier.OnDamageTaken += PlayHitAnimation;
            
            // Havuzdan çıkarken veya tekrar açılırken görseli düzelt
            // Ama hemen değil, bu karenin sonunda (End of Frame) veya Start'ta düzeltmek daha güvenlidir.
            // Şimdilik sadece tween varsa öldürüyoruz.
            if (_initialized) ResetVisuals();
        }

        private void OnDisable()
        {
            if (_barrier != null)
                _barrier.OnDamageTaken -= PlayHitAnimation;
            
            // Kapanırken animasyonu durdur ve şekli düzelt
            if (_currentTween != null) _currentTween.Kill(true);
            
            // Kapanırken kesinlikle orijinal haline dönmeli
            ResetVisuals();
        }

        /// <summary>
        /// Objenin o anki duruşunu "Orijinal Hali" olarak kaydeder.
        /// </summary>
        private void InitializeBaseline()
        {
            if (_visualModel != null)
            {
                _baseScale = _visualModel.localScale;
                _baseRotation = _visualModel.localRotation;
                _initialized = true;
            }
        }

        private void PlayHitAnimation()
        {
            if (_visualModel == null || _mode == BarrierAnimMode.None) return;

            // Eğer henüz init olmadıysa (Start çalışmadan vurulduysa) şimdi yap
            if (!_initialized) InitializeBaseline();

            // Önceki animasyonu iptal et ve objeyi temiz haline getir
            if (_currentTween != null && _currentTween.IsActive())
            {
                _currentTween.Kill(true); 
            }
            
            // Her vuruşta, animasyona başlamadan önce boyutu "Base" değere çek.
            // Bu sayede animasyonlar üst üste binip objeyi devasa yapmaz veya küçültmez.
            _visualModel.localScale = _baseScale;
            _visualModel.localRotation = _baseRotation;

            // Animasyonu başlat
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

        private void ResetVisuals()
        {
            if (_visualModel != null && _initialized)
            {
                _visualModel.localScale = _baseScale;
                _visualModel.localRotation = _baseRotation;
            }
        }
        
        // Editörde ayar değiştirirken anlık görmek için
        private void OnValidate()
        {
            if (Application.isPlaying && _initialized) ResetVisuals();
        }
    }
}