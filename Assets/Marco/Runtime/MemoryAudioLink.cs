using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

namespace LoQueCallaste
{
    [RequireComponent(typeof(PlayableDirector))]
    public sealed class MemoryAudioLink : MonoBehaviour
    {
        public SchoolAudioDirector mix;
        public bool enablePreviewKey;
        private PlayableDirector sequence;
        private bool active;

        private void OnEnable()
        {
            sequence = GetComponent<PlayableDirector>();
            sequence.played += Begin;
            sequence.stopped += End;
            if (sequence.state == PlayState.Playing) Begin(sequence);
        }

        private void OnDisable()
        {
            sequence.played -= Begin;
            sequence.stopped -= End;
            End(sequence);
        }

        private void Update()
        {
            if (!enablePreviewKey || Keyboard.current == null || !Keyboard.current.rKey.wasPressedThisFrame)
                return;
            if (active) sequence.Stop();
            else
            {
                sequence.time = 0;
                sequence.Play();
            }
        }

        private void Begin(PlayableDirector director)
        {
            active = true;
            if (mix) mix.BeginCinematic();
        }

        private void End(PlayableDirector director)
        {
            if (!active) return;
            active = false;
            if (mix) mix.EndCinematic();
        }
    }
}
