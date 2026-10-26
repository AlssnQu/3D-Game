using UnityEngine;

namespace LoQueCallaste
{
    /// <summary>Localized environmental texture, independent of story triggers and creature AI.</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class SpatialSoundEmitter : MonoBehaviour
    {
        public AudioClip[] clips;
        public Transform listener;
        [Min(1)] public float audibleRadius = 16;
        public Vector2 interval = new Vector2(9, 17);
        public Vector2 pitchRange = new Vector2(.94f, 1.04f);
        public bool automatic = true;
        public bool Suppressed { get; private set; }
        private AudioSource source;
        private int previous = -1;
        private float nextAt;
        private static float globalNextAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSharedClock() { globalNextAt = 0; }
        private void Awake() { source = GetComponent<AudioSource>(); }
        private void OnEnable() { Schedule(); }
        private void OnDisable() { if (source) source.Stop(); }
        private void Schedule() { nextAt = Time.time + Random.Range(Mathf.Max(1, interval.x), Mathf.Max(1, Mathf.Max(interval.x, interval.y))); }
        private void ResolveListener()
        {
            if (listener) return;
            var found = FindFirstObjectByType<AudioListener>();
            if (found && found.isActiveAndEnabled) listener = found.transform;
        }
        private void Update()
        {
            ResolveListener();
            if (source && source.isPlaying && listener && Vector3.Distance(transform.position, listener.position) > audibleRadius) source.Stop();
            if (automatic && Time.timeScale > 0 && !Suppressed && Time.time >= nextAt)
            { TryPlay(); Schedule(); }
        }
        public bool TryPlay()
        {
            ResolveListener();
            if (!isActiveAndEnabled || Suppressed || Time.timeScale <= 0 || !source || source.isPlaying || !listener || Time.time < globalNextAt || clips == null || clips.Length == 0) return false;
            if (Vector3.Distance(transform.position, listener.position) > Mathf.Max(1, audibleRadius)) return false;
            int index = previous >= 0 && clips.Length > 1 ? (previous + Random.Range(1, clips.Length)) % clips.Length : Random.Range(0, clips.Length);
            for (int i = 0; i < clips.Length && !clips[index]; i++) index = (index + 1) % clips.Length;
            if (!clips[index]) return false;
            previous = index;
            source.clip = clips[index]; source.loop = false;
            source.pitch = Random.Range(Mathf.Clamp(pitchRange.x, .5f, 2), Mathf.Clamp(Mathf.Max(pitchRange.x, pitchRange.y), .5f, 2));
            source.Play(); globalNextAt = Time.time + 2;
            return true;
        }
        // Can be called from a Timeline signal or the team's cinematic controller.
        public void SetSuppressed(bool value)
        {
            Suppressed = value;
            if (value && source) source.Stop();
            Schedule();
        }
    }
}
