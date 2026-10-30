using UnityEngine;
using UnityEngine.InputSystem; 

public class JugadorMovimiento : MonoBehaviour
{
    public float velocidad = 5f;
    public float sensibilidad = 0.1f;
    public Transform camara;

    CharacterController controlador;
    float rotacionVertical = 0f;

    void Start()
    {
        controlador = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        if (Mouse.current != null)
        {
            float mouseX = Mouse.current.delta.x.ReadValue() * sensibilidad;
            float mouseY = Mouse.current.delta.y.ReadValue() * sensibilidad;

            transform.Rotate(0, mouseX, 0);

            rotacionVertical -= mouseY;
            rotacionVertical = Mathf.Clamp(rotacionVertical, -80f, 80f);

            camara.localRotation = Quaternion.Euler(rotacionVertical, 0, 0);
        }
    }
}
