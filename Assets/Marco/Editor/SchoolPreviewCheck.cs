using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using LoQueCallaste.Hud;
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
        private static MemoryAudioLink memory;
        private static TeamBatteryReadout battery;
        private static bool[] controlStates;
        private static float originalCharge;

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
                        memory = sequence.GetComponent<MemoryAudioLink>();
                        battery = Object.FindFirstObjectByType<TeamBatteryReadout>();
                        Require(battery && battery.hud.HasReading, "HUD conectado a batería real");
                        originalCharge = battery.flashlight.bateria;
                        battery.flashlight.bateria = 12;
                        battery.hud.SetCharge01(battery.Charge01);
                        Require(battery.hud.StatusLabel == "BATERÍA BAJA", "Carga real baja muestra advertencia");
                        battery.flashlight.Recargar(100);
                        battery.hud.SetCharge01(battery.Charge01);
                        Require(battery.hud.DisplayedCharge01 == 1, "Recarga del equipo se refleja en HUD");
                        battery.flashlight.bateria = originalCharge;
                        controlStates = memory.controlsToSuspend.Select(control => control && control.enabled).ToArray();
                        var clips = player.footsteps.woodClips;
                        player.footsteps.woodClips = new[] { clips[0], null, clips[0] };
                        for (int i = 0; i < 20; i++)
                            Require(player.footsteps.PlayStep(SchoolSurface.Wood), "Pasos con paleta incompleta " + i);
                        player.footsteps.woodClips = clips;
                        sequence.Play();
                        deadline = EditorApplication.timeSinceStartup + 1.3;
                        break;
                    case 1:
                        Require(memory.fade.alpha > .99f && !battery.hud.IsVisible, "Fundido a negro y HUD oculto");
                        Require(memory.controlsToSuspend.All(control => !control || !control.enabled), "Controles suspendidos durante recuerdo");
                        Require(mix.CinematicFocus && mix.FocusBlend > .9f && mix.textures.All(item => item.Suppressed), "Recuerdo reduce ambiente y suspende murmullos");
                        deadline = EditorApplication.timeSinceStartup + 6;
                        break;
                    case 2:
                        Require(!mix.CinematicFocus && mix.FocusBlend < .05f && mix.textures.All(item => !item.Suppressed), "Timeline termina y devuelve la mezcla");
                        Require(memory.fade.alpha == 0 && battery.hud.IsVisible, "Imagen y HUD vuelven al terminar");
                        Require(memory.controlsToSuspend.Select(control => control && control.enabled).SequenceEqual(controlStates), "Controles recuperan estados previos");
                        sequence.time = 0;
                        sequence.Play();
                        deadline = EditorApplication.timeSinceStartup + .3;
                        break;
                    case 3:
                        Require(mix.CinematicFocus, "Recuerdo se puede repetir");
                        sequence.Stop();
                        deadline = EditorApplication.timeSinceStartup + 1.3;
                        break;
                    case 4:
                        Require(memory.fade.alpha == 0 && battery.hud.IsVisible, "Interrupción restaura imagen y HUD");
                        memory.controlsToSuspend[0].enabled = false;
                        sequence.time = 0;
                        sequence.Play();
                        deadline = EditorApplication.timeSinceStartup + .6;
                        break;
                    case 5:
                        sequence.Stop();
                        sequence.time = 0;
                        sequence.Play();
                        Require(memory.IsPresenting && mix.CinematicFocus, "Reinicio durante el fundido conserva la presentación");
                        deadline = EditorApplication.timeSinceStartup + .6;
                        break;
                    case 6:
                        Require(!battery.hud.IsVisible && memory.controlsToSuspend.All(control => !control || !control.enabled),
                            "Reinicio mantiene HUD oculto y controles bloqueados");
                        memory.enabled = false;
                        Require(!memory.IsPresenting && memory.fade.alpha == 0, "Desactivar el enlace limpia la presentación");
                        Require(!memory.controlsToSuspend[0].enabled, "Control previamente desactivado conserva su estado");
                        Require(battery.hud.IsVisible, "Desactivar tras reinicio devuelve el HUD original");
                        memory.controlsToSuspend[0].enabled = controlStates[0];
                        deadline = EditorApplication.timeSinceStartup + 1.3;
                        break;
                    case 7:
                        Require(!mix.CinematicFocus && mix.FocusBlend < .05f, "Interrumpir el recuerdo devuelve la mezcla");
                        foreach (var texture in mix.textures) texture.SetSuppressed(true);
                        var audition = Object.FindFirstObjectByType<SchoolAudioAudition>();
                        Require(audition, "Modo de escucha presente en el colegio");
                        foreach (var clip in audition.breath.clips.Concat(audition.cloth.clips).Concat(audition.phone.clips))
                        {
                            clip.LoadAudioData();
                            var samples = new float[clip.samples * clip.channels];
                            Require(clip.GetData(samples, 0), "Lectura de muestras originales " + clip.name);
                            Require(samples.Max(value => Mathf.Abs(value)) < .27f,
                                "Importación conserva margen de pico " + clip.name);
                            Require(Mathf.Abs(samples[0]) < .001f && Mathf.Abs(samples[samples.Length - 1]) < .001f,
                                "Entrada y salida sin discontinuidad " + clip.name);
                        }
                        audition.breath.SetSuppressed(false);
                        deadline = EditorApplication.timeSinceStartup + 2.3;
                        break;
                    case 8:
                        var breathReview = Object.FindFirstObjectByType<SchoolAudioAudition>();
                        Require(breathReview.AuditionBreath(), "Respiración original reproducible en 3D");
                        var breathSource = breathReview.breath.GetComponent<AudioSource>();
                        Require(breathSource.isPlaying && breathSource.spatialBlend == 1, "Respiración usa posición espacial");
                        Require(breathSource.clip.channels == 1 && breathSource.clip.frequency == 48000,
                            "Respiración mono a 48 kHz");
                        mix.SetEffects(0);
                        Require(breathSource.volume == 0, "Aislamiento de efectos silencia criatura");
                        breathReview.RestoreMix();
                        Require(breathSource.volume > 0, "Restaurar mezcla recupera criatura");
                        mix.BeginCinematic();
                        Require(!breathReview.AuditionCloth() && !breathSource.isPlaying,
                            "Recuerdo impide audiciones y detiene respiración");
                        mix.EndCinematic();
                        breathReview.cloth.SetSuppressed(false);
                        deadline = EditorApplication.timeSinceStartup + 2.3;
                        break;
                    case 9:
                        var clothReview = Object.FindFirstObjectByType<SchoolAudioAudition>();
                        Require(clothReview.AuditionCloth(), "Roce de tela original reproducible");
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Marco/Prefabs/CreatureAudio.prefab");
                        var emitters = prefab.GetComponentsInChildren<SpatialSoundEmitter>();
                        Require(emitters.Length == 2 && emitters.All(item => !item.automatic && !item.listener),
                            "Prefab de criatura preparado para eventos y sin vínculos a la escena");
                        Require(emitters.All(item => item.transform.localPosition == Vector3.zero),
                            "Emisores centrados para seguir el modelo de criatura");
                        clothReview.phone.SetSuppressed(false);
                        deadline = EditorApplication.timeSinceStartup + 2.6;
                        break;
                    default:
                        var phoneReview = Object.FindFirstObjectByType<SchoolAudioAudition>();
                        Require(phoneReview.AuditionPhone(), "Señal original de llamada reproducible");
                        Require(phoneReview.phone.GetComponent<AudioSource>().spatialBlend == 0,
                            "Señal telefónica subjetiva en 2D");
                        phoneReview.RestoreMix();
                        foreach (var texture in mix.textures) texture.SetSuppressed(false);
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
