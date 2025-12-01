using UnityEngine;

namespace IndianOceanAssets.Engine2_5D
{
    public class EnemyStats : MonoBehaviour
    {
        [Header("Düşman Verisi")]
        [SerializeField] private EnemyDefinition _enemyDefinition;

        private Health _health;

        // --- KRİTİK EKLEME: AI'nın veriye ulaşması için bu satır ŞART ---
        public EnemyDefinition Definition => _enemyDefinition; 

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (_enemyDefinition != null && _health != null)
            {
                _health.InitializeHealth(_enemyDefinition.MaxHealth, _enemyDefinition.DeathEffectPool);
            }
        }
        public void InitializeRuntime(EnemyDefinition newData)
    {
        _enemyDefinition = newData; // Veriyi değiştir
        
        if (_health != null)
        {
            // Canı yeni veriye göre fulle
            _health.InitializeHealth(newData.MaxHealth, newData.DeathEffectPool);
        }
    }
        
    }
}