#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace LoQueCallaste.Hud
{
    public sealed class HudRuntimeChecks : MonoBehaviour
    {
        [Serializable] private class Report { public string unity; public string status; public string[] passed; public string failure; }
        private readonly List<string> passed=new List<string>();
        private void Check(bool condition,string name){if(!condition)throw new Exception(name);passed.Add(name);}
        private IEnumerator Start()
        {
            var tests=Tests();
            while(true){bool next;try{next=tests.MoveNext();}catch(Exception e){Finish(e.ToString());yield break;}if(!next)break;yield return tests.Current;}
            Finish(null);
        }
        private IEnumerator Tests()
        {
            yield return null;
            var hud=FindFirstObjectByType<FlashlightHud>();var battery=FindFirstObjectByType<DemoBatteryReadout>();
            Check(hud && battery,"HUD y proveedor de prueba presentes");
            Check(hud.GetComponent<Canvas>().renderMode==RenderMode.ScreenSpaceOverlay,"Canvas superpuesto");
            var scaler=hud.GetComponent<CanvasScaler>();Check(scaler.uiScaleMode==CanvasScaler.ScaleMode.ScaleWithScreenSize && scaler.referenceResolution==new Vector2(1920,1080),"Escala adaptable a resolución");
            var cross=(RectTransform)hud.transform.Find("Retícula");Check(cross.anchorMin==new Vector2(.5f,.5f) && cross.anchorMax==cross.anchorMin && cross.anchoredPosition==Vector2.zero,"Retícula centrada");
            Check(!hud.GetComponent<CanvasGroup>().blocksRaycasts,"El HUD no bloquea clics del juego");
            foreach(var graphic in hud.GetComponentsInChildren<Graphic>())Check(!graphic.raycastTarget,"Gráfico sin captura de input: "+graphic.name);
            battery.SetCharge01(.5f);yield return null;yield return null;
            Check(hud.PercentageLabel.Contains("50%") && hud.DisplayedCharge01==.5f,"El HUD sigue cambios reales del proveedor");
            var fill=hud.transform.Find("Área segura/Panel batería/Carga").GetComponent<Image>();
            Check(Mathf.Abs(fill.rectTransform.localScale.y-.5f)<.001f && fill.rectTransform.pivot.y==0,"Altura visible proporcional al 50% desde la base");
            battery.SetCharge01(.15f);yield return null;yield return null;
            Check(hud.PercentageLabel.Contains("15%") && fill.color.r>fill.color.g && Mathf.Abs(fill.rectTransform.localScale.y-.15f)<.001f,"Aviso rojo y altura visible de batería baja");
            hud.SetVisible(false);battery.SetCharge01(0);yield return null;yield return null;
            Check(!hud.IsVisible && hud.PercentageLabel.Contains("0%") && fill.rectTransform.localScale.y==0,"La batería sigue actualizándose con HUD oculto");
            hud.SetVisible(true);Check(hud.IsVisible,"Visibilidad restaurada");
            Destroy(battery.gameObject);yield return null;yield return null;
            Check(!hud.HasReading && hud.PercentageLabel.Contains("—"),"Fuente destruida se desconecta sin mostrar datos antiguos");
            hud.SetChargePercent(87);Check(hud.PercentageLabel.Contains("87%"),"Admite un evento externo de porcentaje");
            hud.SetCharge01(float.PositiveInfinity);Check(hud.DisplayedCharge01==0,"Valores no finitos no rompen la interfaz");
        }
        private void Finish(string failure)
        {
            string path=Path.GetFullPath("../output/integracion-marco/verification/hud-runtime.json");Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path,JsonUtility.ToJson(new Report{unity=Application.unityVersion,status=failure==null?"PASS":"FAIL",passed=passed.ToArray(),failure=failure},true));
            if(failure!=null)Debug.LogError(failure);else Debug.Log("MARCO_HUD_RUNTIME_PASS: "+passed.Count);
            UnityEditor.EditorApplication.Exit(failure==null?0:1);
        }
    }
}
#endif
