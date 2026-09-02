using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Core;

namespace ShadowFire.Missions
{
    public static class MissionFactory
    {
        public static List<MissionDataSO> GetAllMissions()
        {
            List<MissionDataSO> list = new List<MissionDataSO>();

            // Mission 1
            var m1 = ScriptableObject.CreateInstance<MissionDataSO>();
            m1.MissionID = 1;
            m1.MissionName = "Abandoned Outpost";
            m1.Description = "Infiltrate the forward defense perimeter and eradicate 3 waves of hostiles.";
            m1.SceneName = "Level01";
            m1.TotalWaves = 3;
            m1.DifficultyMultiplier = 1.0f;
            m1.BaseXpReward = 1000;
            m1.CreditReward = 600;
            m1.CompletionBonus = 250;
            m1.HasBossFinalWave = false;
            list.Add(m1);

            // Mission 2
            var m2 = ScriptableObject.CreateInstance<MissionDataSO>();
            m2.MissionID = 2;
            m2.MissionName = "Industrial Sector";
            m2.Description = "Secure the power core against heavy runner swarms and armored hostiles across 4 waves.";
            m2.SceneName = "Level02";
            m2.TotalWaves = 4;
            m2.DifficultyMultiplier = 1.35f;
            m2.BaseXpReward = 2000;
            m2.CreditReward = 1100;
            m2.CompletionBonus = 500;
            m2.HasBossFinalWave = false;
            list.Add(m2);

            // Mission 3
            var m3 = ScriptableObject.CreateInstance<MissionDataSO>();
            m3.MissionID = 3;
            m3.MissionName = "Research Facility";
            m3.Description = "Deep penetration into the bio-core. Survive escalating waves and eliminate the Boss Titan.";
            m3.SceneName = "Level03";
            m3.TotalWaves = 5;
            m3.DifficultyMultiplier = 1.8f;
            m3.BaseXpReward = 3500;
            m3.CreditReward = 2200;
            m3.CompletionBonus = 1000;
            m3.HasBossFinalWave = true;
            m3.BossType = EnemyType.Boss;
            list.Add(m3);

            return list;
        }

        public static MissionDataSO GetMissionByID(int id)
        {
            var all = GetAllMissions();
            var found = all.Find(m => m.MissionID == id);
            return found ?? all[0];
        }
    }
}
