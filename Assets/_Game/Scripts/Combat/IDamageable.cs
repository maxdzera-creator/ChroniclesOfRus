namespace ChroniclesOfRus.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        bool CanReceiveDamage { get; }
        void ReceiveDamage(DamageInfo damageInfo);
    }
}
