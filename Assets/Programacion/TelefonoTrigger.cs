using UnityEngine;

public class TelefonoTrigger : MonoBehaviour
{
    JugadorMovimiento jugador;
    bool yaUsado = false;

    void Start()
    {
        jugador = FindFirstObjectByType<JugadorMovimiento>();
    }

    void Interactuar()
    {
        if (yaUsado) return;
        yaUsado = true;

        jugador.enabled = false;

        Debug.Log("Jugador congelado");
    }
}
