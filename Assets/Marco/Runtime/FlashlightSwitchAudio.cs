using UnityEngine;

namespace LoQueCallaste
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class FlashlightSwitchAudio : MonoBehaviour
    {
        public AudioClip switchOn, switchOff;
        private AudioSource source;
        private bool? previous;
        private void Awake() { source=GetComponent<AudioSource>(); }
        // Connect to the real flashlight's state-changed event; this does not control battery or light.
        public void OnStateChanged(bool enabled) { PlayState(enabled); }
        public bool PlayState(bool enabled)
        {
            if(!isActiveAndEnabled || !source || previous==enabled)return false;
            var clip=enabled?switchOn:switchOff;
            if(!clip)return false;
            previous=enabled;source.clip=clip;source.loop=false;source.pitch=1;source.Play();return true;
        }
    }
}
