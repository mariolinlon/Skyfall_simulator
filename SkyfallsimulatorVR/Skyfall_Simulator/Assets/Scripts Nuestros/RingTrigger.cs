using UnityEngine;

public class RingTrigger : MonoBehaviour
{
    [Header("Puntuación")]
    public int points = 100;
    public bool oneShot = true;

    [Header("Validación de dirección")]
    public bool requireForwardEntry = false;  // ponlo en true si quieres validar dirección
    [Range(-1f, 1f)] public float minDot = 0.0f; // 0.0≈de frente; 0.3–0.5 más estricto

    bool done;

    void OnTriggerEnter(Collider other)
    {
        if (done && oneShot) return;

        if (requireForwardEntry)
        {
            // Comprueba que entra “de frente” según el forward del aro (eje Z+)
            Vector3 toOther = (other.bounds.center - transform.position).normalized;
            if (Vector3.Dot(transform.forward, toOther) < minDot) return;
        }

        // (Opcional) filtra por tag/layer del jugador:
        // if (!other.CompareTag("Player")) return;

        Score.Add(points);   // usa la clase Score que te pasé antes
        done = true;
    }
}
