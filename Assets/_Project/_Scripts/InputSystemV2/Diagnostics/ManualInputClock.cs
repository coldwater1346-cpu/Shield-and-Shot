using System;
using Shield_Shot.InputSystemV2.Application;

namespace Shield_Shot.InputSystemV2.Diagnostics
{
    public sealed class ManualInputClock
        : IInputClock
    {
        public double Now { get; private set; }

        public ManualInputClock(
            double initialTime = 0d)
        {
            ValidateTime(
                initialTime,
                nameof(initialTime));

            Now = initialTime;
        }

        public void SetTime(
            double timestamp)
        {
            ValidateTime(
                timestamp,
                nameof(timestamp));

            if (timestamp < Now)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(timestamp),
                    timestamp,
                    "Manual clock cannot move backwards.");
            }

            Now = timestamp;
        }

        public void Advance(
            double duration)
        {
            if (double.IsNaN(duration) ||
                double.IsInfinity(duration) ||
                duration < 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(duration));
            }

            Now += duration;
        }

        public void Reset(
            double timestamp = 0d)
        {
            ValidateTime(
                timestamp,
                nameof(timestamp));

            Now = timestamp;
        }

        private static void ValidateTime(
            double value,
            string parameterName)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value) ||
                value < 0d)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName);
            }
        }
    }
}