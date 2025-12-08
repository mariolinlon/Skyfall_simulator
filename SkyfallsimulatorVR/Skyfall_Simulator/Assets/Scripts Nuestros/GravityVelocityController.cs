using UnityEngine;
using System.Reflection;

/// Controla la velocidad terminal y añade caída extra tras aciertos.
/// - Reaplica Terminal Velocity cada frame (por si otro sistema la pisa).
/// - Si conviven dos gravedades (Move Provider + Gravity Provider), añade
///   caída aditiva para que el bonus se note aunque el otro tope su caída.
[DefaultExecutionOrder(10000)]
public class GravityVelocityController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí tu componente Gravity Provider (el que muestra Terminal Velocity en el Inspector).")]
    public Component gravityProvider;

    [Tooltip("CharacterController del XR Origin (para aplicar caída aditiva si hace falta).")]
    public CharacterController controller;

    [Header("Terminal Velocity")]
    [Tooltip("Valor inicial de la velocidad terminal (ej.: 4).")]
    public float baseTerminal = 4f;

    [Tooltip("Incremento de terminal por aro acertado (ej.: 1.5).")]
    public float addPerHit = 1.5f;

    [Tooltip("Límite superior para no descontrolar la caída.")]
    public float maxTerminal = 30f;

    [Tooltip("Reaplicar la terminal cada frame para evitar que otro sistema la sobrescriba.")]
    public bool enforceEveryFrame = true;

    [Header("Caída aditiva (por si también usas la gravedad del Move Provider)")]
    [Tooltip("m/s extra por cada +1 de terminal sobre la base. Si no usas Move Provider con gravedad, puedes dejarlo en 0.")]
    public float extraPerTerminalUnit = 1.0f;

    float _current;       // terminal actual
    float _extraDown;     // caída extra (m/s) cuando hay bonus

    // Cache de miembro (propiedad o campo) que controla la terminal en el Gravity Provider
    PropertyInfo _pi;
    FieldInfo _fi;

    void Awake()
    {
        if (!gravityProvider) gravityProvider = GetComponent<Component>();
        CacheMember(gravityProvider);
        SetTerminal(baseTerminal);
    }

    void FixedUpdate() { if (enforceEveryFrame) ApplyTerminal(); }
    void Update()
    {
        if (enforceEveryFrame) ApplyTerminal();

        // Caída aditiva para convivir con la gravedad del Move Provider (si está activa)
        if (controller && _extraDown > 0f)
        {
            controller.Move(Vector3.down * (_extraDown * Time.deltaTime));
        }
    }
    void LateUpdate() { if (enforceEveryFrame) ApplyTerminal(); }

    // Llamar al acertar un aro
    public void OnScored()
    {
        SetTerminal(Mathf.Min(_current + addPerHit, maxTerminal));
        _extraDown = Mathf.Max(0f, (_current - baseTerminal) * extraPerTerminalUnit);
    }

    // Llamar al fallar (FailZone)
    public void OnMissed()
    {
        SetTerminal(baseTerminal);
        _extraDown = 0f;
    }

    // ----- Internos -----
    void ApplyTerminal() => WriteFloat(_current);

    void SetTerminal(float v)
    {
        _current = v;
        WriteFloat(v);
    }

    void CacheMember(Component c)
    {
        if (!c) return;
        var t = c.GetType();

        // Intenta primero propiedad (pública o privada)
        _pi = t.GetProperty("TerminalVelocity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (_pi == null)
            _pi = t.GetProperty("terminalVelocity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Si no hay propiedad, intenta campos (incluye patrones comunes)
        if (_pi == null)
        {
            _fi = t.GetField("TerminalVelocity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (_fi == null) _fi = t.GetField("terminalVelocity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (_fi == null) _fi = t.GetField("m_TerminalVelocity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }

    void WriteFloat(float v)
    {
        if (!gravityProvider) return;

        if (_pi != null && _pi.PropertyType == typeof(float))
        {
            _pi.SetValue(gravityProvider, v);
            return;
        }
        if (_fi != null && _fi.FieldType == typeof(float))
        {
            _fi.SetValue(gravityProvider, v);
            return;
        }

        // Si algo cambió en runtime, reintenta cachear el miembro
        CacheMember(gravityProvider);
        if (_pi != null && _pi.PropertyType == typeof(float)) _pi.SetValue(gravityProvider, v);
        else if (_fi != null && _fi.FieldType == typeof(float)) _fi.SetValue(gravityProvider, v);
    }
}
