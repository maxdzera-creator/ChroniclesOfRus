using System;
using UnityEngine;

namespace ChroniclesOfRus.Combat
{
    public readonly struct DamageInfo
    {
        public float Amount { get; }
        public GameObject Source { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitDirection { get; }
        public DamageType DamageType { get; }

        public DamageInfo(
            float amount,
            GameObject source,
            Vector3 hitPoint,
            Vector3 hitDirection,
            DamageType damageType)
        {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), "Damage amount cannot be negative.");

            Amount = amount;
            Source = source;
            HitPoint = hitPoint;
            HitDirection = hitDirection.sqrMagnitude > 0f ? hitDirection.normalized : Vector3.zero;
            DamageType = damageType;
        }
    }
}
