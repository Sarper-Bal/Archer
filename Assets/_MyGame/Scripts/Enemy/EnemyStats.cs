using UnityEngine;
using IndianOceanAssets.Engine2_5D.Data;

namespace IndianOceanAssets.Engine2_5D
{
    public class EnemyStats : MonoBehaviour
    {
        [field: SerializeField] public EnemyDefinition Definition { get; private set; }

        // [YENİ] Ganimet bayrağı
        public bool LootClaimed { get; set; } = false;

        public void InitializeRuntime(EnemyDefinition data)
        {
            Definition = data;
            
            // [YENİ] Düşman her doğduğunda bayrağı indir (Sıfırla)
            LootClaimed = false; 
            
            // Diğer stat işlemleri...
            // (Mevcut kodların burada devam ediyor olabilir)
        }
    }
}