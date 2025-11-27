using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools; // Projenin havuz kütüphanesi

namespace IndianOceanAssets.Engine2_5D
{
    // Unity menüsünde sağ tıklayıp oluşturmak için yol:
    [CreateAssetMenu(fileName = "ExplosionPool", menuName = "Arcade Idle/Pools/Explosion Pool")]
    public class ExplosionPool : ObjectPool<ExplosionEffect>
    {
        // ObjectPool sınıfından "ExplosionEffect" türü için türettik.
        // Artık Unity bu havuzu tanıyacak.
    }
}