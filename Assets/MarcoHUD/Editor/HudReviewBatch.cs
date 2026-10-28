using UnityEditor;
using UnityEngine;

namespace LoQueCallaste.Hud.Editor
{
    [InitializeOnLoad]
    public static class HudReviewBatch
    {
        static HudReviewBatch()
        {
            EditorApplication.playModeStateChanged+=state=>
            {
                if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("Marco.HudReview",false))
                {SessionState.SetBool("Marco.HudReview",false);new GameObject("Comprobaciones del HUD").AddComponent<HudRuntimeChecks>();}
            };
        }
        public static void Run()
        {
            HudSetup.CreateReviewScene();SessionState.SetBool("Marco.HudReview",true);EditorApplication.EnterPlaymode();
        }
    }
}
