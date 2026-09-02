using System;
using System.Collections;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Managers;
using ShadowFire.Audio;
using ShadowFire.Player;
using ShadowFire.Effects;

namespace ShadowFire.Modes
{
    public enum GameModeType
    {
        Survival,
        Extraction,
        BossRush
    }

    public abstract class GameModeBase : MonoBehaviour
    {
        public abstract GameModeType ModeType { get; }
        public abstract string ModeName { get; }
        public abstract string ObjectiveDescription { get; }

        public event Action<string> OnObjectiveUpdated;
        public event Action OnModeVictory;
        public event Action OnModeDefeat;

        protected virtual void Start() { }

        protected void UpdateObjective(string text)
        {
            OnObjectiveUpdated?.Invoke(text);
        }

        protected void TriggerVictory()
        {
            OnModeVictory?.Invoke();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(5000);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLevelUp();
            }
        }

        protected void TriggerDefeat()
        {
            OnModeDefeat?.Invoke();
        }
    }

    public class SurvivalMode : GameModeBase
    {
        public override GameModeType ModeType => GameModeType.Survival;
        public override string ModeName => "Endless Survival";
        public override string ObjectiveDescription => "Survive endless enemy waves and purge all hostiles.";

        protected override void Start()
        {
            base.Start();
            UpdateObjective("SURVIVE ENDLESS HOSTILE WAVES");
        }
    }

    public class ExtractionMode : GameModeBase
    {
        public override GameModeType ModeType => GameModeType.Extraction;
        public override string ModeName => "Extraction Protocol";
        public override string ObjectiveDescription => "Survive to Wave 10, call the Dropship, and defend the Extraction Zone.";

        [SerializeField] private int targetWave = 10;
        [SerializeField] private float extractionHoldTime = 45f;
        private bool _extractionActive = false;
        private float _timeRemaining;
        private GameObject _beaconObject;

        protected override void Start()
        {
            base.Start();
            UpdateObjective($"REACH WAVE {targetWave} TO CALL EXTRACTION");

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStarted += HandleWaveStarted;
            }
        }

        private void HandleWaveStarted(int wave)
        {
            if (!_extractionActive)
            {
                UpdateObjective($"SURVIVE UNTIL WAVE {targetWave} (CURRENT: {wave})");
                if (wave >= targetWave)
                {
                    StartExtractionSequence();
                }
            }
        }

        private void StartExtractionSequence()
        {
            _extractionActive = true;
            _timeRemaining = extractionHoldTime;

            // Spawn visual beacon at center
            _beaconObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _beaconObject.name = "Extraction_Beacon";
            _beaconObject.transform.position = new Vector3(0, 0.2f, 0);
            _beaconObject.transform.localScale = new Vector3(8f, 0.1f, 8f);
            _beaconObject.GetComponent<Collider>().isTrigger = true;
            _beaconObject.GetComponent<MeshRenderer>().material = Effects.ProceduralMeshGenerator.GetMaterial("glowcyan");

            StartCoroutine(ExtractionCountdownRoutine());
        }

        private IEnumerator ExtractionCountdownRoutine()
        {
            while (_timeRemaining > 0)
            {
                _timeRemaining -= Time.deltaTime;
                UpdateObjective($"HOLD EXTRACTION ZONE: {Mathf.CeilToInt(_timeRemaining)}s");

                if (PlayerController.Instance != null)
                {
                    float dist = Vector3.Distance(PlayerController.Instance.transform.position, Vector3.zero);
                    if (dist > 12f)
                    {
                        UpdateObjective($"RETURN TO EXTRACTION ZONE! ({Mathf.CeilToInt(_timeRemaining)}s)");
                    }
                }

                yield return null;
            }

            UpdateObjective("DROPSHIP SECURED — EXTRACTION COMPLETE!");
            if (_beaconObject != null) Destroy(_beaconObject);
            TriggerVictory();
        }
    }

    public class BossRushMode : GameModeBase
    {
        public override GameModeType ModeType => GameModeType.BossRush;
        public override string ModeName => "Boss Titan Rush";
        public override string ObjectiveDescription => "Defeat sequential empowered Boss Titans.";

        private int _bossesDefeated = 0;
        private int _totalBosses = 5;

        protected override void Start()
        {
            base.Start();
            UpdateObjective($"PURGE TITAN 1 OF {_totalBosses}");

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveCompleted += HandleWaveCompleted;
            }
        }

        private void HandleWaveCompleted(int wave)
        {
            _bossesDefeated++;
            if (_bossesDefeated >= _totalBosses)
            {
                UpdateObjective("ALL TITANS ELIMINATED — VICTORY!");
                TriggerVictory();
            }
            else
            {
                UpdateObjective($"PURGE TITAN {_bossesDefeated + 1} OF {_totalBosses}");
            }
        }
    }
}
