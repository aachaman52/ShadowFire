using UnityEngine;
using ShadowFire.Core;

namespace ShadowFire.Missions
{
    [CreateAssetMenu(fileName = "NewMission", menuName = "ShadowFire/Mission Data")]
    public class MissionDataSO : ScriptableObject
    {
        public int MissionID = 1;
        public string MissionName = "Abandoned Outpost";
        [TextArea(2, 4)]
        public string Description = "Infiltrate the perimeter and purge all incoming hostiles.";
        public string SceneName = "Level01";

        [Header("Mission Parameters")]
        public int TotalWaves = 3;
        public float DifficultyMultiplier = 1.0f;
        public int RequiredPlayerLevel = 1;
        public int RequiredPreviousMissionID = 0;

        [Header("Rewards")]
        public int BaseXpReward = 1000;
        public int CreditReward = 600;
        public int CompletionBonus = 250;

        [Header("Boss Config")]
        public bool HasBossFinalWave = false;
        public EnemyType BossType = EnemyType.Boss;
    }
}
