using System.Collections;
using UnityEngine;
using ShadowFire.Core;

namespace ShadowFire.Animation
{
    public class WeaponAnimationController : MonoBehaviour
    {
        [Header("Animated Weapon Parts")]
        public Transform RootModel;
        public Transform SlideOrBolt;
        public Transform PumpGrip;
        public Transform Magazine;
        public Transform ScopeOptic;

        [Header("Animation Config")]
        [SerializeField] private WeaponType weaponType = WeaponType.Rifle;
        [SerializeField] private float kickbackDistance = 0.08f;
        [SerializeField] private float kickbackAngle = 6.0f;
        [SerializeField] private float returnSpeed = 16f;

        private Vector3 _initialLocalPos;
        private Quaternion _initialLocalRot;
        private Vector3 _initialSlidePos;
        private Vector3 _initialPumpPos;
        private Vector3 _initialMagPos;

        private Vector3 _currentKickPos;
        private Vector3 _targetKickPos;
        private Quaternion _currentKickRot;
        private Quaternion _targetKickRot;

        private bool _isCycling = false;
        private bool _isReloading = false;

        public void Initialize(WeaponType type)
        {
            weaponType = type;

            if (RootModel != null)
            {
                _initialLocalPos = RootModel.localPosition;
                _initialLocalRot = RootModel.localRotation;
            }

            if (SlideOrBolt != null) _initialSlidePos = SlideOrBolt.localPosition;
            if (PumpGrip != null) _initialPumpPos = PumpGrip.localPosition;
            if (Magazine != null) _initialMagPos = Magazine.localPosition;

            _currentKickRot = Quaternion.identity;
            _targetKickRot = Quaternion.identity;
        }

        public void OnFired()
        {
            // Apply physical kickback to viewmodel
            _targetKickPos += new Vector3(
                Random.Range(-0.008f, 0.008f),
                Random.Range(0.01f, 0.025f),
                -kickbackDistance
            );

            _targetKickRot *= Quaternion.Euler(
                -kickbackAngle,
                Random.Range(-kickbackAngle * 0.4f, kickbackAngle * 0.4f),
                Random.Range(-kickbackAngle * 0.3f, kickbackAngle * 0.3f)
            );

            // Trigger mechanical cycling
            if (!_isCycling && gameObject.activeInHierarchy)
            {
                StartCoroutine(CycleMechanismRoutine());
            }
        }

        private IEnumerator CycleMechanismRoutine()
        {
            _isCycling = true;

            switch (weaponType)
            {
                case WeaponType.Rifle:
                case WeaponType.SMG:
                    // Instant slide blowback and return
                    if (SlideOrBolt != null)
                    {
                        SlideOrBolt.localPosition = _initialSlidePos - new Vector3(0, 0, 0.06f);
                        yield return new WaitForSeconds(0.04f);
                        SlideOrBolt.localPosition = _initialSlidePos;
                    }
                    break;

                case WeaponType.Sniper:
                    // Bolt action: Lift bolt -> pull back -> push forward -> lock down
                    if (SlideOrBolt != null)
                    {
                        yield return new WaitForSeconds(0.12f);
                        // Lift
                        SlideOrBolt.localRotation = Quaternion.Euler(0, 0, -45f);
                        yield return new WaitForSeconds(0.1f);
                        // Pull Back
                        SlideOrBolt.localPosition = _initialSlidePos - new Vector3(0, 0, 0.12f);
                        yield return new WaitForSeconds(0.18f);
                        // Push Forward
                        SlideOrBolt.localPosition = _initialSlidePos;
                        yield return new WaitForSeconds(0.12f);
                        // Lock Down
                        SlideOrBolt.localRotation = Quaternion.identity;
                    }
                    break;

                case WeaponType.Shotgun:
                    // Pump rack: rack back -> rack forward
                    if (PumpGrip != null)
                    {
                        yield return new WaitForSeconds(0.08f);
                        PumpGrip.localPosition = _initialPumpPos - new Vector3(0, 0, 0.14f);
                        yield return new WaitForSeconds(0.15f);
                        PumpGrip.localPosition = _initialPumpPos;
                    }
                    break;
            }

            _isCycling = false;
        }

        public void OnReloadStarted(float reloadDuration)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ReloadAnimationRoutine(reloadDuration));
            }
        }

        private IEnumerator ReloadAnimationRoutine(float duration)
        {
            _isReloading = true;
            float elapsed = 0f;

            // Tilt weapon slightly to side for reload
            Quaternion reloadTilt = Quaternion.Euler(15f, -25f, 20f);
            Vector3 reloadLower = new Vector3(0, -0.08f, 0);

            // Drop Mag
            if (Magazine != null)
            {
                Magazine.localPosition = _initialMagPos - new Vector3(0, 0.35f, 0);
            }

            while (elapsed < duration * 0.6f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.6f);
                if (RootModel != null)
                {
                    RootModel.localPosition = Vector3.Lerp(_initialLocalPos, _initialLocalPos + reloadLower, t);
                    RootModel.localRotation = Quaternion.Slerp(_initialLocalRot, _initialLocalRot * reloadTilt, t);
                }
                yield return null;
            }

            // Snap new Mag in
            if (Magazine != null)
            {
                Magazine.localPosition = _initialMagPos;
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = (elapsed - duration * 0.6f) / (duration * 0.4f);
                if (RootModel != null)
                {
                    RootModel.localPosition = Vector3.Lerp(_initialLocalPos + reloadLower, _initialLocalPos, t);
                    RootModel.localRotation = Quaternion.Slerp(_initialLocalRot * reloadTilt, _initialLocalRot, t);
                }
                yield return null;
            }

            if (RootModel != null)
            {
                RootModel.localPosition = _initialLocalPos;
                RootModel.localRotation = _initialLocalRot;
            }

            _isReloading = false;
        }

        private void Update()
        {
            // Smoothly recover from recoil kick
            _targetKickPos = Vector3.Lerp(_targetKickPos, Vector3.zero, Time.deltaTime * returnSpeed);
            _currentKickPos = Vector3.Lerp(_currentKickPos, _targetKickPos, Time.deltaTime * 24f);

            _targetKickRot = Quaternion.Slerp(_targetKickRot, Quaternion.identity, Time.deltaTime * returnSpeed);
            _currentKickRot = Quaternion.Slerp(_currentKickRot, _targetKickRot, Time.deltaTime * 24f);

            if (RootModel != null && !_isReloading)
            {
                RootModel.localPosition = _initialLocalPos + _currentKickPos;
                RootModel.localRotation = _initialLocalRot * _currentKickRot;
            }
        }
    }
}
