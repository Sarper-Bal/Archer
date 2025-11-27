using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;

namespace IndianOceanAssets.Engine2_5D
{
    // Unity'de sağ tık menüsüne ekler
    [CreateAssetMenu(fileName = "DeathEffectPool", menuName = "Arcade Idle/Pools/Death Effect Pool")]
    public class DeathEffectPool : ObjectPool<ExplosionEffect>
    {
        // İçi boş kalabilir. 
        // ObjectPool<ExplosionEffect> sınıfından miras aldığı için
        // ExplosionEffect scriptindeki "Initialize" fonksiyonu artık bunu kabul edecektir.
    }
}