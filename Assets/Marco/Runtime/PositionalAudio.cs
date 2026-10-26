using UnityEngine;
using UnityEngine.Audio;

namespace LoQueCallaste
{
    public static class PositionalAudio
    {
        public static void Configure(AudioSource source, float minDistance = 1.5f, float maxDistance = 15f, AudioMixerGroup group = null)
        {
            if (!source) return;
            if (float.IsNaN(minDistance) || float.IsInfinity(minDistance)) minDistance = 1.5f;
            if (float.IsNaN(maxDistance) || float.IsInfinity(maxDistance)) maxDistance = 15f;
            minDistance = Mathf.Max(.1f, minDistance);
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = minDistance;
            source.maxDistance = Mathf.Max(minDistance + 0.1f, maxDistance);
            source.dopplerLevel = 0f;
            source.spread = 0f;
            source.reverbZoneMix = 0.8f;
            source.outputAudioMixerGroup = group;
        }
    }
}
