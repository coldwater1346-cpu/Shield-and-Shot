namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public interface ISpecialShield
    {
        float CurrentGauge { get; }
        float MaxGauge { get; }
        bool IsSpecialAbilityActive { get; }

        void ChargeGauge(float amount);
        void ActivateSpecialAbility();
        void DeactivateSpecialAbility();
    }
}