using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace LoQueCallaste.Editor
{
    public static class DemoSceneBuilder
    {
        public const string ScenePath = "Assets/Marco/Scenes/AudioCinematicsLab.unity";
        private static Transform world;
        private static Material wall, lowerWall, floor, wood, metal, porcelain, black, paper, red, pale, screen;

        [MenuItem("Marco/Crear escena de audio y cinemáticas")]
        public static void Build()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory("Assets/Marco/Scenes");
            Directory.CreateDirectory("Assets/Marco/Materials");
            Directory.CreateDirectory("Assets/Marco/Cinematics");
            AssetDatabase.Refresh();
            ImportAudio();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            world = new GameObject("ESCUELA · Escenario de prueba").transform;
            wall = Mat("Yeso envejecido", new Color(0.39f, 0.42f, 0.40f));
            lowerWall = Mat("Pintura verde", new Color(0.12f, 0.23f, 0.20f));
            floor = Mat("Baldosas", new Color(0.29f, 0.31f, 0.29f));
            wood = Mat("Madera gastada", new Color(0.22f, 0.15f, 0.10f));
            metal = Mat("Casilleros", new Color(0.15f, 0.23f, 0.24f));
            porcelain = Mat("Porcelana", new Color(0.6f, 0.63f, 0.58f));
            black = Mat("Carcasa del teléfono", new Color(0.035f, 0.04f, 0.045f));
            paper = Mat("Papel", new Color(0.7f, 0.65f, 0.49f));
            red = Mat("Rojo de emergencia", new Color(0.48f, 0.035f, 0.025f));
            pale = Mat("Silueta provisional", new Color(0.38f, 0.39f, 0.34f));
            screen = Mat("Pantalla", new Color(0.21f, 0.43f, 0.4f));
            screen.EnableKeyword("_EMISSION"); screen.SetColor("_EmissionColor",new Color(.07f,.19f,.14f));
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.17f,.20f,.23f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.025f,.04f,.05f);
            RenderSettings.fogDensity = .025f;
            RenderSettings.skybox = null;

            Corridor(); Bathroom(); Classroom();
            var player = Player();
            var listener = player.view.GetComponent<AudioListener>();
            if (!listener) player.view.gameObject.AddComponent<AudioListener>();
            var ambience1 = Source("Ambiente del pasillo",new Vector3(0,2,15),"amb_corridor_procedural",.13f,true,8,28);
            var ambience2 = Source("Goteo y ambiente del baño",new Vector3(9,1.5f,14),"amb_bathroom_procedural",.12f,true,2,9);
            Reverb("Reverb · Pasillo",new Vector3(0,1.6f,20),AudioReverbPreset.Hallway,3,7);
            Reverb("Reverb · Baño",new Vector3(10,1.6f,14),AudioReverbPreset.Bathroom,2,5.5f);
            SpatialAudioSetup.CreateAssets();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("MARCO_SCENE_BUILT: "+ScenePath);
        }

        private static void ImportAudio()
        {
            foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Marco/Audio"}))
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                importer.forceToMono=true;
                var settings=importer.defaultSampleSettings;
                settings.loadType=AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat=AudioCompressionFormat.PCM;
                settings.sampleRateSetting=AudioSampleRateSetting.OverrideSampleRate;
                settings.sampleRateOverride=48000;
                importer.defaultSampleSettings=settings;importer.SaveAndReimport();
            }
        }
        private static Material Mat(string name,Color color)
        {
            string path="Assets/Marco/Materials/"+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            material.color=color;material.SetFloat("_Smoothness",.08f);
            return material;
        }
        private static GameObject Box(string name,Vector3 pos,Vector3 size,Material material,Transform parent=null,bool collider=true)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;
            obj.transform.position=pos;obj.transform.localScale=size;obj.transform.SetParent(parent?parent:world,true);
            obj.GetComponent<Renderer>().sharedMaterial=material;
            if(!collider)Object.DestroyImmediate(obj.GetComponent<Collider>());
            return obj;
        }
        private static void Wall(string name,Vector3 pos,Vector3 size)
        {
            Box(name,pos,size,wall);
            var lower=pos;lower.y=.65f;var s=size;s.y=1.3f;s.x+=.015f;s.z+=.015f;
            Box(name+" · zócalo",lower,s,lowerWall);
        }
        private static void LightAt(string name,Vector3 pos,Color color,float intensity,float range)
        {
            var obj=new GameObject(name);obj.transform.SetParent(world);obj.transform.position=pos;
            var light=obj.AddComponent<Light>();light.type=LightType.Point;light.color=color;light.intensity=intensity;light.range=range;light.shadows=LightShadows.Soft;
            var flicker=obj.AddComponent<AmbientMotion>();flicker.bulb=light;flicker.intensity=intensity;
        }
        private static void Sign(string text,Vector3 pos,Quaternion rotation,float width=2.2f)
        {
            var board=Box("Cartel · "+text,pos,new Vector3(width,.45f,.06f),black);
            board.transform.rotation=rotation;
            var obj=new GameObject("Texto · "+text);obj.transform.SetParent(board.transform,false);obj.transform.localPosition=new Vector3(0,0,-.55f);
            // Counteract the sign's scale: text is authored in world-sized units.
            obj.transform.localScale=new Vector3(1/width,1/.45f,1/.06f);
            var tm=obj.AddComponent<TextMesh>();tm.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");tm.text=text;tm.fontSize=60;tm.characterSize=.055f;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.color=new Color(.77f,.83f,.73f);
            string textMaterialPath="Assets/Marco/Materials/Texto con profundidad.mat";
            var textMaterial=AssetDatabase.LoadAssetAtPath<Material>(textMaterialPath);
            if(!textMaterial){textMaterial=new Material(Shader.Find("Marco/WorldText"));AssetDatabase.CreateAsset(textMaterial,textMaterialPath);}
            textMaterial.mainTexture=tm.font.material.mainTexture;obj.GetComponent<MeshRenderer>().sharedMaterial=textMaterial;
        }
        private static void Corridor()
        {
            Box("Suelo del pasillo",new Vector3(0,-.12f,17),new Vector3(5.6f,.24f,36),floor);
            for(int z=0;z<35;z++)for(int x=-2;x<=2;x++)
                Box("Baldosa",new Vector3(x,.005f,z),new Vector3(.97f,.015f,.97f),((z+x)%2==0)?floor:porcelain,null,false);
            Wall("Pared izquierda entrada",new Vector3(-2.7f,1.7f,10),new Vector3(.3f,3.4f,22));
            Wall("Pared izquierda fondo",new Vector3(-2.7f,1.7f,30),new Vector3(.3f,3.4f,10));
            Wall("Pared derecha entrada",new Vector3(2.7f,1.7f,5),new Vector3(.3f,3.4f,12));
            Wall("Pared derecha fondo",new Vector3(2.7f,1.7f,25),new Vector3(.3f,3.4f,18));
            Wall("Cierre entrada",new Vector3(0,1.7f,-1),new Vector3(5.5f,3.4f,.3f));
            Wall("Cierre fondo",new Vector3(0,1.7f,35),new Vector3(5.5f,3.4f,.3f));
            Box("Techo del pasillo",new Vector3(0,3.5f,17),new Vector3(5.6f,.2f,36),wall);
            for(int i=0;i<6;i++)
            {
                float z=7+i*.62f;Box("Casillero",new Vector3(-2.28f,1.05f,z),new Vector3(.6f,2,.57f),metal);
                Box("Tirador",new Vector3(-1.95f,1.15f,z+.15f),new Vector3(.06f,.18f,.04f),black,null,false);
                for(int j=0;j<3;j++)Box("Ranura",new Vector3(-1.968f,1.65f+j*.075f,z),new Vector3(.012f,.025f,.30f),black,null,false);
            }
            for(int i=0;i<4;i++)
            {
                float z=4+i*8;Box("Luminaria",new Vector3(0,3.32f,z),new Vector3(1.4f,.08f,.25f),porcelain,null,false);
                LightAt("Fluorescente",new Vector3(0,2.9f,z),new Color(.65f,.8f,.78f),1.35f,9);
            }
            Sign("PABELLÓN B",new Vector3(0,2.7f,4.5f),Quaternion.identity,2.5f);
            Sign("BAÑOS →",new Vector3(0,2.7f,12),Quaternion.identity,2.0f);
            Sign("AULA 03",new Vector3(-2.48f,2.65f,23),Quaternion.Euler(0,90,0),2f);
            Sign("AZOTEA",new Vector3(0,2.5f,34.7f),Quaternion.identity,2f);
            LightAt("Emergencia roja",new Vector3(0,2.8f,32),new Color(1,.08f,.035f),2f,8);
            Box("Puerta de salida",new Vector3(0,1.2f,34.7f),new Vector3(1.7f,2.4f,.12f),red);
            // Deliberately static silhouette; creature AI and the licensed model belong to other tasks.
            Box("Silueta · torso",new Vector3(.2f,1.3f,30),new Vector3(.42f,.75f,.24f),pale);
            Box("Silueta · cabeza",new Vector3(.2f,1.87f,30),new Vector3(.25f,.32f,.24f),pale);
            Box("Silueta · pierna izquierda",new Vector3(.05f,.48f,30),new Vector3(.14f,.95f,.18f),pale);
            Box("Silueta · pierna derecha",new Vector3(.35f,.48f,30),new Vector3(.14f,.95f,.18f),pale);
        }
        private static void Bathroom()
        {
            Box("Suelo baño",new Vector3(8,-.12f,14),new Vector3(10.6f,.24f,8),porcelain);
            Box("Techo baño",new Vector3(8,3.5f,14),new Vector3(10.6f,.2f,8),wall);
            Wall("Baño norte",new Vector3(8,1.7f,18),new Vector3(10.6f,3.4f,.3f));
            Wall("Baño sur",new Vector3(8,1.7f,10),new Vector3(10.6f,3.4f,.3f));
            Wall("Baño fondo",new Vector3(13.4f,1.7f,14),new Vector3(.3f,3.4f,8));
            Box("Encimera del teléfono",new Vector3(10,1.03f,14),new Vector3(2.6f,.16f,1.2f),porcelain);
            Box("Soporte",new Vector3(10,.5f,14),new Vector3(.3f,1,.7f),metal);
            for(int i=0;i<2;i++)
            {
                float z=11.5f+i*4.7f;Box("Lavabo",new Vector3(12.5f,.94f,z),new Vector3(.9f,.25f,.8f),porcelain);
                Box("Grifo",new Vector3(12.8f,1.16f,z),new Vector3(.12f,.35f,.12f),metal);
                Box("Espejo opaco",new Vector3(13.2f,1.8f,z),new Vector3(.07f,1.05f,.8f),metal);
            }
            Box("Cabina",new Vector3(6.5f,1.1f,17),new Vector3(1.7f,2.2f,.12f),metal);
            Sign("NO ESTABAS SOLO",new Vector3(10,2.3f,17.8f),Quaternion.identity,3.2f);
            LightAt("Luz del baño",new Vector3(9,2.9f,14),new Color(.48f,.65f,.74f),2.0f,10);
        }
        private static void Classroom()
        {
            Box("Suelo aula",new Vector3(-7.7f,-.12f,24),new Vector3(10,.24f,10),floor);
            Wall("Aula fondo",new Vector3(-12.7f,1.7f,24),new Vector3(.3f,3.4f,10));
            Wall("Aula norte",new Vector3(-7.7f,1.7f,29),new Vector3(10,3.4f,.3f));
            Wall("Aula sur",new Vector3(-7.7f,1.7f,19),new Vector3(10,3.4f,.3f));
            Box("Techo aula",new Vector3(-7.7f,3.5f,24),new Vector3(10,.2f,10),wall);
            for(int r=0;r<3;r++)for(int c=0;c<3;c++)
            {
                Vector3 p=new Vector3(-5-c*2,.8f,21+r*2.4f);
                Box("Pupitre",p,new Vector3(1.15f,.1f,.75f),wood);
                Box("Pata",p+new Vector3(-.4f,-.4f,0),new Vector3(.07f,.8f,.5f),metal);
                Box("Pata",p+new Vector3(.4f,-.4f,0),new Vector3(.07f,.8f,.5f),metal);
                Box("Asiento",p+new Vector3(0,-.32f,.65f),new Vector3(.65f,.1f,.6f),wood);
                Box("Respaldo",p+new Vector3(0,.04f,.95f),new Vector3(.65f,.7f,.08f),wood);
            }
            Box("Pizarra",new Vector3(-7.5f,1.9f,28.8f),new Vector3(5,1.2f,.1f),lowerWall);
            Sign("¿POR QUÉ CALLASTE?",new Vector3(-7.5f,2,28.68f),Quaternion.identity,4f);
            LightAt("Luz aula",new Vector3(-7.5f,2.9f,24),new Color(.5f,.6f,.72f),1.5f,10);
        }
        private static DemoPlayerController Player()
        {
            var obj=new GameObject("Jugador de prueba");obj.transform.position=new Vector3(0,.05f,2);
            obj.AddComponent<AudioPlayer>();var cc=obj.AddComponent<CharacterController>();cc.height=1.75f;cc.radius=.27f;cc.center=new Vector3(0,.88f,0);
            var controller=obj.AddComponent<DemoPlayerController>();
            var cameraObj=new GameObject("Main Camera");cameraObj.tag="MainCamera";cameraObj.transform.SetParent(obj.transform,false);cameraObj.transform.localPosition=new Vector3(0,1.6f,0);
            var camera=cameraObj.AddComponent<Camera>();camera.nearClipPlane=.06f;camera.farClipPlane=70;camera.fieldOfView=72;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=RenderSettings.fogColor;cameraObj.AddComponent<AudioListener>();controller.view=camera;
            var light=new GameObject("Linterna").AddComponent<Light>();light.transform.SetParent(cameraObj.transform,false);light.transform.localPosition=new Vector3(.18f,-.15f,.14f);light.type=LightType.Spot;light.spotAngle=61;light.innerSpotAngle=32;light.range=23;light.intensity=3.2f;light.color=new Color(1f,.89f,.69f);light.shadows=LightShadows.Soft;controller.flashlight=light;
            var step=new GameObject("Pasos").AddComponent<AudioSource>();step.transform.SetParent(obj.transform,false);step.transform.localPosition=new Vector3(0,.2f,0);PositionalAudio.Configure(step,.5f,8);controller.footsteps=step;controller.stepClips=Enumerable.Range(0,4).Select(i=>Clip("step_"+i.ToString("00")+"_procedural")).ToArray();
            return controller;
        }
        private static AudioClip Clip(string name) { return AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Marco/Audio/"+name+".wav"); }
        private static AudioSource Source(string name,Vector3 pos,string clip,float volume,bool loop,float min,float max)
        {
            var obj=new GameObject(name);obj.transform.SetParent(world);obj.transform.position=pos;var source=obj.AddComponent<AudioSource>();PositionalAudio.Configure(source,min,max);source.volume=volume;source.loop=loop;source.clip=clip==null?null:Clip(clip);source.playOnAwake=loop;return source;
        }
        private static void Reverb(string name,Vector3 pos,AudioReverbPreset preset,float min,float max)
        {
            var obj=new GameObject(name);obj.transform.SetParent(world);obj.transform.position=pos;var zone=obj.AddComponent<AudioReverbZone>();zone.reverbPreset=preset;zone.minDistance=min;zone.maxDistance=max;
        }
        public static void BuildWindows()
        {
            if(!File.Exists(ScenePath))Build();
            string path=Path.GetFullPath("../output/integracion-marco/Windows/LoQueCallaste-AudioDemo.exe");Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build fallido: "+report.summary.result);
            Debug.Log("MARCO_BUILD_OK: "+path);
        }
        public static void ExportPackage()
        {
            AssetDatabase.ExportPackage("Assets/Marco",Path.GetFullPath("../output/integracion-marco/Marco-Audio-Cinematicas.unitypackage"),ExportPackageOptions.Recurse);
        }

        public static void Deliver()
        {
            Build();
            RenderPreviews();
            ExportPackage();
            BuildWindows();
        }

        public static void RenderPreviews()
        {
            var camera=Object.FindFirstObjectByType<DemoPlayerController>().view;
            Vector3 originalPosition=camera.transform.position;Quaternion originalRotation=camera.transform.rotation;
            string folder=Path.GetFullPath("../output/integracion-marco/previews");Directory.CreateDirectory(folder);
            var positions=new[]{new Vector3(0,1.65f,5.5f),new Vector3(7.5f,1.65f,13.1f),new Vector3(-3.6f,1.65f,23)};
            var targets=new[]{new Vector3(0,1.4f,22),new Vector3(10,1.16f,14),new Vector3(-8,1.4f,25)};
            var names=new[]{"01-pasillo.png","02-bano.png","03-aula.png"};
            RenderTexture previous=RenderTexture.active;
            for(int i=0;i<positions.Length;i++)
            {
                camera.transform.position=positions[i];camera.transform.LookAt(targets[i]);
                var rt=new RenderTexture(1600,1000,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
                var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,names[i]),image.EncodeToPNG());
                camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);
            }
            camera.transform.position=originalPosition;camera.transform.rotation=originalRotation;
            Debug.Log("MARCO_PREVIEWS_OK: "+folder);
        }
    }
}
