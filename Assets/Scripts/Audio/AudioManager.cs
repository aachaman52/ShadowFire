using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Core;

namespace ShadowFire.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Volume Buses")]
        [Range(0f, 1f)] public float MasterVolume = 1.0f;
        [Range(0f, 1f)] public float MusicVolume = 0.7f;
        [Range(0f, 1f)] public float SfxVolume = 1.0f;

        private AudioSource _2dSource;
        private readonly Dictionary<WeaponType, AudioClip> _gunshotClips = new Dictionary<WeaponType, AudioClip>();
        private AudioClip _explosionClip;
        private AudioClip _reloadClip;
        private AudioClip _footstepClip;
        private AudioClip _pickupClip;
        private AudioClip _levelUpClip;
        private AudioClip _bossRoarClip;
        private AudioClip _tickClip;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _2dSource = gameObject.AddComponent<AudioSource>();
            _2dSource.playOnAwake = false;

            GenerateCachedClips();
        }

        private void GenerateCachedClips()
        {
            _gunshotClips[WeaponType.Rifle] = ProceduralAudioGenerator.GenerateGunshot(WeaponType.Rifle);
            _gunshotClips[WeaponType.SMG] = ProceduralAudioGenerator.GenerateGunshot(WeaponType.SMG);
            _gunshotClips[WeaponType.Sniper] = ProceduralAudioGenerator.GenerateGunshot(WeaponType.Sniper);
            _gunshotClips[WeaponType.Shotgun] = ProceduralAudioGenerator.GenerateGunshot(WeaponType.Shotgun);
            _gunshotClips[WeaponType.RocketLauncher] = ProceduralAudioGenerator.GenerateGunshot(WeaponType.RocketLauncher);

            _explosionClip = ProceduralAudioGenerator.GenerateExplosion();
            _reloadClip = ProceduralAudioGenerator.GenerateReload();
            _footstepClip = ProceduralAudioGenerator.GenerateFootstep();
            _pickupClip = ProceduralAudioGenerator.GeneratePickup();
            _levelUpClip = ProceduralAudioGenerator.GenerateLevelUp();
            _bossRoarClip = ProceduralAudioGenerator.GenerateBossRoar();
            _tickClip = ProceduralAudioGenerator.GenerateTick();
        }

        public void Play2DSFX(AudioClip clip, float volumeScale = 1.0f, float pitch = 1.0f)
        {
            if (clip == null || _2dSource == null) return;
            _2dSource.pitch = pitch;
            _2dSource.PlayOneShot(clip, volumeScale * SfxVolume * MasterVolume);
        }

        public void Play3DSFX(AudioClip clip, Vector3 position, float volumeScale = 1.0f, float pitch = 1.0f)
        {
            if (clip == null) return;

            GameObject temp = new GameObject("TempAudioSource");
            temp.transform.position = position;

            AudioSource source = temp.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volumeScale * SfxVolume * MasterVolume;
            source.pitch = pitch;
            source.spatialBlend = 1.0f; // 3D sound
            source.minDistance = 2f;
            source.maxDistance = 45f;
            source.Play();

            Destroy(temp, clip.length / Mathf.Max(0.1f, pitch) + 0.1f);
        }

        public void PlayGunshot(WeaponType type)
        {
            if (_gunshotClips.TryGetValue(type, out var clip))
            {
                float pitch = Random.Range(0.92f, 1.08f);
                Play2DSFX(clip, 0.85f, pitch);
            }
        }

        public void PlayExplosion(Vector3 position)
        {
            float pitch = Random.Range(0.85f, 1.15f);
            Play3DSFX(_explosionClip, position, 1.0f, pitch);
        }

        public void PlayReload() => Play2DSFX(_reloadClip, 0.7f, Random.Range(0.95f, 1.05f));
        public void PlayFootstep() => Play2DSFX(_footstepClip, 0.4f, Random.Range(0.88f, 1.12f));
        public void PlayJump() => Play2DSFX(_footstepClip, 0.6f, 1.3f);
        public void PlayPickup() => Play2DSFX(_pickupClip, 0.8f, Random.Range(0.95f, 1.05f));
        public void PlayLevelUp() => Play2DSFX(_levelUpClip, 1.0f, 1.0f);
        public void PlayUpgradeSelect() => Play2DSFX(_pickupClip, 1.0f, 1.25f);
        public void PlayBossRoar(Vector3 pos) => Play3DSFX(_bossRoarClip, pos, 1.0f, 0.9f);
        public void PlayTick() => Play2DSFX(_tickClip, 0.5f, 1.0f);
        public void PlayWaveStart() => Play2DSFX(_levelUpClip, 0.9f, 0.8f);
        public void PlayWaveComplete() => Play2DSFX(_levelUpClip, 0.9f, 1.1f);
        public void PlayEnemyDeath(Vector3 pos) => Play3DSFX(_footstepClip, pos, 0.7f, 0.7f);
        public void PlayGameOver() => Play2DSFX(_bossRoarClip, 1.0f, 0.6f);
    }
}
