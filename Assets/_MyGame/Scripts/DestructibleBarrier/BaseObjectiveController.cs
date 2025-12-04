using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Interactables; // DestructibleBarrier için
using IndianOceanAssets.Engine2_5D.Managers;      // SmartWaveManager için

namespace IndianOceanAssets.Engine2_5D
{
    [RequireComponent(typeof(DestructibleBarrier))]
    public class BaseObjectiveController : MonoBehaviour
    {
        private DestructibleBarrier _myBarrier;
        private SmartWaveManager _waveManager;

        private void Awake()
        {
            _myBarrier = GetComponent<DestructibleBarrier>();
            _waveManager = FindObjectOfType<SmartWaveManager>();
        }

        private void OnEnable()
        {
            if (_myBarrier != null)
                _myBarrier.OnDeath += HandleBaseDestruction;
        }

        private void OnDisable()
        {
            if (_myBarrier != null)
                _myBarrier.OnDeath -= HandleBaseDestruction;
        }

        private void HandleBaseDestruction()
        {
            Debug.Log("🚨 ANA ÜS YIKILDI! Kaybetme prosedürü başlatılıyor...");

            if (_waveManager != null)
            {
                // Manager'a "Bitti bu iş, resetle" komutunu ver
                _waveManager.TriggerWaveFailure();
            }
            else
            {
                Debug.LogError("⚠️ SmartWaveManager sahnede bulunamadı!");
            }
        }
    }
}