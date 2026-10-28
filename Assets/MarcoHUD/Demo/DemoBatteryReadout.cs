using UnityEngine;
using UnityEngine.InputSystem;

namespace LoQueCallaste.Hud
{
    /// <summary>Only used by HudReview.unity; not the production flashlight controller.</summary>
    public sealed class DemoBatteryReadout : MonoBehaviour, IBatteryReadout
    {
        [Range(0, 1)] public float charge = 1;
        public Light testLight;
        public FlashlightHud hud;
        public bool drainOverTime;
        public float Charge01 => charge;
        private bool lampOn = true;
        private void Start() { if (hud) hud.BindBattery(this); }
        private void Update()
        {
            if (Application.isBatchMode) return;
            var keys = Keyboard.current;
            if (keys != null)
            {
                if (keys.digit1Key.wasPressedThisFrame) SetCharge01(1);
                if (keys.digit2Key.wasPressedThisFrame) SetCharge01(.5f);
                if (keys.digit3Key.wasPressedThisFrame) SetCharge01(.15f);
                if (keys.digit4Key.wasPressedThisFrame) SetCharge01(0);
                if (keys.fKey.wasPressedThisFrame) lampOn = !lampOn;
                if (keys.hKey.wasPressedThisFrame && hud) hud.SetVisible(!hud.IsVisible);
                if (keys.rKey.wasPressedThisFrame) drainOverTime = !drainOverTime;
            }
            if (drainOverTime && lampOn) charge = Mathf.Max(0, charge - Time.deltaTime * .08f);
            if (testLight) testLight.enabled = lampOn && charge > 0;
        }
        public void SetCharge01(float value) { charge = Mathf.Clamp01(value); }
    }
}
