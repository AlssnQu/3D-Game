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
            var sceneMix=FindFirstObjectByType<SchoolAudioDirector>();
            Check(sceneMix && sceneMix.channels.Length==sources.Length,"Todas las fuentes del laboratorio tienen categoría de mezcla");
            Check(sources.Length>=3,"Emisores de ambiente y pasos presentes");
            foreach(var source in sources)
            {
                if(source==sceneMix.transitionSource)
                {Check(source.spatialBlend==0 && source.reverbZoneMix==0,"Motivo del recuerdo 2D separado del espacio físico");continue;}
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
            Check(emitters.Length==5,"Cinco texturas localizadas, incluidos murmullos abstractos");
            foreach(var emitter in emitters)
            {
                Check(emitter.clips.Length>0 && Array.TrueForAll(emitter.clips,c=>c && c.channels==1),"Paleta licenciada mono asignada: "+emitter.name);
                emitter.SetSuppressed(true);
            }
            var testEmitter=new GameObject("Emisor de revisión").AddComponent<SpatialSoundEmitter>();
            testEmitter.automatic=false;testEmitter.listener=walker.view.transform;testEmitter.transform.position=walker.view.transform.position+Vector3.right;
            var variedEmitter=Array.Find(emitters,e=>e.clips.Length>1);
            testEmitter.clips=new[]{variedEmitter.clips[0],variedEmitter.clips[1]};
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
            var alternative=Array.Find(testEmitter.clips,c=>c!=firstClip);
            testEmitter.clips=new[]{firstClip,null,firstClip,alternative};
            Check(testEmitter.TryPlay() && testSource.clip!=firstClip,"Evita repetición incluso con referencias nulas y muestras duplicadas");
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
            var feet=walker.surfaceFootsteps;
            Check(feet && feet.woodClips.Length==4 && Array.TrueForAll(feet.woodClips,c=>c && c.channels==1),"Paletas diferenciadas para baldosa y madera");
            Check(!feet.Advance(.5f,false,SchoolSurface.Tile),"No hay pasos al estar en el aire");
            Check(!feet.Advance(0,true,SchoolSurface.Tile),"No hay pasos sin desplazamiento real");
            Check(feet.PlayStep(SchoolSurface.Tile) && feet.GetComponent<AudioSource>().isPlaying,"Paso de baldosa reproducido");
            var tile=feet.GetComponent<AudioSource>().clip;
            Check(feet.PlayStep(SchoolSurface.Wood) && feet.GetComponent<AudioSource>().clip!=tile && feet.LastSurface==SchoolSurface.Wood,"Cambio de superficie cambia la grabación");
            var woodClip=feet.GetComponent<AudioSource>().clip;feet.PlayStep(SchoolSurface.Wood);
            Check(feet.GetComponent<AudioSource>().clip!=woodClip,"Los pasos de madera no repiten muestra inmediata");
            Check(walker.switchAudio && walker.switchAudio.PlayState(false),"Apagar linterna reproduce el interruptor grabado");
            Check(!walker.switchAudio.PlayState(false),"Un estado repetido no duplica el clic");
            Check(walker.switchAudio.PlayState(true),"Encender linterna usa su grabación correspondiente");

            var mixObject=new GameObject("Revisión de mezcla");var mix=mixObject.AddComponent<SchoolAudioDirector>();mix.transitionSeconds=.1f;
            var ambience=new GameObject("Bus ambiente").AddComponent<AudioSource>();
            var effect=new GameObject("Bus efectos").AddComponent<AudioSource>();
            var dialogue=new GameObject("Bus voz").AddComponent<AudioSource>();
            mix.channels=new[]{new SchoolAudioDirector.Channel{source=ambience,bus=SchoolAudioDirector.Bus.Ambience,authoredVolume=.4f},new SchoolAudioDirector.Channel{source=effect,bus=SchoolAudioDirector.Bus.Effects,authoredVolume=.8f},new SchoolAudioDirector.Channel{source=dialogue,bus=SchoolAudioDirector.Bus.Dialogue,authoredVolume=.5f}};
            mix.textures=new[]{testEmitter};testEmitter.SetSuppressed(false);
            mix.SetMaster(.5f);
            Check(Mathf.Approximately(ambience.volume,.2f) && Mathf.Approximately(dialogue.volume,.25f),"Volumen general proporcional por categorías");
            mix.SetEffects(.25f);
            Check(Mathf.Approximately(effect.volume,.1f) && Mathf.Approximately(dialogue.volume,.25f),"Efectos independientes de las voces");
            mix.SetReducedImpact(true);
            Check(Mathf.Approximately(effect.volume,.06f),"Modo de impactos suaves reduce efectos");
            mix.BeginCinematic();mix.BeginCinematic();
            Check(testEmitter.Suppressed,"Inicio de recuerdo suprime texturas sin doble captura de estado");
            yield return new WaitForSecondsRealtime(.2f);
            Check(ambience.volume<.05f && Mathf.Approximately(dialogue.volume,.25f),"El fundido deja espacio a la voz del recuerdo");
            mix.EndCinematic();
            Check(!testEmitter.Suppressed,"Al terminar se restaura el estado anterior de la textura");
            yield return new WaitForSecondsRealtime(.2f);
            Check(Mathf.Approximately(ambience.volume,.2f),"El ambiente vuelve gradualmente a su nivel anterior");
            testEmitter.SetSuppressed(true);mix.BeginCinematic();mix.EndCinematic();
            Check(testEmitter.Suppressed,"Se conserva una textura que ya estaba suprimida");
            mix.SetMaster(float.NaN);Check(effect.volume==0 && ambience.volume==0 && dialogue.volume==0,"Ganancia inválida produce silencio controlado");
            mix.enabled=false;
            Check(Mathf.Approximately(ambience.volume,.4f) && Mathf.Approximately(effect.volume,.8f),"Desactivar el director restaura ganancias de autoría");
            Destroy(mixObject);Destroy(ambience.gameObject);Destroy(effect.gameObject);Destroy(dialogue.gameObject);
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
