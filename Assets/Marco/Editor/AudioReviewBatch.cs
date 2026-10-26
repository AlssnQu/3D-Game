using UnityEditor;
using UnityEngine;

namespace LoQueCallaste.Editor
{
    [InitializeOnLoad]
    public static class AudioReviewBatch
    {
        static AudioReviewBatch()
        {
            EditorApplication.playModeStateChanged+=state=>
            {
                if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("Marco.AudioReview",false))
                {SessionState.SetBool("Marco.AudioReview",false);new GameObject("Comprobaciones de audio").AddComponent<SpatialAudioRuntimeChecks>();}
            };
        }
        public static void Run()
        {
            DemoSceneBuilder.Build();SessionState.SetBool("Marco.AudioReview",true);EditorApplication.EnterPlaymode();
        }
    }
}
