using UnityEngine;
using UnityEngine.Audio;

namespace LoQueCallaste
{
    [CreateAssetMenu(menuName = "Lo Que Callaste/Perfil de audio 3D")]
    public sealed class SpatialAudioProfile : ScriptableObject
    {
        [Min(0.1f)] public float minDistance = 1.5f;
        [Min(0.2f)] public float maxDistance = 15f;
        [Range(0, 1)] public float volume = 0.4f;
        [Range(0, 1.1f)] public float reverbMix = 0.8f;
        public AudioMixerGroup mixerGroup;

        public void Apply(AudioSource source)
        {
            if (!source) return;
            PositionalAudio.Configure(source, Mathf.Max(0.1f, minDistance), maxDistance, mixerGroup);
            source.volume = volume;
            source.reverbZoneMix = reverbMix;
        }
    }
}
