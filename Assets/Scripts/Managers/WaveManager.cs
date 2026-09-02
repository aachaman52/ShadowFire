using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Enemies;
using ShadowFire.Environment;
using ShadowFire.Audio;
using ShadowFire.Effects;
using ShadowFire.Missions;
using ShadowFire.Models;

namespace ShadowFire.Managers
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Wave Progress")]
        [SerializeField] private int currentWave = 0;
        [SerializeField] private int totalEnemiesThisWave = 0;
        [SerializeField] private int enemiesRemaining = 0;
        [SerializeField] private float countdownDuration = 5f;
        [SerializeField] private bool isWaveActive = false;

        [Header("Enemy Prefabs / Prototypes")]
        public GameObject ZombiePrefab;
        public GameObject RunnerPrefab;
        public GameObject TankPrefab;
        public GameObject ShooterPrefab;
        public GameObject BossPrefab;

        [Header("Arena Reference")]
        public ArenaBuilder Arena;

        public int CurrentWave => currentWave;
        public int EnemiesRemaining => enemiesRemaining;
        public int TotalEnemies => totalEnemiesThisWave;
        public bool IsWaveActive => isWaveActive;

        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveCompleted;
        public event Action<int, int> OnEnemyCountChanged;
        public event Action<int> OnCountdownTick;

        private List<EnemyBase> _activeEnemies = new List<EnemyBase>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            StartCoroutine(StartNextWaveWithCountdown());
        }

        public IEnumerator StartNextWaveWithCountdown()
        {
            isWaveActive = false;
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.WaveCountdown);

            for (int t = (int)countdownDuration; t > 0; t--)
            {
                OnCountdownTick?.Invoke(t);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayTick();
                yield return new WaitForSeconds(1f);
            }

            OnCountdownTick?.Invoke(0);
            StartWave(currentWave + 1);
        }

        private void StartWave(int waveNumber)
        {
            currentWave = waveNumber;
            isWaveActive = true;
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.InGame);

            bool isBossWave = (currentWave % 5 == 0);

            if (isBossWave)
            {
                totalEnemiesThisWave = 1 + (currentWave / 5) * 2;
            }
            else
            {
                totalEnemiesThisWave = 3 + currentWave * 2;
            }

            enemiesRemaining = totalEnemiesThisWave;
            OnWaveStarted?.Invoke(currentWave);
            OnEnemyCountChanged?.Invoke(enemiesRemaining, totalEnemiesThisWave);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWaveStart();
            }

            StartCoroutine(SpawnWaveRoutine(isBossWave));
        }

        private IEnumerator SpawnWaveRoutine(bool isBossWave)
        {
            float healthMult = 1.0f + (currentWave - 1) * 0.10f;
            float speedMult = Mathf.Min(1.4f, 1.0f + (currentWave - 1) * 0.02f);
            float damageMult = 1.0f + (currentWave - 1) * 0.05f;

            if (isBossWave)
            {
                // Spawn Boss at Boss Spawn Point
                Vector3 bPos = Arena != null && Arena.BossSpawnPoint != null ? Arena.BossSpawnPoint.position : new Vector3(0, 0.5f, 25f);
                SpawnEnemy(EnemyType.Boss, bPos, healthMult * 1.2f, speedMult, damageMult);
                yield return new WaitForSeconds(2.0f);

                // Spawn Minion escorts
                for (int i = 1; i < totalEnemiesThisWave; i++)
                {
                    Vector3 sPos = GetRandomSpawnPosition();
                    SpawnEnemy(EnemyType.Runner, sPos, healthMult, speedMult, damageMult);
                    yield return new WaitForSeconds(1.2f);
                }
            }
            else
            {
                // Standard Wave enemy composition
                for (int i = 0; i < totalEnemiesThisWave; i++)
                {
                    EnemyType typeToSpawn = PickEnemyTypeForWave(currentWave);
                    Vector3 sPos = GetRandomSpawnPosition();
                    SpawnEnemy(typeToSpawn, sPos, healthMult, speedMult, damageMult);

                    float spawnDelay = Mathf.Max(0.8f, 1.8f - (currentWave * 0.1f));
                    yield return new WaitForSeconds(spawnDelay);
                }
            }
        }

        private EnemyType PickEnemyTypeForWave(int wave)
        {
            float roll = UnityEngine.Random.value;
            if (wave == 1) return EnemyType.Zombie;
            if (wave == 2) return roll < 0.7f ? EnemyType.Zombie : EnemyType.Runner;
            if (wave < 5)
            {
                if (roll < 0.5f) return EnemyType.Zombie;
                if (roll < 0.8f) return EnemyType.Runner;
                return EnemyType.Shooter;
            }

            // High waves: mixed threats
            if (roll < 0.35f) return EnemyType.Zombie;
            if (roll < 0.65f) return EnemyType.Runner;
            if (roll < 0.85f) return EnemyType.Shooter;
            return EnemyType.Tank;
        }

        private void SpawnEnemy(EnemyType type, Vector3 position, float hMult, float sMult, float dMult)
        {
            GameObject enemyObj = null;

            switch (type)
            {
                case EnemyType.Zombie:
                    enemyObj = ZombiePrefab != null ? Instantiate(ZombiePrefab, position, Quaternion.identity) : CreateProceduralEnemy(type, position);
                    break;
                case EnemyType.Runner:
                    enemyObj = RunnerPrefab != null ? Instantiate(RunnerPrefab, position, Quaternion.identity) : CreateProceduralEnemy(type, position);
                    break;
                case EnemyType.Tank:
                    enemyObj = TankPrefab != null ? Instantiate(TankPrefab, position, Quaternion.identity) : CreateProceduralEnemy(type, position);
                    break;
                case EnemyType.Shooter:
                    enemyObj = ShooterPrefab != null ? Instantiate(ShooterPrefab, position, Quaternion.identity) : CreateProceduralEnemy(type, position);
                    break;
                case EnemyType.Boss:
                    enemyObj = BossPrefab != null ? Instantiate(BossPrefab, position, Quaternion.identity) : CreateProceduralEnemy(type, position);
                    break;
            }

            if (enemyObj != null)
            {
                EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.Initialize(hMult, sMult, dMult);
                    _activeEnemies.Add(enemy);
                }
            }
        }

        private GameObject CreateProceduralEnemy(EnemyType type, Vector3 pos)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.transform.position = pos;
            obj.layer = LayerMask.NameToLayer("Enemy");

            var agent = obj.AddComponent<UnityEngine.AI.NavMeshAgent>();

            EnemyBase enemyComponent = null;
            var mr = obj.GetComponent<MeshRenderer>();

            switch (type)
            {
                case EnemyType.Zombie:
                    obj.name = "Enemy_Zombie";
                    obj.transform.localScale = new Vector3(0.85f, 0.95f, 0.85f);
                    mr.material = ProceduralMeshGenerator.GetMaterial("enemy");
                    agent.radius = 0.4f;
                    agent.height = 1.75f;
                    enemyComponent = obj.AddComponent<ZombieEnemy>();
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(0.35f, 0.2f, 0.35f), Quaternion.identity, 0.85f);
                    break;

                case EnemyType.Runner:
                    obj.name = "Enemy_Runner";
                    obj.transform.localScale = new Vector3(0.75f, 0.85f, 0.75f);
                    mr.material = ProceduralMeshGenerator.GetMaterial("glowred");
                    agent.radius = 0.35f;
                    agent.height = 1.6f;
                    enemyComponent = obj.AddComponent<RunnerEnemy>();
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(0.32f, 0.15f, 0.32f), Quaternion.identity, 0.75f);
                    break;

                case EnemyType.Tank:
                    obj.name = "Enemy_Tank";
                    obj.transform.localScale = new Vector3(1.15f, 1.25f, 1.15f);
                    mr.material = ProceduralMeshGenerator.GetMaterial("gunmetal");
                    agent.radius = 0.6f;
                    agent.height = 2.1f;
                    enemyComponent = obj.AddComponent<TankEnemy>();
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(0.55f, 0.25f, 0.5f), Quaternion.identity, 1.25f);
                    break;

                case EnemyType.Shooter:
                    obj.name = "Enemy_Shooter";
                    obj.transform.localScale = new Vector3(0.85f, 0.95f, 0.85f);
                    mr.material = ProceduralMeshGenerator.GetMaterial("glowcyan");
                    agent.radius = 0.4f;
                    agent.height = 1.75f;
                    enemyComponent = obj.AddComponent<ShooterEnemy>();
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(0.35f, 0.2f, 0.35f), Quaternion.identity, 0.95f);
                    break;

                case EnemyType.Boss:
                    obj.name = "Enemy_Boss_Overlord";
                    obj.transform.localScale = new Vector3(1.5f, 1.75f, 1.5f);
                    mr.material = ProceduralMeshGenerator.GetMaterial("boss");
                    agent.radius = 0.8f;
                    agent.height = 2.8f;
                    enemyComponent = obj.AddComponent<BossEnemy>();
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(0.7f, 0.35f, 0.65f), Quaternion.identity, 1.6f);
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(-0.7f, 0.35f, 0.65f), Quaternion.identity, 1.6f);
                    break;
            }

            return obj;
        }

        private Vector3 GetRandomSpawnPosition()
        {
            if (Arena != null && Arena.EnemySpawnPoints.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, Arena.EnemySpawnPoints.Count);
                return Arena.EnemySpawnPoints[idx].position;
            }
            return new Vector3(UnityEngine.Random.Range(-25f, 25f), 0.5f, UnityEngine.Random.Range(-25f, 25f));
        }

        public void RegisterEnemyKilled(EnemyBase enemy)
        {
            if (_activeEnemies.Contains(enemy))
            {
                _activeEnemies.Remove(enemy);
            }

            enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);
            OnEnemyCountChanged?.Invoke(enemiesRemaining, totalEnemiesThisWave);

            if (GameManager.Instance != null)
            {
                int score = (enemy.Type == EnemyType.Boss) ? 2000 : (enemy.Type == EnemyType.Tank ? 350 : 100);
                GameManager.Instance.AddKill(score);
            }

            if (enemiesRemaining <= 0 && isWaveActive)
            {
                CompleteWave();
            }
        }

        private void CompleteWave()
        {
            isWaveActive = false;
            OnWaveCompleted?.Invoke(currentWave);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddWaveBonus(currentWave);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWaveComplete();
            }

            if (LevelManager.Instance != null && currentWave >= LevelManager.Instance.TotalWavesInLevel)
            {
                LevelManager.Instance.CompleteLevel();
            }
            else
            {
                StartCoroutine(StartNextWaveWithCountdown());
            }
        }
    }
}
