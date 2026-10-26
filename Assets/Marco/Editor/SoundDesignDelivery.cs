using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LoQueCallaste.Editor
{
    public static class SoundDesignDelivery
    {
        private const int Rate=48000;
        [Serializable] private class ClipInfo { public string file; public float seconds; public int channels; public int sampleRate; public float peak; }
        [Serializable] private class PaletteReport { public ClipInfo[] clips; public float previewPeak; public string note; }
        public static void Deliver()
        {
            PrepareKenney();
            DemoSceneBuilder.Build();
            CreatePreview();
            DemoSceneBuilder.ExportPackage();
            DemoSceneBuilder.BuildWindows();
        }
        private static void PrepareKenney()
        {
            // The downloaded archive remains intact outside Assets. Keep only edited mono WAVs in the game.
            foreach(string path in Directory.GetFiles("Assets/Marco/Audio/Curated","kenney_*.ogg"))
            {
                var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);var data=Read(clip);
                if(clip.channels!=1 || clip.frequency!=Rate)throw new Exception("Importar primero como mono PCM 48kHz: "+path);
                double total=0;foreach(float value in data)total+=value;
                float dc=(float)(total/data.Length),peak=0;
                for(int i=0;i<data.Length;i++){data[i]-=dc;peak=Mathf.Max(peak,Mathf.Abs(data[i]));}
                float gain=.55f/Mathf.Max(.001f,peak);int fade=Mathf.Min(384,data.Length/4);
                using(var writer=new BinaryWriter(File.Create(Path.ChangeExtension(path,".wav"))))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+data.Length*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                    writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(Rate);writer.Write(Rate*2);writer.Write((short)2);writer.Write((short)16);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(data.Length*2);
                    for(int i=0;i<data.Length;i++)
                    {
                        float envelope=Mathf.Min(Mathf.Clamp01(i/(float)fade),Mathf.Clamp01((data.Length-1-i)/(float)fade));
                        writer.Write((short)(Mathf.Clamp(data[i]*gain*envelope,-1,1)*32767));
                    }
                }
                AssetDatabase.DeleteAsset(path);
            }
            AssetDatabase.Refresh();
        }
        public static void CreatePreview()
        {
            var infos=new List<ClipInfo>();
            foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Marco/Audio/Curated"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                var samples=Read(clip);float peak=0;
                foreach(float sample in samples){if(float.IsNaN(sample)||float.IsInfinity(sample))throw new Exception("Muestra inválida: "+path);peak=Mathf.Max(peak,Mathf.Abs(sample));}
                if(clip.channels!=1 || clip.frequency!=Rate || peak>=.9999f)throw new Exception("Revisar formato o clipping: "+path+" peak="+peak);
                infos.Add(new ClipInfo{file=path,seconds=clip.length,channels=clip.channels,sampleRate=clip.frequency,peak=peak});
            }
            var mix=new float[Rate*24*2];
            var hum=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Marco/Audio/amb_corridor_procedural.wav");
            for(int i=0;i<2;i++)Add(mix,hum,i*12,.13f,0);
            for(int i=0;i<6;i++)Add(mix,DemoSceneBuilder.CuratedClip("kenney_footstep_concrete_"+(i%4).ToString("000")),1.5f+i*.6f,.38f,0);
            Add(mix,DemoSceneBuilder.CuratedClip("kenney_impactMetal_light_001"),6,.3f,.8f);
            Add(mix,DemoSceneBuilder.CuratedClip("laleksic_floor_creak2"),9.5f,.42f,-.65f);
            Add(mix,DemoSceneBuilder.CuratedClip("laleksic_tap_water"),13,.2f,.55f);
            Add(mix,DemoSceneBuilder.CuratedClip("laleksic_door_creak_open"),18,.4f,-.3f);
            float maximum=0;foreach(float value in mix)maximum=Mathf.Max(maximum,Mathf.Abs(value));
            // Preserve headroom; never boost a quiet mix just to reach a target peak.
            float gain=maximum>.7f?.7f/maximum:1;
            string folder=Path.GetFullPath("../output/integracion-marco");Directory.CreateDirectory(folder);
            using(var writer=new BinaryWriter(File.Create(Path.Combine(folder,"Escucha-Lo-Que-Callaste.wav"))))
            {
                int bytes=mix.Length*2;
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+bytes);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16);writer.Write((short)1);writer.Write((short)2);writer.Write(Rate);writer.Write(Rate*4);writer.Write((short)4);writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(bytes);
                for(int i=0;i<mix.Length;i++)
                {
                    float time=(i/2)/(float)Rate;
                    float fade=Mathf.Min(Mathf.Clamp01(time/.5f),Mathf.Clamp01((24-time)/1.5f));
                    writer.Write((short)(Mathf.Clamp(mix[i]*gain*fade,-1,1)*32767));
                }
            }
            File.WriteAllText(Path.Combine(folder,"verification","paleta-audio.json"),JsonUtility.ToJson(new PaletteReport{clips=infos.ToArray(),previewPeak=maximum*gain,note="Maqueta estéreo de diseño; no es una captura de audio del motor. Pausas y panorámica ilustrativas. Requiere escucha y aprobación del equipo."},true));
            Debug.Log("MARCO_SOUND_PALETTE_PASS: "+infos.Count);
        }
        private static float[] Read(AudioClip clip)
        {
            if(!clip)throw new Exception("Clip ausente");
            var data=new float[clip.samples*clip.channels];
            if(!clip.GetData(data,0))throw new Exception("No se pudo leer: "+clip.name);
            return data;
        }
        private static void Add(float[] mix,AudioClip clip,float at,float gain,float pan)
        {
            var data=Read(clip);int offset=Mathf.RoundToInt(at*Rate);
            float left=Mathf.Sqrt((1-pan)*.5f)*gain,right=Mathf.Sqrt((1+pan)*.5f)*gain;
            for(int i=0;i<clip.samples && (offset+i)*2+1<mix.Length;i++)
            {
                float sample=data[i*clip.channels];int p=(offset+i)*2;
                mix[p]+=sample*left;mix[p+1]+=sample*right;
                // Small diffuse tail for this presentation mix, independent of Unity's reverb.
                int delay=p+Rate/4*2;
                if(delay+1<mix.Length){mix[delay]+=sample*right*.12f;mix[delay+1]+=sample*left*.12f;}
            }
        }
    }
}
