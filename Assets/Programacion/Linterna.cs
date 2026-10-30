using UnityEngine;
using UnityEngine.InputSystem; 

public class Linterna : MonoBehaviour
{
    Light luz;
    public float bateria = 100f;
    public float consumoPorSegundo = 0.6f;
    float intensidadInicial;

    void Start()
    {
        luz = GetComponent<Light>();
        intensidadInicial = luz.intensity;
        luz.enabled = false; 
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            luz.enabled = !luz.enabled;
        }
        
        if (luz.enabled)
        {
            bateria -= consumoPorSegundo * Time.deltaTime;

            if (bateria < 0f)
            {
                bateria = 0f;
            }

            luz.intensity = intensidadInicial * (bateria / 100f);

            if (bateria == 0f)
            {
                luz.enabled = false;
            }
        }
    }
}
