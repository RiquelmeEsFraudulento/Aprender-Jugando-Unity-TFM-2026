// ============================================================
// EnemyBrain.cs  —  MENTE COLMENA
// ============================================================
// Componente singleton: debe existir UNO solo en la escena.
// Crea un GameObject vacío en la escena y ponle este script.
//
// RESPONSABILIDADES:
//   · Registrar y desregistrar enemigos al nacer/morir
//   · Contar cuántos quedan vivos
//   · Activar modo Berserker cuando quedan la mitad o menos
//   · Notificar a todos los enemigos vivos cuando el modo cambia
//
// NOTA: Los enemigos se auto-registran en su Start().
//       Tú no tienes que hacer nada manual en el inspector.
// ============================================================
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────
    // Solo existe UNA Mente Colmena por escena.
    // Los enemigos la buscan así: EnemyBrain.Instancia
    public static EnemyBrain Instancia { get; private set; }

    // ── Inspector ─────────────────────────────────────────────
    [Header("Configuración de la colmena")]
    [Tooltip("Porcentaje de enemigos vivos bajo el cual se activa Berserker (0-1). Ej: 0.5 = mitad")]
    public float umbralBerserker = 0.5f;   // Al 50% se activa la rabia

    [Header("Estado (solo lectura en Inspector)")]
    public int totalRegistrados = 0;        // Cuántos enemigos hay en total
    public int vivos            = 0;        // Cuántos siguen en pie
    public bool modoBerserker   = false;    // true = la colmena está en rabia

    // ── Array interno de enemigos ─────────────────────────────
    // Tamaño fijo: máximo 20 enemigos por escena.
    // En C++: EnemyAI* enemigos[20];
    private EnemyAI[] _enemigos      = new EnemyAI[20];
    private int       _numRegistrados = 0;

    // ── Debug switch ──────────────────────────────────────────
    private const bool LOG_COLMENA = true;

    // ══════════════════════════════════════════════════════════
    // CICLO DE VIDA UNITY
    // ══════════════════════════════════════════════════════════
    void Awake()
    {
        // Patrón Singleton: si ya existe una instancia, destruye este duplicado
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }

    // ══════════════════════════════════════════════════════════
    // API PÚBLICA — llamada por EnemyAI
    // ══════════════════════════════════════════════════════════

    // Registra un enemigo cuando nace (se llama en EnemyAI.Start)
    public void RegistrarEnemigo(EnemyAI enemigo)
    {
        if (_numRegistrados >= _enemigos.Length) return; // Array lleno

        _enemigos[_numRegistrados] = enemigo;
        _numRegistrados++;
        totalRegistrados = _numRegistrados;
        vivos            = _numRegistrados;

        DebugColmena($"[Colmena] Registrado '{enemigo.name}' | Total: {totalRegistrados}");
    }

    // Desregistra un enemigo cuando muere (se llama en EnemyAI.OnMuerte)
    public void DesregistrarEnemigo(EnemyAI enemigo)
    {
        // Buscamos el hueco del enemigo muerto y lo sacamos del array
        for (int i = 0; i < _numRegistrados; i++)
        {
            if (_enemigos[i] == enemigo)
            {
                // Desplazamos los de la derecha para tapar el hueco
                for (int j = i; j < _numRegistrados - 1; j++)
                    _enemigos[j] = _enemigos[j + 1];

                _enemigos[_numRegistrados - 1] = null;
                _numRegistrados--;
                break;
            }
        }

        vivos = _numRegistrados; // Actualizamos el contador
        ActualizarModoBerserker();

        DebugColmena($"[Colmena] Murió '{enemigo.name}' | Vivos: {vivos}/{totalRegistrados}");

        // ¿Todos muertos? Aquí podrías avisar a un GameManager
        if (vivos <= 0)
            DebugColmena("[Colmena] ¡TODOS LOS ENEMIGOS HAN MUERTO! → Victoria");
    }

    // ══════════════════════════════════════════════════════════
    // LÓGICA INTERNA
    // ══════════════════════════════════════════════════════════

    // Comprueba si hay que activar el modo Berserker
    // Se llama cada vez que muere un enemigo
    void ActualizarModoBerserker()
    {
        if (totalRegistrados == 0) return; // Sin enemigos, nada que calcular

        // porcentaje = vivos / total  (entre 0.0 y 1.0)
        float porcentajeVivos = (float)vivos / (float)totalRegistrados;

        // Si aún no estaba en Berserker y el porcentaje cae por debajo del umbral...
        if (!modoBerserker && porcentajeVivos <= umbralBerserker)
        {
            modoBerserker = true;
            DebugColmena($"[Colmena] ¡MODO BERSERKER ACTIVADO! Vivos: {vivos}/{totalRegistrados}");

            // Notificamos a todos los enemigos vivos para que cambien su comportamiento
            NotificarBerserkerATodos();
        }
    }

    // Recorre todos los enemigos vivos y les dice que entren en Berserker
    void NotificarBerserkerATodos()
    {
        for (int i = 0; i < _numRegistrados; i++)
        {
            if (_enemigos[i] != null)
                _enemigos[i].ActivarBerserker();
        }
    }

    // ── Debug ─────────────────────────────────────────────────
    void DebugColmena(string msg) { if (LOG_COLMENA) Debug.Log(msg); }
}