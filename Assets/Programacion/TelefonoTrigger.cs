using UnityEngine;
using UnityEngine.Events;

public class TelefonoTrigger : MonoBehaviour
{
    JugadorMovimiento jugador;
    bool yaUsado = false;

    public UnityEvent alMarcar;

    void Start()
    {
        jugador = FindFirstObjectByType<JugadorMovimiento>();
    }

    void Interactuar()
    {
        if (yaUsado) return;
        yaUsado = true;

        jugador.enabled = false;
        alMarcar.Invoke();

        Debug.Log("Jugador congelado y evento alMarcar disparado");
    }

    public void DevolverControl()
    {
        jugador.enabled = true;
    }
}
