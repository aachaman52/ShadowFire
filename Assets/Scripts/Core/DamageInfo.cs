using UnityEngine;

namespace ShadowFire.Core
{
    [System.Serializable]
    public struct DamageInfo
    {
        public float Amount;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public bool IsCritical;
        public GameObject Source;
        public Vector3 KnockbackForce;
        public HitType HitType;

        public DamageInfo(float amount, Vector3 hitPoint, Vector3 hitNormal, bool isCritical = false, GameObject source = null, Vector3 knockbackForce = default, HitType hitType = HitType.Default)
        {
            Amount = amount;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            IsCritical = isCritical;
            Source = source;
            KnockbackForce = knockbackForce;
            HitType = hitType;
        }
    }
}
