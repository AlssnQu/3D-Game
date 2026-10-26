using UnityEngine;
using UnityEngine.Audio;

namespace LoQueCallaste
{
    public static class PositionalAudio
    {
        public static void Configure(AudioSource source, float minDistance = 1.5f, float maxDistance = 15f, AudioMixerGroup group = null)
        {
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
