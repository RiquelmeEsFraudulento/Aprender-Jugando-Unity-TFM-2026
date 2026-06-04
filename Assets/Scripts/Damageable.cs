// ============================================================
// Damageable.cs — VERSIÓN ENCAPSULADA COMPLETA
// CON EJERCICIOS DIDÁCTICOS PARA BACHILLERATO TIC 1
// ============================================================
// Gestiona la vida de un enemigo y los efectos de estado.
//
// ORGANIZACIÓN EN TRES BLOQUES:
//   BLOQUE 1 — Inicialización de variables + constantes + estructuras
//   BLOQUE 2 — Ejercicios en estilo C++ (el alumno trabaja aquí)
//   BLOQUE 3 — Funciones de trabajo sucio Unity/C# (alumno NO toca)
//
// TODA la funcionalidad original se mantiene íntegra.
// Los Debug originales se respetan tal cual.
// Nuevos debugs se marquen con prefijo [EJ] para filtrar fácil.
// ============================================================

using UnityEngine;
using UnityEngine.Events;

// =====================================================================
// BLOQUE 1 — INICIALIZACIÓN DE VARIABLES Y ESTRUCTURAS
// =====================================================================

public class Damageable : MonoBehaviour
{
    // ┌─────────────────────────────────────────────────────────┐
    // │  1A — CONSTANTES DEL ALUMNO (puede modificarlas)        │
    // └─────────────────────────────────────────────────────────┘

    public const int TIPO_NORMAL    = 0;
    public const int TIPO_VENENO    = 1;
    public const int TIPO_SANGRADO  = 2;
    public const int TIPO_ROJO      = 3;
    public const int TIPO_AZUL      = 4;
    public const int TIPO_GRIS      = 5;

    public const int ESTADO_NINGUNO = 0;
    public const int ESTADO_SLEEP   = 1;
    public const int ESTADO_CONFUSO = 2;

    // ── NOTA PARA EL ALUMNO ──────────────────────────────────────
    // En el proyecto real estos valores están en DamageTypes.cs
    // como un enum. Aquí usamos constantes int para que puedas
    // practicar con switch/case y if/else.
    //
    // Equivalencias:
    //   TIPO_NORMAL   = DamageType.Normal
    //   TIPO_VENENO   = DamageType.Poison
    //   TIPO_SANGRADO = DamageType.Bleed
    //   TIPO_ROJO     = DamageType.Red
    //   TIPO_AZUL     = DamageType.Blue
    //   TIPO_GRIS     = DamageType.Gray

    // ┌─────────────────────────────────────────────────────────┐
    // │  1B — ESTRUCTURA DE DATOS DEL ENEMIGO (ficha)           │
    // └─────────────────────────────────────────────────────────┘
    //
    // Piensa en esta estructura como la ficha de personaje de un
    // juego de mesa. Agrega todos los datos del enemigo en un
    // solo sitio, para que el alumno pueda leerlos fácilmente.

    [System.Serializable]
    public struct FichaEnemigo
    {
        // ── Datos básicos ──────────────────────────────────────
        public string nombre;              // Nombre del enemigo
        public int vidaMaxima;             // Vida máxima que puede tener
        public int vidaActual;             // Vida que tiene ahora
        public bool estaVivo;              // true = vivo, false = muerto

        // ── Efectos de estado ──────────────────────────────────
        public bool envenenado;            // true = tiene veneno activo
        public int golpesVeneno;        // Golpes recibidos que acumulan veneno
        public int turnosVenenoRestantes;  // Turnos que le quedan de veneno
        public int danyoVenenoPorTurno;    // Daño que hace el veneno cada turno

        public int golpesSangrado;         // Golpes acumulados de sangrado (0-4)
        public int danyoSangradoExplosion; // Daño extra al llegar a 5 golpes

        public int estadoEspecial;         // 0=ninguno, 1=sleep, 2=confuso
        public int turnosEstadoRestantes;  // Turnos que le quedan de estado

        // ── Contador para estado especial ──────────────────────
        public int golpesRecibidosParaEstado; // Golpes hacia sleep/confused

        // ── Filtros de vulnerabilidad ──────────────────────────
        public int coloresVulnerables;     // Número de colores que le afectan
        public int armasPermitidas;        // Número de armas que le pueden dañar

        // ── Susceptibilidad ────────────────────────────────────
        public bool puedeDormir;           // true = le afecta el sueño
        public bool puedeConfundirse;      // true = le afecta la confusión
    }

    // ── Instancia visible para el alumno ──────────────────────
    public FichaEnemigo ficha;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1C — VARIABLES DE DEBUG ORIGINALES (se respetan)       │
    // └─────────────────────────────────────────────────────────┘

    private const bool LOG_INIT      = true;
    private const bool LOG_COOLDOWN  = true;
    private const bool LOG_DANIO     = true;
    private const bool LOG_VENENO    = true;
    private const bool LOG_SANGRADO  = true;
    private const bool LOG_MUERTE    = true;
    private const bool LOG_ESTADO    = true;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1D — VARIABLES DE DEBUG PARA EJERCICIOS [EJ]           │
    // └─────────────────────────────────────────────────────────┘
    //
    // Estos debugs tienen el prefijo [EJ] para que puedas
    // filtrarlos fácilmente en la consola de Unity.

    [Header("═══ DEBUG EJERCICIOS [EJ] ═══")]
    public int debugEJ_golpesVeneno;
    public bool debugEJ_venenoActivo;
    public int debugEJ_turnosVenenoRestantes;
    public int debugEJ_danyoVenenoAcumulado;
    public int debugEJ_golpesSangrado;
    public bool debugEJ_sangradoExplotado;
    public int debugEJ_estadoProcesado;
    public bool debugEJ_etiquetaPermitida;
    public bool debugEJ_colorVulnerable;
    public int debugEJ_tipoEfectoProcesado;
    public int debugEJ_golpesParaEstado;
    public int debugEJ_turnosEstadoRestantes;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1E — REFERENCIA AL HUD                                  │
    // └─────────────────────────────────────────────────────────┘

    public PlayerHUDController _hud = null;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1F — INSPECTOR (variables originales, se mantienen)     │
    // └─────────────────────────────────────────────────────────┘

    [Header("Vida")]
    public int maxHealth     = 10;
    public int currentHealth = 10;

    [Header("Cooldown entre golpes")]
    public float cooldownEntreGolpes = 0.25f;

    [Header("Filtros de vulnerabilidad")]
    public DamageType[] vulnerableTypes;
    public string[] allowedWeaponTags;

    [Header("Susceptibilidad a estados")]
    public bool puedeSerAfectadoSueno = false;
    public bool puedeSerAfectadoconfuso = false;

    [Header("Eventos Unity")]
    public UnityEvent onHit;
    public UnityEvent onDeath;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1G — ESTADO PRIVADO ORIGINAL (se mantiene)              │
    // └─────────────────────────────────────────────────────────┘

    private float tiempoDesdeUltimoGolpe = 999f;

    public bool estaEnvenenado = false;
    private float tiempoVeneno       = 0f;
    private float acumuladorVeneno   = 0f;

    private int golpesDeVeneno = 0;
    private const int GOLPES_PARA_VENENO = 5; // Golpes necesarios para activar el veneno
    private const float DURACION_VENENO       = 3f;
    private const float INTERVALO_VENENO      = 1f;
    private const float DANIO_VENENO_POR_TICK = 1f;

    private int golpesDeSangrado = 0;

    private const int GOLPES_PARA_EXPLOTAR   = 5;
    private const int DANIO_EXPLOSION_BLEED  = 3;

    public enum EstadoEspecial { None, Sleep, Confused }

    [SerializeField] public EstadoEspecial estadoEspecial = EstadoEspecial.None;

    public float timerEstadoEspecial = 0f;
    public const float DURACION_SLEEP    = 5f;
    public const float DURACION_CONFUSED = 4f;

    [HideInInspector] public int golpesRecibidosParaEstado = 0;
    private const int GOLPES_PARA_ESTADO = 8;

    // ══════════════════════════════════════════════════════════
    // AWAKE — Inicialización
    // ══════════════════════════════════════════════════════════

    void Awake()
    {
        InicializarVida();
        _hud = FindAnyObjectByType<PlayerHUDController>();
    }

    public void InicializarVida()
    {
        currentHealth = maxHealth;
        estadoEspecial = EstadoEspecial.None;
        timerEstadoEspecial = 0f;
        golpesRecibidosParaEstado = 0;

        // ── Inicializar la ficha visible del alumno ──────────
        ficha.nombre = gameObject.name;
        ficha.vidaMaxima = maxHealth;
        ficha.vidaActual = currentHealth;
        ficha.estaVivo = true;
        ficha.envenenado = false;
        ficha.turnosVenenoRestantes = 0;
        ficha.golpesVeneno = 0;
        ficha.danyoVenenoPorTurno = 1;  // 0.5 redondeado = 1
        ficha.golpesSangrado = 0;
        ficha.danyoSangradoExplosion = DANIO_EXPLOSION_BLEED;
        ficha.estadoEspecial = ESTADO_NINGUNO;
        ficha.turnosEstadoRestantes = 0;
        ficha.golpesRecibidosParaEstado = 0;
        ficha.coloresVulnerables = (vulnerableTypes != null) ? vulnerableTypes.Length : 0;
        ficha.armasPermitidas = (allowedWeaponTags != null) ? allowedWeaponTags.Length : 0;
        ficha.puedeDormir = puedeSerAfectadoSueno;
        ficha.puedeConfundirse = puedeSerAfectadoconfuso;

        DebugInit(
            $"[Init] '{gameObject.name}' | Vida: {currentHealth}/{maxHealth}" +
            $" | Cooldown: {cooldownEntreGolpes}s" +
            $" | Tags: [{FormatearArray(allowedWeaponTags)}]" +
            $" | Tipos: [{FormatearArray(vulnerableTypes)}]" +
            $" | Sleep: {puedeSerAfectadoSueno} | Confused: {puedeSerAfectadoconfuso}"
        );
    }

    // ══════════════════════════════════════════════════════════
    // UPDATE
    // ══════════════════════════════════════════════════════════

    void Update()
    {
        AvanzarCooldown();
        if (estaEnvenenado) ProcesarVenenoPorTiempo();
        ProcesarEstadoEspecial();
        SincronizarFicha();
        ActualizarDebugEjercicios();
    }

    /// <summary>
    /// Sincroniza la ficha visible del alumno con el estado real.
    /// Se llama cada frame para que los datos estén siempre correctos.
    /// </summary>
    void SincronizarFicha()
    {
        ficha.vidaActual = currentHealth;
        ficha.estaVivo = currentHealth > 0;
        ficha.envenenado = estaEnvenenado;
        ficha.golpesSangrado = golpesDeSangrado;
        ficha.golpesVeneno = golpesDeVeneno;
        ficha.golpesRecibidosParaEstado = golpesRecibidosParaEstado;

        switch (estadoEspecial)
        {
            case EstadoEspecial.Sleep:
                ficha.estadoEspecial = ESTADO_SLEEP;
                break;
            case EstadoEspecial.Confused:
                ficha.estadoEspecial = ESTADO_CONFUSO;
                break;
            default:
                ficha.estadoEspecial = ESTADO_NINGUNO;
                break;
        }
    }

    /// <summary>
    /// Actualiza las variables de debug de ejercicios.
    /// </summary>
    void ActualizarDebugEjercicios()
    {
        debugEJ_venenoActivo = estaEnvenenado;
        debugEJ_turnosVenenoRestantes = ficha.turnosVenenoRestantes;
        debugEJ_golpesSangrado = golpesDeSangrado;
        debugEJ_golpesVeneno = golpesDeVeneno;
        debugEJ_golpesParaEstado = golpesRecibidosParaEstado;
        debugEJ_turnosEstadoRestantes = ficha.turnosEstadoRestantes;

        switch (estadoEspecial)
        {
            case EstadoEspecial.Sleep:
                debugEJ_estadoProcesado = ESTADO_SLEEP;
                break;
            case EstadoEspecial.Confused:
                debugEJ_estadoProcesado = ESTADO_CONFUSO;
                break;
            default:
                debugEJ_estadoProcesado = ESTADO_NINGUNO;
                break;
        }
    }

    // =====================================================================
    // BLOQUE 2 — EJERCICIOS EN ESTILO C++
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │         HERRAMIENTAS QUE PUEDES USAR LIBREMENTE         │
    // │         (ya están implementadas, no las toques)         │
    // └─────────────────────────────────────────────────────────┘

    /// <summary>
    /// Devuelve el tiempo que pasó desde el último frame.
    /// En Unity esto es Time.deltaTime.
    /// En C++ sería el dt de tu bucle de juego.
    /// </summary>
    float ObtenerTiempoFrame()
    {
        return Time.deltaTime;
    }

    /// <summary>
    /// Redondea un float hacia arriba al entero más cercano.
    /// En Unity esto es Mathf.CeilToInt.
    /// Ejemplo: RedondearArriba(2.3) = 3, RedondearArriba(4.0) = 4
    /// </summary>
    int RedondearArriba(float valor)
    {
        return Mathf.CeilToInt(valor);
    }

    /// <summary>
    /// Devuelve el número de elementos en el array de tipos vulnerables.
    /// Si el array es null o está vacío, devuelve 0.
    /// </summary>
    int LongitudArrayTipos()
    {
        if (vulnerableTypes == null) return 0;
        return vulnerableTypes.Length;
    }

    /// <summary>
    /// Devuelve el número de elementos en el array de tags de armas.
    /// Si el array es null o está vacío, devuelve 0.
    /// </summary>
    int LongitudArrayTags()
    {
        if (allowedWeaponTags == null) return 0;
        return allowedWeaponTags.Length;
    }

    /// <summary>
    /// Devuelve el tipo de daño en la posición i del array vulnerableTypes.
    /// Si el array es null o el índice no es válido, devuelve -1.
    /// </summary>
    int TipoEnPosicion(int i)
    {
        if (vulnerableTypes == null) return -1;
        if (i < 0 || i >= vulnerableTypes.Length) return -1;
        return TipoAConstante(vulnerableTypes[i]);
    }

    /// <summary>
    /// Devuelve el tag de arma en la posición i del array allowedWeaponTags.
    /// Si el array es null o el índice no es válido, devuelve "".
    /// </summary>
    string TagEnPosicion(int i)
    {
        if (allowedWeaponTags == null) return "";
        if (i < 0 || i >= allowedWeaponTags.Length) return "";
        return allowedWeaponTags[i];
    }

    /// <summary>
    /// Convierte un DamageType a su constante int correspondiente.
    /// </summary>
    int TipoAConstante(DamageType tipo)
    {
        switch (tipo)
        {
            case DamageType.Normal:  return TIPO_NORMAL;
            case DamageType.Poison:  return TIPO_VENENO;
            case DamageType.Bleed:   return TIPO_SANGRADO;
            case DamageType.Red:     return TIPO_ROJO;
            case DamageType.Blue:    return TIPO_AZUL;
            case DamageType.Gray:    return TIPO_GRIS;
            default:                 return -1;
        }
    }

    /// <summary>
    /// Convierte una constante int a su EstadoEspecial correspondiente.
    /// </summary>
    EstadoEspecial ConstanteAEstado(int constante)
    {
        switch (constante)
        {
            case ESTADO_SLEEP:   return EstadoEspecial.Sleep;
            case ESTADO_CONFUSO: return EstadoEspecial.Confused;
            default:             return EstadoEspecial.None;
        }
    }

    /// <summary>
    /// Devuelve el daño de veneno por tick como entero (redondeado arriba).
    /// El valor original es 0.5, que se redondea a 1.
    /// </summary>
    int DanyoVenenoPorTickEntero()
    {
        return RedondearArriba(DANIO_VENENO_POR_TICK);
    }

    /// <summary>
    /// Devuelve la duración del veneno en turnos (1 turno = 1 segundo).
    /// </summary>
    int DuracionVenenoEnTurnos()
    {
        return (int)DURACION_VENENO;
    }

    /// <summary>
    /// Devuelve la duración del estado Sleep en turnos.
    /// </summary>
    int DuracionSleepEnTurnos()
    {
        return (int)DURACION_SLEEP;
    }

    /// <summary>
    /// Devuelve la duración del estado Confused en turnos.
    /// </summary>
    int DuracionConfusedEnTurnos()
    {
        return (int)DURACION_CONFUSED;
    }

    /// <summary>
    /// Devuelve los golpes necesarios para aplicar un estado especial.
    /// </summary>
    int GolpesNecesariosParaEstado()
    {
        return GOLPES_PARA_ESTADO;
    }

    /// <summary>
    /// Devuelve los golpes necesarios para que el sangrado explote.
    /// </summary>
    int GolpesParaSangradoExplotar()
    {
        return GOLPES_PARA_EXPLOTAR;
    }

    /// <summary>
    /// Devuelve el daño extra cuando el sangrado explota.
    /// </summary>
    int DanyoExplosionSangrado()
    {
        return DANIO_EXPLOSION_BLEED;
    }

    /// <summary>
    /// Devuelve la vida actual del enemigo.
    /// </summary>
    int VidaActual()
    {
        return currentHealth;
    }

    /// <summary>
    /// Devuelve la vida máxima del enemigo.
    /// </summary>
    int VidaMaxima()
    {
        return maxHealth;
    }

    /// <summary>
    /// Devuelve TRUE si el enemigo está vivo (vida > 0).
    /// </summary>
    bool EstaVivo()
    {
        return currentHealth > 0;
    }

    /// <summary>
    /// Devuelve TRUE si el enemigo está envenenado.
    /// </summary>
    bool EstaEnvenenado()
    {
        return estaEnvenenado;
    }

    /// <summary>
    /// Devuelve los golpes de sangrado acumulados.
    /// </summary>
    int GolpesSangrado()
    {
        return golpesDeSangrado;
    }

    /// <summary>
    /// Devuelve los golpes recibidos hacia el estado especial.
    /// </summary>
    int GolpesParaEstado()
    {
        return golpesRecibidosParaEstado;
    }

    /// <summary>
    /// Devuelve el estado especial actual como constante int.
    /// </summary>
    public int EstadoEspecialActual()
    {
        switch (estadoEspecial)
        {
            case EstadoEspecial.Sleep:   return ESTADO_SLEEP;
            case EstadoEspecial.Confused: return ESTADO_CONFUSO;
            default:                      return ESTADO_NINGUNO;
        }
    }

    /// <summary>
    /// Devuelve TRUE si el enemigo puede ser afectado por sueño.
    /// </summary>
    bool PuedeDormir()
    {
        return puedeSerAfectadoSueno;
    }

    /// <summary>
    /// Devuelve TRUE si el enemigo puede ser afectado por confusión.
    /// </summary>
    bool PuedeConfundirse()
    {
        return puedeSerAfectadoconfuso;
    }

    /// <summary>
    /// Devuelve el tipo de daño actual como constante int.
    /// (Usado en ProcesarEfectoDeEstado)
    /// </summary>
    int TipoDanyoActual(DamageType tipo)
    {
        return TipoAConstante(tipo);
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 1 — ¿Es etiqueta permitida? (while + for)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cuando un arma golpea a un enemigo, el juego debe
    //  comprobar si ese tipo de arma (su "etiqueta") está en la lista
    //  de armas permitidas. Si la lista está vacía, cualquier arma sirve.
    //
    //  EJEMPLO REAL: Un enemigo tiene allowedWeaponTags = ["Rapier", "Kick"]
    //  - Si le golpeas con "Rapier" → SÍ está permitida → le haces daño
    //  - Si le golpeas con "LightSaber" → NO está permitida → no le haces daño
    //  - Si allowedWeaponTags está vacío → cualquier arma vale
    //
    //  OBJETIVO: Comprobar si una etiqueta de arma está en la lista
    //  de armas permitidas del enemigo.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: LongitudArrayTags(), TagEnPosicion(int i), bucle while o for
    //  ❌ NO uses: allowedWeaponTags directamente, LINQ, Contains()
    //
    // ══════════════════════════════════════════════════════════════════════
    //
    //  PASOS A SEGUIR:
    //
    //  PASO 1 — Caso especial: lista vacía
    //  Si la lista de tags está vacía (longitud 0), cualquier arma
    //  está permitida. Devuelve true directamente.
    //
    // ──────────────────────────────────────────────────────────
    //
    //  PASO 2 — Recorrer la lista con un bucle
    //  Necesitarás un bucle que recorra cada posición del array.
    //  Puedes usar for o while (ambos son válidos).
    //
    //  Usa LongitudArrayTags() para saber cuántos elementos hay.
    //  Usa TagEnPosicion(i) para obtener el tag en la posición i.
    //
    // ──────────────────────────────────────────────────────────
    //
    //  PASO 3 — Comparar cada elemento
    //  Dentro del bucle, compara el tag en la posición i con la
    //  etiqueta que estás buscando.
    //
    //  Si encuentras una coincidencia, devuelve true inmediatamente
    //  (no necesitas seguir buscando).
    //
    // ──────────────────────────────────────────────────────────
    //
    //  PASO 4 — No encontrado
    //  Si el bucle termina sin encontrar la etiqueta, devuelve false.
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="etiqueta">Etiqueta del arma a comprobar (ej: "Rapier").</param>
    /// <returns>true si la etiqueta está permitida, false si no.</returns>
    bool EsEtiquetaPermitida(string etiqueta)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: si la lista está vacía, cualquier arma vale
        int totalTags = LongitudArrayTags();
        if (totalTags == 0)
            return true;

        // PASO 2: recorremos la lista con un bucle while
        int i = 0;
        while (i < totalTags)
        {
            // PASO 3: obtenemos el tag en la posición i y comparamos
            string tagActual = TagEnPosicion(i);

            if (tagActual == etiqueta)
            {
                // ¡Encontrado! La etiqueta está permitida
                return true;
            }

            // Avanzamos al siguiente elemento
            i = i + 1;
        }

        // PASO 4: si llegamos aquí, no se encontró la etiqueta
        return false;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 2 — ¿Es color vulnerable? (for + if/else)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El sable láser tiene colores (Rojo, Azul, Gris).
    //  Cada enemigo es vulnerable a ciertos colores. Cuando el sable
    //  golpea, el juego comprueba si el color del sable está en la
    //  lista de colores vulnerables del enemigo.
    //
    //  EJEMPLO REAL: Un enemigo tiene vulnerableTypes = [Red, Blue]
    //  - Sable ROJO → SÍ es vulnerable → le haces daño
    //  - Sable AZUL → SÍ es vulnerable → le haces daño
    //  - Sable GRIS → NO es vulnerable → no le haces daño
    //  - Si vulnerableTypes está vacío → vulnerable a TODO
    //
    //  OBJETIVO: Comprobar si un tipo de daño de color está en la lista
    //  de colores vulnerables del enemigo.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: LongitudArrayTipos(), TipoEnPosicion(int i), TipoAConstante()
    //  ❌ NO uses: vulnerableTypes directamente, LINQ, Contains()
    //
    // ══════════════════════════════════════════════════════════════════════
    //
    //  PASOS A SEGUIR:
    //
    //  PASO 1 — Caso especial: lista vacía = vulnerable a todo
    //  Si la lista de tipos está vacía, el enemigo es vulnerable
    //  a cualquier color. Devuelve true directamente.
    //
    // ──────────────────────────────────────────────────────────
    //
    //  PASO 2 — Convertir el tipo de daño a constante int
    //  Usa TipoAConstante(tipo) para convertir el DamageType
    //  a un número entero que puedas comparar.
    //
    // ──────────────────────────────────────────────────────────
    //
    //  PASO 3 — Recorrer la lista con un bucle for
    //  Usa LongitudArrayTipos() para saber cuántos elementos hay.
    //  Usa TipoEnPosicion(i) para obtener el tipo en la posición i.
    //
    // ──────────────────────────────────────────────────────────
    //
    //  PASO 4 — Comparar cada elemento
    //  Si encuentras una coincidencia, devuelve true inmediatamente.
    //
    // ──────────────────────────────────────────────────────────
    //
    //  PASO 5 — No encontrado
    //  Si el bucle termina sin encontrar el color, devuelve false.
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="tipo">Tipo de daño de color a comprobar (ej: DamageType.Red).</param>
    /// <returns>true si el enemigo es vulnerable a ese color, false si no.</returns>
    bool EsColorVulnerable(DamageType tipo)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: si la lista está vacía, vulnerable a todo
        int totalTipos = LongitudArrayTipos();
        if (totalTipos == 0)
            return true;

        // PASO 2: convertimos el tipo de daño a constante int
        int tipoBuscado = TipoAConstante(tipo);

        // PASO 3: recorremos la lista con un bucle for
        for (int i = 0; i < totalTipos; i++)
        {
            // PASO 4: obtenemos el tipo en la posición i y comparamos
            int tipoEnLista = TipoEnPosicion(i);

            if (tipoEnLista == tipoBuscado)
            {
                // ¡Encontrado! El enemigo es vulnerable a este color
                return true;
            }
        }

        // PASO 5: si llegamos aquí, el color no está en la lista
        return false;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 3 — Procesar efecto de estado (switch/case)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cuando un golpe tiene un tipo de daño especial
    //  (Normal, Veneno, Sangrado), el juego debe aplicar el efecto
    //  correspondiente. Este ejercicio usa switch/case para decidir
    //  qué efecto aplicar según el tipo de daño.
    //
    //  EJEMPLO REAL:
    //  - DamageType.Normal  → solo quita vida, sin efecto extra
    //  - DamageType.Poison  → activa el veneno
    //  - DamageType.Bleed   → acumula un golpe de sangrado
    //  - DamageType.Red     → solo quita vida (es un color, no un efecto)
    //
    //  OBJETIVO: Recibe un tipo de daño y devuelve un número que indica
    //  qué efecto se debe aplicar:
    //    0 = Normal (solo daño directo)
    //    1 = Veneno (activar veneno)
    //    2 = Sangrado (acumular golpe de sangrado)
    //    3 = Color (daño de color, sin efecto extra)
    //   -1 = Desconocido (tipo no reconocido)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: TipoAConstante(), switch/case
    //  ❌ NO uses: DamageType directamente en el switch
    //
    // ══════════════════════════════════════════════════════════════════════
    //
    //  PASOS A SEGUIR:
    //
    //  PASO 1 — Convertir el tipo a constante int
    //  Usa TipoAConstante(tipo) para obtener un número.
    //
    // ──────────────────────────────────────────────────────────
    //
    //  PASO 2 — Usar switch/case para decidir el efecto
    //  Según el valor de la constante, devuelve:
    //    TIPO_NORMAL   → 0 (solo daño)
    //    TIPO_VENENO   → 1 (activar veneno)
    //    TIPO_SANGRADO → 2 (acumular sangrado)
    //    TIPO_ROJO     → 3 (color, sin efecto)
    //    TIPO_AZUL     → 3 (color, sin efecto)
    //    TIPO_GRIS     → 3 (color, sin efecto)
    //    default       → -1 (desconocido)
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="tipo">Tipo de daño del golpe recibido.</param>
    /// <returns>Código del efecto: 0=normal, 1=veneno, 2=sangrado, 3=color, -1=desconocido</returns>
    int ProcesarEfectoDeEstadoEj(DamageType tipo)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: convertimos el tipo de daño a constante int
        int tipoConstante = TipoAConstante(tipo);

        // PASO 2: usamos switch para decidir el efecto
        switch (tipoConstante)
        {
            case TIPO_NORMAL:
                // Solo daño directo, sin efecto extra
                return 0;

            case TIPO_VENENO:
                // Activar veneno
                return 1;

            case TIPO_SANGRADO:
                // Acumular golpe de sangrado
                return 2;

            case TIPO_ROJO:
            case TIPO_AZUL:
            case TIPO_GRIS:
                // Daño de color, sin efecto extra
                return 3;

            default:
                // Tipo no reconocido
                return -1;
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 4 — Procesar veneno por tiempo (while + acumulador)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El veneno quita vida cada segundo durante varios turnos.
    //  Este ejercicio simula el paso del tiempo: cada "tic" de tiempo
    //  que pasa, se acumula. Cuando el acumulador llega a 1 segundo,
    //  se aplica el daño de veneno.
    //
    //  EJEMPLO REAL:
    //  - Duración del veneno: 3 segundos
    //  - Daño por tick: 1.0 (redondeado a 1)
    //  - Cada 1 segundo → quita 1 de vida
    //  - Después de 3 segundos → el veneno termina
    //
    //  OBJETIVO: Simular el procesamiento del veneno durante un frame.
    //  Recibe el tiempo transcurrido y el acumulador actual, y devuelve
    //  cuántos tics de daño se deben aplicar este frame.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: ObtenerTiempoFrame(), while, if
    //  ❌ NO uses: Time.deltaTime directamente, Mathf.CeilToInt directamente
    //
    // ══════════════════════════════════════════════════════════════════════
    //
    //  PASOS A SEGUIR:
    //
    //  PASO 1 — Sumar el tiempo del frame al acumulador
    //  El acumulador lleva la cuenta de cuánto tiempo ha pasado
    //  desde el último tick de daño. Suma ObtenerTiempoFrame().
    //
    // ──────────────────────────────────────────────────────────
    //
    //  PASO 2 — Comprobar si hay que aplicar daño (while)
    //  Mientras el acumulador sea >= 1.0 (1 segundo), hay que
    //  aplicar un tick de daño. Usa un while para contar cuántos
    //  tics se aplican (normalmente 0 o 1, pero podrían ser más
    //  si el frame fue muy lento).
    //
    //  Cada vez que se aplica un tic:
    //    - Resta 1.0 al acumulador
    //    - Suma 1 al contador de tics
    //
    // ──────────────────────────────────────────────────────────
    //
    //  PASO 3 — Calcular el daño total
    //  El daño por tick es DANIO_VENENO_POR_TICK (1.0)
    //  Daño total = tics * dañoPorTickRedondeado
    //
    // ──────────────────────────────────────────────────────────
    //
    //  PASO 4 — Devolver el resultado
    //  Devuelve una struct con:
    //    - nuevoAcumulador: el acumulador después de aplicar tics
    //    - ticsAplicados: cuántos tics se aplicaron
    //    - danyoTotal: daño total a aplicar
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Resultado del procesamiento de veneno en un frame.
    /// </summary>
    public struct ResultadoVeneno
    {
        public float nuevoAcumulador;  // Acumulador después de procesar
        public int ticsAplicados;      // Cuántos tics de daño se aplicaron
        public int danyoTotal;         // Daño total a aplicar este frame
    }

    /// <param name="acumuladorActual">Acumulador de tiempo actual.</param>
    /// <returns>Resultado con nuevo acumulador, tics aplicados y daño total.</returns>
    ResultadoVeneno ProcesarVenenoPorTiempo(float acumuladorActual)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: sumamos el tiempo del frame al acumulador
        float acumulador = acumuladorActual + ObtenerTiempoFrame();

        // PASO 2: comprobamos cuántos tics se aplican con un while
        int tics = 0;
        float intervalo = 1f;  // 1 segundo por tick

        while (acumulador >= intervalo)
        {
            acumulador = acumulador - intervalo;  // restamos 1 segundo
            tics = tics + 1;                       // un tick más
        }

        // PASO 3: calculamos el daño total
        int danyoPorTick = DanyoVenenoPorTickEntero();
        int danyoTotal = tics * danyoPorTick;

        // PASO 4: devolvemos el resultado
        ResultadoVeneno resultado;
        resultado.nuevoAcumulador = acumulador;
        resultado.ticsAplicados = tics;
        resultado.danyoTotal = danyoTotal;
        return resultado;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 5 — ¿Cuántos turnos de veneno faltan? (for + acumulador)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El veneno dura 6 turnos. Cada turno quita vida.
    //  Este ejercicio calcula cuántos turnos faltan para que el
    //  veneno termine, dado el tiempo que lleva activo.
    //
    //  OBJETIVO: Recibe el tiempo que lleva el veneno activo y
    //  devuelve cuántos turnos completos faltan para que termine.
    //  Si ya terminó, devuelve 0.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: DuracionVenenoEnTurnos(), if/else
    //  ❌ NO uses: DURACION_VENENO directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="tiempoActivo">Tiempo que lleva el veneno activo en segundos.</param>
    /// <returns>Turnos completos que faltan para que termine el veneno.</returns>
    int TurnosVenenoRestantes(int tiempoActivo)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos la duración total en turnos
        int duracionTotal = DuracionVenenoEnTurnos();  // 6

        // PASO 2: calculamos cuántos turnos faltan
        int faltan = duracionTotal - tiempoActivo;

        // PASO 3: si ya terminó o se pasó, devolvemos 0
        if (faltan < 0)
            faltan = 0;

        return faltan;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 6 — ¿Cuántos golpes faltan para que explote el sangrado?
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El sangrado acumula golpes. Cada 5 golpes, "explota"
    //  y hace daño extra. Este ejercicio calcula cuántos golpes
    //  faltan para la próxima explosión.
    //
    //  OBJETIVO: Recibe los golpes de sangrado actuales y devuelve
    //  cuántos faltan para llegar a 5. Si ya tiene 5 o más, devuelve 0.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: GolpesParaSangradoExplotar(), if
    //  ❌ NO uses: GOLPES_PARA_EXPLOTAR directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="golpesActuales">Golpes de sangrado acumulados (0-4).</param>
    /// <returns>Golpes que faltan para la explosión de sangrado.</returns>
    int GolpesFaltantesParaSangrado(int golpesActuales)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos el umbral de explosión
        int umbral = GolpesParaSangradoExplotar();  // 5

        // PASO 2: calculamos cuántos faltan
        int faltan = umbral - golpesActuales;

        // PASO 3: si ya tiene 5 o más, devolvemos 0
        if (faltan < 0)
            faltan = 0;

        return faltan;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 7 — ¿El sangrado explota este golpe? (if/else)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cuando un golpe de sangrado llega, se acumula.
    //  Si al acumular llega a 5, el sangrado "explota" y hace
    //  daño extra. Este ejercicio comprueba si un golpe nuevo
    //  causa la explosión.
    //
    //  OBJETIVO: Recibe los golpes actuales y devuelve:
    //    - true si el siguiente golpe causa explosión
    //    - false si no causa explosión
    //    - También devuelve el daño extra si explota
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: GolpesParaSangradoExplotar(), DanyoExplosionSangrado(), if/else
    //  ❌ NO uses: GOLPES_PARA_EXPLOTAR ni DANIO_EXPLOSION_BLEED directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Resultado de comprobar si el sangrado explota.
    /// </summary>
    public struct ResultadoSangrado
    {
        public bool explota;           // true si el siguiente golpe causa explosión
        public int danyoExtra;         // Daño extra si explota (0 si no)
        public int golpesDespues;      // Golpes acumulados después del golpe (0 si explota)
    }

    /// <param name="golpesActuales">Golpes de sangrado actuales antes del nuevo golpe.</param>
    /// <returns>Resultado con si explota, daño extra y golpes después.</returns>
    ResultadoSangrado ComprobarExplosionSangrado(int golpesActuales)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: calculamos cuántos golpes habría después de este golpe
        int golpesDespues = golpesActuales + 1;

        // PASO 2: obtenemos el umbral
        int umbral = GolpesParaSangradoExplotar();  // 5

        ResultadoSangrado resultado;

        // PASO 3: ¿llega al umbral?
        if (golpesDespues >= umbral)
        {
            // ¡Explota! Hace daño extra y se reinicia
            resultado.explota = true;
            resultado.danyoExtra = DanyoExplosionSangrado();  // 3
            resultado.golpesDespues = 0;  // se reinicia el contador
        }
        else
        {
            // No explota, solo acumula
            resultado.explota = false;
            resultado.danyoExtra = 0;
            resultado.golpesDespues = golpesDespues;
        }

        return resultado;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 8 — ¿Cuántos golpes faltan para estado especial?
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Para dormir o confundir a un enemigo, hay que
    //  golpearlo 8 veces. Este ejercicio calcula cuántos golpes
    //  faltan para llegar al umbral.
    //
    //  OBJETIVO: Recibe los golpes actuales hacia el estado y
    //  devuelve cuántos faltan para llegar a 8.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: GolpesNecesariosParaEstado(), if
    //  ❌ NO uses: GOLPES_PARA_ESTADO directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="golpesActuales">Golpes acumulados hacia el estado especial.</param>
    /// <returns>Golpes que faltan para aplicar el estado especial.</returns>
    int GolpesFaltantesParaEstado(int golpesActuales)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos el umbral
        int umbral = GolpesNecesariosParaEstado();  // 8

        // PASO 2: calculamos cuántos faltan
        int faltan = umbral - golpesActuales;

        // PASO 3: si ya tiene 8 o más, devolvemos 0
        if (faltan < 0)
            faltan = 0;

        return faltan;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 9 — ¿Qué estado aplicar según golpes? (if/else + switch)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cuando un enemigo recibe 8 golpes, se le aplica
    //  un estado especial (sueño o confusión) según lo que el
    //  jugador haya seleccionado. Este ejercicio decide qué estado
    //  aplicar.
    //
    //  OBJETIVO: Recibe los golpes actuales y el estado seleccionado
    //  por el jugador, y devuelve:
    //    - Si golpes < 8 → no se aplica nada (devuelve 0)
    //    - Si golpes >= 8 y estado == SLEEP → devuelve 1 (aplicar sueño)
    //    - Si golpes >= 8 y estado == CONFUSO → devuelve 2 (aplicar confusión)
    //    - Si golpes >= 8 pero estado == NINGUNO → devuelve 0 (no aplicar)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: GolpesNecesariosParaEstado(), if/else, switch/case
    //  ❌ NO uses: GOLPES_PARA_ESTADO directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="golpesActuales">Golpes acumulados.</param>
    /// <param name="estadoSeleccionado">Estado elegido por el jugador (0, 1, 2).</param>
    /// <returns>0=nada, 1=aplicar sueño, 2=aplicar confusión</returns>
    int EstadoAplicarSegunGolpes(int golpesActuales, int estadoSeleccionado)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: ¿llegó al umbral de golpes?
        int umbral = GolpesNecesariosParaEstado();  // 8

        if (golpesActuales < umbral)
        {
            // No llegó al umbral, no se aplica nada
            return 0;
        }

        // PASO 2: llegó al umbral, ¿qué estado aplicar?
        switch (estadoSeleccionado)
        {
            case ESTADO_SLEEP:
                return 1;  // aplicar sueño

            case ESTADO_CONFUSO:
                return 2;  // aplicar confusión

            case ESTADO_NINGUNO:
            default:
                return 0;  // no aplicar nada
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 10 — Calcular daño total de sangrado (for + acumulador)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Si un enemigo recibe N golpos de sangrado seguidos,
    //  ¿cuánto daño total hará el sangrado (incluyendo explosiones)?
    //  Este ejercicio simula N golpes de sangrado y calcula el
    //  daño total acumulado.
    //
    //  REGLAS:
    //  - Cada golpe no explosivo: 0 daño extra
    //  - Cada 5º golpe: explota y hace 3 de daño extra
    //  - El contador se reinicia después de cada explosión
    //
    //  OBJETIVO: Recibe el número de golpes de sangrado que recibirá
    //  el enemigo y devuelve el daño total que hará el sangrado.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: GolpesParaSangradoExplotar(), DanyoExplosionSangrado(), for, if
    //  ❌ NO uses: GOLPES_PARA_EXPLOTAR ni DANIO_EXPLOSION_BLEED directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="numeroDeGolpes">Número de golpes de sangrado que recibirá.</param>
    /// <returns>Daño total que hará el sangrado (solo las explosiones).</returns>
    int CalcularDanyoSangradoTotal(int numeroDeGolpes)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos constantes
        int umbral = GolpesParaSangradoExplotar();      // 5
        int danyoExplosion = DanyoExplosionSangrado();  // 3

        // PASO 2: variables para el bucle
        int golpesAcumulados = 0;   // golpes desde la última explosión
        int danyoTotal = 0;         // daño acumulado de todas las explosiones

        // PASO 3: simulamos cada golpe con un for
        for (int i = 0; i < numeroDeGolpes; i++)
        {
            // Acumulamos un golpe
            golpesAcumulados = golpesAcumulados + 1;

            // PASO 4: ¿llegó al umbral de explosión?
            if (golpesAcumulados >= umbral)
            {
                // ¡Explota! Sumamos el daño y reiniciamos
                danyoTotal = danyoTotal + danyoExplosion;
                golpesAcumulados = 0;
            }
        }

        // PASO 5: devolvemos el daño total
        return danyoTotal;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // =====================================================================
    // BLOQUE 3 — TRABAJO SUCIO UNITY/C# (alumno NO toca)
    // =====================================================================
    //
    // Todas las funciones a continuación encapsulan llamadas específicas
    // de Unity 6.4 y C#. El alumno NO necesita entenderlas.
    // Las funciones de los ejercicios de arriba LLAMAN a estas internamente
    // a través de las funciones auxiliares del Bloque 2.
    // ════════════════════════════════════════════════════════════


    // ══════════════════════════════════════════════════════════
    // API PÚBLICA ORIGINAL (se mantiene compatible)
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Comprueba si un tipo de arma puede dañar a este enemigo.
    /// Llama internamente a EsColorVulnerable y EsEtiquetaPermitida.
    /// </summary>
    public bool CanBeDamagedBy(DamageType tipo, string weaponTag)
    {
        DebugCooldown($"[Filtro] '{gameObject.name}' | tag='{weaponTag}' | tipo={tipo}");

        if (EstaEnCooldown()) return false;

        // ── Usamos la función del ejercicio 1 ──
        if (!EsEtiquetaPermitida(weaponTag)) return false;

        // ── Usamos la función del ejercicio 2 ──
        if (weaponTag == "LightSaber" && !EsColorVulnerable(tipo)) return false;
        if (weaponTag == "Rapier" && !EsColorVulnerable(tipo)) return false;

        return true;
    }

    /// <summary>
    /// Registra un golpe hacia el estado especial.
    /// Devuelve true cuando se alcanzan los 8 golpes.
    /// </summary>
    public bool RegistrarGolpeParaEstado()
    {
        golpesRecibidosParaEstado++;
        DebugEstado($"[GolpesEstado] '{gameObject.name}' golpe {golpesRecibidosParaEstado}/{GOLPES_PARA_ESTADO}");
        return golpesRecibidosParaEstado >= GOLPES_PARA_ESTADO;
    }

    /// <summary>
    /// Resetea el contador de golpes para estado.
    /// </summary>
    public void ResetearGolpesParaEstado()
    {
        golpesRecibidosParaEstado = 0;
        DebugEstado($"[GolpesEstado] '{gameObject.name}' contador reseteado.");
    }

    public int GetGolpesParaEstado()
    {
        return golpesRecibidosParaEstado;
    }

    /// <summary>
    /// Aplica daño al enemigo. Procesa el efecto de estado correspondiente.
    /// </summary>
    public virtual void TakeDamage(int amount, DamageType damageType, GameObject source)
    {
        if (amount <= 0) return;

        DebugDanio($"[Hit] '{gameObject.name}' -{amount} | tipo={damageType}" +
            $" | fuente='{source?.name}' | Vida antes: {currentHealth}");

        float damagetosteal = amount;
        if (_hud != null) _hud.OnLifeStealDamageDealt(damagetosteal);

        ReiniciarCooldown();
        AplicarDanioDirecto(amount);
        onHit?.Invoke();

        // ── Usamos la función del ejercicio 3 para decidir el efecto ──
        ProcesarEfectoDeEstado(damageType);

        // ── Interrumpir estado especial al recibir daño ───────
        if (estadoEspecial != EstadoEspecial.None)
        {
            DebugEstado($"[Estado] '{gameObject.name}' interrumpido de {estadoEspecial} por golpe.");
            LimpiarEstadoEspecial();
        }

        DebugDanio($"[Hit] Vida después: {currentHealth}/{maxHealth}");
    }

    // ══════════════════════════════════════════════════════════
    // ESTADOS SLEEP / CONFUSED — API (se mantiene)
    // ══════════════════════════════════════════════════════════

    public virtual bool AplicarSleep()
    {
        if (!puedeSerAfectadoSueno)
        {
            DebugEstado($"[Sleep] '{gameObject.name}' es inmune → ignorado.");
            return false;
        }
        if (currentHealth <= 0) return false;

        estadoEspecial = EstadoEspecial.Sleep;
        timerEstadoEspecial = 0f;
        ResetearGolpesParaEstado();
        DebugEstado($"[Sleep] '{gameObject.name}' dormido por {DURACION_SLEEP}s.");
        return true;
    }

    public virtual bool AplicarConfused()
    {
        if (!puedeSerAfectadoconfuso)
        {
            DebugEstado($"[Confused] '{gameObject.name}' es inmune → ignorado.");
            return false;
        }
        if (currentHealth <= 0) return false;

        estadoEspecial = EstadoEspecial.Confused;
        timerEstadoEspecial = 0f;
        ResetearGolpesParaEstado();
        DebugEstado($"[Confused] '{gameObject.name}' confuso por {DURACION_CONFUSED}s.");
        return true;
    }

    public EstadoEspecial GetEstadoEspecial()
    {
        return estadoEspecial;
    }

    public virtual bool EstaDormido()
    {
        return estadoEspecial == EstadoEspecial.Sleep;
    }

    public virtual bool EstaConfuso()
    {
        return estadoEspecial == EstadoEspecial.Confused;
    }

    // ══════════════════════════════════════════════════════════
    // PROCESAMIENTO DE ESTADO ESPECIAL (llama al ejercicio 3)
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Procesa el efecto de estado según el tipo de daño.
    /// USA la función del ejercicio 3 (ProcesarEfectoDeEstado).
    /// </summary>
    void ProcesarEfectoDeEstado(DamageType tipo)
    {
        // ── Llamamos a la función del ejercicio 3 ──
        int efecto = ProcesarEfectoDeEstadoEj(tipo);

        // ── Actualizamos el debug del ejercicio ──
        debugEJ_tipoEfectoProcesado = efecto;

        // ── Ejecutamos la acción según el código devuelto ──
        switch (efecto)
        {
            case 1:  // TIPO_VENENO
                RegistrarGolpeDeVeneno();
                break;

            case 2:  // TIPO_SANGRADO
                RegistrarGolpeDeSangrado();
                break;

            case 0:  // TIPO_NORMAL
            case 3:  // TIPOS DE COLOR (Red, Blue, Gray)
                // Solo daño directo, sin efecto extra
                break;

            default:
                Debug.LogWarning($"[Damageable] ⚠ DamageType '{tipo}' sin caso en el switch.");
                break;
        }
    }

    // ══════════════════════════════════════════════════════════
    // PROCESAMIENTO DE ESTADO ESPECIAL — TEMPORIZADOR
    // ══════════════════════════════════════════════════════════

    public virtual void ProcesarEstadoEspecial()
    {
        if (estadoEspecial == EstadoEspecial.None) return;

        timerEstadoEspecial += Time.deltaTime;

        if (estadoEspecial == EstadoEspecial.Sleep && timerEstadoEspecial >= DURACION_SLEEP)
        {
            DebugEstado($"[Sleep] '{gameObject.name}' despertó tras {DURACION_SLEEP}s.");
            LimpiarEstadoEspecial();
        }
        else if (estadoEspecial == EstadoEspecial.Confused && timerEstadoEspecial >= DURACION_CONFUSED)
        {
            DebugEstado($"[Confused] '{gameObject.name}' se recuperó tras {DURACION_CONFUSED}s.");
            LimpiarEstadoEspecial();
        }
    }

    public virtual void LimpiarEstadoEspecial()
    {
        estadoEspecial = EstadoEspecial.None;
        timerEstadoEspecial = 0f;
        ResetearGolpesParaEstado();
    }

    // ══════════════════════════════════════════════════════════
    // COOLDOWN (se mantiene original)
    // ══════════════════════════════════════════════════════════

    public void AvanzarCooldown()
    {
        if (tiempoDesdeUltimoGolpe < cooldownEntreGolpes)
            tiempoDesdeUltimoGolpe += Time.deltaTime;
    }

    public void ReiniciarCooldown()
    {
        tiempoDesdeUltimoGolpe = 0f;
        DebugCooldown($"[Cooldown] '{gameObject.name}' bloqueado por {cooldownEntreGolpes}s");
    }

    public bool EstaEnCooldown()
    {
        bool enCooldown = tiempoDesdeUltimoGolpe < cooldownEntreGolpes;
        if (enCooldown)
            DebugCooldown($"[Cooldown] '{gameObject.name}' aún en cooldown" +
                $" ({tiempoDesdeUltimoGolpe:F3}/{cooldownEntreGolpes}s) → BLOQUEADO");
        return enCooldown;
    }

    // ══════════════════════════════════════════════════════════
    // VENENO (llama al ejercicio 4)
    // ══════════════════════════════════════════════════════════
        void RegistrarGolpeDeVeneno()
    {
        golpesDeVeneno++;
        DebugVeneno($"[Veneno] Golpe {golpesDeVeneno}/{GOLPES_PARA_VENENO} en '{gameObject.name}'");
        if (golpesDeVeneno >= GOLPES_PARA_VENENO)
        {
            ActivarVeneno();
            golpesDeVeneno = 0; // reset after activating
        }
    }


    public void ActivarVeneno()
    {
        estaEnvenenado = true;
        tiempoVeneno = 0f;
        acumuladorVeneno = 0f;
        DebugVeneno($"[Veneno] ACTIVADO en '{gameObject.name}'" +
            $" | {DURACION_VENENO}s | -{DANIO_VENENO_POR_TICK}/tick");
    }

    /// <summary>
    /// Procesa el veneno cada frame.
    /// USA la función del ejercicio 4 (ProcesarVenenoPorTiempo).
    /// </summary>
    public void ProcesarVenenoPorTiempo()
    {
        // ── Actualizamos el tiempo total del veneno ──
        tiempoVeneno += Time.deltaTime;

        // ── Llamamos a la función del ejercicio 4 ──
        ResultadoVeneno resultado = ProcesarVenenoPorTiempo(acumuladorVeneno);

        // ── Aplicamos los resultados ──
        acumuladorVeneno = resultado.nuevoAcumulador;

        // ── Actualizamos debug del ejercicio ──
        debugEJ_danyoVenenoAcumulado = resultado.danyoTotal;
        ficha.turnosVenenoRestantes = TurnosVenenoRestantes((int)tiempoVeneno);

        // ── Aplicamos el daño de los tics ──
        if (resultado.danyoTotal > 0)
        {
            AplicarDanioDirecto(resultado.danyoTotal);
            DebugVeneno($"[Veneno] Tick -{resultado.danyoTotal}" +
                $" | {tiempoVeneno:F1}/{DURACION_VENENO}s" +
                $" | Vida: {currentHealth}/{maxHealth}");
        }

        // ── Comprobamos si el veneno terminó ──
        if (tiempoVeneno >= DURACION_VENENO)
            DesactivarVeneno();
    }

    void DesactivarVeneno()
    {
        estaEnvenenado = false;
        tiempoVeneno = 0f;
        acumuladorVeneno = 0f;
        DebugVeneno($"[Veneno] TERMINADO en '{gameObject.name}'");
    }

    // ══════════════════════════════════════════════════════════
    // SANGRADO (llama a los ejercicios 6 y 7)
    // ══════════════════════════════════════════════════════════

    void RegistrarGolpeDeSangrado()
    {
        // ── Usamos la función del ejercicio 7 para comprobar explosión ──
        ResultadoSangrado resultado = ComprobarExplosionSangrado(golpesDeSangrado);

        // ── Actualizamos el contador ──
        golpesDeSangrado = resultado.golpesDespues;

        // ── Actualizamos debug ──
        debugEJ_golpesSangrado = golpesDeSangrado;
        debugEJ_sangradoExplotado = resultado.explota;

        DebugSangrado($"[Sangrado] Golpe {golpesDeSangrado}/{GOLPES_PARA_EXPLOTAR}" +
            $" en '{gameObject.name}'");

        // ── Si explotó, aplicamos el daño extra ──
        if (resultado.explota)
        {
            DebugSangrado($"[Sangrado] ¡EXPLOSIÓN! -{resultado.danyoExtra}" +
                $" en '{gameObject.name}' | Vida antes: {currentHealth}");
            AplicarDanioDirecto(resultado.danyoExtra);
            DebugSangrado($"[Sangrado] Vida después: {currentHealth}/{maxHealth}");
        }
    }

    // ══════════════════════════════════════════════════════════
    // DAÑO DIRECTO (se mantiene original)
    // ══════════════════════════════════════════════════════════

    void AplicarDanioDirecto(int cantidad)
    {
        currentHealth -= cantidad;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            LimpiarEstadoEspecial();
            Morir();
        }
    }

    // ══════════════════════════════════════════════════════════
    // FILTROS DE VULNERABILIDAD (llaman a los ejercicios 1 y 2)
    // ══════════════════════════════════════════════════════════
    //
    // NOTA: Las funciones EsEtiquetaPermitida y EsColorVulnerable
    // están implementadas arriba en el Bloque 2 (ejercicios 1 y 2).
    // Aquí NO las duplicamos — las llamadas en CanBeDamagedBy
    // ya usan las versiones del ejercicio.
    // ══════════════════════════════════════════════════════════

    // ══════════════════════════════════════════════════════════
    // MUERTE (se mantiene original)
    // ══════════════════════════════════════════════════════════

    public virtual void Morir()
    {
        DebugMuerte($"[Muerte] '{gameObject.name}' ha muerto.");
        onDeath?.Invoke();
        Destroy(gameObject);
    }

    // ══════════════════════════════════════════════════════════
    // HELPERS DE DEBUG ORIGINALES (se respetan tal cual)
    // ══════════════════════════════════════════════════════════

    void DebugInit(string msg)     { if (LOG_INIT)     Debug.Log(msg); }
    void DebugCooldown(string msg) { if (LOG_COOLDOWN) Debug.Log(msg); }
    void DebugDanio(string msg)    { if (LOG_DANIO)    Debug.Log(msg); }
    void DebugVeneno(string msg)   { if (LOG_VENENO)   Debug.Log(msg); }
    void DebugSangrado(string msg) { if (LOG_SANGRADO) Debug.Log(msg); }
    public void DebugMuerte(string msg) { if (LOG_MUERTE) Debug.Log(msg); }
    void DebugEstado(string msg)   { if (LOG_ESTADO)   Debug.Log(msg); }

    string FormatearArray<T>(T[] arr)
    {
        if (arr == null || arr.Length == 0) return "cualquiera";
        return string.Join(", ", arr);
    }

    // ══════════════════════════════════════════════════════════
    // FIN DEL ARCHIVO
    // ══════════════════════════════════════════════════════════
}