using UnityEngine;

namespace ShadowFire.Core
{
    public interface IKnockbackable
    {
        void ApplyKnockback(Vector3 force);
    }
}
