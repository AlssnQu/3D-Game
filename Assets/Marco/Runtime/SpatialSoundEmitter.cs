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
        private AudioClip previous;
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
            int available = 0;
            foreach(var clip in clips)if(clip && clip!=previous)available++;
            bool allowPrevious=available==0;
            if(allowPrevious)foreach(var clip in clips)if(clip)available++;
            if(available==0)return false;
            int choice=Random.Range(0,available);AudioClip selected=null;
            foreach(var clip in clips)
                if(clip && (allowPrevious || clip!=previous) && choice--==0){selected=clip;break;}
            if(!selected)return false;
            previous = selected;
            source.clip = selected; source.loop = false;
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
