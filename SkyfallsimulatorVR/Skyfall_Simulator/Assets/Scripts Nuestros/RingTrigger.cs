using UnityEngine;
using System;

public class RingTrigger : MonoBehaviour
{
    // ===== Racha global =====
    public static int nextRingPoints = 100;
    public static event Action<int> OnNextPointsChanged; // avisa al HUD
    public static event Action OnMissed;                  // mensaje de fallo

    static void SetNext(int v)
    {
        nextRingPoints = v;
        OnNextPointsChanged?.Invoke(nextRingPoints);
    }

    [Header("Puntuación de este aro")]
    public int points = 100;

    [Header("Spawn del siguiente aro")]
    public RingTrigger ringPrefab;
    public float yOffset = -12f;
    public Vector2 rangeXZ = new Vector2(6f, 6f);
    public Transform spawnParent;
    public float minY = -Mathf.Infinity;

    [Header("Dirección (opcional)")]
    public bool requireForwardEntry = false;
    [Range(-1f, 1f)] public float minDot = 0.0f;

    bool done;

    void Awake()
    {
        if (points <= 0) points = nextRingPoints;
    }

    void OnTriggerEnter(Collider other)
    {
        if (done) return;

        if (requireForwardEntry)
        {
            Vector3 toOther = (other.bounds.center - transform.position).normalized;
            if (Vector3.Dot(transform.forward, toOther) < minDot) return;
        }

        // ACIERTO
        Score.Add(points);
        SetNext(points + 10);   // siguiente vale +10
        done = true;
        SpawnNextRing();
    }

    // Llamado por la FailZone al fallar
    public void OnMiss()
    {
        if (done) return;
        SetNext(100);           // resetea a 100
        OnMissed?.Invoke();     // notifica para mostrar mensaje HUD
        done = true;
        SpawnNextRing();
    }

    void SpawnNextRing()
    {
        if (!ringPrefab) return;

        Vector3 basePos = transform.position;
        float rx = UnityEngine.Random.Range(-rangeXZ.x, rangeXZ.x);
        float rz = UnityEngine.Random.Range(-rangeXZ.y, rangeXZ.y);

        Vector3 nextPos = new Vector3(basePos.x + rx, basePos.y + yOffset, basePos.z + rz);

        if (nextPos.y < minY) return;

        var go = Instantiate(ringPrefab.gameObject, nextPos, transform.rotation, spawnParent);
        var rt = go.GetComponent<RingTrigger>();
        if (rt) rt.points = nextRingPoints; // establece la puntuación del nuevo aro
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 c = transform.position + new Vector3(0f, yOffset, 0f);
        Vector3 size = new Vector3(rangeXZ.x * 2f, 0.01f, rangeXZ.y * 2f);
        Gizmos.DrawWireCube(c, size);
    }
}
