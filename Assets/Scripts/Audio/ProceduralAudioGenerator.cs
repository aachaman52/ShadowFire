using UnityEngine;
using ShadowFire.Core;

namespace ShadowFire.Audio
{
    public static class ProceduralAudioGenerator
    {
        public static AudioClip GenerateGunshot(WeaponType type)
        {
            int sampleRate = 44100;
            float duration = 0.25f;
            switch (type)
            {
                case WeaponType.SMG: duration = 0.12f; break;
                case WeaponType.Sniper: duration = 0.55f; break;
                case WeaponType.Shotgun: duration = 0.40f; break;
                case WeaponType.RocketLauncher: duration = 0.70f; break;
                default: duration = 0.22f; break;
            }

            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * (type == WeaponType.Sniper ? 8f : 18f));
                float noise = (Random.value * 2f - 1f);
                float freq = (type == WeaponType.RocketLauncher ? 90f : (type == WeaponType.Sniper ? 140f : 240f));
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);

                samples[i] = (noise * 0.7f + tone * 0.3f) * envelope;
            }

            AudioClip clip = AudioClip.Create($"SFX_Gun_{type}", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateExplosion()
        {
            int sampleRate = 44100;
            float duration = 0.85f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 5f);
                float noise = (Random.value * 2f - 1f);
                float subBass = Mathf.Sin(2f * Mathf.PI * 55f * t);

                samples[i] = (noise * 0.5f + subBass * 0.5f) * envelope;
            }

            AudioClip clip = AudioClip.Create("SFX_Explosion", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateReload()
        {
            int sampleRate = 44100;
            float duration = 0.3f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-Mathf.Abs(t - 0.15f) * 40f);
                float click = Mathf.Sin(2f * Mathf.PI * 800f * t);
                samples[i] = click * envelope * 0.6f;
            }

            AudioClip clip = AudioClip.Create("SFX_Reload", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateFootstep()
        {
            int sampleRate = 44100;
            float duration = 0.08f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 60f);
                float lowThud = Mathf.Sin(2f * Mathf.PI * 110f * t);
                samples[i] = (lowThud * 0.8f + (Random.value * 2f - 1f) * 0.2f) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("SFX_Footstep", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GeneratePickup()
        {
            int sampleRate = 44100;
            float duration = 0.2f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 12f);
                float freq = 520f + (t * 800f);
                float chime = Mathf.Sin(2f * Mathf.PI * freq * t);
                samples[i] = chime * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create("SFX_Pickup", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateLevelUp()
        {
            int sampleRate = 44100;
            float duration = 0.6f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 4f);
                float freq = (t < 0.2f ? 440f : (t < 0.4f ? 554f : 659f));
                float fanfare = Mathf.Sin(2f * Mathf.PI * freq * t);
                samples[i] = fanfare * envelope * 0.6f;
            }

            AudioClip clip = AudioClip.Create("SFX_LevelUp", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateBossRoar()
        {
            int sampleRate = 44100;
            float duration = 1.2f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Sin(Mathf.PI * (t / duration));
                float roarFreq = 80f + Mathf.Sin(2f * Mathf.PI * 4f * t) * 30f;
                float roarTone = Mathf.Sin(2f * Mathf.PI * roarFreq * t);
                float noise = (Random.value * 2f - 1f);
                samples[i] = (roarTone * 0.6f + noise * 0.4f) * envelope * 0.8f;
            }

            AudioClip clip = AudioClip.Create("SFX_BossRoar", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateTick()
        {
            int sampleRate = 44100;
            float duration = 0.05f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 80f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * 1000f * t) * envelope * 0.3f;
            }

            AudioClip clip = AudioClip.Create("SFX_Tick", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
