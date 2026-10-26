#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LoQueCallaste
{
    public sealed class SpatialAudioRuntimeChecks : MonoBehaviour
    {
        [Serializable] private class Report { public string unity; public string status; public string[] passed; public string failure; }
        private readonly List<string> passed=new List<string>();
        private void Check(bool condition,string name){if(!condition)throw new Exception(name);passed.Add(name);}
        private IEnumerator Start()
        {
            var tests=Tests();
            while(true){bool next;try{next=tests.MoveNext();}catch(Exception e){Finish(e.ToString());yield break;}if(!next)break;yield return tests.Current;}
            Finish(null);
        }
        private IEnumerator Tests()
        {
            yield return null;
            Check(FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length==1,"Un único AudioListener activo");
            var zones=FindObjectsByType<AudioReverbZone>(FindObjectsSortMode.None);
            Check(zones.Length==2,"Dos zonas de reverberación");
            bool hall=false,bath=false;
            foreach(var zone in zones){hall|=zone.reverbPreset==AudioReverbPreset.Hallway;bath|=zone.reverbPreset==AudioReverbPreset.Bathroom;Check(zone.maxDistance>zone.minDistance,"Radios válidos: "+zone.name);}
            Check(hall && bath,"Presets distintos para pasillo y baño");
            var sources=FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            Check(sources.Length>=3,"Emisores de ambiente y pasos presentes");
            foreach(var source in sources)
            {
                Check(source.spatialBlend==1 && source.rolloffMode==AudioRolloffMode.Logarithmic,"Distancia y posición 3D: "+source.name);
                Check(source.reverbZoneMix>0 && source.dopplerLevel==0,"Envío a reverb y Doppler: "+source.name);
                if(source.loop)Check(source.clip && source.clip.channels==1 && source.isPlaying,"Ambiente mono reproduciéndose: "+source.name);
            }
            var walker=FindFirstObjectByType<DemoPlayerController>();
            Check(walker && walker.stepClips.Length==4 && Array.TrueForAll(walker.stepClips,c=>c && c.channels==1),"Cuatro pasos mono asignados");
            var probe=new GameObject("Verificación de perfiles").AddComponent<AudioSource>();
            var profile=ScriptableObject.CreateInstance<SpatialAudioProfile>();profile.minDistance=2;profile.maxDistance=1;profile.volume=.3f;profile.Apply(probe);
            Check(probe.maxDistance>probe.minDistance && probe.volume==.3f,"El perfil corrige límites de distancia");
            var zoneProbe=new GameObject("Verificación de radios").AddComponent<SchoolReverbArea>();zoneProbe.fullEffectRadius=4;zoneProbe.outerRadius=1;zoneProbe.Apply();
            Check(zoneProbe.GetComponent<AudioReverbZone>().maxDistance>4,"La zona corrige un radio exterior inválido");
            double dsp=AudioSettings.dspTime;
            yield return new WaitForSecondsRealtime(.15f);
            Check(AudioSettings.dspTime>dsp,"El reloj de audio avanza en modo Play");
            var emitters=FindObjectsByType<SpatialSoundEmitter>(FindObjectsSortMode.None);
            Check(emitters.Length==4,"Cuatro texturas localizadas de casillero, aula y baño");
            foreach(var emitter in emitters)
            {
                Check(emitter.clips.Length>0 && Array.TrueForAll(emitter.clips,c=>c && c.channels==1),"Paleta licenciada mono asignada: "+emitter.name);
                emitter.SetSuppressed(true);
            }
            var testEmitter=new GameObject("Emisor de revisión").AddComponent<SpatialSoundEmitter>();
            testEmitter.automatic=false;testEmitter.listener=walker.view.transform;testEmitter.transform.position=walker.view.transform.position+Vector3.right;
            testEmitter.clips=new[]{emitters[0].clips[0],emitters[0].clips[1]};
            var testSource=testEmitter.GetComponent<AudioSource>();PositionalAudio.Configure(testSource);
            Check(testEmitter.TryPlay() && testSource.isPlaying,"Reproducción manual desde el emisor espacial");
            var firstClip=testSource.clip;
            Check(!testEmitter.TryPlay(),"No acumula voces sobre el mismo emisor");
            testEmitter.SetSuppressed(true);
            Check(!testSource.isPlaying && !testEmitter.TryPlay(),"La cinemática puede suprimir y detener la textura");
            testEmitter.SetSuppressed(false);
            Check(!testEmitter.TryPlay(),"El intervalo global evita ráfagas entre emisores");
            yield return new WaitForSecondsRealtime(2.1f);
            testEmitter.transform.position=walker.view.transform.position+Vector3.right*100;
            Check(!testEmitter.TryPlay(),"No reproduce fuera del radio audible");
            testEmitter.transform.position=walker.view.transform.position+Vector3.right;
            Check(testEmitter.TryPlay() && testSource.clip!=firstClip,"Evita repetición inmediata de la muestra");
            testEmitter.SetSuppressed(true);

            var acousticSource=new GameObject("Prueba de oclusión",typeof(AudioSource));acousticSource.transform.position=new Vector3(100,3,100);
            var filter=acousticSource.AddComponent<AudioLowPassFilter>();filter.cutoffFrequency=22000;
            var occlusion=acousticSource.AddComponent<SimpleAudioOcclusion>();
            var target=new GameObject("Destino acústico");target.transform.position=new Vector3(100,3,110);occlusion.listener=target.transform;
            Physics.SyncTransforms();occlusion.RefreshOcclusion();
            Check(!occlusion.IsOccluded,"Visión acústica libre sin pared");
            var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.transform.position=new Vector3(100,3,105);blocker.transform.localScale=new Vector3(4,4,1);
            Physics.SyncTransforms();occlusion.RefreshOcclusion();
            Check(occlusion.IsOccluded,"La pared interpuesta activa el filtrado");
            yield return new WaitForSecondsRealtime(.8f);
            Check(filter.cutoffFrequency<3000,"El sonido tras la pared pierde agudos gradualmente");
            blocker.GetComponent<Collider>().isTrigger=true;Physics.SyncTransforms();occlusion.RefreshOcclusion();
            Check(!occlusion.IsOccluded,"Los triggers de gameplay no actúan como paredes");
            occlusion.enabled=false;
            Check(filter.cutoffFrequency==22000,"Desactivar oclusión restaura el filtro original");
            profile.minDistance=float.NaN;profile.maxDistance=float.PositiveInfinity;profile.volume=float.NaN;profile.Apply(probe);
            Check(probe.minDistance>0 && probe.maxDistance>probe.minDistance && probe.volume==0,"Los perfiles rechazan parámetros no finitos");
            Destroy(testEmitter.gameObject);Destroy(acousticSource);Destroy(target);Destroy(blocker);
            Destroy(probe.gameObject);Destroy(profile);Destroy(zoneProbe.gameObject);
        }
        private void Finish(string failure)
        {
            string path=Path.GetFullPath("../output/integracion-marco/verification/audio-runtime.json");Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path,JsonUtility.ToJson(new Report{unity=Application.unityVersion,status=failure==null?"PASS":"FAIL",passed=passed.ToArray(),failure=failure},true));
            if(failure!=null)Debug.LogError(failure);else Debug.Log("MARCO_AUDIO_RUNTIME_PASS: "+passed.Count);
            UnityEditor.EditorApplication.Exit(failure==null?0:1);
        }
    }
}
#endif
