using UnityEngine;

namespace LoQueCallaste.Hud
{
    public sealed class TeamBatteryReadout : MonoBehaviour, IBatteryReadout
    {
        public Linterna flashlight;
        public FlashlightHud hud;
        private bool bound;
        public float Charge01 => flashlight ? flashlight.bateria / 100f : 0;

        private void OnEnable() { RefreshBinding(); }

        private void LateUpdate()
        {
            if (bound && !flashlight)
            {
                if (hud) hud.UnbindBattery();
                bound = false;
            }
            else if (!bound && flashlight && hud) RefreshBinding();
        }

        private void RefreshBinding()
        {
            bound = flashlight && hud && hud.BindBattery(this);
        }

        private void OnDisable()
        {
            if (bound && hud) hud.UnbindBattery();
            bound = false;
        }
    }
}
