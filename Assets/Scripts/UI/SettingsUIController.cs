using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShadowFire.Audio;
using ShadowFire.SaveSystem;
using ShadowFire.Player;

namespace ShadowFire.UI
{
    public class SettingsUIController : MonoBehaviour
    {
        public static SettingsUIController Instance { get; private set; }

        [Header("UI Controls")]
        public GameObject SettingsPanel;
        public TMP_Dropdown QualityDropdown;
        public Toggle FullscreenToggle;
        public Slider SensitivitySlider;
        public TextMeshProUGUI SensitivityValueText;
        public Slider FovSlider;
        public TextMeshProUGUI FovValueText;

        public Slider MasterVolumeSlider;
        public Slider MusicVolumeSlider;
        public Slider SfxVolumeSlider;

        public Button CloseButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            LoadAndApplySettings();

            if (CloseButton != null)
            {
                CloseButton.onClick.AddListener(CloseSettings);
            }

            if (SensitivitySlider != null)
            {
                SensitivitySlider.onValueChanged.AddListener((val) =>
                {
                    if (SensitivityValueText != null) SensitivityValueText.text = val.ToString("F1");
                    if (PlayerController.Instance != null) PlayerController.Instance.MouseSensitivity = val;
                });
            }

            if (FovSlider != null)
            {
                FovSlider.onValueChanged.AddListener((val) =>
                {
                    if (FovValueText != null) FovValueText.text = Mathf.RoundToInt(val).ToString();
                    if (Camera.main != null) Camera.main.fieldOfView = val;
                });
            }

            if (MasterVolumeSlider != null)
            {
                MasterVolumeSlider.onValueChanged.AddListener((val) =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.MasterVolume = val;
                });
            }

            if (QualityDropdown != null)
            {
                QualityDropdown.onValueChanged.AddListener((idx) => QualitySettings.SetQualityLevel(idx));
            }

            if (FullscreenToggle != null)
            {
                FullscreenToggle.onValueChanged.AddListener((isFull) => Screen.fullScreen = isFull);
            }

            if (SettingsPanel != null) SettingsPanel.SetActive(false);
        }

        public void OpenSettings()
        {
            if (SettingsPanel != null) SettingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            SaveSettings();
            if (SettingsPanel != null) SettingsPanel.SetActive(false);
        }

        private void LoadAndApplySettings()
        {
            GameSaveData data = SaveSystem.SaveSystem.Load();

            if (SensitivitySlider != null)
            {
                SensitivitySlider.value = data.MouseSensitivity;
                if (SensitivityValueText != null) SensitivityValueText.text = data.MouseSensitivity.ToString("F1");
            }
            if (PlayerController.Instance != null) PlayerController.Instance.MouseSensitivity = data.MouseSensitivity;

            if (FovSlider != null)
            {
                FovSlider.value = data.FieldOfView;
                if (FovValueText != null) FovValueText.text = Mathf.RoundToInt(data.FieldOfView).ToString();
            }

            if (MasterVolumeSlider != null) MasterVolumeSlider.value = data.MasterVolume;
            if (MusicVolumeSlider != null) MusicVolumeSlider.value = data.MusicVolume;
            if (SfxVolumeSlider != null) SfxVolumeSlider.value = data.SfxVolume;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.MasterVolume = data.MasterVolume;
                AudioManager.Instance.MusicVolume = data.MusicVolume;
                AudioManager.Instance.SfxVolume = data.SfxVolume;
            }

            if (QualityDropdown != null) QualityDropdown.value = data.QualityLevel;
            if (FullscreenToggle != null) FullscreenToggle.isOn = data.IsFullscreen;
        }

        private void SaveSettings()
        {
            GameSaveData data = SaveSystem.SaveSystem.Load();
            if (SensitivitySlider != null) data.MouseSensitivity = SensitivitySlider.value;
            if (FovSlider != null) data.FieldOfView = FovSlider.value;
            if (MasterVolumeSlider != null) data.MasterVolume = MasterVolumeSlider.value;
            if (MusicVolumeSlider != null) data.MusicVolume = MusicVolumeSlider.value;
            if (SfxVolumeSlider != null) data.SfxVolume = SfxVolumeSlider.value;
            if (QualityDropdown != null) data.QualityLevel = QualityDropdown.value;
            if (FullscreenToggle != null) data.IsFullscreen = FullscreenToggle.isOn;

            SaveSystem.SaveSystem.Save(data);
        }
    }
}
