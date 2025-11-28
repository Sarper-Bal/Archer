using UnityEngine;

namespace IndianOceanAssets.Engine2_5D
{
    public class EnemyStats : MonoBehaviour
    {
        [Header("Düşman Verisi")]
        [SerializeField] private EnemyDefinition _enemyDefinition;

        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (_enemyDefinition != null && _health != null)
            {
                // Düşman doğduğunda (havuzdan çıkınca), verisindeki canı ve efekt havuzunu yükle
                _health.InitializeHealth(_enemyDefinition.MaxHealth, _enemyDefinition.DeathEffectPool);
            }
        }
    }
}