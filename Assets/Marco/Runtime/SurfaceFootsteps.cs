using UnityEngine;

namespace LoQueCallaste
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class SurfaceFootsteps : MonoBehaviour
    {
        public AudioClip[] tileClips;
        public AudioClip[] woodClips;
        [Min(.2f)] public float metresPerStep=1.1f;
        public SchoolSurface LastSurface { get; private set; }
        private AudioSource source;
        private float distance;
        private AudioClip previous;
        private void Awake() { source=GetComponent<AudioSource>(); }
        private void OnDisable() { distance=0;if(source)source.Stop(); }
        public bool Advance(float metres,bool grounded,SchoolSurface surface)
        {
            if(!isActiveAndEnabled || !grounded || float.IsNaN(metres) || float.IsInfinity(metres) || metres<=.001f)
            { distance=0;return false; }
            // Teleports must not generate a burst of footsteps.
            distance+=Mathf.Min(metres,.5f);
            if(distance<Mathf.Max(.2f,metresPerStep))return false;
            distance%=Mathf.Max(.2f,metresPerStep);
            return PlayStep(surface);
        }
        // May also be called by the real controller's animation events.
        public bool PlayStep(SchoolSurface surface)
        {
            if(!isActiveAndEnabled || !source || Time.timeScale<=0)return false;
            var clips=surface==SchoolSurface.Wood?woodClips:tileClips;
            if(clips==null || clips.Length==0)clips=tileClips;
            if(clips==null || clips.Length==0)return false;
            int alternatives = 0;
            foreach (var clip in clips)
                if (clip && clip != previous) alternatives++;
            bool repeat = alternatives == 0;
            if (repeat)
                foreach (var clip in clips)
                    if (clip) alternatives++;
            if (alternatives == 0) return false;
            int choice = Random.Range(0, alternatives);
            foreach (var clip in clips)
            {
                if (!clip || (!repeat && clip == previous)) continue;
                if (choice-- != 0) continue;
                previous = clip;
                break;
            }
            LastSurface = surface;
            source.pitch=Random.Range(.97f,1.03f);source.clip=previous;source.loop=false;source.Play();
            return true;
        }
    }
}
