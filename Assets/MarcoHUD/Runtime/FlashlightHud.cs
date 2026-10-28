using UnityEngine;
using UnityEngine.UI;

namespace LoQueCallaste.Hud
{
    public sealed class FlashlightHud : MonoBehaviour
    {
        [SerializeField] private Image chargeFill;
        [SerializeField] private Text percentageText;
        [SerializeField] private Text statusText;
        [SerializeField] private CanvasGroup visibility;
        [Tooltip("Optional component implementing IBatteryReadout. Otherwise use SetCharge01 or SetChargePercent.")]
        [SerializeField] private MonoBehaviour batterySource;
        [SerializeField] private Color normalColor = new Color(0.78f, 0.86f, 0.83f);
        [SerializeField] private Color lowColor = new Color(0.85f, 0.28f, 0.23f);
        private IBatteryReadout readout;
        private bool hasReading;
        public float DisplayedCharge01 { get; private set; }
        public bool HasReading => hasReading;
        public bool IsVisible => !visibility || visibility.alpha > 0;
        public string PercentageLabel => percentageText ? percentageText.text : "";
        public string StatusLabel => statusText ? statusText.text : "";

        public void Configure(Image fill, Text label, CanvasGroup group, Text status = null)
        {
            chargeFill = fill; percentageText = label; visibility = group; statusText = status;
            Render();
        }
        private void Awake()
        {
            if (batterySource) BindBattery(batterySource);
            Render();
        }
        private void LateUpdate()
        {
            if (readout == null) return;
            if (!batterySource) { UnbindBattery(); return; }
            SetCharge01(readout.Charge01);
        }
        public bool BindBattery(MonoBehaviour source)
        {
            if (!source) { UnbindBattery(); return false; }
            if (!(source is IBatteryReadout battery))
            {
                UnbindBattery();
                Debug.LogWarning("La fuente debe implementar IBatteryReadout; también puedes conectar SetCharge01 a un evento.", this);
                return false;
            }
            batterySource = source; readout = battery; SetCharge01(battery.Charge01);
            return true;
        }
        public void UnbindBattery()
        {
            batterySource = null; readout = null; hasReading = false; DisplayedCharge01 = 0; Render();
        }
        public void SetChargePercent(float percentage) { SetCharge01(percentage / 100f); }
        public void SetCharge01(float charge)
        {
            DisplayedCharge01 = float.IsNaN(charge) || float.IsInfinity(charge) ? 0 : Mathf.Clamp01(charge);
            hasReading = true; Render();
        }
        public void SetVisible(bool visible)
        {
            if (!visibility) return;
            visibility.alpha = visible ? 1 : 0;
            visibility.interactable = false;
            visibility.blocksRaycasts = false;
        }
        private void Render()
        {
            if (chargeFill)
            {
                chargeFill.fillAmount = hasReading ? DisplayedCharge01 : 0;
                // A plain UI Image has no sprite, so its mesh ignores fillAmount.
                // The bottom-pivoted rectangle provides a proportional visible fill.
                chargeFill.rectTransform.localScale = new Vector3(1, hasReading ? DisplayedCharge01 : 0, 1);
                chargeFill.color = DisplayedCharge01 <= 0.2f ? lowColor : normalColor;
            }
            if (percentageText)
            {
                percentageText.text = hasReading ? "BATERÍA  " + Mathf.RoundToInt(DisplayedCharge01 * 100) + "%" : "BATERÍA  —";
                percentageText.color = hasReading && DisplayedCharge01 <= 0.2f ? lowColor : normalColor;
            }
            if (statusText)
            {
                statusText.text = !hasReading || DisplayedCharge01 > .2f ? "" : DisplayedCharge01 <= 0 ? "BATERÍA AGOTADA" : "BATERÍA BAJA";
                statusText.color = lowColor;
            }
        }
    }
}
