using UnityEngine;
using UnityEngine.InputSystem; 

public class Linterna : MonoBehaviour
{
    Light luz;

    void Start()
    {
        luz = GetComponent<Light>();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            luz.enabled = !luz.enabled;
        }
    }
}
