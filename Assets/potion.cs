// ============================================================
// Potion.cs
// ============================================================
// Colócalo en el objeto Pocion en Unity.
// Asegúrate de que el objeto tenga tag "Potion" y un Collider
// con IsTrigger = true.
//
// MECÁNICA:
// - Cuando el jugador toca la poción, esta se cura poco a poco.
// - Usa Update() + acumulador de tiempo, SIN coroutines.
// - Cura 1 punto por segundo durante 5 segundos (total: 5).
// - Al terminar la curación la poción se destruye.
//
// NOTA TÉCNICA (para alumno de C++):
// En C++ haríamos: player.vida += 1; cada segundo en el timer.
// Aquí usamos Time.deltaTime para acumular tiempo entre frames.
//
// IMPORTANTE: El jugador necesita un componente PlayerHealth
// accesible desde aquí. Si usas SimpleWalk, expón la vida como
// campo público o añade PlayerHealth al mismo GameObject.
// ============================================================

using UnityEngine;

public class Potion : MonoBehaviour
{
    // ── Configuración ────────────────────────────────────────
    [Header("Curación")]
    public float curacionPorSegundo   = 1f;   // puntos curados por tick
    public float duracionCuracion     = 5f;   // segundos que dura el efecto
    public float vidaMaximaJugador    = 20f;  // límite de curación

    // ── Estado interno ───────────────────────────────────────
    private bool   curacionActiva     = false;
    private float  tiempoCuracion     = 0f;
    private float  acumuladorCuracion = 0f;
    private PlayerHealth jugador      = null;  // referencia a la vida del jugador

    // ── Update: tick de curación sin coroutine ───────────────
    void Update()
    {
        if (curacionActiva)
            ProcesarCuracionPorTiempo();
    }

    // ── Detectar contacto con el jugador ────────────────────
    // OnTriggerEnter = "algo entró en mi zona de trigger".
    // En C++: callback de colisión del motor físico.
    void OnTriggerEnter(Collider other)
    {
        if (curacionActiva) return;                      // ya está curando

        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph == null) return;                          // no es el jugador

        jugador = ph;
        IniciarCuracion();
        Debug.Log("[Potion] Curación iniciada.");
    }

    // ── Iniciar curación ─────────────────────────────────────
    void IniciarCuracion()
    {
        curacionActiva     = true;
        tiempoCuracion     = 0f;
        acumuladorCuracion = 0f;
    }

    // ── Procesar tick de curación ────────────────────────────
    // Se llama cada frame mientras la curación esté activa.
    void ProcesarCuracionPorTiempo()
    {
        tiempoCuracion     += Time.deltaTime;
        acumuladorCuracion += Time.deltaTime;

        // Cada segundo completo, curar 1 punto.
        if (acumuladorCuracion >= 1f)
        {
            acumuladorCuracion -= 1f;
            CurarUnPunto();
        }

        // Cuando se acaban los 5 segundos, terminar y destruir.
        if (tiempoCuracion >= duracionCuracion)
        {
            TerminarCuracion();
        }
    }

    void CurarUnPunto()
    {
        if (jugador == null) return;

        jugador.vida += curacionPorSegundo;

        if (jugador.vida > vidaMaximaJugador)
            jugador.vida = vidaMaximaJugador;

        Debug.Log("[Potion] Curado +1 | Vida: " + jugador.vida);
    }

    void TerminarCuracion()
    {
        Debug.Log("[Potion] Curación completada. Destruyendo poción.");
        Destroy(gameObject);
    }
}