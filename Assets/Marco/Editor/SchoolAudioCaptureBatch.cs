using System;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace LoQueCallaste.Editor
{
    [InitializeOnLoad]
    public static class SchoolAudioCaptureBatch
    {
        private const string Pending = "Marco.SchoolAudioCapture";
        private const int FramesPerSecond = 30;
        private const double Duration = 36;
        private static readonly double[] CueTimes = { 8, 11, 15, 22, 26 };
        private static BinaryWriter writer;
        private static SchoolAudioAudition audition;
        private static PlayerAudioLink player;
        private static PlayableDirector sequence;
        private static SpatialSoundEmitter whisper;
        private static Vector3 hall, bathroom;
        private static long sampleValues;
        private static int sampleRate, previousFrame, nextCue, oldCaptureRate, oldTargetRate;
        private static bool recording;
        private static float peak;
        private static double energy;
        private static UnityEngine.Random.State randomState;
        private static string output;

        static SchoolAudioCaptureBatch()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
                SessionState.EraseBool(Pending);
                try { Begin(); }
                catch (Exception error) { Finish(error); }
            };
        }

        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/Marco/Scenes/SchoolAudioPreview.unity");
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }

        private static void Begin()
        {
            oldCaptureRate = Time.captureFramerate;
            oldTargetRate = Application.targetFrameRate;
            randomState = UnityEngine.Random.state;
            if (AudioSettings.speakerMode != AudioSpeakerMode.Stereo)
                throw new InvalidOperationException("La captura requiere salida estéreo.");
            Time.captureFramerate = FramesPerSecond;
            Application.targetFrameRate = FramesPerSecond;
            UnityEngine.Random.InitState(3044);
            player = Object.FindFirstObjectByType<PlayerAudioLink>();
            audition = Object.FindFirstObjectByType<SchoolAudioAudition>();
            sequence = Object.FindFirstObjectByType<PlayableDirector>();
            player.GetComponent<JugadorMovimiento>().enabled = false;
            player.controller.enabled = false;
            player.enabled = false;
            sequence.GetComponent<MemoryAudioLink>().enablePreviewKey = false;
            foreach (var texture in audition.mix.textures) texture.automatic = false;
            whisper = audition.mix.textures.Single(item => item.name == "Murmullo distante");
            hall = Object.FindObjectsByType<SchoolReverbArea>(FindObjectsSortMode.None)
                .Single(area => area.name == "Eco · hall ceiling (2)").transform.position;
            bathroom = Object.FindObjectsByType<SchoolReverbArea>(FindObjectsSortMode.None)
                .Single(area => area.name == "Eco del baño").transform.position;
            sampleRate = AudioSettings.outputSampleRate;
            output = Path.GetFullPath("Review/Marco/SchoolAudioPreview.wav");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            writer = new BinaryWriter(File.Create(output));
            WriteHeader(0);
            if (!AudioRenderer.Start()) throw new InvalidOperationException("No se pudo iniciar AudioRenderer.");
            recording = true;
            previousFrame = -1;
            sampleValues = 0;
            nextCue = 0;
            peak = 0;
            energy = 0;
            EditorApplication.update += Capture;
        }

        private static void Capture()
        {
            try
            {
                if (previousFrame == Time.frameCount) return;
                previousFrame = Time.frameCount;
                double time = sampleValues / (sampleRate * 2.0);
                if (time >= Duration)
                {
                    if (nextCue != CueTimes.Length) throw new InvalidOperationException("Faltan señales en la captura.");
                    if (peak < .001f || peak >= .98f) throw new InvalidOperationException("Nivel de captura inválido " + peak);
                    Finish(null);
                    return;
                }
                Vector3 position = hall;
                if (time < 8) position.z += Mathf.Lerp(8, 0, Mathf.Clamp01(((float)time - 2) / 6));
                if (time >= 17) position = Vector3.Lerp(hall, bathroom, Mathf.Clamp01(((float)time - 17) / 4));
                Vector3 before = audition.listener.position;
                player.transform.position += position - before;
                Vector3 movement = position - before;
                movement.y = 0;
                player.footsteps.Advance(time > 2 && time < 21 ? movement.magnitude : 0, true, SchoolSurface.Wood);
                if (nextCue < CueTimes.Length && time >= CueTimes[nextCue]) FireCue(nextCue++);
                int count = AudioRenderer.GetSampleCountForCaptureFrame();
                if (count <= 0) throw new InvalidOperationException("AudioRenderer no produjo muestras.");
                using (var buffer = new NativeArray<float>(count * 2, Allocator.Temp))
                {
                    if (!AudioRenderer.Render(buffer)) throw new InvalidOperationException("Falló el render de audio.");
                    for (int index = 0; index < buffer.Length; index++)
                    {
                        float value = buffer[index];
                        if (float.IsNaN(value) || float.IsInfinity(value)) throw new InvalidOperationException("Muestra no finita.");
                        peak = Mathf.Max(peak, Mathf.Abs(value));
                        energy += value * value;
                        writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(value, -1, 1) * 32767));
                    }
                    sampleValues += buffer.Length;
                }
            }
            catch (Exception error) { Finish(error); }
        }

        private static void FireCue(int cue)
        {
            bool played = true;
            switch (cue)
            {
                case 0: played = whisper.TryPlay(); break;
                case 1: played = audition.AuditionBreath(); break;
                case 2: played = audition.AuditionCloth(); break;
                case 3: played = audition.AuditionPhone(); break;
                case 4: sequence.Play(); break;
            }
            if (!played) throw new InvalidOperationException("No se reprodujo la señal de revisión " + cue);
        }

        private static void WriteHeader(int dataBytes)
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataBytes);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)2);
            writer.Write(sampleRate);
            writer.Write(sampleRate * 4);
            writer.Write((short)4);
            writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataBytes);
        }

        private static void Finish(Exception error)
        {
            EditorApplication.update -= Capture;
            if (recording) AudioRenderer.Stop();
            recording = false;
            Time.captureFramerate = oldCaptureRate;
            Application.targetFrameRate = oldTargetRate;
            UnityEngine.Random.state = randomState;
            if (writer != null)
            {
                writer.Seek(0, SeekOrigin.Begin);
                WriteHeader(checked((int)(sampleValues * 2)));
                writer.Dispose();
                writer = null;
            }
            if (error == null)
            {
                var report = new CaptureReport
                {
                    unity = Application.unityVersion, sampleRate = sampleRate,
                    seconds = sampleValues / (sampleRate * 2.0), peakDbfs = 20 * Math.Log10(peak),
                    rmsDbfs = 20 * Math.Log10(Math.Sqrt(energy / sampleValues)), cues = nextCue,
                    origin = "Unity AudioRenderer. Guided listener positions in SchoolAudioPreview."
                };
                File.WriteAllText(Path.ChangeExtension(output, ".json"), JsonUtility.ToJson(report, true));
                Debug.Log("MARCO_CAPTURE_PASS: " + output);
            }
            else Debug.LogException(error);
            EditorApplication.Exit(error == null ? 0 : 1);
        }

        [Serializable]
        private sealed class CaptureReport
        {
            public string unity, origin;
            public int sampleRate, cues;
            public double seconds, peakDbfs, rmsDbfs;
        }
    }
}
