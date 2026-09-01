using UnityEngine;
using ShadowFire.Core;

namespace ShadowFire.Audio
{
    public static class MultiLayerSoundSynthesizer
    {
        public static AudioClip GenerateLayeredGunshot(WeaponType type)
        {
            int sampleRate = 44100;
            float duration = 0.35f;
            switch (type)
            {
                case WeaponType.SMG: duration = 0.18f; break;
                case WeaponType.Sniper: duration = 0.75f; break;
                case WeaponType.Shotgun: duration = 0.50f; break;
                case WeaponType.RocketLauncher: duration = 0.90f; break;
                default: duration = 0.32f; break;
            }

            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            float snapFreq = type == WeaponType.Sniper ? 3200f : (type == WeaponType.SMG ? 2200f : 2800f);
            float bodyFreq = type == WeaponType.RocketLauncher ? 75f : (type == WeaponType.Sniper ? 110f : (type == WeaponType.Shotgun ? 95f : 160f));

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // Layer 1: High Frequency Transient Snap (First 15ms)
                float snapEnv = Mathf.Exp(-t * 120f);
                float snap = Mathf.Sin(2f * Mathf.PI * snapFreq * t) * (Random.value * 2f - 1f) * snapEnv * 0.9f;

                // Layer 2: Low-Mid Punch Body
                float bodyEnv = Mathf.Exp(-t * (type == WeaponType.Sniper ? 10f : 18f));
                float body = Mathf.Sin(2f * Mathf.PI * bodyFreq * t) * bodyEnv * 0.8f;

                // Layer 3: Mechanical Slide Clank (Delayed at ~40ms)
                float mechEnv = Mathf.Exp(-Mathf.Abs(t - 0.045f) * 60f);
                float mech = Mathf.Sin(2f * Mathf.PI * 1400f * t) * mechEnv * 0.4f;

                // Layer 4: Distant Spatial Reverb Tail
                float tailEnv = Mathf.Exp(-t * 5f);
                float tail = (Random.value * 2f - 1f) * tailEnv * 0.35f;

                samples[i] = Mathf.Clamp(snap + body + mech + tail, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create($"SFX_Layered_{type}", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateLayeredExplosion()
        {
            int sampleRate = 44100;
            float duration = 1.1f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // Sub-bass heavy thump
                float subEnv = Mathf.Exp(-t * 3.5f);
                float sub = Mathf.Sin(2f * Mathf.PI * (50f - t * 20f) * t) * subEnv * 0.85f;

                // Debris and blast noise
                float blastEnv = Mathf.Exp(-t * 6f);
                float blast = (Random.value * 2f - 1f) * blastEnv * 0.65f;

                // Shockwave roar
                float roarEnv = Mathf.Sin(Mathf.Clamp01(t / 0.5f) * Mathf.PI) * Mathf.Exp(-t * 3f);
                float roar = Mathf.Sin(2f * Mathf.PI * 120f * t) * roarEnv * 0.5f;

                samples[i] = Mathf.Clamp(sub + blast + roar, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("SFX_Layered_Explosion", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateTitanRoar()
        {
            int sampleRate = 44100;
            float duration = 1.6f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration));

                float pitchMod = 70f + Mathf.Sin(2f * Mathf.PI * 5f * t) * 35f;
                float tone1 = Mathf.Sin(2f * Mathf.PI * pitchMod * t);
                float tone2 = Mathf.Sin(2f * Mathf.PI * (pitchMod * 1.5f) * t);
                float noise = (Random.value * 2f - 1f) * 0.5f;

                samples[i] = (tone1 * 0.5f + tone2 * 0.3f + noise) * env * 0.9f;
            }

            AudioClip clip = AudioClip.Create("SFX_Titan_Roar", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
