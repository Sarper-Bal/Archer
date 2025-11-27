namespace IndianOceanAssets.Engine2_5D
{
    // Bu arayüzü uygulayan her nesne (Düşman, Oyuncu, Kutu) hasar alabilir demektir.
    public interface IDamageable
    {
        void TakeDamage(float amount);
        bool IsDead { get; }
    }
}