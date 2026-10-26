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
            int start=Random.Range(0,clips.Length),index=start;
            for(int i=0;i<clips.Length;i++)
            {
                index=(start+i)%clips.Length;
                if(clips[index] && clips[index]!=previous)break;
            }
            if(!clips[index])return false;
            previous=clips[index];LastSurface=surface;
            source.pitch=Random.Range(.97f,1.03f);source.clip=previous;source.loop=false;source.Play();
            return true;
        }
    }
}
