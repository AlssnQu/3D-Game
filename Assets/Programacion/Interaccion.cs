using UnityEngine;
using UnityEngine.InputSystem;

public class Interaccion : MonoBehaviour
{
    public float distancia = 2.5f;

    void Update()
    {
        RaycastHit golpe;

        if (Physics.Raycast(transform.position, transform.forward, out golpe, distancia))
        {
            if (golpe.collider.CompareTag("Interactable"))
            {
                bool apretoE = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
                bool hizoClic = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

                if (apretoE || hizoClic)
                {
                    golpe.collider.SendMessage("Interactuar", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        Debug.DrawRay(transform.position, transform.forward * distancia, Color.red);
    }
}
