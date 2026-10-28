using UnityEngine;

namespace LoQueCallaste
{
    [RequireComponent(typeof(AudioReverbZone))]
    public sealed class SchoolReverbArea : MonoBehaviour
    {
        public AudioReverbPreset preset = AudioReverbPreset.Hallway;
        [Min(0)] public float fullEffectRadius = 3;
        [Min(0.1f)] public float outerRadius = 7;

        private void Awake() { Apply(); }
        private void OnValidate() { Apply(); }

        public void Apply()
        {
            var zone = GetComponent<AudioReverbZone>();
            if (!zone) return;
            zone.reverbPreset = preset;
            zone.minDistance = Mathf.Max(0, fullEffectRadius);
            zone.maxDistance = Mathf.Max(zone.minDistance + 0.1f, outerRadius);
        }
    }
}
