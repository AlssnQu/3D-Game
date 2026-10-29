using UnityEngine;
using UnityEngine.InputSystem; 

public class JugadorMovimiento : MonoBehaviour
{
    public float velocidad = 5f;
    CharacterController controlador;

    void Start()
    {
        controlador = GetComponent<CharacterController>();
    }

    void Update()
    {
        float h = 0f;
        float v = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) v = 1f;
            if (Keyboard.current.sKey.isPressed) v = -1f;
            if (Keyboard.current.dKey.isPressed) h = 1f;
            if (Keyboard.current.aKey.isPressed) h = -1f;
        }

        Vector3 direccion = (transform.right * h) + (transform.forward * v);
        controlador.SimpleMove(direccion * velocidad);
    }
}
