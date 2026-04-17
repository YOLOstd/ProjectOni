namespace ProjectOni.Core
{
    /// <summary>
    /// Interface for any entity that can receive damage.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }
}
