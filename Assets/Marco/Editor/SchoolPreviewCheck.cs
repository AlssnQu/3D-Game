using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace LoQueCallaste.Editor
{
    [InitializeOnLoad]
    public static class SchoolPreviewCheck
    {
        private const string Pending = "Marco.SchoolPreviewCheck";
        private static double deadline;
        private static int phase;
        private static SchoolAudioDirector mix;
        private static PlayableDirector sequence;
        private static PlayerAudioLink player;

        static SchoolPreviewCheck()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
                SessionState.EraseBool(Pending);
                deadline = EditorApplication.timeSinceStartup + 2;
                EditorApplication.update += Check;
            };
        }

        public static void Run()
        {
            SchoolAudioPreview.Build();
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }

        private static void Check()
        {
            try
            {
                if (EditorApplication.timeSinceStartup < deadline) return;
                switch (phase++)
                {
                    case 0:
                        player = Object.FindFirstObjectByType<PlayerAudioLink>();
                        mix = Object.FindFirstObjectByType<SchoolAudioDirector>();
                        sequence = Object.FindFirstObjectByType<PlayableDirector>();
                        Require(player && player.controller && player.flashlight, "Jugador real y linterna asignados");
                        Debug.Log($"MARCO_PLAYER: {player.transform.position}, suelo={player.controller.isGrounded}");
                        var below = player.controller.Move(Vector3.down * .2f);
                        Require((below & CollisionFlags.Below) != 0, "El piso del colegio sostiene al jugador");
                        Require(player.footsteps.PlayStep(SchoolSurface.Wood), "Paso de madera reproducible");
                        Require(player.switchAudio.PlayState(true) && player.switchAudio.PlayState(false), "Clics distintos disponibles");
                        Require(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(item => item.enabled) == 1, "Un oyente activo");
                        Require(Object.FindObjectsByType<AudioReverbZone>(FindObjectsSortMode.None).Length >= 7, "Reverberación distribuida por el pasillo y baño");
                        Require(Object.FindObjectsByType<JugadorMovimiento>(FindObjectsSortMode.None).Length == 1, "Un solo controlador del equipo");
                        Require(mix.channels.Length == Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length, "Todas las fuentes están incluidas en la mezcla");
                        Require(mix.channels.Where(channel => channel.source.loop).All(channel => channel.source.isPlaying), "Ambientes activos");
                        sequence.Play();
                        deadline = EditorApplication.timeSinceStartup + 1.3;
                        break;
                    case 1:
                        Require(mix.CinematicFocus && mix.FocusBlend > .9f && mix.textures.All(item => item.Suppressed), "Recuerdo reduce ambiente y suspende murmullos");
                        deadline = EditorApplication.timeSinceStartup + 6;
                        break;
                    case 2:
                        Require(!mix.CinematicFocus && mix.FocusBlend < .05f && mix.textures.All(item => !item.Suppressed), "Timeline termina y devuelve la mezcla");
                        sequence.time = 0;
                        sequence.Play();
                        deadline = EditorApplication.timeSinceStartup + .3;
                        break;
                    case 3:
                        Require(mix.CinematicFocus, "Recuerdo se puede repetir");
                        sequence.Stop();
                        deadline = EditorApplication.timeSinceStartup + 1.3;
                        break;
                    default:
                        Require(!mix.CinematicFocus && mix.FocusBlend < .05f, "Interrumpir el recuerdo devuelve la mezcla");
                        Finish(0);
                        break;
                }
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                Finish(1);
            }
        }

        private static void Require(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException(name);
            Debug.Log("MARCO_SCHOOL_PASS: " + name);
        }

        private static void Finish(int result)
        {
            EditorApplication.update -= Check;
            EditorApplication.Exit(result);
        }
    }
}
