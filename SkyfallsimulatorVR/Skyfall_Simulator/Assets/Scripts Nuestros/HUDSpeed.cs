using TMPro;
using UnityEngine;

public class HUDFallSpeed : MonoBehaviour
{
    [Header("Refs")]
    public Transform rig;            // XR Origin o Main Camera
    public TMP_Text fallText;        // TMP donde se escribe
    [Tooltip("(Opcional) arrastra tu Gravity Provider para mostrar Terminal Velocity")]
    public Component gravityProvider; // solo para leer un campo/propiedad float "TerminalVelocity"

    [Header("Opciones")]
    public bool positiveDown = true; // true: caída positiva; false: firma real (abajo negativo)
    public float smooth = 8f;        // suavizado exponencial
    public int decimals = 1;         // decimales
    public float maxStepMeters = 10f;// filtro anti picos (teleports)

    Vector3 _lastPos;
    float _fallMs; // m/s (suavizada)

    void Start()
    {
        if (!rig) rig = Camera.main ? Camera.main.transform : transform;
        if (!fallText) fallText = GetComponent<TMP_Text>();
        _lastPos = rig.position;
    }

    void Update()
    {
        Vector3 pos = rig.position;
        float dy = pos.y - _lastPos.y;                // + arriba, - abajo (Unity)
        float dt = Mathf.Max(Time.deltaTime, 1e-5f);

        // Filtro anti picos (teleport/respawn)
        if (Mathf.Abs(dy) > maxStepMeters) { _lastPos = pos; return; }

        float instFall = dy / dt;                     // m/s (arriba +, abajo -)
        if (positiveDown) instFall = -instFall;       // queremos caída positiva

        // Suavizado exponencial
        _fallMs = Mathf.Lerp(_fallMs, instFall, 1f - Mathf.Exp(-smooth * dt));

        float kmh = _fallMs * 3.6f;

        string terminalStr = "";
        float terminal = ReadTerminalVelocity();
        if (terminal > 0f) terminalStr = $"  (Terminal: {terminal.ToString($"F{decimals}")})";

        if (fallText)
            fallText.text = $"Caída: {_fallMs.ToString($"F{decimals}")} m/s ({kmh.ToString($"F{decimals}")} km/h){terminalStr}";

        _lastPos = pos;
    }

    float ReadTerminalVelocity()
    {
        if (!gravityProvider) return -1f;
        var t = gravityProvider.GetType();
        var p = t.GetProperty("TerminalVelocity");
        if (p != null && p.PropertyType == typeof(float)) return (float)p.GetValue(gravityProvider);
        var f = t.GetField("TerminalVelocity");
        if (f != null && f.FieldType == typeof(float)) return (float)f.GetValue(gravityProvider);
        // intenta camelCase por si acaso
        p = t.GetProperty("terminalVelocity");
        if (p != null && p.PropertyType == typeof(float)) return (float)p.GetValue(gravityProvider);
        f = t.GetField("terminalVelocity");
        if (f != null && f.FieldType == typeof(float)) return (float)f.GetValue(gravityProvider);
        return -1f;
    }
}
