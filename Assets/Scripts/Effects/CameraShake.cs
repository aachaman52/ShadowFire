using UnityEngine;

namespace ShadowFire.Effects
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [Header("Trauma Settings")]
        [SerializeField] private float traumaDecayRate = 1.6f;
        [SerializeField] private float maxTranslationShake = 0.25f;
        [SerializeField] private float maxRotationShake = 5f;
        [SerializeField] private float frequency = 25f;

        private float _trauma = 0f;
        private Vector3 _initialLocalPos;
        private Quaternion _initialLocalRot;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            _initialLocalPos = transform.localPosition;
            _initialLocalRot = transform.localRotation;
        }

        public void AddTrauma(float amount)
        {
            _trauma = Mathf.Clamp01(_trauma + amount);
        }

        private void Update()
        {
            if (_trauma > 0)
            {
                // Trauma-squared shake response
                float shake = _trauma * _trauma;

                float offsetX = (Mathf.PerlinNoise(0, Time.time * frequency) * 2f - 1f) * maxTranslationShake * shake;
                float offsetY = (Mathf.PerlinNoise(1, Time.time * frequency) * 2f - 1f) * maxTranslationShake * shake;
                float offsetZ = (Mathf.PerlinNoise(2, Time.time * frequency) * 2f - 1f) * maxTranslationShake * shake;

                float pitch = (Mathf.PerlinNoise(3, Time.time * frequency) * 2f - 1f) * maxRotationShake * shake;
                float yaw = (Mathf.PerlinNoise(4, Time.time * frequency) * 2f - 1f) * maxRotationShake * shake;
                float roll = (Mathf.PerlinNoise(5, Time.time * frequency) * 2f - 1f) * maxRotationShake * shake;

                transform.localPosition = _initialLocalPos + new Vector3(offsetX, offsetY, offsetZ);
                transform.localRotation = _initialLocalRot * Quaternion.Euler(pitch, yaw, roll);

                _trauma = Mathf.Max(0, _trauma - traumaDecayRate * Time.deltaTime);
            }
            else
            {
                transform.localPosition = _initialLocalPos;
                transform.localRotation = _initialLocalRot;
            }
        }
    }
}
