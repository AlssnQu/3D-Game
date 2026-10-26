using UnityEngine;
using UnityEngine.InputSystem;

namespace LoQueCallaste
{
    [RequireComponent(typeof(CharacterController), typeof(AudioPlayer))]
    public sealed class DemoPlayerController : MonoBehaviour
    {
        public Camera view;
        public Light flashlight;
        public AudioSource footsteps;
        public AudioClip[] stepClips;
        public float walkSpeed = 2.5f;
        public float sensitivity = 1.8f;
        public string InteractionHint { get; private set; }
        private CharacterController controller;
        private float pitch, verticalSpeed, stepClock;
        private int lastStep = -1;
        private bool focused = true;

        private void Awake() { controller = GetComponent<CharacterController>(); }
        private void Start()
        {
            if (!Application.isBatchMode) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        }
        private void OnApplicationFocus(bool focus) { focused = focus; }
        private void Update()
        {
            if (Application.isBatchMode || !focused) return;
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null || mouse == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            if (mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; return; }
            if (Cursor.lockState != CursorLockMode.Locked) return;
            transform.Rotate(0, mouse.delta.ReadValue().x * sensitivity * 0.08f, 0);
            pitch = Mathf.Clamp(pitch - mouse.delta.ReadValue().y * sensitivity * 0.08f, -75, 75);
            view.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            float x = (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
            float z = (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0);
            Vector3 direction = Vector3.ClampMagnitude(transform.right * x + transform.forward * z, 1);
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            controller.Move((direction * walkSpeed + Vector3.up * verticalSpeed) * Time.deltaTime);
            if (direction.sqrMagnitude > 0.01f && controller.isGrounded)
            {
                stepClock -= Time.deltaTime;
                if (stepClock <= 0 && footsteps && stepClips != null && stepClips.Length > 0)
                {
                    lastStep = (lastStep + 1) % stepClips.Length;
                    if (stepClips[lastStep]) footsteps.PlayOneShot(stepClips[lastStep], 0.45f);
                    stepClock = 0.47f;
                }
            }
            if (keyboard.fKey.wasPressedThisFrame && flashlight) flashlight.enabled = !flashlight.enabled;
            InteractionHint = "";
            if (Physics.Raycast(view.transform.position, view.transform.forward, out RaycastHit hit, 2.5f, ~0, QueryTriggerInteraction.Ignore))
            {
                foreach (var behaviour in hit.collider.GetComponentsInParent<MonoBehaviour>())
                {
                    if (!(behaviour is IReviewInteractable interactable) || !interactable.CanInteract) continue;
                    InteractionHint = interactable.Hint;
                    if (keyboard.eKey.wasPressedThisFrame) interactable.Interact();
                    break;
                }
            }
        }
    }
}
