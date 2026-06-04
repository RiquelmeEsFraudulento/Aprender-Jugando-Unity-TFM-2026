// ============================================================
// KickCollider.cs — VERSIÓN ENCAPSULADA COMPLETA
// CON EJERCICIOS DIDÁCTICOS PARA BACHILLERATO TIC 1
// ============================================================
// Gestiona el hitbox de las patadas.
//
// COMPARTE la función ProcesarEstadoEspecial con RapierHitbox
// y LightSaberHitbox. La lógica es idéntica.
//
// ORGANIZACIÓN EN TRES BLOQUES:
//   BLOQUE 1 — Inicialización de variables + constantes
//   BLOQUE 2 — Ejercicios en estilo C++ (compartidos con RapierHitbox)
//   BLOQUE 3 — Funciones de trabajo sucio Unity/C# (alumno NO toca)
//
// TODA la funcionalidad original se mantiene íntegra.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class KickHitbox : MonoBehaviour
{
    // =====================================================================
    // BLOQUE 1 — INICIALIZACIÓN DE VARIABLES Y ESTRUCTURAS
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │  1A — CONSTANTES DEL ALUMNO                             │
    // └─────────────────────────────────────────────────────────┘

    public const int MODO_NORMAL   = 0;
    public const int MODO_VENENO   = 1;
    public const int MODO_SANGRADO = 2;

    public const int ESTADO_NINGUNO = 0;
    public const int ESTADO_SLEEP   = 1;
    public const int ESTADO_CONFUSO = 2;

    public const int GOLPES_PARA_ESTADO = 8;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1B — ESTRUCTURA DE DATOS DE LA FICHA DE PATADA         │
    // └─────────────────────────────────────────────────────────┘

    [System.Serializable]
    public struct FichaPatada
    {
        public int danyoBase;               // Daño base de la patada
        public int estadoSeleccionado;      // Estado especial elegido por el jugador
        public int golpesAcumulados;        // Golpes hacia el estado especial
        public bool hitboxActivo;           // true = puede golpear
    }

    public FichaPatada ficha;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1C — VARIABLES DE DEBUG PARA EJERCICIOS [EJ]           │
    // └─────────────────────────────────────────────────────────┘

    [Header("═══ DEBUG EJERCICIOS [EJ] ═══")]
    public int debugEJ_estadoProcesado;
    public int debugEJ_golpesParaEstado;
    public bool debugEJ_estadoAplicado;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1D — INSPECTOR (variables originales)                   │
    // └─────────────────────────────────────────────────────────┘

    [Header("Damage settings")]
    public int damage = 1;

    [Tooltip("Root transform of the character that owns this hitbox (player).")]
    public Transform ownerRoot;

    [Tooltip("Color damage type (Red/Blue/Gray).")]
    public DamageType damageType = DamageType.Red;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1E — ESTADO INTERNO ORIGINAL                            │
    // └─────────────────────────────────────────────────────────┘

    readonly HashSet<Damageable> alreadyHit = new HashSet<Damageable>();

    private SimpleWalk playerCombat;

    // ══════════════════════════════════════════════════════════
    // AWAKE
    // ══════════════════════════════════════════════════════════

    void Awake()
    {
        if (playerCombat == null)
            playerCombat = FindAnyObjectByType<SimpleWalk>();

        ficha.danyoBase = damage;
        ficha.estadoSeleccionado = ESTADO_NINGUNO;
        ficha.golpesAcumulados = 0;
        ficha.hitboxActivo = false;
    }

    // ══════════════════════════════════════════════════════════
    // ONENABLE / ONTRIGGERENTER
    // ══════════════════════════════════════════════════════════

    void OnEnable()
    {
        alreadyHit.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        if (EsGolpePropio(other)) return;

        Damageable damageable = BuscarVidaEnEnemigo(other);
        if (damageable == null) return;

        if (YaFueGolpeadoEnEsteSwing(damageable)) return;

        string weaponTag = gameObject.tag;
        if (!damageable.CanBeDamagedBy(damageType, weaponTag)) return;

        GameObject source = ownerRoot != null ? ownerRoot.gameObject : gameObject;
        damageable.TakeDamage(damage, damageType, source);

        // ── USAMOS LA FUNCIÓN DEL EJERCICIO (compartida) ──
        ProcesarEstadoEspecial(damageable);
    }

    // =====================================================================
    // BLOQUE 2 — EJERCICIOS EN ESTILO C++
    // =====================================================================
    //
    // NOTA: Este archivo usa la MISMA función ProcesarEstadoEspecial
    // que RapierHitbox. El ejercicio está definido en RapierHitbox.cs.
    // Aquí solo se llama.
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │         HERRAMIENTAS QUE PUEDES USAR LIBREMENTE         │
    // │         (ya están implementadas, no las toques)         │
    // └─────────────────────────────────────────────────────────┘

    /// <summary>
    /// Convierte un EstadoEspecial de Damageable a constante int.
    /// </summary>
    int EstadoEspecialAConstante(Damageable.EstadoEspecial estado)
    {
        switch (estado)
        {
            case Damageable.EstadoEspecial.Sleep:   return ESTADO_SLEEP;
            case Damageable.EstadoEspecial.Confused: return ESTADO_CONFUSO;
            default:                                 return ESTADO_NINGUNO;
        }
    }

    /// <summary>
    /// Devuelve TRUE si el jugador tiene un estado especial seleccionado.
    /// </summary>
    bool JugadorTieneEstadoSeleccionado()
    {
        if (playerCombat == null) return false;
        return playerCombat.estadoSeleccionado != Damageable.EstadoEspecial.None;
    }

    /// <summary>
    /// Devuelve el estado especial seleccionado por el jugador como constante int.
    /// </summary>
    int EstadoJugador()
    {
        if (playerCombat == null) return ESTADO_NINGUNO;
        return EstadoEspecialAConstante(playerCombat.estadoSeleccionado);
    }

    /// <summary>
    /// Devuelve los golpes necesarios para aplicar un estado especial.
    /// </summary>
    int GolpesNecesarios()
    {
        return GOLPES_PARA_ESTADO;
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO — ProcesarEstadoEspecial (MISMO que RapierHitbox)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cuando el jugador golpea a un enemigo con una patada,
    //  se acumulan golpes. Al llegar a 8, se aplica el estado especial
    //  que el jugador haya seleccionado (sueño o confusión).
    //
    //  Este ejercicio es COMPARTIDO por RapierHitbox, KickCollider y
    //  LightSaberHitbox. Todos usan la misma lógica.
    //
    //  OBJETIVO: Recibe los golpes actuales del enemigo hacia el estado
    //  y el estado seleccionado por el jugador. Devuelve:
    //    - Si golpes < 8 → devuelve los golpes + 1 (acumula)
    //    - Si golpes >= 8 y estado == SLEEP → devuelve -1 (aplicar sleep)
    //    - Si golpes >= 8 y estado == CONFUSO → devuelve -2 (aplicar confused)
    //    - Si golpes >= 8 y estado == NINGUNO → devuelve 0 (resetear, no aplicar)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: GolpesNecesarios(), EstadoJugador(), if/else, switch/case
    //  ❌ NO uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="golpesActuales">Golpes acumulados hacia el estado.</param>
    /// <returns>
    ///   >= 0: nuevos golpes acumulados (solo se acumuló)
    ///   -1:   aplicar SLEEP
    ///   -2:   aplicar CONFUSED
    /// </returns>
    int ProcesarEstadoEspecial(int golpesActuales)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: acumulamos un golpe
        int nuevosGolpes = golpesActuales + 1;

        // PASO 2: ¿llegó al umbral?
        int umbral = GolpesNecesarios();

        if (nuevosGolpes < umbral)
        {
            return nuevosGolpes;
        }

        // PASO 3: llegó al umbral, ¿qué estado aplicar?
        int estadoJugador = EstadoJugador();

        switch (estadoJugador)
        {
            case ESTADO_SLEEP:
                return -1;

            case ESTADO_CONFUSO:
                return -2;

            case ESTADO_NINGUNO:
            default:
                return 0;
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // =====================================================================
    // BLOQUE 3 — TRABAJO SUCIO UNITY/C# (alumno NO toca)
    // =====================================================================

    /// <summary>
    /// Procesa el estado especial tras golpear a un enemigo con patada.
    /// USA la función del ejercicio (compartida con RapierHitbox).
    /// </summary>
    void ProcesarEstadoEspecial(Damageable damageable)
    {
        if (playerCombat == null)
            playerCombat = FindAnyObjectByType<SimpleWalk>();

        int estadoSeleccionado = EstadoJugador();

        if (estadoSeleccionado == ESTADO_NINGUNO)
            return;

        if (damageable.GetEstadoEspecial() != Damageable.EstadoEspecial.None)
            return;

        // ── Llamamos a la función del ejercicio ──
        int resultado = ProcesarEstadoEspecial(damageable.golpesRecibidosParaEstado);

        debugEJ_golpesParaEstado = resultado;
        debugEJ_estadoAplicado = (resultado == -1 || resultado == -2);

        if (resultado >= 0)
        {
            damageable.golpesRecibidosParaEstado = resultado;
        }
        else if (resultado == -1)
        {
            if (damageable.AplicarSleep())
            {
                EnemyScript enemy = damageable.GetComponent<EnemyScript>();
                if (enemy != null)
                    enemy.ActivarSleep();
                Debug.Log($"[KickHitbox] SLEEP aplicado a '{damageable.name}' tras {GOLPES_PARA_ESTADO} golpes.");
            }
            damageable.golpesRecibidosParaEstado = 0;
        }
        else if (resultado == -2)
        {
            if (damageable.AplicarConfused())
            {
                EnemyScript enemy = damageable.GetComponent<EnemyScript>();
                if (enemy != null)
                    enemy.ActivarConfused();
                Debug.Log($"[KickHitbox] CONFUSED aplicado a '{damageable.name}' tras {GOLPES_PARA_ESTADO} golpes.");
            }
            damageable.golpesRecibidosParaEstado = 0;
        }
    }

    // ══════════════════════════════════════════════════════════
    // HELPERS DE COLISIÓN (se mantienen originales)
    // ══════════════════════════════════════════════════════════

    bool EsGolpePropio(Collider other)
    {
        return ownerRoot != null && other.transform.root == ownerRoot;
    }

    Damageable BuscarVidaEnEnemigo(Collider other)
    {
        return other.GetComponentInParent<Damageable>();
    }

    bool YaFueGolpeadoEnEsteSwing(Damageable damageable)
    {
        return !alreadyHit.Add(damageable);
    }

    public void Reactivar()
    {
        alreadyHit.Clear();
    }
}