using UnityEngine;
using UnityEngine.UI;
using ShadowFire.Core;
using ShadowFire.Player;

namespace ShadowFire.Effects
{
    public class ScreenDamageFlash : MonoBehaviour
    {
        public static ScreenDamageFlash Instance { get; private set; }

        [SerializeField] private Image flashImage;
        [SerializeField] private Image lowHealthVignette;
        [SerializeField] private float flashDuration = 0.25f;

        private float _currentFlashAlpha = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnDamaged += HandleDamaged;
            }
        }

        private void HandleDamaged(DamageInfo info)
        {
            _currentFlashAlpha = 0.6f;
        }

        private void Update()
        {
            if (flashImage != null)
            {
                if (_currentFlashAlpha > 0)
                {
                    _currentFlashAlpha -= (Time.deltaTime / flashDuration);
                    flashImage.color = new Color(1f, 0, 0, Mathf.Clamp01(_currentFlashAlpha));
                }
            }

            if (lowHealthVignette != null && PlayerStats.Instance != null)
            {
                float healthRatio = PlayerStats.Instance.CurrentHealth / PlayerStats.Instance.MaxHealth;
                if (healthRatio < 0.25f && PlayerStats.Instance.IsAlive)
                {
                    float pulse = (Mathf.Sin(Time.time * 6f) * 0.5f + 0.5f) * 0.4f + 0.2f;
                    lowHealthVignette.color = new Color(0.8f, 0, 0, pulse);
                }
                else
                {
                    lowHealthVignette.color = new Color(0.8f, 0, 0, 0);
                }
            }
        }

        public void BindImages(Image flash, Image vignette)
        {
            flashImage = flash;
            lowHealthVignette = vignette;
        }
    }
}
