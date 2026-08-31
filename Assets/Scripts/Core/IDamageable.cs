using System;
using UnityEngine;

namespace ShadowFire.Core
{
    public interface IDamageable
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsAlive { get; }

        event Action<DamageInfo> OnDamaged;
        event Action<DamageInfo> OnDied;

        void TakeDamage(DamageInfo damageInfo);
        void Heal(float amount);
    }
}
