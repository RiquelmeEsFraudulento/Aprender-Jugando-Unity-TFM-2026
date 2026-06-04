// ============================================================
// Boss.cs — VERSIÓN ENCAPSULADA COMPLETA Y CORREGIDA
// CON EJERCICIOS DIDÁCTICOS PARA BACHILLERATO TIC 1
// ============================================================
// Boss: hereda de EnemyScript → Damageable.
//
// ╔══════════════════════════════════════════════════════════╗
// ║           FLUJO CORRECTO DEL BOSS                       ║
// ╠══════════════════════════════════════════════════════════╣
// ║                                                          ║
// ║  ESTADO INICIAL: INVULNERABLE                            ║
// ║  ├─ Recibe golpes → 0 DAÑO (animación de hit)           ║
// ║  ├─ Acumula golpes hacia Sleep y Confused                ║
// ║  └─ AI sigue funcionando normalmente                    ║
// ║                                                          ║
// ║  FASE 1: APlicar SLEEP (8 golpes acumulados)             ║
// ║  ├─ Boss se DUERME (animation Sleep)                    ║
// ║  ├─ AI se pausa mientras duerme                         ║
// ║  ├─ Dura 5 segundos O hasta que reciba 1 golpe          ║
// ║  ├─ Al despertar: _yaRecibioSleep = true                 ║
// ║  └─ Boss vuelve a la normalidad (AI reanuda)            ║
// ║                                                          ║
// ║  FASE 2: Aplicar CONFUSED (8 golpes acumulados)          ║
// ║  ├─ Boss se CONFUNDE (animation AirPunch hacia atrás)   ║
// ║  ├─ AI se pausa mientras está confuso                   ║
// ║  ├─ Dura 4 segundos O hasta que reciba 1 golpe          ║
// ║  ├─ Al recuperarse: _yaRecibioConfused = true            ║
// ║  └─ Boss vuelve a la normalidad (AI reanuda)            ║
// ║                                                          ║
// ║  FASE 3: VULNERABLE                                      ║
// ║  ├─ Solo cuando _yaRecibioSleep Y _yaRecibioConfused    ║
// ║  ├─ Boss puede recibir daño normalmente                  ║
// ║  └─ SIGUIENTE GOLPE = MUERTE                             ║
// ║                                                          ║
// ║  MUERTE:                                                 ║
// ║  ├─ Animación de muerte                                  ║
// ║  ├─ Desactiva componentes                               ║
// ║  ├─ Da XP al jugador                                     ║
// ║  └─ Destruye el GameObject tras 2 segundos              ║
// ║                                                          ║
// ╚══════════════════════════════════════════════════════════╝
//
// ORGANIZACIÓN EN TRES BLOQUES:
//   BLOQUE 1 — Inicialización de variables + constantes
//   BLOQUE 2 — Ejercicios en estilo C++ (el alumno trabaja aquí)
//   BLOQUE 3 — Funciones de trabajo sucio Unity/C# (alumno NO toca)
//
// ⚠️ REGLAS CRÍTICAS:
//   1. Boss NUNCA muere antes de recibir Sleep Y Confused
//   2. Sleep dura 5 segundos O hasta recibir 1 golpe (lo que ocurra primero)
//   3. Confused dura 4 segundos O hasta recibir 1 golpe (lo que ocurra primero)
//   4. Al salir de Sleep/Confused, el boss reanuda su AI
//   5. Solo cuando ambos (Sleep + Confused) fueron aplicados, el boss es vulnerable
// ============================================================

using System.Collections;
using UnityEngine;

public class Boss : EnemyScript
{
    // =====================================================================
    // BLOQUE 1 — INICIALIZACIÓN DE VARIABLES Y ESTRUCTURAS
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │  1A — CONSTANTES DEL ALUMNO                             │
    // └─────────────────────────────────────────────────────────┘

    //public const int ESTADO_NINGUNO = 0;
    //public const int ESTADO_SLEEP   = 1;
    /// <summary>
    /// public const int ESTADO_CONFUSO = 2;
    /// </summary>

    public const int GOLPES_PARA_ESTADO = 8;

    // ── Contadores separados para cada estado ──────────────────
    // El boss necesita acumular golpes HACIA sleep y HACIA confused
    // de forma independiente. Por eso hay dos contadores.

    // ┌─────────────────────────────────────────────────────────┐
    // │  1B — ESTRUCTURA DE DATOS DE LA fichaB DEL BOSS          │
    // └─────────────────────────────────────────────────────────┘

    [System.Serializable]
    public struct fichaBBoss
    {
        // ── Estado de vulnerabilidad ───────────────────────────
        public bool yaRecibioSleep;      // true = ya completó el ciclo de sleep
        public bool yaRecibioConfused;   // true = ya completó el ciclo de confused
        public bool esVulnerable;        // true = puede recibir daño (ambos estados completados)
        public bool estaDerrotado;       // true = ha sido derrotado

        // ── Estado actual ──────────────────────────────────────
        public int estadoActual;         // 0=ninguno, 1=sleep, 2=confuso

        // ── Contadores de golpes hacia cada estado ─────────────
        public int golpesHaciaSleep;     // Golpes acumulados hacia sleep (0-7)
        public int golpesHaciaConfused;  // Golpes acumulados hacia confused (0-7)

        // ── Vida ───────────────────────────────────────────────
        public int vidaInicial;          // Vida con la que empieza
        public int vidaActual;           // Vida actual (se actualiza al recibir daño)
        public int danyoAlJugador;       // Daño que hace al jugador
    }

    public fichaBBoss fichaB;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1C — VARIABLES DE DEBUG PARA EJERCICIOS [EJ]           │
    // └─────────────────────────────────────────────────────────┘

    [Header("═══ DEBUG EJERCICIOS [EJ] ═══")]
    public bool debugEJ_esVulnerable;
    public bool debugEJ_yaDormido;
    public bool debugEJ_yaConfundido;
    public int debugEJ_accionTakeDamage;
    public int debugEJ_golpesHaciaSleep;
    public int debugEJ_golpesHaciaConfused;
    public bool debugEJ_comprobarVulnerable;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1D — ESTADO PRIVADO ORIGINAL (se mantiene)             │
    // └─────────────────────────────────────────────────────────┘

    // ── Flags de estados completados ──────────────────────────
    private bool _yaRecibioSleep = false;
    private bool _yaRecibioConfused = false;
    private bool _esVulnerable = false;
    private bool _derrotado = false;

    // ── Contadores de golpes hacia cada estado ────────────────
    // Se acumulan por separado en el Damageable padre
    private int _golpesHaciaSleep = 0;
    private int _golpesHaciaConfused = 0;

    // ── Corrutinas de estados ─────────────────────────────────
    private Coroutine _sleepCoroutine;
    private Coroutine _confusedCoroutine;

    [Header("Boss — Configuración")]
    public int vidaInicial = 100;
    public int danyoAlJugador = 15;

    // ══════════════════════════════════════════════════════════
    // START
    // ══════════════════════════════════════════════════════════

    public override void Start()
    {
        base.Start();
        ConfigurarBoss();
    }

    void ConfigurarBoss()
    {
        currentHealth = vidaInicial;
        maxHealth = vidaInicial;
        danyoAtaque = danyoAlJugador;
        puedeSerAfectadoSueno = true;
        puedeSerAfectadoconfuso = true;

        _yaRecibioSleep = false;
        _yaRecibioConfused = false;
        _esVulnerable = false;
        _derrotado = false;
        _golpesHaciaSleep = 0;
        _golpesHaciaConfused = 0;

        // ── Inicializar fichaB visible del alumno ──────────────
        fichaB.yaRecibioSleep = false;
        fichaB.yaRecibioConfused = false;
        fichaB.esVulnerable = false;
        fichaB.estaDerrotado = false;
        fichaB.estadoActual = ESTADO_NINGUNO;
        fichaB.golpesHaciaSleep = 0;
        fichaB.golpesHaciaConfused = 0;
        fichaB.vidaInicial = vidaInicial;
        fichaB.danyoAlJugador = danyoAlJugador;

        Debug.Log($"[Boss] ═══ INICIADO ═══ Vida:{currentHealth} | Dmg:{danyoAtaque} | Estado: INVULNERABLE");
    }

    // =====================================================================
    // BLOQUE 2 — EJERCICIOS EN ESTILO C++
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │         HERRAMIENTAS QUE PUEDES USAR LIBREMENTE         │
    // │         (ya están implementadas, no las toques)         │
    // └─────────────────────────────────────────────────────────┘

    /// <summary>
    /// Devuelve TRUE si el boss ya completó el ciclo de Sleep.
    /// (Fue dormido y despertó, ya sea por tiempo o por golpe)
    /// </summary>
    bool YaRecibioSleep()
    {
        return _yaRecibioSleep;
    }

    /// <summary>
    /// Devuelve TRUE si el boss ya completó el ciclo de Confused.
    /// </summary>
    bool YaRecibioConfused()
    {
        return _yaRecibioConfused;
    }

    /// <summary>
    /// Devuelve TRUE si el boss es vulnerable (ambos estados completados).
    /// </summary>
    bool EsVulnerable()
    {
        return _esVulnerable;
    }

    /// <summary>
    /// Devuelve TRUE si el boss está derrotado.
    /// </summary>
    //bool EstaDerrotado()
    //{
        //return _derrotado;
    //}

    /// <summary>
    /// Devuelve TRUE si el boss está dormido actualmente.
    /// </summary>
    //bool EstaDormido()
    //{
        //return estadoEspecial == EstadoEspecial.Sleep;
    //}

    /// <summary>
    /// Devuelve TRUE si el boss está confuso actualmente.
    /// </summary>
    //bool EstaConfuso()
    //{
        //return estadoEspecial == EstadoEspecial.Confused;
    //}

    /// <summary>
    /// Devuelve el estado especial actual como constante int.
    /// </summary>
    int EstadoActual()
    {
        switch (estadoEspecial)
        {
            case EstadoEspecial.Sleep:   return ESTADO_SLEEP;
            case EstadoEspecial.Confused: return ESTADO_CONFUSO;
            default:                      return ESTADO_NINGUNO;
        }
    }

    /// <summary>
    /// Devuelve los golpes necesarios para aplicar un estado.
    /// </summary>
    int GolpesNecesarios()
    {
        return GOLPES_PARA_ESTADO;
    }

    /// <summary>
    /// Devuelve los golpes acumulados hacia Sleep.
    /// </summary>
    int GolpesHaciaSleep()
    {
        return _golpesHaciaSleep;
    }

    /// <summary>
    /// Devuelve los golpes acumulados hacia Confused.
    /// </summary>
    int GolpesHaciaConfused()
    {
        return _golpesHaciaConfused;
    }

    /// <summary>
    /// Devuelve la vida actual del boss.
    /// </summary>
    int VidaActual()
    {
        return currentHealth;
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 1 — TakeDamage: ¿Qué hacer al recibir daño?
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El boss tiene una lógica especial al recibir daño.
    //  Este ejercicio es el CORAZÓN del sistema del boss.
    //
    //  FLUJO DE DECISIONES:
    //
    //  ┌──────────────────────────────────────────────────────────┐
    //  │  ¿Derrotado? → SÍ → No hacer nada (return 0)             │
    //  │  ↓ NO                                                    │
    //  │  ¿Vulnerable? → SÍ → MATAR (return 4)                    │
    //  │  ↓ NO                                                    │
    //  │  ¿Está dormido? → SÍ → DESPERTAR (return 1)              │
    //  │  ↓ NO                                                    │
    //  │  ¿Está confuso? → SÍ → DESCONFUNDIR (return 2)           │
    //  │  ↓ NO                                                    │
    //  │  BLOQUEAR daño (return 3)                                │
    //  └──────────────────────────────────────────────────────────┘
    //
    //  OBJETIVO: Decidir qué acción tomar al recibir un golpe.
    //    0 = No hacer nada (ya derrotado)
    //    1 = Despertar de Sleep (está dormido)
    //    2 = Desconfundir (está confuso)
    //    3 = Bloquear daño (invulnerable, sin estado activo)
    //    4 = Muerte (vulnerable)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: EstaDerrotado(), EsVulnerable(), EstaDormido(), EstaConfuso(), if/else
    //  ❌ NO uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>
    ///   0 = Nada (derrotado)
    ///   1 = DespertarSleep
    ///   2 = Desconfundir
    ///   3 = Bloquear
    ///   4 = Muerte
    /// </returns>
    int TakeDamage()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: ¿ya está derrotado? Si sí, ignorar
        if (EstaDerrotado())
            return 0;

        // PASO 2: ¿es vulnerable? Si sí, el golpe es MORTAL
        if (EsVulnerable())
            return 4;

        // PASO 3: ¿está dormido? Si sí, el golpe lo despierta
        if (EstaDormido())
            return 1;

        // PASO 4: ¿está confuso? Si sí, el golpe lo desconfunde
        if (EstaConfuso())
            return 2;

        // PASO 5: no es vulnerable y no tiene estado activo → BLOQUEAR
        return 3;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 2 — Comprobar vulnerabilidad: ¿El boss es vulnerable?
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El boss solo es vulnerable cuando YA COMPLETÓ ambos
    //  ciclos de estado (Sleep Y Confused). No antes.
    //
    //  IMPORTANTE: "Completó" significa que fue dormido/confundido
    //  y luego despertó/recuperó (ya sea por tiempo o por golpe).
    //
    //  EJEMPLO:
    //  - _yaRecibioSleep = false, _yaRecibioConfused = false → NO vulnerable
    //  - _yaRecibioSleep = true,  _yaRecibioConfused = false → NO vulnerable
    //  - _yaRecibioSleep = false, _yaRecibioConfused = true  → NO vulnerable
    //  - _yaRecibioSleep = true,  _yaRecibioConfused = true  → ¡VULNERABLE!
    //
    //  OBJETIVO: Devuelve true si el boss es vulnerable.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: YaRecibioSleep(), YaRecibioConfused(), operador &&
    //  ❌ NO uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>true si el boss es vulnerable al golpe mortal.</returns>
    bool ComprobarVulnerabilidadEJ()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // El boss es vulnerable si completó AMBOS ciclos de estado
        if (YaRecibioSleep() && YaRecibioConfused())
            return true;
        else
            return false;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 3 — ¿Cuántos golpes faltan para Sleep?
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Para dormir al boss, hay que golpearlo 8 veces.
    //  Este ejercicio calcula cuántos golpes faltan.
    //
    //  OBJETIVO: Recibe los golpes actuales y devuelve cuántos faltan.
    //  Si ya tiene 8 o más, devuelve 0.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: GolpesNecesarios(), if
    //  ❌ NO uses: GOLPES_PARA_ESTADO directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="golpesActuales">Golpes acumulados hacia sleep.</param>
    /// <returns>Golpes que faltan para dormir al boss.</returns>
    int GolpesFaltantesSleep(int golpesActuales)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        int umbral = GolpesNecesarios();
        int faltan = umbral - golpesActuales;

        if (faltan < 0)
            faltan = 0;

        return faltan;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 4 — Procesar estado especial del boss
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cuando el boss recibe golpes, se acumulan hacia
    //  el estado que el jugador haya seleccionado (sleep o confused).
    //  Al llegar a 8 golpes, se aplica el estado.
    //
    //  DIFERENCIA CON ENEMIGOS NORMALES:
    //  - El boss tiene CONTADORES SEPARADOS para sleep y confused
    //  - Un golpe solo cuenta hacia el estado que el jugador eligió
    //
    //  OBJETIVO: Recibe los golpes actuales hacia un estado, el estado
    //  del boss, y el estado seleccionado por el jugador. Devuelve:
    //    >= 0: nuevos golpes acumulados (solo se acumuló)
    //    -1:   aplicar SLEEP
    //    -2:   aplicar CONFUSED
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: GolpesNecesarios(), EstadoActual(), if/else, switch/case
    //  ❌ NO uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="golpesHaciaEstado">Golpes acumulados hacia el estado seleccionado.</param>
    /// <param name="estadoSeleccionado">Estado elegido por el jugador (1=sleep, 2=confused).</param>
    /// <returns>
    ///   >= 0: nuevos golpes acumulados
    ///   -1:   aplicar SLEEP
    ///   -2:   aplicar CONFUSED
    /// </returns>
    int ProcesarEstadoEspecial(int golpesHaciaEstado, int estadoSeleccionado)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: acumulamos un golpe
        int nuevosGolpes = golpesHaciaEstado + 1;

        // PASO 2: ¿llegó al umbral?
        int umbral = GolpesNecesarios();

        if (nuevosGolpes < umbral)
        {
            return nuevosGolpes;
        }

        // PASO 3: llegó al umbral, ¿qué estado aplicar?
        switch (estadoSeleccionado)
        {
            case ESTADO_SLEEP:
                return -1;  // aplicar SLEEP

            case ESTADO_CONFUSO:
                return -2;  // aplicar CONFUSED

            default:
                return 0;   // no aplicar nada (resetear)
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // =====================================================================
    // BLOQUE 3 — TRABAJO SUCIO UNITY/C# (alumno NO toca)
    // =====================================================================

    // ══════════════════════════════════════════════════════════
    // OVERRIDE: AplicarSleep — Inicia el ciclo de Sleep
    // ══════════════════════════════════════════════════════════

    public override bool AplicarSleep()
    {
        if (_derrotado) return false;

        bool ok = base.AplicarSleep();

        if (ok)
        {
            Debug.Log($"[Boss] ✦ SLEEP aplicado | S:{_yaRecibioSleep} C:{_yaRecibioConfused}");
            IniciarCicloSleep();
        }

        return ok;
    }

    // ══════════════════════════════════════════════════════════
    // OVERRIDE: AplicarConfused — Inicia el ciclo de Confused
    // ══════════════════════════════════════════════════════════

    public override bool AplicarConfused()
    {
        if (_derrotado) return false;

        bool ok = base.AplicarConfused();

        if (ok)
        {
            Debug.Log($"[Boss] ✦ CONFUSED aplicado | S:{_yaRecibioSleep} C:{_yaRecibioConfused}");
            IniciarCicloConfused();
        }

        return ok;
    }

    // ══════════════════════════════════════════════════════════
    // CICLO DE SLEEP
    // ══════════════════════════════════════════════════════════
    //
    // El boss se duerme durante 5 segundos.
    // Si recibe un golpe, se despierta inmediatamente.
    // Al despertar (por tiempo o por golpe), marca _yaRecibioSleep = true.

    void IniciarCicloSleep()
    {
        if (_sleepCoroutine != null) StopCoroutine(_sleepCoroutine);
        _sleepCoroutine = StartCoroutine(CorutinaSleep());
    }

    IEnumerator CorutinaSleep()
    {
        // ── Pausar el AI del boss ──
        animator.SetTrigger("Sleep");
        // Desactivar movimiento
        isStunned = true;

        float duracionSleep = 5f;
        float elapsed = 0f;

        Debug.Log("[Boss] 💤 Durmiendo...");

        // ── Esperar 5 segundos o hasta que reciba un golpe (se interrumpe en TakeDamage) ──
        while (elapsed < duracionSleep)
        {
            elapsed += Time.deltaTime;

            // Si ya no está dormido, significa que un golpe lo despertó
            if (!EstaDormido())
            {
                Debug.Log("[Boss] 💤 Interrumpido por golpe");
            }

            yield return null;
        }

        // ── Si llegamos aquí, el boss durmió los 5 segundos completos ──
        if (EstaDormido())
        {
            DespertarDeSleep();
        }
    }

    void DespertarDeSleep()
    {
        _yaRecibioSleep = true;
        fichaB.yaRecibioSleep = true;

        // ── Salir del estado de sleep ──
        LimpiarEstadoEspecial();

        // ── Reanudar AI ──
        isStunned = false;
        animator.SetTrigger("Idle");

        _golpesHaciaSleep = 0;
        fichaB.golpesHaciaSleep = 0;

        Debug.Log($"[Boss] ✦ SLEEP → Completado por tiempo | _yaRecibioSleep={_yaRecibioSleep}");

        ComprobarVulnerabilidad();
    }

    // ══════════════════════════════════════════════════════════
    // CICLO DE CONFUSED
    // ══════════════════════════════════════════════════════════
    //
    // El boss se confunde durante 4 segundos.
    // Si recibe un golpe, se desconfunde inmediatamente.
    // Al recuperarse, marca _yaRecibioConfused = true.

    void IniciarCicloConfused()
    {
        if (_confusedCoroutine != null) StopCoroutine(_confusedCoroutine);
        _confusedCoroutine = StartCoroutine(CorutinaConfused());
    }

    IEnumerator CorutinaConfused()
    {
        // ── Pausar el AI del boss ──
        animator.SetTrigger("AirPunch");
        isStunned = true;

        float duracionConfused = 4f;
        float elapsed = 0f;

        Debug.Log("[Boss] 🌀 Confuso...");

        while (elapsed < duracionConfused)
        {
            elapsed += Time.deltaTime;

            if (!EstaConfuso())
            {
                Debug.Log("[Boss] 🌀 Interrumpido por golpe");
            }

            yield return null;
        }

        if (EstaConfuso())
        {
            DespertarDeConfused();
        }
    }

    void DespertarDeConfused()
    {
        _yaRecibioConfused = true;
        fichaB.yaRecibioConfused = true;

        LimpiarEstadoEspecial();

        isStunned = false;
        animator.SetTrigger("Idle");

        _golpesHaciaConfused = 0;
        fichaB.golpesHaciaConfused = 0;

        Debug.Log($"[Boss] ✦ CONFUSED → Completado por tiempo | _yaRecibioConfused={_yaRecibioConfused}");

        ComprobarVulnerabilidad();
    }


    // ══════════════════════════════════════════════════════════
    // OVERRIDE: TakeDamage — Lógica principal del boss
    // ══════════════════════════════════════════════════════════
    //
    //  ⚠️ ESTA ES LA FUNCIÓN MÁS IMPORTANTE ⚠️
    //
    //  FLUJO:
    //  1. Llamar a TakeDamage() del ejercicio 1 para decidir la acción
    //  2. Ejecutar la acción correspondiente
    //  3. Si el boss está dormido/confuso, despertarlo (corta la corrutina)
    //  4. Si es vulnerable, matar al boss
    //  5. Si no es vulnerable, bloquear el daño (0 daño)
    //
    // ══════════════════════════════════════════════════════════

    public override void TakeDamage(int amount, DamageType damageType, GameObject source)
    {
        // ── 1. Llamar a la función del Ejercicio 1 ──
        int accion = TakeDamage();

        debugEJ_accionTakeDamage = accion;

        // ── 2. Ejecutar la acción según el resultado ──
        switch (accion)
        {
            case 0:
                // ── Derrotado: ignorar ──
                return;

            case 1:
                // ── Está dormido: DESPERTAR ──
                DespertarDeSleepPorGolpe();
                return;

            case 2:
                // ── Está confuso: DESCONFUNDIR ──
                DespertarDeConfusedPorGolpe();
                return;

            case 3:
                // ── Invulnerable sin estado: BLOQUEAR ──
                BloquearDanyo(amount);
                return;

            case 4:
                // ── Vulnerable: GOLPE MORTAL ──
                EjecutarGolpeMortal(amount);
                return;
        }
    }

    void DespertarDeSleepPorGolpe()
    {
        // ── Registrar que sleep fue completado ──
        _yaRecibioSleep = true;
        fichaB.yaRecibioSleep = true;

        Debug.Log($"[Boss] ✦ SLEEP → Completado por golpe | _yaRecibioSleep={_yaRecibioSleep}");

        // ── Cortar la corrutina de sleep ──
        if (_sleepCoroutine != null) StopCoroutine(_sleepCoroutine);

        // ── Salir del estado de sleep ──
        LimpiarEstadoEspecial();

        // ── Reanudar AI ──
        isStunned = false;
        animator.SetTrigger("Idle");

        // ── Resetear contador de golpes hacia sleep ──
        _golpesHaciaSleep = 0;
        fichaB.golpesHaciaSleep = 0;

        // ── Feedback visual: animación de hit ──
        animator.SetTrigger("Hit");

        // ── Comprobar si ahora es vulnerable ──
        ComprobarVulnerabilidad();
    }

    void DespertarDeConfusedPorGolpe()
    {
        // ── Registrar que confused fue completado ──
        _yaRecibioConfused = true;
        fichaB.yaRecibioConfused = true;

        Debug.Log($"[Boss] ✦ CONFUSED → Completado por golpe | _yaRecibioConfused={_yaRecibioConfused}");

        // ── Cortar la corrutina de confused ──
        if (_confusedCoroutine != null) StopCoroutine(_confusedCoroutine);

        // ── Salir del estado de confused ──
        LimpiarEstadoEspecial();

        // ── Reanudar AI ──
        isStunned = false;
        animator.SetTrigger("Idle");

        // ── Resetear contador de golpes hacia confused ──
        _golpesHaciaConfused = 0;
        fichaB.golpesHaciaConfused = 0;

        // ── Feedback visual: animación de hit ──
        animator.SetTrigger("Hit");

        // ── Comprobar si ahora es vulnerable ──
        ComprobarVulnerabilidad();
    }

    void BloquearDanyo(int amountOriginal)
    {
        Debug.Log($"[Boss] ✖ BLOQUEADO (-{amountOriginal}→0) | S:{_yaRecibioSleep} C:{_yaRecibioConfused}");

        // ── 0 daño: solo animación de hit ──
        animator.SetTrigger("Hit");

        // ── Acumular golpes hacia los estados ──
        AcumularGolpesHaciaEstados();
    }

    void AcumularGolpesHaciaEstados()
    {
        // ── Leer qué estado seleccionó el jugador ──
        SimpleWalk playerCombat = FindAnyObjectByType<SimpleWalk>();
        if (playerCombat == null) return;

        int estadoSeleccionado = ESTADO_NINGUNO;
        switch (playerCombat.estadoSeleccionado)
        {
            case Damageable.EstadoEspecial.Sleep:   estadoSeleccionado = ESTADO_SLEEP;   break;
            case Damageable.EstadoEspecial.Confused: estadoSeleccionado = ESTADO_CONFUSO; break;
        }

        if (estadoSeleccionado == ESTADO_NINGUNO) return;

        // ── Acumular hacia el estado correspondiente ──
        int resultado;
        if (estadoSeleccionado == ESTADO_SLEEP)
        {
            // ── Usar función del ejercicio 4 ──
            resultado = ProcesarEstadoEspecial(_golpesHaciaSleep, estadoSeleccionado);
            _golpesHaciaSleep = resultado;
            fichaB.golpesHaciaSleep = resultado;
        }
        else if (estadoSeleccionado == ESTADO_CONFUSO)
        {
            resultado = ProcesarEstadoEspecial(_golpesHaciaConfused, estadoSeleccionado);
            _golpesHaciaConfused = resultado;
            fichaB.golpesHaciaConfused = resultado;
        }
        else
        {
            return;
        }

        debugEJ_golpesHaciaSleep = _golpesHaciaSleep;
        debugEJ_golpesHaciaConfused = _golpesHaciaConfused;

        // ── Comprobar si se alcanzó el umbral ──
        if (resultado < 0)
        {
            if (resultado == -1)
            {
                // Aplicar SLEEP
                AplicarSleep();
            }
            else if (resultado == -2)
            {
                // Aplicar CONFUSED
                AplicarConfused();
            }
        }
    }

    void EjecutarGolpeMortal(int amount)
    {
        Debug.Log("[Boss] ★★★ GOLPE FINAL ★★★");

        currentHealth = 0;
        _derrotado = true;
        fichaB.estaDerrotado = true;
        fichaB.vidaActual = 0;

        EjecutarMuerte();
    }

    void ComprobarVulnerabilidad()
    {
        // ── Usar función del ejercicio 2 ──
        bool vulnerable = ComprobarVulnerabilidadEJ();

        debugEJ_comprobarVulnerable = vulnerable;

        if (vulnerable && !_esVulnerable)
        {
            _esVulnerable = true;
            fichaB.esVulnerable = true;

            Debug.Log("╔══════════════════════════════════════╗");
            Debug.Log("║  [Boss] ¡VULNERABLE AL GOLPE FINAL!  ║");
            Debug.Log("╚══════════════════════════════════════╝");
        }
    }


    // ══════════════════════════════════════════════════════════
    // MUERTE DEL BOSS
    // ══════════════════════════════════════════════════════════

    void EjecutarMuerte()
    {
        Debug.Log("[Boss] ████ DERROTADO ████");

        // ── Parar corrutinas de estados ──
        if (_sleepCoroutine != null) StopCoroutine(_sleepCoroutine);
        if (_confusedCoroutine != null) StopCoroutine(_confusedCoroutine);
        StopAllCoroutines();

        // ── Animación de muerte ──
        animator.SetTrigger("Death");

        // ── Desactivar componentes ──
        this.enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        // ── Notificar al EnemyManager ──
        if (enemyManager != null)
        {
            enemyManager.SetEnemyAvailability(this, false);
            enemyManager.RemoveEnemy(this);
        }

        // ── Dar XP al jugador ──
        var player = FindAnyObjectByType<SimpleWalk>();
        if (player != null)
        {
            var ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.GanarXP(XPEarned);
                ph.Lootbox();
            }
        }

        // ── Destruir tras animación ──
        StartCoroutine(Destruir());
    }

    IEnumerator Destruir()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public override void Morir()
    {
        if (_derrotado) return;
        _derrotado = true;
        EjecutarMuerte();
    }


    // ══════════════════════════════════════════════════════════
    // SINCRONIZAR fichaB Y DEBUG
    // ══════════════════════════════════════════════════════════

    void SincronizarfichaB()
    {
        fichaB.yaRecibioSleep = _yaRecibioSleep;
        fichaB.yaRecibioConfused = _yaRecibioConfused;
        fichaB.esVulnerable = _esVulnerable;
        fichaB.estaDerrotado = _derrotado;
        fichaB.estadoActual = EstadoActual();
        fichaB.golpesHaciaSleep = _golpesHaciaSleep;
        fichaB.golpesHaciaConfused = _golpesHaciaConfused;
        fichaB.vidaInicial = vidaInicial;
        fichaB.danyoAlJugador = danyoAlJugador;
    }

    void ActualizarDebugEjercicios()
    {
        fichaB.yaRecibioSleep = _yaRecibioSleep;
        fichaB.yaRecibioConfused = _yaRecibioConfused;
        fichaB.esVulnerable = _esVulnerable;
        fichaB.golpesHaciaSleep = _golpesHaciaSleep;
        fichaB.golpesHaciaConfused = _golpesHaciaConfused;
    }


    // ══════════════════════════════════════════════════════════
    // API PÚBLICA
    // ══════════════════════════════════════════════════════════

    public bool RecibioSleep()    => _yaRecibioSleep;
    public bool RecibioConfused() => _yaRecibioConfused;
    //public bool EsVulnerable()    => _esVulnerable;
    public bool EstaDerrotado()   => _derrotado;

    // =====================================================================
    // FIN DEL ARCHIVO
    // =====================================================================
}