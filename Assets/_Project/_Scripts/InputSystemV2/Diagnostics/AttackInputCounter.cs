using Shield_Shot.InputSystemV2.Combat.Application;
using Shield_Shot.InputSystemV2.Combat.Domain;

namespace Shield_Shot.InputSystemV2.Combat.Diagnostics
{
    public sealed class AttackInputCounter
        : IAttackInputSink
    {
        public long TotalCount { get; private set; }

        public long BeganCount { get; private set; }
        public long AimChangedCount { get; private set; }
        public long ChargeChangedCount { get; private set; }
        public long ReleasedCount { get; private set; }
        public long CanceledCount { get; private set; }

        public long AimingCount { get; private set; }
        public long ChargingCount { get; private set; }
        public long FullyChargedCount { get; private set; }

        public float MaximumChargeRatio { get; private set; }
        public float LatestChargeRatio { get; private set; }
        public float LastReleasedChargeRatio { get; private set; }

        public bool HasLastSample { get; private set; }

        public AttackInputSample LastSample
        {
            get;
            private set;
        }

        public void Receive(
            in AttackInputSample sample)
        {
            TotalCount++;

            switch (sample.Phase)
            {
                case AttackInputPhase.Began:
                    BeganCount++;
                    break;

                case AttackInputPhase.AimChanged:
                    AimChangedCount++;
                    break;

                case AttackInputPhase.ChargeChanged:
                    ChargeChangedCount++;
                    break;

                case AttackInputPhase.Released:
                    ReleasedCount++;
                    break;

                case AttackInputPhase.Canceled:
                    CanceledCount++;
                    break;
            }

            switch (sample.ChargeState)
            {
                case AttackChargeState.Aiming:
                    AimingCount++;
                    break;

                case AttackChargeState.Charging:
                    ChargingCount++;
                    break;

                case AttackChargeState.FullyCharged:
                    FullyChargedCount++;
                    break;
            }

            if (sample.ChargeRatio >
                MaximumChargeRatio)
            {
                MaximumChargeRatio =
                    sample.ChargeRatio;
            }

            LatestChargeRatio =
                sample.ChargeRatio;

            if (sample.Phase ==
                AttackInputPhase.Released)
            {
                LastReleasedChargeRatio =
                    sample.ChargeRatio;
            }

            LastSample = sample;
            HasLastSample = true;
        }

        public void Reset()
        {
            TotalCount = 0;

            BeganCount = 0;
            AimChangedCount = 0;
            ChargeChangedCount = 0;
            ReleasedCount = 0;
            CanceledCount = 0;

            AimingCount = 0;
            ChargingCount = 0;
            FullyChargedCount = 0;

            MaximumChargeRatio = 0f;
            LatestChargeRatio = 0f;
            LastReleasedChargeRatio = 0f;

            LastSample = default;
            HasLastSample = false;
        }
    }
}
