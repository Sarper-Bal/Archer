using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools; // Mevcut havuz sistemin
using IndianOceanAssets.Engine2_5D; // EnemyBehaviorController için

namespace ArcadeBridge.ArcadeIdleEngine.Spawners
{
    // Unity editöründe sağ tıkla oluşturabilmen için menü yolu
    [CreateAssetMenu(fileName = "NewEnemyPool", menuName = "Arcade Idle/Pools/Enemy Pool")]
    public class EnemyPool : ObjectPool<EnemyBehaviorController>
    {
        // ObjectPool sınıfı zaten "Get", "Return" gibi fonksiyonlara sahip.
        // T (Tip) olarak 'EnemyBehaviorController' verdik çünkü düşmanın kök (root) scripti bu.
        // Bu sayede havuzdan çektiğimizde direkt elimize kontrolcüsü gelecek.
    }
}