using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening; 
using ArcadeBridge.ArcadeIdleEngine.Booting; // LoadingScreenTween için

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class SceneTravelManager : MonoBehaviour
    {
        public static SceneTravelManager Instance;

        [Header("📺 Görsel Ayarlar")]
        [Tooltip("Loading Screen Canvas (üzerinde LoadingScreenTween olan obje).")]
        [SerializeField] private LoadingScreenTween _loadingScreen; 
        
        [Tooltip("Animasyonun görülmesi için minimum bekleme süresi.")]
        [SerializeField] private float _minWaitDuration = 2.0f;

        [Header("🛠️ Test Ayarları")]
        [SerializeField] private string _testTargetSceneName;

        private bool _isTraveling = false;

        private void Awake()
        {
            // Singleton: Sahne değişse bile bu obje yok olmasın.
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Başlangıçta loading ekranını gizle
                if (_loadingScreen != null) 
                    _loadingScreen.gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Sağ tık testi için
        [ContextMenu("🚀 Test Travel (Inspector)")]
        public void TestTravel()
        {
            if (string.IsNullOrEmpty(_testTargetSceneName))
            {
                Debug.LogError("❌ Hata: Sahne adı boş! Inspector'dan doldur.");
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

            // 1. PERDEYİ KAPAT (Loading Ekranını Aç)
            if (_loadingScreen != null)
            {
                _loadingScreen.gameObject.SetActive(true);
            }
            
            // Animasyonun başlaması için kısa bir bekleme (Görsel glitch olmaması için)
            yield return new WaitForSeconds(0.5f);

            // 2. RAM TEMİZLİĞİ (Garbage Collection)
            // Yeni sahneye geçmeden önce eski sahnenin artıklarını temizle
            System.GC.Collect();
            yield return Resources.UnloadUnusedAssets();

            // 3. ASENKRON YÜKLEME (Donmadan Yükle)
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            
            // Otomatik geçişi durdur ki biz isteyince geçsin (Opsiyonel ama daha güvenli)
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                // Yükleme %90'a geldiğinde bitmiş sayılır
                if (operation.progress >= 0.9f)
                {
                    // Minimum bekleme süresi doldu mu? Dolduysa sahneyi aktif et.
                    yield return new WaitForSeconds(_minWaitDuration);
                    operation.allowSceneActivation = true;
                }
                yield return null;
            }

            // 4. PERDEYİ AÇ (Loading Ekranını Kapat)
            if (_loadingScreen != null)
            {
                _loadingScreen.gameObject.SetActive(false);
            }

            _isTraveling = false;
            Debug.Log($"✅ Sahne başarıyla yüklendi: {sceneName}");
        }
    }
}