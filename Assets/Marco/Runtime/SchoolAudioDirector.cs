using System;
using UnityEngine;

namespace LoQueCallaste
{
    /// <summary>Scene-local gain routing. Does not change AudioListener.volume or global project settings.</summary>
    public sealed class SchoolAudioDirector : MonoBehaviour
    {
        public enum Bus { Ambience, Effects, Dialogue }
        [Serializable] public sealed class Channel
        {
            public AudioSource source;
            public Bus bus;
            [Range(0,1)] public float authoredVolume = 1;
        }
        public Channel[] channels = Array.Empty<Channel>();
        public SpatialSoundEmitter[] textures = Array.Empty<SpatialSoundEmitter>();
        public AudioSource transitionSource;
        public AudioClip memoryIn, memoryOut;
        [SerializeField, Range(0,1)] private float master = .85f;
        [SerializeField, Range(0,1)] private float ambience = 1;
        [SerializeField, Range(0,1)] private float effects = 1;
        [SerializeField, Range(0,1)] private float dialogue = 1;
        [SerializeField] private bool reducedImpact;
        [Min(.05f)] public float transitionSeconds = .8f;
        private float focus;
        private bool cinematic;
        private bool[] previousSuppression;
        public float Master => master;
        public float Ambience => ambience;
        public float Effects => effects;
        public float Dialogue => dialogue;
        public bool ReducedImpact => reducedImpact;
        public bool CinematicFocus => cinematic;
        public float FocusBlend => focus;

        private static float Gain(float value) => float.IsNaN(value)||float.IsInfinity(value)?0:Mathf.Clamp01(value);
        private void OnEnable() { Apply(); }
        private void Update()
        {
            focus = Mathf.MoveTowards(focus, cinematic ? 1 : 0, Time.unscaledDeltaTime / Mathf.Max(.05f, transitionSeconds));
            Apply();
        }
        public void SetMaster(float value) { master=Gain(value);Apply(); }
        public void SetAmbience(float value) { ambience=Gain(value);Apply(); }
        public void SetEffects(float value) { effects=Gain(value);Apply(); }
        public void SetDialogue(float value) { dialogue=Gain(value);Apply(); }
        public void SetReducedImpact(bool value) { reducedImpact=value;Apply(); }
        public void BeginCinematic() { SetCinematicFocus(true); }
        public void EndCinematic() { SetCinematicFocus(false); }
        public void SetCinematicFocus(bool value)
        {
            if(value==cinematic)return;
            cinematic=value;
            if(transitionSource && (value?memoryIn:memoryOut))
            { transitionSource.clip=value?memoryIn:memoryOut;transitionSource.loop=false;transitionSource.Play(); }
            if(value)
            {
                previousSuppression=new bool[textures.Length];
                for(int i=0;i<textures.Length;i++)if(textures[i])
                { previousSuppression[i]=textures[i].Suppressed;textures[i].SetSuppressed(true); }
            }
            else RestoreTextures();
        }
        private void RestoreTextures()
        {
            for(int i=0;previousSuppression!=null && i<textures.Length && i<previousSuppression.Length;i++)
                if(textures[i])textures[i].SetSuppressed(previousSuppression[i]);
            previousSuppression=null;
        }
        private void Apply()
        {
            if(!isActiveAndEnabled)return;
            foreach(var channel in channels)
            {
                if(channel==null || !channel.source)continue;
                float bus=channel.bus==Bus.Ambience?ambience:channel.bus==Bus.Dialogue?dialogue:effects;
                float duck=channel.bus==Bus.Ambience?Mathf.Lerp(1,.18f,focus):channel.bus==Bus.Effects?Mathf.Lerp(1,.45f,focus):1;
                float comfort=reducedImpact && channel.bus==Bus.Effects?.6f:1;
                channel.source.volume=Gain(channel.authoredVolume)*master*bus*duck*comfort;
            }
        }
        private void OnDisable()
        {
            RestoreTextures();cinematic=false;focus=0;
            if(transitionSource)transitionSource.Stop();
            foreach(var channel in channels)if(channel!=null && channel.source)channel.source.volume=Gain(channel.authoredVolume);
        }
    }
}
