using UnityEngine;
using UnityEngine.InputSystem;

namespace LoQueCallaste
{
    /// <summary>Review overlay for the lab only. Production menus can call the director API directly.</summary>
    public sealed class AudioReviewControls : MonoBehaviour
    {
        public SchoolAudioDirector director;
        public DemoPlayerController walker;
        private bool visible;
        private bool previousWalker;
        private CursorLockMode previousLock;
        private bool previousCursor;
        private void Update()
        {
            if(Application.isBatchMode || Keyboard.current==null)return;
            if(Keyboard.current.mKey.wasPressedThisFrame)SetVisible(!visible);
            if(visible && Keyboard.current.escapeKey.wasPressedThisFrame)SetVisible(false);
        }
        private void SetVisible(bool value)
        {
            if(value==visible)return;
            visible=value;
            if(value)
            {
                previousWalker=walker && walker.enabled;
                previousLock=Cursor.lockState;previousCursor=Cursor.visible;
                if(walker)walker.enabled=false;
                Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
            }
            else
            {
                if(walker)walker.enabled=previousWalker;
                Cursor.lockState=previousLock;Cursor.visible=previousCursor;
            }
        }
        private void OnDisable() { if(visible)SetVisible(false); }
        private void OnGUI()
        {
            if(Application.isBatchMode || !director)return;
            if(!visible){GUI.Label(new Rect(20,20,420,28),"M · Ajustes de sonido");return;}
            float width=Mathf.Min(510,Screen.width-32);
            GUILayout.BeginArea(new Rect((Screen.width-width)/2,Mathf.Max(16,(Screen.height-350)/2),width,350),GUI.skin.box);
            GUILayout.Label("LO QUE CALLASTE · SONIDO");GUILayout.Space(10);
            director.SetMaster(Slider("Volumen general",director.Master));
            director.SetAmbience(Slider("Ambiente del colegio",director.Ambience));
            director.SetEffects(Slider("Pasos y efectos",director.Effects));
            director.SetDialogue(Slider("Voces",director.Dialogue));
            director.SetReducedImpact(GUILayout.Toggle(director.ReducedImpact,"Impactos más suaves"));
            bool focus=GUILayout.Toggle(director.CinematicFocus,"Probar mezcla durante un recuerdo");
            director.SetCinematicFocus(focus);
            GUILayout.Label("Esta opción prueba la mezcla; no inicia una cinemática.");
            if(GUILayout.Button("Volver al recorrido · M / Esc"))SetVisible(false);
            GUILayout.EndArea();
        }
        private float Slider(string label,float value)
        {
            GUILayout.BeginHorizontal();GUILayout.Label(label+"  "+Mathf.RoundToInt(value*100)+"%",GUILayout.Width(225));
            float result=GUILayout.HorizontalSlider(value,0,1);GUILayout.EndHorizontal();return result;
        }
    }
}
