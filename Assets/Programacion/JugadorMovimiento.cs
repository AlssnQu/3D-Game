using UnityEngine;
using UnityEngine.InputSystem;

public class JugadorMovimiento : MonoBehaviour
{
    [Header("Ajustes Base")]
    public float velocidad = 5f;
    public float sensibilidad = 0.1f;
    public Transform camara;

    [Header("Salto y Gravedad")]
    public float fuerzaSalto = 1.5f;
    public float gravedad = -15f;

    [Header("Agacharse")]
    public float velocidadAgachado = 2.5f;
    public float alturaNormal = 2f;
    public float alturaAgachado = 1f;
    public float alturaCamaraNormal = 0.6f;
    public float alturaCamaraAgachado = 0.1f;

    CharacterController controlador;
    float rotacionVertical = 0f;
    Vector3 velocidadVertical;
    bool estaAgachado = false;

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
            // WASD
            if (Keyboard.current.wKey.isPressed) v = 1f;
            if (Keyboard.current.sKey.isPressed) v = -1f;
            if (Keyboard.current.dKey.isPressed) h = 1f;
            if (Keyboard.current.aKey.isPressed) h = -1f;

            // Agacharse (Ctrl izq)
            if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
            {
                estaAgachado = !estaAgachado;
                controlador.height = estaAgachado ? alturaAgachado : alturaNormal;

                Vector3 posCamara = camara.localPosition;
                posCamara.y = estaAgachado ? alturaCamaraAgachado : alturaCamaraNormal;
                camara.localPosition = posCamara;
            }

            // Saltar (Espacio, solo en el suelo, no agachado)
            if (Keyboard.current.spaceKey.wasPressedThisFrame && controlador.isGrounded && !estaAgachado)
            {
                velocidadVertical.y = Mathf.Sqrt(fuerzaSalto * -2f * gravedad);
            }
        }

        // movimiento horizontal (cambiando de SimpleMove a Move para permitir salto)
        float velocidadActual = estaAgachado ? velocidadAgachado : velocidad;
        Vector3 direccion = (transform.right * h) + (transform.forward * v);
        controlador.Move(direccion * velocidadActual * Time.deltaTime);

        // Aplicar gravedad y salto vertical
        if (controlador.isGrounded && velocidadVertical.y < 0)
        {
            velocidadVertical.y = -2f;
        }
        velocidadVertical.y += gravedad * Time.deltaTime;
        controlador.Move(velocidadVertical * Time.deltaTime);

        // Rotación de cámara con el mouse
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
