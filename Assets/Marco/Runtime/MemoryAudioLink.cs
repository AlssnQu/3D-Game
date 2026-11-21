using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using LoQueCallaste.Hud;

namespace LoQueCallaste
{
    [RequireComponent(typeof(PlayableDirector))]
    public sealed class MemoryAudioLink : MonoBehaviour
    {
        public SchoolAudioDirector mix;
        public bool enablePreviewKey;
        public CanvasGroup fade;
        public FlashlightHud hud;
        public Behaviour[] controlsToSuspend = System.Array.Empty<Behaviour>();
        [Min(.05f)] public float fadeSeconds = .45f;
        private PlayableDirector sequence;
        private bool active;
        private bool[] previousControls;
        private bool previousHudVisibility;
        private bool previousFocus;
        public bool IsPresenting => active || previousControls != null;

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
            if (active) sequence.Stop();
            End(sequence);
            if (fade) fade.alpha = 0;
            RestoreControls();
        }

        private void Update()
        {
            if (fade)
                fade.alpha = Mathf.MoveTowards(fade.alpha, active ? 1 : 0,
                    Time.unscaledDeltaTime / Mathf.Max(.05f, fadeSeconds));
            if (!active && (!fade || fade.alpha <= 0)) RestoreControls();

            if (!enablePreviewKey || Keyboard.current == null || !Keyboard.current.rKey.wasPressedThisFrame)
                return;
            if (active) sequence.Stop();
            else if (!IsPresenting)
            {
                sequence.time = 0;
                sequence.Play();
            }
        }

        private void Begin(PlayableDirector director)
        {
            if (active) return;
            // A new playback can arrive while the previous fade is still returning.
            // Keep the original control snapshot until the last presentation finishes.
            if (previousControls != null)
            {
                active = true;
                if (mix) mix.BeginCinematic();
                return;
            }
            active = true;
            previousControls = new bool[controlsToSuspend.Length];
            for (int i = 0; i < controlsToSuspend.Length; i++)
            {
                if (!controlsToSuspend[i]) continue;
                previousControls[i] = controlsToSuspend[i].enabled;
                controlsToSuspend[i].enabled = false;
            }
            previousHudVisibility = hud && hud.IsVisible;
            if (hud) hud.SetVisible(false);
            previousFocus = mix && mix.CinematicFocus;
            if (mix) mix.BeginCinematic();
        }

        private void End(PlayableDirector director)
        {
            if (!active) return;
            active = false;
            if (mix) mix.SetCinematicFocus(previousFocus);
        }

        private void RestoreControls()
        {
            if (previousControls == null) return;
            for (int i = 0; i < previousControls.Length && i < controlsToSuspend.Length; i++)
                if (controlsToSuspend[i]) controlsToSuspend[i].enabled = previousControls[i];
            previousControls = null;
            if (hud) hud.SetVisible(previousHudVisibility);
        }
    }
}
