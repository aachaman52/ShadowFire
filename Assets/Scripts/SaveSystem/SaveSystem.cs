using System;
using System.IO;
using UnityEngine;

namespace ShadowFire.SaveSystem
{
    [Serializable]
    public class GameSaveData
    {
        public int HighScore = 0;
        public int HighestWave = 1;
        public int TotalLifetimeKills = 0;

        public float MouseSensitivity = 1.8f;
        public float FieldOfView = 75f;
        public float MasterVolume = 1.0f;
        public float MusicVolume = 0.7f;
        public float SfxVolume = 1.0f;
        public int QualityLevel = 2;
        public bool IsFullscreen = true;
    }

    public static class SaveSystem
    {
        private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "shadowfire_save.json");
        private static GameSaveData _cachedData;

        public static GameSaveData Load()
        {
            if (_cachedData != null) return _cachedData;

            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    _cachedData = JsonUtility.FromJson<GameSaveData>(json);
                    return _cachedData;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to load save file: {e.Message}. Generating new save data.");
                }
            }

            _cachedData = new GameSaveData();
            Save(_cachedData);
            return _cachedData;
        }

        public static void Save(GameSaveData data)
        {
            _cachedData = data;
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write save file: {e.Message}");
            }
        }

        public static void SaveGameStats(int score, int wave, int kills)
        {
            GameSaveData data = Load();
            data.HighScore = Mathf.Max(data.HighScore, score);
            data.HighestWave = Mathf.Max(data.HighestWave, wave);
            data.TotalLifetimeKills += kills;
            Save(data);
        }
    }
}
