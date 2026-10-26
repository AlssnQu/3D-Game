using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LoQueCallaste.Editor
{
    public static class SpatialAudioSetup
    {
        [MenuItem("Marco/Audio/Crear perfiles y prefabs de reverberación")]
        public static void CreateAssets()
        {
            Directory.CreateDirectory("Assets/Marco/Profiles");
            Directory.CreateDirectory("Assets/Marco/Prefabs");
            AssetDatabase.Refresh();
            Profile("Efectos espaciales",1.5f,15,.4f,.8f);
            Profile("Pasos del jugador",.5f,8,.45f,.8f);
            Zone("ReverbPasillo",AudioReverbPreset.Hallway,3,7);
            Zone("ReverbBano",AudioReverbPreset.Bathroom,2,5.5f);
            AssetDatabase.SaveAssets();
        }

        private static void Profile(string name,float min,float max,float volume,float reverb)
        {
            string path="Assets/Marco/Profiles/"+name+".asset";
            var profile=AssetDatabase.LoadAssetAtPath<SpatialAudioProfile>(path);
            if(!profile){profile=ScriptableObject.CreateInstance<SpatialAudioProfile>();AssetDatabase.CreateAsset(profile,path);}
            profile.minDistance=min;profile.maxDistance=max;profile.volume=volume;profile.reverbMix=reverb;
            EditorUtility.SetDirty(profile);
        }

        private static void Zone(string name,AudioReverbPreset preset,float min,float max)
        {
            var go=new GameObject(name);
            var area=go.AddComponent<SchoolReverbArea>();
            area.preset=preset;area.fullEffectRadius=min;area.outerRadius=max;area.Apply();
            PrefabUtility.SaveAsPrefabAsset(go,"Assets/Marco/Prefabs/"+name+".prefab");
            UnityEngine.Object.DestroyImmediate(go);
        }

        public static void VerifySpatialScene()
        {
            DemoSceneBuilder.Build();
            var listeners=UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            var zones=UnityEngine.Object.FindObjectsByType<AudioReverbZone>(FindObjectsSortMode.None);
            if(listeners.Length!=1 || zones.Length!=2)throw new Exception("El laboratorio necesita un oyente y dos reverberaciones.");
            foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                if(source.spatialBlend!=1 || source.reverbZoneMix<=0 || source.minDistance<=0 || source.maxDistance<=source.minDistance)
                    throw new Exception("Fuente 3D mal configurada: "+source.name);
            var profile=AssetDatabase.LoadAssetAtPath<SpatialAudioProfile>("Assets/Marco/Profiles/Efectos espaciales.asset");
            var probe=new GameObject("Audio verification").AddComponent<AudioSource>();
            profile.Apply(probe);
            if(probe.spatialBlend!=1 || probe.volume!=profile.volume)throw new Exception("El perfil no configura el emisor.");
            UnityEngine.Object.DestroyImmediate(probe.gameObject);
            string folder=Path.GetFullPath("../output/integracion-marco/verification");Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"issue-17.txt"),"PASS · Unity "+Application.unityVersion+"\nAudioListener único; dos zonas; emisores 3D válidos; perfil aplicado.\n");
            Debug.Log("MARCO_ISSUE17_PASS");
        }
    }
}
