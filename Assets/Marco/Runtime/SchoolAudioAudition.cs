using UnityEngine;
using UnityEngine.InputSystem;

namespace LoQueCallaste
{
    /// <summary>Keyboard audition tools for the review scene.</summary>
    public sealed class SchoolAudioAudition : MonoBehaviour
    {
        public SchoolAudioDirector mix;
        public MemoryAudioLink memory;
        public SpatialSoundEmitter breath;
        public SpatialSoundEmitter cloth;
        public SpatialSoundEmitter phone;
        public Transform listener;
        public bool Visible { get; private set; }
        private float master, ambience, effects, dialogue;
        private bool reducedImpact;

        private void Start()
        {
            if (!mix) return;
            master = mix.Master;
            ambience = mix.Ambience;
            effects = mix.Effects;
            dialogue = mix.Dialogue;
            reducedImpact = mix.ReducedImpact;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !mix) return;
            if (keyboard.mKey.wasPressedThisFrame) Visible = !Visible;
            if (!Visible || (memory && memory.IsPresenting)) return;
            if (keyboard.digit1Key.wasPressedThisFrame) mix.SetAmbience(mix.Ambience > 0 ? 0 : ambience);
            if (keyboard.digit2Key.wasPressedThisFrame) mix.SetEffects(mix.Effects > 0 ? 0 : effects);
            if (keyboard.digit3Key.wasPressedThisFrame) mix.SetDialogue(mix.Dialogue > 0 ? 0 : dialogue);
            if (keyboard.digit4Key.wasPressedThisFrame) mix.SetReducedImpact(!mix.ReducedImpact);
            if (keyboard.digit5Key.wasPressedThisFrame) RestoreMix();
            if (keyboard.minusKey.wasPressedThisFrame) mix.SetMaster(mix.Master - .05f);
            if (keyboard.equalsKey.wasPressedThisFrame) mix.SetMaster(mix.Master + .05f);
            if (keyboard.bKey.wasPressedThisFrame) AuditionBreath();
            if (keyboard.vKey.wasPressedThisFrame) AuditionCloth();
            if (keyboard.nKey.wasPressedThisFrame) AuditionPhone();
        }

        public bool AuditionBreath() => Audition(breath, true);
        public bool AuditionCloth() => Audition(cloth, true);
        public bool AuditionPhone() => Audition(phone, false);

        private bool Audition(SpatialSoundEmitter emitter, bool spatial)
        {
            if (!isActiveAndEnabled || !emitter || !listener || (memory && memory.IsPresenting)) return false;
            if (spatial) emitter.transform.position = listener.position + listener.right * 3 + listener.forward;
            return emitter.TryPlay();
        }

        public void RestoreMix()
        {
            if (!mix) return;
            mix.SetMaster(master);
            mix.SetAmbience(ambience);
            mix.SetEffects(effects);
            mix.SetDialogue(dialogue);
            mix.SetReducedImpact(reducedImpact);
        }

        private void OnGUI()
        {
            if (Application.isBatchMode || !mix || (memory && memory.IsPresenting)) return;
            if (!Visible)
            {
                GUI.Label(new Rect(20, 20, 320, 28), "M · Escucha y mezcla");
                return;
            }
            GUILayout.BeginArea(new Rect(20, 20, Mathf.Min(520, Screen.width - 40), 240), GUI.skin.box);
            GUILayout.Label("UNSPOKEN · REVISIÓN SONORA");
            GUILayout.Label($"General {mix.Master:P0}    - / = ajustar");
            GUILayout.Label($"1 Ambiente {mix.Ambience:P0}   2 Efectos {mix.Effects:P0}   3 Recuerdos {mix.Dialogue:P0}");
            GUILayout.Label("4 Impactos suaves " + (mix.ReducedImpact ? "activados" : "desactivados"));
            GUILayout.Label("5 Restaurar mezcla inicial");
            GUILayout.Label("B Respiración · V Tela · N Línea sin respuesta");
            GUILayout.Label("La fuente de criatura se prueba a 3 m a la derecha.");
            GUILayout.Label("R Probar recuerdo · M Ocultar panel");
            GUILayout.EndArea();
        }
    }
}
