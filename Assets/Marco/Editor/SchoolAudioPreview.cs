using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace LoQueCallaste.Editor
{
    public static class SchoolAudioPreview
    {
        private const string MapPath = "Assets/Scenes/Example.unity";
        private const string PreviewPath = "Assets/Marco/Scenes/SchoolAudioPreview.unity";
        private const string TimelinePath = "Assets/Marco/Scenes/SchoolMemoryPreview.playable";

        [MenuItem("Marco/Audio/Preparar prueba en el colegio")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var map = EditorSceneManager.OpenScene(MapPath);
            var geometry = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            var hall = geometry.Single(item => item.name == "hall ceiling (2)");
            var bathroom = geometry.Single(item => item.name == "japanase school handwashing Station and bathroom");

            var player = Object.FindObjectsByType<JugadorMovimiento>(FindObjectsSortMode.None).Single().gameObject;
            var playerCamera = player.GetComponentInChildren<Camera>();
            var playerListener = playerCamera.GetComponent<AudioListener>();
            if (!playerListener) playerListener = playerCamera.gameObject.AddComponent<AudioListener>();
            playerListener.enabled = true;
            foreach (var listener in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
                listener.enabled = listener == playerListener;
            foreach (var floor in geometry.Where(item => item.name.StartsWith("wood floor", StringComparison.OrdinalIgnoreCase)))
                if (!floor.TryGetComponent<FootstepSurface>(out _))
                    floor.gameObject.AddComponent<FootstepSurface>().surface = SchoolSurface.Wood;

            Physics.SyncTransforms();
            var ceiling = hall.GetComponentInChildren<Renderer>().bounds;
            var belowCeiling = new Vector3(ceiling.center.x, ceiling.min.y - .25f, ceiling.center.z);
            if (!Physics.Raycast(belowCeiling, Vector3.down,
                    out var floorHit, 12f, ~0, QueryTriggerInteraction.Ignore))
                throw new InvalidOperationException("No se encontró piso desde dentro del pasillo.");
            Vector3 hallPosition = floorHit.point + Vector3.up * 1.5f;
            Vector3 bathPosition = new Vector3(bathroom.position.x, hallPosition.y, bathroom.position.z);

            var controller = player.GetComponent<CharacterController>();
            var flashlight = player.GetComponentInChildren<Linterna>().GetComponent<Light>();

            var root = new GameObject("Audio y recuerdos · Marco").transform;
            var stepSource = Source(player.transform, "Pasos", player.transform.position, .34f, 8);
            var steps = stepSource.gameObject.AddComponent<SurfaceFootsteps>();
            steps.tileClips = Steps("concrete");
            steps.woodClips = Steps("wood");
            var clickSource = Source(flashlight.transform, "Clic de linterna", flashlight.transform.position, .22f, 3);
            var click = clickSource.gameObject.AddComponent<FlashlightSwitchAudio>();
            click.switchOn = Clip("laleksic_switch_on");
            click.switchOff = Clip("laleksic_switch_off");
            var link = player.AddComponent<PlayerAudioLink>();
            link.controller = controller;
            link.flashlight = flashlight;
            link.footsteps = steps;
            link.switchAudio = click;

            var channels = new List<SchoolAudioDirector.Channel>();
            foreach (var section in geometry.Where(item => item.name.StartsWith("hall ceiling", StringComparison.Ordinal)))
            {
                var bounds = section.GetComponentInChildren<Renderer>().bounds;
                var centre = new Vector3(bounds.center.x, hallPosition.y, bounds.center.z);
                Reverb(root, "Eco · " + section.name, centre, AudioReverbPreset.Hallway, 2, 4);
            }
            Reverb(root, "Eco del baño", bathPosition, AudioReverbPreset.Bathroom, 1.5f, 4);
            var hallBed = Bed(root, "Aire del pasillo", hallPosition, "amb_corridor_procedural", .16f, 18);
            var bathBed = Bed(root, "Goteo del baño", bathPosition, "amb_bathroom_procedural", .19f, 6);
            var whisper = Source(root, "Murmullo distante", hallPosition + hall.forward * 4f, .20f, 10);
            var emitter = whisper.gameObject.AddComponent<SpatialSoundEmitter>();
            emitter.clips = new[] { Clip("sardin_whisper_texture_01"), Clip("sardin_whisper_texture_02") };
            emitter.audibleRadius = 10;
            emitter.interval = new Vector2(10, 18);
            whisper.gameObject.AddComponent<AudioLowPassFilter>().cutoffFrequency = 22000;
            whisper.gameObject.AddComponent<SimpleAudioOcclusion>().listener = playerListener.transform;
            emitter.listener = playerListener.transform;

            var textures = new List<SpatialSoundEmitter> { emitter };
            var locker = geometry.Where(item => item.name.Trim().StartsWith("locker", StringComparison.Ordinal))
                .OrderBy(item => Vector3.Distance(item.position, hallPosition)).First();
            var door = geometry.Where(item => item.name.StartsWith("slideing door", StringComparison.Ordinal))
                .OrderBy(item => Vector3.Distance(item.position, hallPosition)).First();
            var metal = Texture(root, "Golpe tenue de casillero", locker.position + Vector3.up,
                new[] { "kenney_impactMetal_light_000", "kenney_impactMetal_light_001", "laleksic_clang_metal" },
                .16f, 9, new Vector2(19, 33), playerListener.transform);
            var creak = Texture(root, "Bisagra de puerta", door.position + Vector3.up,
                new[] { "laleksic_door_creak_open", "laleksic_door_creak_close" },
                .14f, 10, new Vector2(27, 43), playerListener.transform);
            textures.Add(metal);
            textures.Add(creak);
            channels.Add(Channel(metal.GetComponent<AudioSource>(), SchoolAudioDirector.Bus.Effects));
            channels.Add(Channel(creak.GetComponent<AudioSource>(), SchoolAudioDirector.Bus.Effects));

            var transition = Source(root, "Motivo del recuerdo", root.position, .32f, 8);
            transition.spatialBlend = 0;
            transition.reverbZoneMix = 0;
            var mix = root.gameObject.AddComponent<SchoolAudioDirector>();
            mix.transitionSource = transition;
            mix.memoryIn = Clip("design_memory_in");
            mix.memoryOut = Clip("design_memory_out");
            mix.textures = textures.ToArray();
            mix.channels = new[] { Channel(hallBed, SchoolAudioDirector.Bus.Ambience),
                Channel(bathBed, SchoolAudioDirector.Bus.Ambience), Channel(whisper, SchoolAudioDirector.Bus.Ambience),
                Channel(stepSource, SchoolAudioDirector.Bus.Effects), Channel(clickSource, SchoolAudioDirector.Bus.Effects),
                Channel(transition, SchoolAudioDirector.Bus.Dialogue) };

            mix.channels = mix.channels.Concat(channels).ToArray();

            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (!timeline)
            {
                timeline = ScriptableObject.CreateInstance<TimelineAsset>();
                timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
                timeline.fixedDuration = 5;
                AssetDatabase.CreateAsset(timeline, TimelinePath);
                var track = timeline.CreateTrack<AudioTrack>(null, "Eco del recuerdo");
                var cue = track.CreateClip(Clip("laleksic_door_creak_close"));
                cue.start = 2;
                cue.duration = Math.Min(cue.duration, 2.5);
                AssetDatabase.SaveAssets();
            }
            var sequence = root.gameObject.AddComponent<PlayableDirector>();
            sequence.playOnAwake = false;
            sequence.extrapolationMode = DirectorWrapMode.None;
            sequence.playableAsset = timeline;
            var memoryEcho = Source(root, "Eco subjetivo del recuerdo", root.position, .18f, 8);
            memoryEcho.spatialBlend = 0;
            memoryEcho.reverbZoneMix = 0;
            sequence.SetGenericBinding(timeline.GetOutputTracks().OfType<AudioTrack>().Single(), memoryEcho);
            mix.channels = mix.channels.Append(Channel(memoryEcho, SchoolAudioDirector.Bus.Dialogue)).ToArray();
            var memory = root.gameObject.AddComponent<MemoryAudioLink>();
            memory.mix = mix;
            memory.enablePreviewKey = true;

            if (Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(item => item.enabled) != 1)
                throw new InvalidOperationException("La prueba requiere un solo oyente activo.");
            if (!EditorSceneManager.SaveScene(map, PreviewPath, saveAsCopy: true))
                throw new InvalidOperationException("No se guardó la escena de revisión.");
            Debug.Log($"MARCO_SCHOOL_OK: piso {floorHit.collider.name}, pasillo {hallPosition}, baño {bathPosition}");
            EditorSceneManager.OpenScene(PreviewPath);
        }

        private static SpatialSoundEmitter Texture(Transform parent, string name, Vector3 position,
            string[] clips, float gain, float range, Vector2 interval, Transform listener)
        {
            var source = Source(parent, name, position, gain, range);
            var texture = source.gameObject.AddComponent<SpatialSoundEmitter>();
            texture.clips = clips.Select(Clip).ToArray();
            texture.listener = listener;
            texture.audibleRadius = range;
            texture.interval = interval;
            source.gameObject.AddComponent<AudioLowPassFilter>().cutoffFrequency = 22000;
            source.gameObject.AddComponent<SimpleAudioOcclusion>().listener = listener;
            return texture;
        }

        private static AudioClip Clip(string name) => DemoSceneBuilder.CuratedClip(name);

        private static AudioClip[] Steps(string material) =>
            Enumerable.Range(0, 4).Select(i => Clip($"kenney_footstep_{material}_{i:000}")).ToArray();

        private static AudioSource Source(Transform parent, string name, Vector3 position, float gain, float range)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            var source = obj.AddComponent<AudioSource>();
            PositionalAudio.Configure(source, .5f, range);
            source.volume = gain;
            return source;
        }

        private static AudioSource Bed(Transform parent, string name, Vector3 position, string clip, float gain, float range)
        {
            var source = Source(parent, name, position, gain, range);
            source.clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Marco/Audio/{clip}.wav");
            if (!source.clip) throw new InvalidOperationException($"Falta {clip}.");
            source.loop = source.playOnAwake = true;
            return source;
        }

        private static SchoolAudioDirector.Channel Channel(AudioSource source, SchoolAudioDirector.Bus bus) =>
            new SchoolAudioDirector.Channel { source = source, bus = bus, authoredVolume = source.volume };

        private static void Reverb(Transform parent, string name, Vector3 position, AudioReverbPreset preset, float inner, float outer)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            var area = obj.AddComponent<SchoolReverbArea>();
            area.preset = preset;
            area.fullEffectRadius = inner;
            area.outerRadius = outer;
            area.Apply();
        }
    }
}
