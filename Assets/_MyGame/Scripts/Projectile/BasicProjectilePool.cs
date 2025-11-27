using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools; // Projenin havuz kütüphanesi

namespace IndianOceanAssets.Engine2_5D
{
    // Unity Editöründe sağ tıkla -> Create -> Arcade Idle -> Pools yolunu açar
    [CreateAssetMenu(fileName = "BasicProjectilePool", menuName = "Arcade Idle/Pools/Basic Projectile Pool")]
    public class BasicProjectilePool : ObjectPool<BasicProjectile>
    {
        // ObjectPool<T> soyut olduğu için bu boş sınıfı türeterek
        // Unity'nin onu tanımasını ve Inspector'da görünmesini sağlıyoruz.
    }
}