using System.Collections;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Managers;
using ShadowFire.Enemies;

namespace ShadowFire.Audio
{
    public enum MusicTrack
    {
        Ambient,
        Combat,
        Boss
    }

    public class DynamicMusicSystem : MonoBehaviour
    {
        public static DynamicMusicSystem Instance { get; private set; }

        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private bool _usingSourceA = true;

        private AudioClip _ambientTrack;
        private AudioClip _combatTrack;
        private AudioClip _bossTrack;

        private MusicTrack _currentTrack = MusicTrack.Ambient;
        private Coroutine _crossfadeRoutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _sourceA = gameObject.AddComponent<AudioSource>();
            _sourceA.loop = true;
            _sourceA.playOnAwake = false;

            _sourceB = gameObject.AddComponent<AudioSource>();
            _sourceB.loop = true;
            _sourceB.playOnAwake = false;

            GenerateProceduralSoundtrack();
        }

        private void Start()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStarted += HandleWaveStarted;
                WaveManager.Instance.OnWaveCompleted += (w) => PlayTrack(MusicTrack.Ambient);
                WaveManager.Instance.OnCountdownTick += (t) => { if (t > 0) PlayTrack(MusicTrack.Ambient); };
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }

            PlayTrack(MusicTrack.Ambient);
        }

        private void HandleWaveStarted(int wave)
        {
            if (wave % 5 == 0)
            {
                PlayTrack(MusicTrack.Boss);
            }
            else
            {
                PlayTrack(MusicTrack.Combat);
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                StopMusic();
            }
            else if (state == GameState.WaveCountdown)
            {
                PlayTrack(MusicTrack.Ambient);
            }
        }

        public void PlayTrack(MusicTrack track, float fadeDuration = 1.2f)
        {
            if (_currentTrack == track && (_sourceA.isPlaying || _sourceB.isPlaying)) return;
            _currentTrack = track;

            AudioClip nextClip = null;
            switch (track)
            {
                case MusicTrack.Ambient: nextClip = _ambientTrack; break;
                case MusicTrack.Combat: nextClip = _combatTrack; break;
                case MusicTrack.Boss: nextClip = _bossTrack; break;
            }

            if (nextClip == null) return;

            if (_crossfadeRoutine != null) StopCoroutine(_crossfadeRoutine);
            _crossfadeRoutine = StartCoroutine(CrossfadeRoutine(nextClip, fadeDuration));
        }

        private IEnumerator CrossfadeRoutine(AudioClip nextClip, float duration)
        {
            AudioSource currentSource = _usingSourceA ? _sourceA : _sourceB;
            AudioSource nextSource = _usingSourceA ? _sourceB : _sourceA;
            _usingSourceA = !_usingSourceA;

            nextSource.clip = nextClip;
            nextSource.volume = 0f;
            nextSource.Play();

            float timer = 0f;
            float targetMusicVol = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume * AudioManager.Instance.MasterVolume : 0.7f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                nextSource.volume = Mathf.Lerp(0f, targetMusicVol, t);
                currentSource.volume = Mathf.Lerp(targetMusicVol, 0f, t);
                yield return null;
            }

            currentSource.Stop();
            nextSource.volume = targetMusicVol;
        }

        public void StopMusic()
        {
            if (_sourceA != null) _sourceA.Stop();
            if (_sourceB != null) _sourceB.Stop();
        }

        private void GenerateProceduralSoundtrack()
        {
            int sampleRate = 44100;

            // 1. Ambient Tension (12 seconds loop)
            _ambientTrack = GenerateAmbientClip(sampleRate, 12.0f);

            // 2. Combat Horde (8 seconds loop at 130 BPM)
            _combatTrack = GenerateCombatClip(sampleRate, 7.38f); // 4 bars at 130 BPM

            // 3. Boss Battle (6.4 seconds loop at 150 BPM)
            _bossTrack = GenerateBossClip(sampleRate, 6.4f); // 4 bars at 150 BPM
        }

        private AudioClip GenerateAmbientClip(int sampleRate, float duration)
        {
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // Dark drone pads (F minor / C# chord)
                float pad1 = Mathf.Sin(2f * Mathf.PI * 87.31f * t) * 0.35f; // F2
                float pad2 = Mathf.Sin(2f * Mathf.PI * 103.83f * t) * 0.25f; // G#2
                float pad3 = Mathf.Sin(2f * Mathf.PI * 130.81f * t) * 0.25f; // C3

                // Low heartbeat pulse (every 1.5s)
                float pulseEnv = Mathf.Exp(-Mathf.Repeat(t, 1.5f) * 8f);
                float pulse = Mathf.Sin(2f * Mathf.PI * 55f * t) * pulseEnv * 0.4f;

                // Wind shimmer
                float shimmer = (Random.value * 2f - 1f) * 0.04f * Mathf.Sin(2f * Mathf.PI * 0.2f * t);

                samples[i] = (pad1 + pad2 + pad3 + pulse + shimmer) * 0.7f;
            }

            AudioClip clip = AudioClip.Create("Music_Ambient", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateCombatClip(int sampleRate, float duration)
        {
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];
            float beatDuration = 60f / 130f; // 130 BPM

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // 4-on-the-floor Kick drum
                float beatTime = Mathf.Repeat(t, beatDuration);
                float kickEnv = Mathf.Exp(-beatTime * 22f);
                float kick = Mathf.Sin(2f * Mathf.PI * (120f - beatTime * 200f) * beatTime) * kickEnv * 0.85f;

                // 16th note rolling bassline (D minor)
                float sixteenth = Mathf.Repeat(t, beatDuration / 4f);
                float bassEnv = Mathf.Exp(-sixteenth * 16f);
                int noteStep = (int)(t / (beatDuration / 4f)) % 16;
                float bassFreq = (noteStep % 4 == 0) ? 73.42f : ((noteStep % 4 == 2) ? 87.31f : 65.41f);
                float bass = Mathf.Sin(2f * Mathf.PI * bassFreq * t) * bassEnv * 0.5f;

                // Hi-Hat on off-beats
                float hatTime = Mathf.Repeat(t + beatDuration * 0.5f, beatDuration);
                float hatEnv = Mathf.Exp(-hatTime * 50f);
                float hat = (Random.value * 2f - 1f) * hatEnv * 0.25f;

                // Synth arpeggio
                float arpTime = Mathf.Repeat(t, beatDuration / 2f);
                float arpEnv = Mathf.Exp(-arpTime * 10f);
                float arpFreq = 293.66f * (1f + (noteStep % 3) * 0.25f);
                float arp = Mathf.Sin(2f * Mathf.PI * arpFreq * t) * arpEnv * 0.3f;

                samples[i] = Mathf.Clamp((kick + bass + hat + arp) * 0.65f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Music_Combat", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateBossClip(int sampleRate, float duration)
        {
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];
            float beatDuration = 60f / 150f; // 150 BPM

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // Fast heavy kick
                float beatTime = Mathf.Repeat(t, beatDuration);
                float kickEnv = Mathf.Exp(-beatTime * 25f);
                float kick = Mathf.Sin(2f * Mathf.PI * (140f - beatTime * 250f) * beatTime) * kickEnv * 0.9f;

                // Distorted sub-bass drone
                float sub = Mathf.Sin(2f * Mathf.PI * 45f * t) * 0.6f;

                // Industrial metal snare on beats 2 & 4
                float snareTime = Mathf.Repeat(t + beatDuration, beatDuration * 2f);
                float snareEnv = Mathf.Exp(-snareTime * 18f);
                float snare = (Random.value * 2f - 1f) * snareEnv * 0.55f;

                // Alarm / siren stab
                float siren = Mathf.Sin(2f * Mathf.PI * (440f + Mathf.Sin(2f * Mathf.PI * 2f * t) * 80f) * t) * 0.25f;

                samples[i] = Mathf.Clamp((kick + sub + snare + siren) * 0.7f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Music_Boss", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
