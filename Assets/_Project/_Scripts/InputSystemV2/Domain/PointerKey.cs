using System;

namespace Shield_Shot.InputSystemV2.Domain
{
    public readonly struct PointerKey : IEquatable<PointerKey>
    {
        public PointerDeviceKind DeviceKind { get; }
        public int PointerId { get; }

        public PointerKey(
            PointerDeviceKind deviceKind,
            int pointerId)
        {
            if (deviceKind == PointerDeviceKind.Unknown)
            {
                throw new ArgumentException(
                    "Pointer device kind cannot be Unknown.",
                    nameof(deviceKind));
            }

            if (pointerId < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pointerId),
                    pointerId,
                    "Pointer ID cannot be negative.");
            }

            DeviceKind = deviceKind;
            PointerId = pointerId;
        }

        public static PointerKey From(in PointerSample sample)
        {
            return new PointerKey(
                sample.DeviceKind,
                sample.PointerId);
        }

        public bool Equals(PointerKey other)
        {
            return DeviceKind == other.DeviceKind &&
                   PointerId == other.PointerId;
        }

        public override bool Equals(object obj)
        {
            return obj is PointerKey other &&
                   Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)DeviceKind * 397) ^ PointerId;
            }
        }

        public static bool operator ==(
            PointerKey left,
            PointerKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            PointerKey left,
            PointerKey right)
        {
            return !left.Equals(right);
        }
    }
}