using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShadowFire.Maps;
using ShadowFire.Modes;
using ShadowFire.Managers;

namespace ShadowFire.UI
{
    public class MapAndModeSelectUI : MonoBehaviour
    {
        public static MapAndModeSelectUI Instance { get; private set; }

        [Header("Objective HUD")]
        public TextMeshProUGUI ObjectiveText;

        [Header("Selection Panel")]
        public GameObject SelectorPanel;
        public Button OutpostBtn;
        public Button BiolabBtn;
        public Button InfernoBtn;

        public Button SurvivalModeBtn;
        public Button ExtractionModeBtn;
        public Button BossRushModeBtn;

        public Button StartGameBtn;

        private MapTheme _selectedMap = MapTheme.OutpostRuin;
        private GameModeType _selectedMode = GameModeType.Survival;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (OutpostBtn != null) OutpostBtn.onClick.AddListener(() => SelectMap(MapTheme.OutpostRuin));
            if (BiolabBtn != null) BiolabBtn.onClick.AddListener(() => SelectMap(MapTheme.ToxicBiolab));
            if (InfernoBtn != null) InfernoBtn.onClick.AddListener(() => SelectMap(MapTheme.InfernoCrater));

            if (SurvivalModeBtn != null) SurvivalModeBtn.onClick.AddListener(() => SelectMode(GameModeType.Survival));
            if (ExtractionModeBtn != null) ExtractionModeBtn.onClick.AddListener(() => SelectMode(GameModeType.Extraction));
            if (BossRushModeBtn != null) BossRushModeBtn.onClick.AddListener(() => SelectMode(GameModeType.BossRush));

            if (StartGameBtn != null) StartGameBtn.onClick.AddListener(ConfirmAndLaunch);

            if (ModeManager.Instance != null)
            {
                ModeManager.Instance.OnModeInitialized += HookModeEvents;
                if (ModeManager.Instance.ActiveMode != null)
                {
                    HookModeEvents(ModeManager.Instance.ActiveMode);
                }
            }

            if (SelectorPanel != null) SelectorPanel.SetActive(false);
        }

        private void HookModeEvents(GameModeBase mode)
        {
            if (mode == null) return;
            mode.OnObjectiveUpdated += (text) =>
            {
                if (ObjectiveText != null) ObjectiveText.text = text;
            };
        }

        public void OpenSelector()
        {
            if (SelectorPanel != null) SelectorPanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void SelectMap(MapTheme theme)
        {
            _selectedMap = theme;
            Debug.Log($"Selected Map: {theme}");
        }

        private void SelectMode(GameModeType mode)
        {
            _selectedMode = mode;
            Debug.Log($"Selected Mode: {mode}");
        }

        private void ConfirmAndLaunch()
        {
            if (SelectorPanel != null) SelectorPanel.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (MapManager.Instance != null)
            {
                MapManager.Instance.LoadMap(_selectedMap);
            }

            if (ModeManager.Instance != null)
            {
                ModeManager.Instance.SetGameMode(_selectedMode);
            }
        }
    }
}
