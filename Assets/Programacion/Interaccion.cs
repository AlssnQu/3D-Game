using UnityEngine;

public class Interaccion : MonoBehaviour
{
    public float distancia = 2.5f;

    void Update()
    {
        RaycastHit golpe;

        if (Physics.Raycast(transform.position, transform.forward, out golpe, distancia))
        {
            Debug.Log("El rayo toca: " + golpe.collider.name);
        }

        Debug.DrawRay(transform.position, transform.forward * distancia, Color.red);
    }
}
