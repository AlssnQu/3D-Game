using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace LoQueCallaste.Hud.Editor
{
    public static class HudSetup
    {
        public const string PrefabPath = "Assets/MarcoHUD/Prefabs/PlayerHUD.prefab";
        public const string ScenePath = "Assets/MarcoHUD/Scenes/HudReview.unity";
        private static readonly Color White = new Color(.83f,.89f,.87f);

        private static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=anchor;rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=position;rect.sizeDelta=size;return rect;
        }
        private static Image Image(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size,Color color)
        {
            var rect=Rect(name,parent,anchor,position,size);var img=rect.gameObject.AddComponent<Image>();img.color=color;img.raycastTarget=false;return img;
        }
        private static Text Label(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size,string content,int fontSize)
        {
            var rect=Rect(name,parent,anchor,position,size);var t=rect.gameObject.AddComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=fontSize;t.color=White;t.text=content;t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;
            t.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(1,-1);return t;
        }
        [MenuItem("Marco/HUD/Crear prefab del jugador")]
        public static void CreatePrefab()
        {
            Directory.CreateDirectory("Assets/MarcoHUD/Prefabs");AssetDatabase.Refresh();
            var root=new GameObject("PlayerHUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(CanvasGroup),typeof(FlashlightHud));
            var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=30;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var group=root.GetComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
            var cross=Rect("Retícula",root.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(18,18));
            Image("Contorno",cross,new Vector2(.5f,.5f),Vector2.zero,new Vector2(7,7),new Color(0,0,0,.7f));
            Image("Punto central",cross,new Vector2(.5f,.5f),Vector2.zero,new Vector2(3,3),White);
            var safe=Rect("Área segura",root.transform,Vector2.zero,Vector2.zero,Vector2.zero);safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;safe.gameObject.AddComponent<HudSafeArea>();
            var panel=Image("Panel batería",safe,Vector2.zero,new Vector2(193,70),new Vector2(310,78),new Color(.025f,.04f,.05f,.82f));
            Image("Cuerpo batería",panel.transform,new Vector2(.5f,.5f),new Vector2(-111,0),new Vector2(30,42),White);
            Image("Interior batería",panel.transform,new Vector2(.5f,.5f),new Vector2(-111,0),new Vector2(24,36),new Color(.025f,.04f,.05f));
            Image("Terminal",panel.transform,new Vector2(.5f,.5f),new Vector2(-111,24),new Vector2(12,5),White);
            var fill=Image("Carga",panel.transform,new Vector2(.5f,.5f),new Vector2(-111,-15),new Vector2(18,30),White);
            fill.rectTransform.pivot=new Vector2(.5f,0);fill.fillAmount=0;
            var label=Label("Porcentaje",panel.transform,new Vector2(.5f,.5f),new Vector2(26,10),new Vector2(220,42),"BATERÍA  —",22);
            var status=Label("Estado de batería",panel.transform,new Vector2(.5f,.5f),new Vector2(26,-20),new Vector2(220,20),"",14);
            root.GetComponent<FlashlightHud>().Configure(fill,label,group,status);
            PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);Object.DestroyImmediate(root);AssetDatabase.SaveAssets();
        }
        [MenuItem("Marco/HUD/Crear escena de revisión")]
        public static void CreateReviewScene()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            CreatePrefab();Directory.CreateDirectory("Assets/MarcoHUD/Scenes");AssetDatabase.Refresh();
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            var camera=Camera.main;camera.transform.position=new Vector3(0,1.65f,-5);camera.transform.LookAt(new Vector3(0,1,3));camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.04f,.055f);
            RenderSettings.skybox=null;RenderSettings.ambientLight=new Color(.25f,.29f,.31f);
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.17f,.25f,.24f);
            string materialPath="Assets/MarcoHUD/Prefabs/ReviewRoom.mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(materialPath);if(existing){Object.DestroyImmediate(material);material=existing;}else AssetDatabase.CreateAsset(material,materialPath);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Suelo de prueba";floor.transform.position=new Vector3(0,-.1f,2);floor.transform.localScale=new Vector3(12,.2f,15);floor.GetComponent<Renderer>().sharedMaterial=material;
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Pared de prueba";wall.transform.position=new Vector3(0,2,6);wall.transform.localScale=new Vector3(12,4,.2f);wall.GetComponent<Renderer>().sharedMaterial=material;
            var crate=GameObject.CreatePrimitive(PrimitiveType.Cube);crate.name="Pupitre de referencia";crate.transform.position=new Vector3(0,.6f,2);crate.transform.localScale=new Vector3(1.5f,1.2f,1);crate.GetComponent<Renderer>().sharedMaterial=material;
            var lamp=new GameObject("Linterna de demostración").AddComponent<Light>();lamp.transform.SetParent(camera.transform,false);lamp.type=LightType.Spot;lamp.range=18;lamp.spotAngle=58;lamp.intensity=4;
            var hudObject=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));var hud=hudObject.GetComponent<FlashlightHud>();
            var demo=new GameObject("Controles de revisión del HUD").AddComponent<DemoBatteryReadout>();demo.hud=hud;demo.testLight=lamp;
            Label("Título de revisión",hudObject.transform,new Vector2(0,1),new Vector2(350,-56),new Vector2(600,50),"LO QUE CALLASTE · REVISIÓN DEL HUD",26);
            Label("Controles de revisión",hudObject.transform,new Vector2(1,0),new Vector2(-350,80),new Vector2(620,80),"1: 100%   2: 50%   3: 15%   4: 0%\nF: linterna   R: descarga gradual   H: ocultar HUD",20);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath);AssetDatabase.SaveAssets();
            Debug.Log("MARCO_HUD_SCENE_BUILT");
        }
        public static void Verify()
        {
            CreateReviewScene();
            var hud=Object.FindFirstObjectByType<FlashlightHud>();
            int checks=0;
            Action<bool,string> check=(condition,label)=>{if(!condition)throw new Exception(label);checks++;};
            check(hud && hud.GetComponent<Canvas>().renderMode==RenderMode.ScreenSpaceOverlay,"Canvas superpuesto");
            check(!hud.HasReading && hud.PercentageLabel.Contains("—"),"Sin fuente no inventa el porcentaje");
            hud.SetCharge01(.42f);check(hud.DisplayedCharge01==.42f && hud.PercentageLabel.Contains("42%"),"Porcentaje normalizado");
            hud.SetChargePercent(15);check(hud.PercentageLabel.Contains("15%"),"Entrada en porcentaje");
            hud.SetCharge01(-1);check(hud.DisplayedCharge01==0,"Límite inferior");
            hud.SetChargePercent(200);check(hud.DisplayedCharge01==1,"Límite superior");
            hud.SetCharge01(float.NaN);check(hud.DisplayedCharge01==0,"NaN manejado");
            hud.SetVisible(false);check(!hud.IsVisible,"Se puede ocultar para recuerdos");
            hud.SetVisible(true);check(hud.IsVisible && !hud.GetComponent<CanvasGroup>().blocksRaycasts,"HUD no bloquea interacción");
            var demo=Object.FindFirstObjectByType<DemoBatteryReadout>();demo.SetCharge01(.75f);check(hud.BindBattery(demo) && hud.PercentageLabel.Contains("75%"),"Contrato de batería enlazado");
            hud.UnbindBattery();check(!hud.HasReading,"Desconexión segura");
            string folder=Path.GetFullPath("../output/integracion-marco/verification");Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"issue-11.txt"),"PASS · Unity "+Application.unityVersion+" · "+checks+" comprobaciones de HUD\n");
            Debug.Log("MARCO_ISSUE11_PASS: "+checks);
        }

        public static void Deliver()
        {
            CreateReviewScene();
            string folder=Path.GetFullPath("../output/integracion-marco");
            Directory.CreateDirectory(Path.Combine(folder,"previews"));
            var hud=Object.FindFirstObjectByType<FlashlightHud>();
            var canvas=hud.GetComponent<Canvas>();var camera=Camera.main;
            var rt=new RenderTexture(1600,900,24);
            var previous=RenderTexture.active;
            try
            {
                camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;
                canvas.worldCamera=camera;canvas.planeDistance=1;
                foreach(int charge in new[]{100,50,15,0})
                {
                    hud.SetChargePercent(charge);Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
                    var image=new Texture2D(1600,900,TextureFormat.RGB24,false);
                    image.ReadPixels(new UnityEngine.Rect(0,0,1600,900),0,0);image.Apply();
                    File.WriteAllBytes(Path.Combine(folder,"previews","hud-"+charge+".png"),image.EncodeToPNG());
                    Object.DestroyImmediate(image);
                }
            }
            finally
            {
                camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(rt);
                canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;hud.UnbindBattery();
            }
            AssetDatabase.ExportPackage("Assets/MarcoHUD",Path.Combine(folder,"Marco-HUD.unitypackage"),ExportPackageOptions.Recurse);
            string path=Path.Combine(folder,"HUD-Windows","LoQueCallaste-HUD.exe");Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build HUD fallido: "+report.summary.result);
            Debug.Log("MARCO_HUD_DELIVERY_OK");
        }
    }
}
