using UnityEngine;

namespace LoQueCallaste
{
    public sealed class PlayerAudioLink : MonoBehaviour
    {
        public CharacterController controller;
        public Light flashlight;
        public SurfaceFootsteps footsteps;
        public FlashlightSwitchAudio switchAudio;

        private Vector3 previousPosition;
        private bool lightWasOn;
        private bool sampled;

        private void OnEnable() { sampled = false; }

        private void LateUpdate()
        {
            if (!controller) return;
            Vector3 position = controller.transform.position;
            bool lightIsOn = flashlight && flashlight.isActiveAndEnabled;
            if (sampled)
            {
                if (switchAudio && lightIsOn != lightWasOn)
                    switchAudio.OnStateChanged(lightIsOn);

                Vector3 movement = position - previousPosition;
                movement.y = 0;
                if (footsteps)
                {
                    SchoolSurface surface = SchoolSurface.Tile;
                    Bounds body = controller.bounds;
                    if (controller.isGrounded && Physics.Raycast(
                        new Vector3(position.x, body.min.y + .08f, position.z),
                        Vector3.down, out var hit, .25f, ~0, QueryTriggerInteraction.Ignore))
                    {
                        var marker = hit.collider.GetComponentInParent<FootstepSurface>();
                        if (marker) surface = marker.surface;
                    }

                    float metres = movement.magnitude;
                    footsteps.Advance(metres < 1f ? metres : 0f, controller.isGrounded, surface);
                }
            }

            previousPosition = position;
            lightWasOn = lightIsOn;
            sampled = true;
        }
    }
}
