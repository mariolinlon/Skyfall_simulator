using UnityEngine;

public class RingFailZone : MonoBehaviour
{
    RingTrigger ring;

    void Awake() => ring = GetComponentInParent<RingTrigger>();

    void OnTriggerEnter(Collider other)
    {
        ring?.OnMiss();  // FALLASTE → siguiente vuelve a 100 + spawnea otro
    }
}
