using System;
using UnityEngine;

namespace ShadowFire.Modes
{
    public class ModeManager : MonoBehaviour
    {
        public static ModeManager Instance { get; private set; }

        [SerializeField] private GameModeType activeModeType = GameModeType.Survival;
        private GameModeBase _activeMode;

        public GameModeType ActiveType => activeModeType;
        public GameModeBase ActiveMode => _activeMode;

        public event Action<GameModeBase> OnModeInitialized;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        public void SetGameMode(GameModeType type)
        {
            activeModeType = type;

            if (_activeMode != null)
            {
                Destroy(_activeMode);
            }

            switch (type)
            {
                case GameModeType.Extraction:
                    _activeMode = gameObject.AddComponent<ExtractionMode>();
                    break;
                case GameModeType.BossRush:
                    _activeMode = gameObject.AddComponent<BossRushMode>();
                    break;
                default:
                    _activeMode = gameObject.AddComponent<SurvivalMode>();
                    break;
            }

            OnModeInitialized?.Invoke(_activeMode);
            Debug.Log($"[ShadowFire] Mode Initialized: {_activeMode.ModeName}");
        }
    }
}
