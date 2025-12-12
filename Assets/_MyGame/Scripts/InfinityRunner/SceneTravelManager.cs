using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening; 

// Namespace satırlarını sildik. Artık script bağımlılığı yok.
// Sadece temel Unity ve DOTween kütüphaneleri yeterli.

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class SceneTravelManager : MonoBehaviour
    {
        public static SceneTravelManager Instance;

        [Header("📺 Görsel Ayarlar")]
        [Tooltip("Loading Screen Canvas'ının kendisini (GameObject olarak) buraya sürükle.")]
        [SerializeField] private GameObject _loadingScreenObject; // Script değil, düz GameObject istiyoruz.
        
        [Tooltip("Yükleme ekranında en az ne kadar beklesin?")]
        [SerializeField] private float _minWaitDuration = 2.0f;

        [Header("🛠️ Test Ayarları")]
        [SerializeField] private string _testTargetSceneName;

        private bool _isTraveling = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Başlangıçta loading objesini gizle
                if (_loadingScreenObject != null) 
                    _loadingScreenObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [ContextMenu("🚀 Test Travel (Inspector)")]
        public void TestTravel()
        {
            if (string.IsNullOrEmpty(_testTargetSceneName))
            {
                Debug.LogError("❌ Hata: Sahne adı boş!");
                return;
            }
            LoadScene(_testTargetSceneName);
        }

        public void LoadScene(string sceneName)
        {
            if (_isTraveling) return;
            StartCoroutine(ProcessSceneTransition(sceneName));
        }

        private IEnumerator ProcessSceneTransition(string sceneName)
        {
            _isTraveling = true;
            Debug.Log($"🔄 Sahne geçişi başlıyor: {sceneName}");

            // 1. PERDEYİ AÇ (Loading Ekranı)
            // Sadece objeyi açıyoruz. Üzerinde script varsa kendi kendine çalışır, bizi ilgilendirmez.
            if (_loadingScreenObject != null)
            {
                _loadingScreenObject.SetActive(true);
            }
            
            // Görselin ekrana gelmesi için 1 kare bekle
            yield return null; 

            // 2. DOTWEEN TEMİZLİĞİ (HATA ÇÖZÜMÜ)
            // Sahne değişmeden önce çalışan tüm animasyonları (düşmanlar, paralar vb.) öldür.
            // Bunu yapmazsak "Missing Target" hatası alırız.
            DOTween.KillAll();

            // 3. RAM TEMİZLİĞİ
            System.GC.Collect();
            yield return Resources.UnloadUnusedAssets();

            // 4. ASENKRON YÜKLEME
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            // Yükleme sırasında bekle
            while (!operation.isDone)
            {
                // Yükleme %90'a geldiğinde ve minimum süre dolduğunda
                if (operation.progress >= 0.9f)
                {
                    yield return new WaitForSeconds(_minWaitDuration);
                    
                    // Son kez temizlik yapıp geçişe izin ver
                    DOTween.KillAll(); 
                    operation.allowSceneActivation = true;
                }
                yield return null;
            }

            // 5. PERDEYİ KAPAT
            if (_loadingScreenObject != null)
            {
                _loadingScreenObject.SetActive(false);
            }

            _isTraveling = false;
            Debug.Log($"✅ Sahne başarıyla yüklendi: {sceneName}");
        }
    }
}