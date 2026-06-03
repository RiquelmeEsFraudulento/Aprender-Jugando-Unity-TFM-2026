// ============================================================
// PlayerScriptSimpleWalk.cs — VERSIÓN ENCAPSULADA COMPLETA
// CON EJERCICIOS DIDÁCTICOS PARA BACHILLERATO TIC 1
// ============================================================
// Un solo archivo .cs con toda la funcionalidad original
// de SimpleWalk.cs, organizada en tres bloques:
//
//   BLOQUE 1 — Inicialización de variables + estructuras
//   BLOQUE 2 — Ejercicios en estilo C++ (el alumno trabaja aquí)
//   BLOQUE 3 — Funciones de trabajo sucio Unity/C# (alumno NO toca)
//
// TODA la funcionalidad original se mantiene íntegra.
// ============================================================

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// =====================================================================
// BLOQUE 1 — INICIALIZACIÓN DE VARIABLES Y ESTRUCTURAS
// =====================================================================

[RequireComponent(typeof(CharacterController))]
public class SimpleWalk : MonoBehaviour
{
    // ┌─────────────────────────────────────────────────────────┐
    // │  1A — CONSTANTES DEL ALUMNO (puede modificarlas)       │
    // └─────────────────────────────────────────────────────────┘

    public const int ESTADO_NINGUNO    = 0;
    public const int ESTADO_SUENO      = 1;
    public const int ESTADO_CONFUSO    = 2;

    public const int ARMA_PATADA       = 0;
    public const int ARMA_RAPIER       = 1;
    public const int ARMA_SABLE        = 2;

    public const int GOLPES_PARA_ESTADO = 8;  // golpes necesarios para dormir/confundir

    // ┌─────────────────────────────────────────────────────────┐
    // │  1B — ESTRUCTURA DE DATOS DEL ALUMNO (tipo ficha)       │
    // └─────────────────────────────────────────────────────────┘

    [System.Serializable]
    public struct DatosDelJugador
    {
        // ── Posición y movimiento ─────────────────────────────
        public float posicionX;
        public float posicionY;
        public float posicionZ;
        public float velocidadMovimiento;
        public float velocidadMaxima;

        // ── Dirección de movimiento ───────────────────────────
        public float direccionHorizontal;
        public float direccionVertical;

        // ── Estado de combate ─────────────────────────────────
        public bool estaAtacando;
        public bool estaContraatacando;
        public bool estaVivo;
        public int enemigoBloqueadoIndice;

        // ── Rangos de ataque ──────────────────────────────────
        public float rangoPatada;
        public float rangoRapier;
        public float rangoSable;

        // ── Estado especial elegible (7/8/9) ─────────────────
        public int estadoElegido;

        // ── Vida ──────────────────────────────────────────────
        public float vidaActual;
        public float vidaMaxima;
    }

    public DatosDelJugador datosJugador;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1C — VARIABLES GLOBALES PARA EL ALUMNO (debug)         │
    // └─────────────────────────────────────────────────────────┘

    [Header("═══ DEBUG VISIBLE PARA EL ALUMNO ═══")]
    public int debugNumeroEnemigosVivos;
    public int debugIndiceEnemigoCercano;
    public float debugDistanciaAlMasCercano;
    public string debugNombreEnemigoCercano;
    public bool debugHayEnemigos;
    public int debugContadorDormidos;
    public int debugContadorConfusos;
    public int debugContadorNormales;
    public int debugIndiceMasVida;
    public int debugIndiceMenosVida;
    public float debugDistanciaPromedio;
    public int debugIndiceMasLejano;
    public int debugEnemigosEnRangoPatada;
    public int debugEnemigosEnRangoRapier;
    public int debugEnemigosEnRangoSable;
    public int debugEnemigosAtacando;
    public int debugGolpesParaDormir;
    public int debugGolpesParaConfundir;
    public float debugVidaJugador;
    public float debugVidaMaximaJugador;
    public bool debugJugadorEstaMuerto;
    public int debugEnemigosEnRangoDeContraataque;
    public float debugDistanciaAlEnemigoBloqueado;
    public int debugArmaRecomendada;  // 0=patada, 1=rapier, 2=sable, -1=ninguna

    // ┌─────────────────────────────────────────────────────────┐
    // │  1D — VARIABLES PRIVADAS (C# Unity, no tocar)           │
    // └─────────────────────────────────────────────────────────┘

    private float speed = 2f;
    public Animator animator;
    public float walkAnimationSpeed = 0.25f;
    private CharacterController controller;
    private Vector3 moveDirection;

    public float detectionRadius = 10f;
    public float rotationDuration = 0.15f;
    public GameObject lockOnPrefab;
    public float lockOnHeightOffset = 2f;
    public float lockOnSmoothTime = 0.1f;
    public float rotationThreshold = 2f;
    private GameObject lockOnInstance;
    private Transform lockOnTransform;
    private Camera lockOnCamera;
    private Vector3 lockOnVelocity;
    public float kickRange = 1.5f;
    public float lightSaberRange = 5f;
    public float rapierRange = 3f;
    private Transform lockedEnemy;
    private bool isLockedOn = false;
    private bool manualUnlock = false;

    public Collider RapierCollider;
    public Collider LightSaberCollider;
    public Collider RightKickCollider;
    public Collider LeftKickCollider;
    public RapierHitbox Rapier;
    public WeaponHitbox LightSaber;
    public KickHitbox RightKick;
    public KickHitbox LeftKick;

    public float attackCooldown = 1.3f;
    public Vector2 moveAxis;
    public bool isAttackingEnemy = false;
    public bool isCountering = false;
    public Transform punchPosition;
    public GameObject lastHitCamera;
    public Transform lastHitFocusObject;
    public EnemyDetection enemyDetection;
    public UnityEvent<EnemyScript> OnHit;
    public UnityEvent<EnemyScript> OnCounterAttack;
    public UnityEvent<EnemyScript> OnTrajectory;
    public Damageable.EstadoEspecial estadoSeleccionado = Damageable.EstadoEspecial.None;
    public System.Action DamageEvent;

    private EnemyManager enemyManager;
    private EnemyScript lockedTarget;
    private Coroutine counterCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine damageCoroutine;
    private PlayerHUDController _hud;
    private string[] attacks;
    private PlayerHealth playerHealth;

    private const bool LOG_LOCK = false;
    private const bool LOG_COMBAT = false;
    private const bool LOG_COUNTER = false;

    // ══════════════════════════════════════════════════════════
    // START
    // ══════════════════════════════════════════════════════════

    void Start()
    {
        controller = GetComponent<CharacterController>();
        enemyManager = FindAnyObjectByType<EnemyManager>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (enemyDetection == null)
            enemyDetection = GetComponentInChildren<EnemyDetection>();

        _hud = FindAnyObjectByType<PlayerHUDController>();
        playerHealth = GetComponent<PlayerHealth>();

        InicializarDatosDelJugador();

        attacks = new string[] {
            "TrCrescent", "TrChut",
            "TrRaSwing", "TrRa360",
            "TrSwSwing", "TrSw360"
        };
    }

    void InicializarDatosDelJugador()
    {
        datosJugador.posicionX = transform.position.x;
        datosJugador.posicionY = transform.position.y;
        datosJugador.posicionZ = transform.position.z;
        datosJugador.velocidadMovimiento = speed;
        datosJugador.velocidadMaxima = 2f;
        datosJugador.direccionHorizontal = 0f;
        datosJugador.direccionVertical = 0f;
        datosJugador.estaAtacando = false;
        datosJugador.estaContraatacando = false;
        datosJugador.estaVivo = true;
        datosJugador.enemigoBloqueadoIndice = -1;
        datosJugador.rangoPatada = kickRange;
        datosJugador.rangoRapier = rapierRange;
        datosJugador.rangoSable = lightSaberRange;
        datosJugador.estadoElegido = ESTADO_NINGUNO;

        if (playerHealth != null)
        {
            datosJugador.vidaActual = playerHealth.vida;
            datosJugador.vidaMaxima = playerHealth.vidaMax;
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
    /// Devuelve "infinito": la distancia más grande posible.
    /// Úsala para inicializar la mejor distancia al empezar la búsqueda.
    /// </summary>
    float DistanciaInfinita() => float.PositiveInfinity;

    /// <summary>
    /// Recibe una distancia y devuelve true si es un número real válido,
    /// o false si es infinito o inválido (enemigo descartado).
    /// </summary>
    bool EsDistanciaValida(float d) => !float.IsInfinity(d) && !float.IsNaN(d);

    /// <summary>
    /// Calcula la distancia al enemigo en la posición i de la lista.
    /// ⚠️ Si el enemigo está muerto, inactivo o no es atacable,
    /// devuelve automáticamente infinito — tú no necesitas comprobarlo.
    /// </summary>
    float CalcularDistancia(int i)
    {
        EnemyScript e = ObtenerEnemyScript(i);
        if (e == null || !e.isActiveAndEnabled || !e.IsAttackable())
            return float.PositiveInfinity;
        return Vector3.Distance(transform.position, e.transform.position);
    }

    /// <summary>
    /// Devuelve el número total de enemigos registrados.
    /// </summary>
    int NumeroDeEnemigos()
    {
        if (enemyManager == null || enemyManager.allEnemies == null) return 0;
        return enemyManager.allEnemies.Count;
    }

    /// <summary>
    /// Devuelve TRUE si el enemigo en la posición i está vivo y atacable.
    /// </summary>
    bool EstaEnemyVivo(int i)
    {
        EnemyScript e = ObtenerEnemyScript(i);
        if (e == null) return false;
        if (!e.isActiveAndEnabled) return false;
        if (!e.IsAttackable()) return false;
        return true;
    }

    /// <summary>
    /// Devuelve la vida actual del enemigo en la posición i.
    /// </summary>
    int VidaDelEnemigo(int i)
    {
        EnemyScript e = ObtenerEnemyScript(i);
        if (e == null) return 0;
        return e.currentHealth;
    }

    /// <summary>
    /// Devuelve TRUE si el enemigo i está preparando un ataque.
    /// </summary>
    bool EnemigoEstaAtacando(int i)
    {
        EnemyScript e = ObtenerEnemyScript(i);
        if (e == null) return false;
        return e.IsPreparingAttack();
    }

    /// <summary>
    /// Devuelve TRUE si el enemigo i está dormido.
    /// </summary>
    bool EnemigoEstaDormido(int i)
    {
        EnemyScript e = ObtenerEnemyScript(i);
        if (e == null) return false;
        return e.EstaDormido();
    }

    /// <summary>
    /// Devuelve TRUE si el enemigo i está confuso.
    /// </summary>
    bool EnemigoEstaConfuso(int i)
    {
        EnemyScript e = ObtenerEnemyScript(i);
        if (e == null) return false;
        return e.EstaConfuso();
    }

    /// <summary>
    /// Devuelve el número de golpes que lleva el enemigo i
    /// hacia el estado especial (sueño o confusión).
    /// </summary>
    int GolpesDelEnemigoParaEstado(int i)
    {
        EnemyScript e = ObtenerEnemyScript(i);
        if (e == null) return 0;
        return e.golpesRecibidosParaEstado;
    }

    /// <summary>
    /// Devuelve TRUE si el enemigo i ya recibió el estado Sleep.
    /// (Solo funciona en Boss).
    /// </summary>
    bool EnemigoRecibioSleep(int i)
    {
        // En un enemigo normal no existe este dato, devolvemos false
        // El alumno puede asumir que esto solo aplica al Boss
        return false;
    }

    /// <summary>
    /// Devuelve TRUE si el enemigo i ya recibió el estado Confused.
    /// (Solo funciona en Boss).
    /// </summary>
    bool EnemigoRecibioConfused(int i)
    {
        return false;
    }

    /// <summary>
    /// Devuelve la vida actual del jugador.
    /// </summary>
    float VidaJugador()
    {
        if (playerHealth == null) return 0f;
        return playerHealth.vida;
    }

    /// <summary>
    /// Devuelve la vida máxima del jugador.
    /// </summary>
    float VidaMaximaJugador()
    {
        if (playerHealth == null) return 0f;
        return playerHealth.vidaMax;
    }

    /// <summary>
    /// Devuelve TRUE si el jugador está muerto.
    /// </summary>
    bool JugadorEstaMuerto()
    {
        if (playerHealth == null) return true;
        return playerHealth.vida <= 0f;
    }

    /// <summary>
    /// Devuelve TRUE si el jugador está atacando ahora mismo.
    /// </summary>
    bool JugadorEstaAtacando() => isAttackingEnemy;

    /// <summary>
    /// Devuelve TRUE si el jugador está contraatacando ahora mismo.
    /// </summary>
    bool JugadorEstaContraatacando() => isCountering;

    /// <summary>
    /// Devuelve el estado especial seleccionado por el jugador.
    /// 0 = ninguno, 1 = sueño, 2 = confusión
    /// </summary>
    int EstadoElegido()
    {
        switch (estadoSeleccionado)
        {
            case Damageable.EstadoEspecial.Sleep:   return ESTADO_SUENO;
            case Damageable.EstadoEspecial.Confused: return ESTADO_CONFUSO;
            default:                                 return ESTADO_NINGUNO;
        }
    }

    /// <summary>
    /// Devuelve TRUE si la distancia está dentro del rango de patada.
    /// </summary>
    bool EnRangoDePatada(float distancia)
    {
        return distancia <= kickRange;
    }

    /// <summary>
    /// Devuelve TRUE si la distancia está dentro del rango de rapier.
    /// </summary>
    bool EnRangoDeRapier(float distancia)
    {
        return distancia <= rapierRange;
    }

    /// <summary>
    /// Devuelve TRUE si la distancia está dentro del rango de sable.
    /// </summary>
    bool EnRangoDeSable(float distancia)
    {
        return distancia <= lightSaberRange;
    }

    /// <summary>
    /// Devuelve TRUE si la distancia está dentro del rango de detección.
    /// </summary>
    bool EnRangoDeDeteccion(float distancia)
    {
        return distancia <= detectionRadius;
    }

    /// <summary>
    /// Devuelve TRUE si el jugador tiene un enemigo bloqueado (lock-on).
    /// </summary>
    bool TieneLockOn() => isLockedOn;


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 1 — Enemigo más cercano
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador necesita saber qué enemigo tiene más cerca
    //  para atacarlo. El juego recorre la lista de enemigos y busca
    //  el que esté vivo y a menor distancia.
    //
    //  OBJETIVO: Dado el número total de enemigos (n),
    //  devuelve el ÍNDICE del enemigo más cercano que sea válido.
    //  Si no hay ningún enemigo válido, devuelve -1.
    //
    //  REGLAS DE ORO:
    //  ✅ Solo puedes usar: DistanciaInfinita(), EsDistanciaValida(), CalcularDistancia()
    //  ❌ No uses: enemyManager, EnemyScript, Vector3 ni nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="numeroDeEnemigos">Número total de enemigos en la lista.</param>
    /// <returns>Índice del enemigo más cercano válido, o -1 si no hay ninguno.</returns>
    int EnemigoMasCercano(int numeroDeEnemigos)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: si no hay enemigos, salimos enseguida
        if (numeroDeEnemigos <= 0) return -1;

        // PASO 2: variables de trabajo para la búsqueda
        int indiceMejor = -1;
        float mejorDistancia = DistanciaInfinita();

        // PASO 3: recorremos todos los enemigos uno a uno
        for (int i = 0; i < numeroDeEnemigos; i++)
        {
            // PASO 4a: calculamos la distancia hasta el enemigo i
            float distanciaEnemigo = CalcularDistancia(i);

            // PASO 4b: comprobamos si esa distancia es un valor real
            bool esValida = EsDistanciaValida(distanciaEnemigo);

            // PASO 4c: solo actualizamos si es válido Y está más cerca
            if (esValida && distanciaEnemigo < mejorDistancia)
            {
                mejorDistancia = distanciaEnemigo;
                indiceMejor = i;
            }
        }

        // PASO 5: devolvemos el resultado
        return indiceMejor;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 2 — Enemigo más lejano
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: A veces el jugador quiere saber qué enemigo está más lejos
    //  para decidir si vale la pena perseguirlo o ignorarlo.
    //
    //  OBJETIVO: Dado el número total de enemigos (n),
    //  devuelve el ÍNDICE del enemigo VIVO más lejano.
    //  Si no hay ningún enemigo válido, devuelve -1.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: DistanciaInfinita(), EsDistanciaValida(), CalcularDistancia(), EstaEnemyVivo()
    //  ❌ No uses: enemyManager, EnemyScript, Vector3 ni nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="numeroDeEnemigos">Número total de enemigos en la lista.</param>
    /// <returns>Índice del enemigo vivo más lejano, o -1 si no hay ninguno.</returns>
    int EnemigoMasLejano(int numeroDeEnemigos)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: si no hay enemigos, salimos
        if (numeroDeEnemigos <= 0) return -1;

        // PASO 2: variables — empezamos con la distancia MÁS PEQUEÑA posible
        // porque queremos encontrar el MÁS LEJANO (el mayor)
        int indiceMejor = -1;
        float mayorDistancia = -1f;  // -1 significa "no encontrado todavía"

        // PASO 3: recorremos todos los enemigos
        for (int i = 0; i < numeroDeEnemigos; i++)
        {
            // PASO 4a: ¿está vivo?
            bool vivo = EstaEnemyVivo(i);

            // PASO 4b: calculamos distancia
            float distancia = CalcularDistancia(i);

            // PASO 4c: ¿es válida?
            bool valida = EsDistanciaValida(distancia);

            // PASO 4d: solo actualizamos si está vivo, es válido Y está más lejos
            if (vivo && valida && distancia > mayorDistancia)
            {
                mayorDistancia = distancia;
                indiceMejor = i;
            }
        }

        // PASO 5: devolvemos el resultado
        return indiceMejor;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 3 — Enemigo con más vida
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador quiere saber qué enemigo tiene más vida
    //  para decidir si atacarlo primero (estrategia) o ignorarlo.
    //
    //  OBJETIVO: Recorre todos los enemigos vivos y devuelve el índice
    //  del que tiene MÁS vida. Si no hay ninguno, devuelve -1.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: NumeroDeEnemigos(), EstaEnemyVivo(), VidaDelEnemigo()
    //  ❌ No uses: enemyManager, EnemyScript, Vector3 ni nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>Índice del enemigo vivo con más vida, o -1 si no hay ninguno.</returns>
    int EnemigoConMasVida()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos el total de enemigos
        int total = NumeroDeEnemigos();

        // PASO 2: variables — empezamos con -1 (no encontrado)
        int indiceMejor = -1;
        int mejorVida = -1;  // -1 significa "no encontrado todavía"

        // PASO 3: recorremos todos los enemigos
        for (int i = 0; i < total; i++)
        {
            // PASO 4a: ¿está vivo?
            bool vivo = EstaEnemyVivo(i);

            // PASO 4b: obtenemos su vida
            int vida = VidaDelEnemigo(i);

            // PASO 4c: solo actualizamos si está vivo Y tiene más vida
            if (vivo && vida > mejorVida)
            {
                mejorVida = vida;
                indiceMejor = i;
            }
        }

        // PASO 5: devolvemos el resultado
        return indiceMejor;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 4 — Enemigo con menos vida
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador quiere rematar al enemigo más débil.
    //  Busca al que tenga menos vida para eliminarlo rápido.
    //
    //  OBJETIVO: Recorre todos los enemigos vivos y devuelve el índice
    //  del que tiene MENOS vida. Si no hay ninguno, devuelve -1.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: NumeroDeEnemigos(), EstaEnemyVivo(), VidaDelEnemigo()
    //  ❌ No uses: enemyManager, EnemyScript, Vector3 ni nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>Índice del enemigo vivo con menos vida, o -1 si no hay ninguno.</returns>
    int EnemigoConMenosVida()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos el total
        int total = NumeroDeEnemigos();

        // PASO 2: variables — empezamos con un valor MUY ALTO
        // porque queremos encontrar el MENOR
        int indiceMejor = -1;
        int menorVida = 999999;  // un número muy grande = "no encontrado"

        // PASO 3: recorremos todos los enemigos
        for (int i = 0; i < total; i++)
        {
            // PASO 4a: ¿está vivo?
            bool vivo = EstaEnemyVivo(i);

            // PASO 4b: obtenemos su vida
            int vida = VidaDelEnemigo(i);

            // PASO 4c: solo actualizamos si está vivo Y tiene menos vida
            if (vivo && vida < menorVida)
            {
                menorVida = vida;
                indiceMejor = i;
            }
        }

        // PASO 5: devolvemos el resultado
        return indiceMejor;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 5 — Contar enemigos por estado (switch/case)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador necesita saber cuántos enemigos están
    //  dormidos, confusos o en estado normal para decidir su estrategia.
    //  Este ejercicio usa switch/case para clasificarlos.
    //
    //  OBJETIVO: Recibe un estado (0=normal, 1=sueño, 2=confusión)
    //  y devuelve cuántos enemigos vivos están en ese estado.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: NumeroDeEnemigos(), EstaEnemyVivo(), EnemigoEstaDormido(), EnemigoEstaConfuso()
    //  ❌ No uses: enemyManager, EnemyScript, Vector3 ni nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="estado">0=normal, 1=sueño, 2=confusión</param>
    /// <returns>Número de enemigos vivos en ese estado.</returns>
    int ContarEnemigosPorEstado(int estado)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos el total
        int total = NumeroDeEnemigos();

        // PASO 2: contador empieza en 0
        int contador = 0;

        // PASO 3: recorremos todos los enemigos
        for (int i = 0; i < total; i++)
        {
            // PASO 4a: ¿está vivo? Si no, lo saltamos
            if (!EstaEnemyVivo(i)) continue;

            // PASO 4b: usamos switch para comprobar el estado
            switch (estado)
            {
                case ESTADO_SUENO:
                    // PASO 4c: si buscamos dormidos, comprobamos
                    if (EnemigoEstaDormido(i))
                        contador = contador + 1;
                    break;

                case ESTADO_CONFUSO:
                    // PASO 4c: si buscamos confusos, comprobamos
                    if (EnemigoEstaConfuso(i))
                        contador = contador + 1;
                    break;

                case ESTADO_NINGUNO:
                default:
                    // PASO 4c: si buscamos normales, comprobamos que NO esté dormido NI confuso
                    if (!EnemigoEstaDormido(i) && !EnemigoEstaConfuso(i))
                        contador = contador + 1;
                    break;
            }
        }

        // PASO 5: devolvemos el contador
        return contador;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 6 — Contar enemigos en rango de ataque (if/else anidados)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador necesita saber cuántos enemigos están
    //  dentro de cada rango de ataque para decidir qué arma usar.
    //   · Patada:  1.5 metros
    //   · Rapier:  3 metros
    //   · Sable:   5 metros
    //
    //  OBJETIVO: Recibe un tipo de arma (0=patada, 1=rapier, 2=sable)
    //  y devuelve cuántos enemigos vivos están dentro de ese rango.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: NumeroDeEnemigos(), EstaEnemyVivo(), CalcularDistancia(),
    //          EsDistanciaValida(), EnRangoDePatada(), EnRangoDeRapier(), EnRangoDeSable()
    //  ❌ No uses: enemyManager, EnemyScript, Vector3 ni nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="tipoArma">0=patada, 1=rapier, 2=sable</param>
    /// <returns>Número de enemigos vivos dentro del rango de esa arma.</returns>
    int ContarEnemigosEnRangoDeArma(int tipoArma)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos el total
        int total = NumeroDeEnemigos();

        // PASO 2: contador empieza en 0
        int contador = 0;

        // PASO 3: recorremos todos los enemigos
        for (int i = 0; i < total; i++)
        {
            // PASO 4a: ¿está vivo?
            if (!EstaEnemyVivo(i)) continue;

            // PASO 4b: calculamos la distancia
            float distancia = CalcularDistancia(i);

            // PASO 4c: ¿es válida?
            if (!EsDistanciaValida(distancia)) continue;

            // PASO 4d: según el arma, comprobamos el rango
            if (tipoArma == ARMA_PATADA)
            {
                if (EnRangoDePatada(distancia))
                    contador = contador + 1;
            }
            else if (tipoArma == ARMA_RAPIER)
            {
                if (EnRangoDeRapier(distancia))
                    contador = contador + 1;
            }
            else if (tipoArma == ARMA_SABLE)
            {
                if (EnRangoDeSable(distancia))
                    contador = contador + 1;
            }
        }

        // PASO 5: devolvemos el contador
        return contador;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 7 — Distancia promedio a enemigos vivos (for + acumulador)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador quiere saber si en general los enemigos
    //  están cerca o lejos. Calcula la distancia promedio.
    //
    //  OBJETIVO: Recorre todos los enemigos vivos, suma sus distancias,
    //  y devuelve el promedio. Si no hay enemigos, devuelve 0.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: NumeroDeEnemigos(), EstaEnemyVivo(), CalcularDistancia(), EsDistanciaValida()
    //  ❌ No uses: enemyManager, EnemyScript, Vector3 ni nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>Distancia promedio a enemigos vivos, o 0 si no hay ninguno.</returns>
    float DistanciaPromedioAEnemigos()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos el total
        int total = NumeroDeEnemigos();

        // PASO 2: variables para acumular
        float suma = 0f;
        int conteo = 0;

        // PASO 3: recorremos todos los enemigos
        for (int i = 0; i < total; i++)
        {
            // PASO 4a: ¿está vivo?
            if (!EstaEnemyVivo(i)) continue;

            // PASO 4b: calculamos distancia
            float distancia = CalcularDistancia(i);

            // PASO 4c: ¿es válida?
            if (!EsDistanciaValida(distancia)) continue;

            // PASO 4d: acumulamos
            suma = suma + distancia;
            conteo = conteo + 1;
        }

        // PASO 5: calculamos el promedio (cuidado: no dividir entre 0)
        if (conteo <= 0) return 0f;

        float promedio = suma / (float)conteo;
        return promedio;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 8 — Contar enemigos que están atacando al jugador
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador necesita saber cuántos enemigos están
    //  preparando un ataque contra él para decidir si esquivar o contraatacar.
    //
    //  OBJETIVO: Recorre todos los enemigos y devuelve cuántos están
    //  preparando un ataque (IsPreparingAttack).
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: NumeroDeEnemigos(), EstaEnemyVivo(), EnemigoEstaAtacando()
    //  ❌ No uses: enemyManager, EnemyScript, Vector3 ni nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>Número de enemigos vivos que están preparando un ataque.</returns>
    int ContarEnemigosAtacando()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos el total
        int total = NumeroDeEnemigos();

        // PASO 2: contador empieza en 0
        int contador = 0;

        // PASO 3: recorremos todos los enemigos
        for (int i = 0; i < total; i++)
        {
            // PASO 4a: ¿está vivo?
            if (!EstaEnemyVivo(i)) continue;

            // PASO 4b: ¿está atacando?
            if (EnemigoEstaAtacando(i))
            {
                contador = contador + 1;
            }
        }

        // PASO 5: devolvemos el contador
        return contador;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 9 — ¿Puede el jugador contraatacar? (condiciones compuestas && ||)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador solo puede contraatacar si:
    //   1. NO está atacando él mismo
    //   2. NO está ya contraatacando
    //   3. Hay al menos un enemigo vivo
    //   4. Hay al menos un enemigo preparando un ataque
    //
    //  OBJETIVO: Devuelve true si TODAS las condiciones se cumplen.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: JugadorEstaAtacando(), JugadorEstaContraatacando(),
    //          NumeroDeEnemigos(), ContarEnemigosAtacando()
    //  ❌ No uses: enemyManager, EnemyScript, Vector3 ni nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>true si el jugador puede contraatacar ahora mismo.</returns>
    bool PuedeContraatacar()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: ¿está atacando? Si sí, NO puede contraatacar
        if (JugadorEstaAtacando()) return false;

        // PASO 2: ¿está ya contraatacando? Si sí, NO puede
        if (JugadorEstaContraatacando()) return false;

        // PASO 3: ¿hay enemigos vivos? Si no, NO puede
        int total = NumeroDeEnemigos();
        if (total <= 0) return false;

        // PASO 4: ¿hay algún enemigo atacando? Si no, NO puede
        int atacando = ContarEnemigosAtacando();
        if (atacando <= 0) return false;

        // PASO 5: si llegamos aquí, todas las condiciones se cumplen
        return true;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 10 — ¿Cuántos golpes faltan para dormir al enemigo? (while)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador necesita golpear 8 veces a un enemigo
    //  para aplicarle el estado de sueño. Este ejercicio calcula
    //  cuántos golpes faltan para llegar al umbral.
    //
    //  OBJETIVO: Recibe el índice de un enemigo y devuelve cuántos
    //  golpes faltan para llegar a 8 (el umbral).
    //  Si ya tiene 8 o más, devuelve 0.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: GolpesDelEnemigoParaEstado(), GOLPES_PARA_ESTADO
    //  ❌ No uses: enemyManager, EnemyScript, Vector3 ni nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="indiceEnemigo">Índice del enemigo en la lista.</param>
    /// <returns>Golpes que faltan para llegar al umbral de 8.</returns>
    int GolpesFaltantesParaEstado(int indiceEnemigo)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos los golpes que ya lleva
        int golpesActuales = GolpesDelEnemigoParaEstado(indiceEnemigo);

        // PASO 2: calculamos cuántos faltan
        int faltan = GOLPES_PARA_ESTADO - golpesActuales;

        // PASO 3: si ya tiene 8 o más, devolvemos 0 (no faltan)
        if (faltan < 0) faltan = 0;

        return faltan;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 11 — Simular turnos de veneno (while con contador)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El veneno quita 1 vida por turno durante N turnos.
    //  Este ejercicio simula cuántos turnos tarda en quitar
    //  toda la vida restante de un enemigo.
    //
    //  OBJETIVO: Recibe la vida actual del enemigo y el daño por turno.
    //  Devuelve cuántos turnos tarda en llegar a 0 o menos.
    //  Usa un while con contador.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: while, contador, variables int
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="vidaActual">Vida actual del enemigo.</param>
    /// <param name="danyoPorTurno">Daño que hace el veneno cada turno.</param>
    /// <returns>Número de turnos hasta que la vida llegue a 0 o menos.</returns>
    int TurnosDeVenenoHastaMatar(int vidaActual, int danyoPorTurno)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: caso especial — si no hay daño, nunca muere
        if (danyoPorTurno <= 0) return -1;  // -1 = infinito

        // PASO 2: variables para el bucle
        int vidaRestante = vidaActual;
        int turnos = 0;

        // PASO 3: mientras quede vida, seguimos contando turnos
        while (vidaRestante > 0)
        {
            vidaRestante = vidaRestante - danyoPorTurno;  // el veneno quita vida
            turnos = turnos + 1;                           // un turno más
        }

        // PASO 4: devolvemos el número de turnos
        return turnos;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 12 — ¿Qué arma recomendar según la distancia? (if/else)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador tiene tres armas con diferentes rangos:
    //   · Patada:  1.5 metros (la más corta pero rápida)
    //   · Rapier:  3 metros (intermedia)
    //   · Sable:   5 metros (la más larga)
    //
    //  OBJETIVO: Recibe una distancia y devuelve el arma recomendada:
    //   · Si distancia <= 1.5 → devuelve 0 (patada, todas sirven pero patada es rápida)
    //   · Si distancia <= 3   → devuelve 1 (rapier)
    //   · Si distancia <= 5   → devuelve 2 (sable)
    //   · Si distancia > 5    → devuelve -1 (fuera de rango)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: EnRangoDePatada(), EnRangoDeRapier(), EnRangoDeSable()
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="distancia">Distancia al enemigo en metros.</param>
    /// <returns>0=patada, 1=rapier, 2=sable, -1=fuera de rango</returns>
    int ArmaRecomendada(float distancia)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: ¿está en rango de patada?
        if (EnRangoDePatada(distancia))
        {
            return ARMA_PATADA;  // 0
        }
        // PASO 2: ¿está en rango de rapier?
        else if (EnRangoDeRapier(distancia))
        {
            return ARMA_RAPIER;  // 1
        }
        // PASO 3: ¿está en rango de sable?
        else if (EnRangoDeSable(distancia))
        {
            return ARMA_SABLE;   // 2
        }
        // PASO 4: está demasiado lejos
        else
        {
            return -1;  // fuera de rango
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 13 — ¿El jugador está en peligro? (condiciones compuestas)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador está en peligro si:
    //   1. Su vida está por debajo del 30% del máximo, O
    //   2. Hay más de 3 enemigos vivos atacando al mismo tiempo
    //
    //  OBJETIVO: Devuelve true si el jugador está en peligro.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: VidaJugador(), VidaMaximaJugador(), ContarEnemigosAtacando()
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>true si el jugador está en peligro.</returns>
    bool JugadorEnPeligro()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: calculamos el 30% de la vida máxima
        float vidaMax = VidaMaximaJugador();
        float umbralPeligro = vidaMax * 0.30f;  // 30% de la vida máxima

        // PASO 2: ¿la vida actual está por debajo del 30%?
        float vida = VidaJugador();
        bool vidaBaja = vida < umbralPeligro;

        // PASO 3: ¿hay más de 3 enemigos atacando?
        int atacando = ContarEnemigosAtacando();
        bool muchosAtacando = atacando > 3;

        // PASO 4: está en peligro si se cumple ALGUNA de las dos condiciones
        if (vidaBaja || muchosAtacando)
            return true;
        else
            return false;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 14 — Contar golpes totales para dormir a TODOS los enemigos (for + acumulador)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador quiere dormir a todos los enemigos.
    //  Cada enemigo necesita 8 golpes. Este ejercicio calcula
    //  cuántos golpes totales faltan para dormir a todos.
    //
    //  OBJETIVO: Recorre todos los enemigos vivos y suma cuántos
    //  golpes faltan en total para que todos estén dormidos.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: NumeroDeEnemigos(), EstaEnemyVivo(), GolpesDelEnemigoParaEstado(),
    //          GOLPES_PARA_ESTADO
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>Total de golpes que faltan para dormir a todos los enemigos vivos.</returns>
    int GolpesTotalesParaDormirATodos()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos el total
        int total = NumeroDeEnemigos();

        // PASO 2: acumulador de golpes
        int golpesTotales = 0;

        // PASO 3: recorremos todos los enemigos
        for (int i = 0; i < total; i++)
        {
            // PASO 4a: ¿está vivo?
            if (!EstaEnemyVivo(i)) continue;

            // PASO 4b: golpes que lleva este enemigo
            int golpesActuales = GolpesDelEnemigoParaEstado(i);

            // PASO 4c: golpes que faltan para este enemigo
            int faltan = GOLPES_PARA_ESTADO - golpesActuales;
            if (faltan < 0) faltan = 0;

            // PASO 4d: acumulamos al total
            golpesTotales = golpesTotales + faltan;
        }

        // PASO 5: devolvemos el total
        return golpesTotales;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 15 — ¿Cuántos turnos de sangrado faltan para explotar? (for + contador)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El sangrado se "explota" cada 5 golpes, haciendo
    //  daño extra. Este ejercicio calcula cuántos golpes faltan
    //  para la próxima explosión de sangrado.
    //
    //  OBJETIVO: Recibe los golpes de sangrado actuales y devuelve
    //  cuántos faltan para llegar a 5. Si ya tiene 5 o más, devuelve 0.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: variables int, if/else
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="golpesSangradoActuales">Golpes de sangrado acumulados (0-4).</param>
    /// <returns>Golpes que faltan para la explosión de sangrado.</returns>
    int GolpesParaExplotarSangrado(int golpesSangradoActuales)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: constante — cada cuántos golpes explota
        int golpesParaExplotar = 5;

        // PASO 2: calculamos cuántos faltan
        int faltan = golpesParaExplotar - golpesSangradoActuales;

        // PASO 3: si ya tiene 5 o más, devolvemos 0
        if (faltan < 0) faltan = 0;

        return faltan;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 16 — ¿El boss está vulnerable? (condiciones compuestas &&)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El boss solo es vulnerable cuando:
    //   1. Ya recibió el estado Sleep (dormido al menos una vez), Y
    //   2. Ya recibió el estado Confused (confundido al menos una vez)
    //
    //  OBJETIVO: Recibe dos bool (recibioSleep, recibioConfused)
    //  y devuelve true si el boss es vulnerable.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: variables bool, operador &&
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="recibioSleep">true si el boss ya fue dormido.</param>
    /// <param name="recibioConfused">true si el boss ya fue confundido.</param>
    /// <returns>true si el boss es vulnerable al golpe final.</returns>
    bool BossEsVulnerable(bool recibioSleep, bool recibioConfused)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: el boss es vulnerable si recibió AMBOS estados
        if (recibioSleep && recibioConfused)
            return true;
        else
            return false;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 17 — ¿Cuántos enemigos están en rango de contraataque? (for + if)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El contraataque solo funciona si el enemigo está
    //  a menos de 5 metros. Este ejercicio cuenta cuántos enemigos
    //  están en ese rango.
    //
    //  OBJETIVO: Recorre todos los enemigos vivos y devuelve cuántos
    //  están a menos de 5 metros del jugador.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: NumeroDeEnemigos(), EstaEnemyVivo(), CalcularDistancia(),
    //          EsDistanciaValida()
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>Número de enemigos vivos a menos de 2 metros.</returns>
    int EnemigosEnRangoDeContraataque()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: constante — rango de contraataque
        float rangoContraataque = 5.0f;

        // PASO 2: obtenemos el total
        int total = NumeroDeEnemigos();

        // PASO 3: contador
        int contador = 0;

        // PASO 4: recorremos todos los enemigos
        for (int i = 0; i < total; i++)
        {
            // PASO 4a: ¿está vivo?
            if (!EstaEnemyVivo(i)) continue;

            // PASO 4b: calculamos distancia
            float distancia = CalcularDistancia(i);

            // PASO 4c: ¿es válida y está en rango?
            if (EsDistanciaValida(distancia) && distancia <= rangoContraataque)
            {
                contador = contador + 1;
            }
        }

        // PASO 5: devolvemos el contador
        return contador;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 18 — ¿Qué estado especial aplicar? (switch/case)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador pulsa 7, 8 o 9 para elegir un estado.
    //  Este ejercicio recibe la tecla pulsada y devuelve qué estado
    //  se debe aplicar.
    //
    //  OBJETIVO: Recibe un código de tecla (7, 8, 9) y devuelve:
    //   · 7 → 1 (sueño)
    //   · 8 → 2 (confusión)
    //   · 9 → 0 (ninguno)
    //   · cualquier otra → 0 (ninguno)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: switch/case
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="tecla">Código de tecla pulsada (7, 8, 9).</param>
    /// <returns>Estado seleccionado: 0=ninguno, 1=sueño, 2=confusión</returns>
    int EstadoSegunTecla(int tecla)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: usamos switch para decidir según la tecla
        switch (tecla)
        {
            case 7:
                return ESTADO_SUENO;    // 1

            case 8:
                return ESTADO_CONFUSO;  // 2

            case 9:
                return ESTADO_NINGUNO;  // 0

            default:
                return ESTADO_NINGUNO;  // si no es 7, 8 o 9, no hay estado
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 19 — Simular cooldown de ataque (while con temporizador)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Después de atacar, el jugador debe esperar 1.3 segundos
    //  antes de poder atacar de nuevo. Este ejercicio simula cuántos
    //  "tics" de 0.1 segundos caben en el cooldown.
    //
    //  OBJETIVO: Recibe el tiempo de cooldown (en segundos) y devuelve
    //  cuántos tics de 0.1 segundos hay que esperar.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: while, contador, float
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="tiempoCooldown">Tiempo de cooldown en segundos (ej: 1.3).</param>
    /// <returns>Número de tics de 0.1s que hay que esperar.</returns>
    int TicsDeCooldown(float tiempoCooldown)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: constante — duración de cada tic
        float duracionTic = 0.1f;

        // PASO 2: variables para el bucle
        float tiempoTranscurrido = 0f;
        int tics = 0;

        // PASO 3: mientras no hayamos llegado al cooldown, seguimos
        while (tiempoTranscurrido < tiempoCooldown)
        {
            tiempoTranscurrido = tiempoTranscurrido + duracionTic;
            tics = tics + 1;
        }

        // PASO 4: devolvemos el número de tics
        return tics;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 20 — ¿El jugador puede atacar ahora? (if/else con múltiples condiciones)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador puede atacar si:
    //   1. NO está ya atacando, Y
    //   2. NO está contraatacando, Y
    //   3. Hay al menos un enemigo vivo
    //
    //  OBJETIVO: Devuelve true si el jugador puede atacar ahora.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: JugadorEstaAtacando(), JugadorEstaContraatacando(), NumeroDeEnemigos()
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>true si el jugador puede atacar ahora.</returns>
    bool PuedeAtacar()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: ¿está atacando? Si sí, NO puede
        if (JugadorEstaAtacando()) return false;

        // PASO 2: ¿está contraatacando? Si sí, NO puede
        if (JugadorEstaContraatacando()) return false;

        // PASO 3: ¿hay enemigos vivos? Si no, NO puede
        int total = NumeroDeEnemigos();
        if (total <= 0) return false;

        // PASO 4: si llegamos aquí, puede atacar
        return true;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  FUNCIONES DE ACTUALIZACIÓN DE DEBUG (llamadas cada frame)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Actualiza todas las variables de debug visibles para el alumno.
    /// Se llama cada frame.
    /// </summary>
    void ActualizarDebug()
    {
        int total = NumeroDeEnemigos();

        debugNumeroEnemigosVivos = total;
        debugHayEnemigos = total > 0;
        debugIndiceEnemigoCercano = EnemigoMasCercano(total);

        if (debugIndiceEnemigoCercano >= 0)
        {
            debugDistanciaAlMasCercano = CalcularDistancia(debugIndiceEnemigoCercano);
            EnemyScript e = ObtenerEnemyScript(debugIndiceEnemigoCercano);
            debugNombreEnemigoCercano = e != null ? e.name : "null";
        }
        else
        {
            debugDistanciaAlMasCercano = 0f;
            debugNombreEnemigoCercano = "ninguno";
        }

        debugContadorDormidos = ContarEnemigosPorEstado(ESTADO_SUENO);
        debugContadorConfusos = ContarEnemigosPorEstado(ESTADO_CONFUSO);
        debugContadorNormales = ContarEnemigosPorEstado(ESTADO_NINGUNO);

        debugIndiceMasVida = EnemigoConMasVida();
        debugIndiceMenosVida = EnemigoConMenosVida();
        debugDistanciaPromedio = DistanciaPromedioAEnemigos();
        debugIndiceMasLejano = EnemigoMasLejano(total);

        debugEnemigosEnRangoPatada = ContarEnemigosEnRangoDeArma(ARMA_PATADA);
        debugEnemigosEnRangoRapier = ContarEnemigosEnRangoDeArma(ARMA_RAPIER);
        debugEnemigosEnRangoSable = ContarEnemigosEnRangoDeArma(ARMA_SABLE);
        debugEnemigosAtacando = ContarEnemigosAtacando();

        debugGolpesParaDormir = GOLPES_PARA_ESTADO;
        debugGolpesParaConfundir = GOLPES_PARA_ESTADO;

        debugVidaJugador = VidaJugador();
        debugVidaMaximaJugador = VidaMaximaJugador();
        debugJugadorEstaMuerto = JugadorEstaMuerto();

        debugEnemigosEnRangoDeContraataque = EnemigosEnRangoDeContraataque();

        if (isLockedOn && lockedEnemy != null)
        {
            debugDistanciaAlEnemigoBloqueado = Vector3.Distance(
                transform.position, lockedEnemy.position);
        }
        else
        {
            debugDistanciaAlEnemigoBloqueado = -1f;
        }

        // Arma recomendada según la distancia al más cercano
        if (debugIndiceEnemigoCercano >= 0)
        {
            debugArmaRecomendada = ArmaRecomendada(debugDistanciaAlMasCercano);
        }
        else
        {
            debugArmaRecomendada = -1;
        }
    }

    /// <summary>
    /// Sincroniza la estructura DatosDelJugador con el estado real.
    /// </summary>
    void SincronizarDatosDelJugador()
    {
        datosJugador.posicionX = transform.position.x;
        datosJugador.posicionY = transform.position.y;
        datosJugador.posicionZ = transform.position.z;
        datosJugador.velocidadMovimiento = speed;
        datosJugador.estaAtacando = isAttackingEnemy;
        datosJugador.estaContraatacando = isCountering;

        if (playerHealth != null)
        {
            datosJugador.vidaActual = playerHealth.vida;
            datosJugador.vidaMaxima = playerHealth.vidaMax;
        }

        switch (estadoSeleccionado)
        {
            case Damageable.EstadoEspecial.Sleep:   datosJugador.estadoElegido = ESTADO_SUENO;   break;
            case Damageable.EstadoEspecial.Confused: datosJugador.estadoElegido = ESTADO_CONFUSO; break;
            default:                                 datosJugador.estadoElegido = ESTADO_NINGUNO; break;
        }
    }


    // =====================================================================
    // BLOQUE 3 — TRABAJO SUCIO UNITY/C# (alumno NO toca estas funciones)
    // =====================================================================

    /// <summary>
    /// Busca el EnemyManager activo en la escena.
    /// </summary>
    EnemyManager BuscarEnemyManager()
    {
        if (enemyManager != null && enemyManager.gameObject != null
            && enemyManager.isActiveAndEnabled)
            return enemyManager;
        enemyManager = FindAnyObjectByType<EnemyManager>();
        return enemyManager;
    }

    /// <summary>
    /// Devuelve el EnemyScript en la posición i de la lista.
    /// </summary>
    EnemyScript ObtenerEnemyScript(int i)
    {
        if (enemyManager == null) return null;
        if (enemyManager.allEnemies == null) return null;
        if (i < 0 || i >= enemyManager.allEnemies.Count) return null;
        return enemyManager.allEnemies[i].enemyScript;
    }

    /// <summary>
    /// Activa el hitbox del Rapier.
    /// </summary>
    void ActivarHitboxRapier()
    {
        if (RapierCollider != null) RapierCollider.enabled = true;
        if (Rapier != null) Rapier.Reactivar();
    }

    /// <summary>
    /// Desactiva el hitbox del Rapier.
    /// </summary>
    void DesactivarHitboxRapier()
    {
        if (RapierCollider != null) RapierCollider.enabled = false;
    }

    /// <summary>
    /// Activa el hitbox del Sable de Luz.
    /// </summary>
    void ActivarHitboxSable()
    {
        if (LightSaberCollider != null) LightSaberCollider.enabled = true;
        if (LightSaber != null) LightSaber.Reactivar();
    }

    /// <summary>
    /// Desactiva el hitbox del Sable de Luz.
    /// </summary>
    void DesactivarHitboxSable()
    {
        if (LightSaberCollider != null) LightSaberCollider.enabled = false;
    }

    /// <summary>
    /// Activa el hitbox de la patada izquierda.
    /// </summary>
    void ActivarHitboxPatadaIzquierda()
    {
        if (LeftKickCollider != null) LeftKickCollider.enabled = true;
        if (LeftKick != null) LeftKick.Reactivar();
    }

    /// <summary>
    /// Desactiva el hitbox de la patada izquierda.
    /// </summary>
    void DesactivarHitboxPatadaIzquierda()
    {
        if (LeftKickCollider != null) LeftKickCollider.enabled = false;
    }

    /// <summary>
    /// Activa el hitbox de la patada derecha.
    /// </summary>
    void ActivarHitboxPatadaDerecha()
    {
        if (RightKickCollider != null) RightKickCollider.enabled = true;
        if (RightKick != null) RightKick.Reactivar();
    }

    /// <summary>
    /// Desactiva el hitbox de la patada derecha.
    /// </summary>
    void DesactivarHitboxPatadaDerecha()
    {
        if (RightKickCollider != null) RightKickCollider.enabled = false;
    }

    /// <summary>
    /// Ejecuta un trigger en el Animator.
    /// </summary>
    void EjecutarAnimacion(string nombre)
    {
        if (animator != null)
            animator.SetTrigger(nombre);
    }

    /// <summary>
    /// Aplica daño al jugador.
    /// </summary>
    void AplicarDanioAlJugador(float cantidad)
    {
        if (playerHealth == null) return;
        playerHealth.TakeDamage(cantidad);
    }

    /// <summary>
    /// Cura al jugador.
    /// </summary>
    void CurarJugador(float cantidad)
    {
        if (playerHealth == null) return;
        playerHealth.Curar(cantidad);
    }

    /// <summary>
    /// Devuelve TRUE si el Transform corresponde a un enemigo vivo.
    /// </summary>
    bool EstaEnemyVivoYActivo(Transform t)
    {
        if (t == null) return false;
        EnemyScript es = t.GetComponent<EnemyScript>();
        if (es == null) return false;
        if (!es.isActiveAndEnabled) return false;
        if (!es.IsAttackable()) return false;
        return true;
    }

    /// <summary>
    /// Devuelve el ángulo horizontal hacia un target.
    /// </summary>
    float AnguloHaciaTarget(Transform target)
    {
        if (target == null) return 0f;
        Vector3 dir = (target.position - transform.position).normalized;
        return Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Devuelve la distancia del jugador a un EnemyScript.
    /// </summary>
    float DistanciaAEnemy(EnemyScript target)
    {
        if (target == null) return float.MaxValue;
        return Vector3.Distance(transform.position, target.transform.position);
    }

    /// <summary>
    /// Devuelve TRUE si es el último enemigo y tiene 1 vida.
    /// </summary>
    bool EsUltimoGolpe()
    {
        if (lockedTarget == null) return false;
        if (enemyManager == null) return false;
        return enemyManager.aliveEnemyCount == 1 && lockedTarget.currentHealth <= 1;
    }

    /// <summary>
    /// Crea el indicador visual de lock-on sobre un enemigo.
    /// </summary>
    void CrearIndicadorLockOn(Transform target)
    {
        if (lockOnPrefab == null) return;
        if (lockOnInstance != null) Destroy(lockOnInstance);

        Vector3 spawnPos = target.position + Vector3.up * lockOnHeightOffset;
        lockOnInstance = Instantiate(lockOnPrefab, spawnPos, Quaternion.identity);
        lockOnTransform = lockOnInstance.transform;

        if (lockOnCamera == null) lockOnCamera = Camera.main;

        RangeIndicator ri = lockOnInstance.GetComponentInChildren<RangeIndicator>();
        if (ri != null) ri.Initialize(target, transform);
    }

    /// <summary>
    /// Actualiza la posición del indicador de lock-on cada frame.
    /// </summary>
    void ActualizarIndicadorLockOn()
    {
        if (!isLockedOn || lockOnInstance == null || lockedEnemy == null) return;

        Vector3 targetPos = lockedEnemy.position + Vector3.up * lockOnHeightOffset;
        lockOnTransform.position = Vector3.SmoothDamp(
            lockOnTransform.position, targetPos, ref lockOnVelocity, lockOnSmoothTime);

        if (lockOnCamera != null)
            lockOnTransform.forward = lockOnCamera.transform.forward;
    }

    /// <summary>
    /// Desbloquea el lock-on silenciosamente.
    /// </summary>
    void DesbloquearLockSilencioso()
    {
        isLockedOn = false;
        lockedEnemy = null;
        lockedTarget = null;
        transform.DOKill();

        if (lockOnInstance != null)
        {
            Destroy(lockOnInstance);
            lockOnInstance = null;
            lockOnTransform = null;
        }
    }

    /// <summary>
    /// Desbloqueo manual (con Tab).
    /// </summary>
    void DesbloquearLockManual()
    {
        DesbloquearLockSilencioso();
        manualUnlock = true;
    }

    /// <summary>
    /// Busca y lockea al enemigo más cercano automáticamente.
    /// </summary>
    void ForzarLockAlMasCercano()
    {
        if (enemyManager == null || enemyManager.aliveEnemyCount == 0) return;

        EnemyScript closest = null;
        float closestDist = Mathf.Infinity;

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript e = enemyManager.allEnemies[i].enemyScript;
            if (e == null || !e.isActiveAndEnabled || !e.IsAttackable()) continue;

            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < closestDist && d <= detectionRadius)
            {
                closestDist = d;
                closest = e;
            }
        }

        if (closest != null)
        {
            LockearEnemigo(closest.transform);
            manualUnlock = false;
        }
    }

    /// <summary>
    /// Lockea a un enemigo específico por su Transform.
    /// </summary>
    void LockearEnemigo(Transform target)
    {
        if (target == null) return;

        lockedEnemy = target;
        isLockedOn = true;

        EnemyScript es = target.GetComponent<EnemyScript>();
        if (es != null && es.IsAttackable())
            lockedTarget = es;
        else
            lockedTarget = null;

        transform.DOKill();
        transform.DOLookAt(
            target.position + Vector3.up * 1.5f,
            rotationDuration,
            AxisConstraint.Y,
            Vector3.up
        ).SetEase(Ease.OutSine);

        CrearIndicadorLockOn(target);
    }

    /// <summary>
    /// Cambia el lock al siguiente/enemigo en orden angular.
    /// </summary>
    void CambiarLockADireccion(int direccion)
    {
        if (lockedEnemy == null || enemyManager == null) return;

        EnemyScript best = null;
        float bestDiff = Mathf.Infinity;
        float currentAngle = AnguloHaciaTarget(lockedEnemy);

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript e = enemyManager.allEnemies[i].enemyScript;
            if (e == null || !e.isActiveAndEnabled || !e.IsAttackable()) continue;
            if (e.transform == lockedEnemy) continue;

            float angle = AnguloHaciaTarget(e.transform);
            float diff = Mathf.DeltaAngle(currentAngle, angle);

            if (direccion > 0 && diff > 0 && diff < bestDiff)
            {
                bestDiff = diff;
                best = e;
            }
            else if (direccion < 0 && diff < 0 && -diff < bestDiff)
            {
                bestDiff = -diff;
                best = e;
            }
        }

        if (best != null)
        {
            LockearEnemigo(best.transform);
        }
    }

    /// <summary>
    /// Valida si el lock actual sigue siendo válido.
    /// </summary>
    void ValidarLockActual()
    {
        if (!isLockedOn || lockedEnemy == null) return;

        if (!EstaEnemyVivoYActivo(lockedEnemy))
        {
            DesbloquearLockSilencioso();
            manualUnlock = false;
            ForzarLockAlMasCercano();
            return;
        }

        float dist = Vector3.Distance(transform.position, lockedEnemy.position);
        if (dist > detectionRadius)
        {
            DesbloquearLockSilencioso();
            manualUnlock = false;
        }
    }

    /// <summary>
    /// Gestiona el auto-lock cuando no hay lock activo.
    /// </summary>
    void GestionarAutoLock()
    {
        if (manualUnlock) return;
        if (isLockedOn && EstaEnemyVivoYActivo(lockedEnemy)) return;

        if (isLockedOn && lockedEnemy != null && !EstaEnemyVivoYActivo(lockedEnemy))
        {
            DesbloquearLockSilencioso();
        }

        if (enemyManager == null || enemyManager.aliveEnemyCount == 0) return;

        ForzarLockAlMasCercano();
    }

    /// <summary>
    /// Cambia el lock al enemigo que está atacando al jugador.
    /// </summary>
    void GestionarAutoSwitchAAttacker()
    {
        if (enemyManager == null) return;
        if (isAttackingEnemy || isCountering) return;

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript e = enemyManager.allEnemies[i].enemyScript;
            if (e == null) continue;
            if (!e.isActiveAndEnabled) continue;
            if (!e.IsAttackable()) continue;
            if (!e.IsPreparingAttack()) continue;

            if (lockedEnemy != e.transform)
            {
                LockearEnemigo(e.transform);
            }
            return;
        }
    }

    /// <summary>
    /// Decide cuál es el mejor enemigo para atacar.
    /// </summary>
    void ResolverTarget()
    {
        if (isLockedOn && EstaEnemyVivoYActivo(lockedEnemy))
        {
            lockedTarget = lockedEnemy.GetComponent<EnemyScript>();
            if (lockedTarget != null && lockedTarget.IsAttackable()) return;
            DesbloquearLockSilencioso();
        }

        if (enemyDetection != null && enemyDetection.InputMagnitude() > 0.2f)
        {
            EnemyScript detected = enemyDetection.CurrentTarget();
            if (detected != null && detected.IsAttackable())
            {
                lockedTarget = detected;
                return;
            }
        }

        if (enemyManager != null && enemyManager.aliveEnemyCount > 0)
        {
            lockedTarget = ObtenerEnemigoMasCercano();
            return;
        }

        lockedTarget = null;
    }

    /// <summary>
    /// Devuelve el EnemyScript del enemigo más cercano.
    /// </summary>
    EnemyScript ObtenerEnemigoMasCercano()
    {
        if (enemyManager == null) return null;

        EnemyScript closest = null;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript e = enemyManager.allEnemies[i].enemyScript;
            if (e == null || !e.isActiveAndEnabled || !e.IsAttackable()) continue;

            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = e;
            }
        }
        return closest;
    }

    /// <summary>
    /// Encuentra el enemigo más cercano que esté preparando ataque.
    /// </summary>
    EnemyScript EnemigoMasCercanoPreparandoAtaque()
    {
        if (enemyManager == null) return null;

        float minDistance = Mathf.Infinity;
        EnemyScript best = null;

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript enemy = enemyManager.allEnemies[i].enemyScript;
            if (enemy == null) continue;
            if (!enemy.isActiveAndEnabled) continue;
            if (!enemy.IsAttackable()) continue;
            if (!enemy.IsPreparingAttack()) continue;

            float d = Vector3.Distance(transform.position, enemy.transform.position);
            if (d < minDistance)
            {
                minDistance = d;
                best = enemy;
            }
        }
        return best;
    }

    /// <summary>
    /// Devuelve TRUE si un ataque específico está en rango.
    /// </summary>
    bool AtaqueEnRango(string nombreAtaque, float distancia)
    {
        if (nombreAtaque == "TrCrescent" || nombreAtaque == "TrChut")
            return distancia <= kickRange;
        if (nombreAtaque == "TrRaSwing" || nombreAtaque == "TrRa360")
            return distancia <= rapierRange;
        if (nombreAtaque == "TrSwSwing" || nombreAtaque == "TrSw360")
            return distancia <= lightSaberRange;
        return false;
    }

    /// <summary>
    /// Devuelve el pool de ataques válido según la distancia.
    /// </summary>
    string[] ObtenerPoolPorDistancia(float distancia)
    {
        if (distancia <= kickRange)
            return new string[] { "TrCrescent", "TrChut", "TrRaSwing", "TrRa360", "TrSwSwing", "TrSw360" };
        if (distancia <= rapierRange)
            return new string[] { "TrRaSwing", "TrRa360", "TrSwSwing", "TrSw360" };
        if (distancia <= lightSaberRange)
            return new string[] { "TrSwSwing", "TrSw360" };
        return null;
    }

    /// <summary>
    /// Elige un ataque de contraataque según tipo y distancia.
    /// </summary>
    string SeleccionarContraataque(string tipoCounter, float distancia)
    {
        string[] kickPool       = { "TrCrescent", "TrChut" };
        string[] rapierPool     = { "TrRaSwing", "TrRa360" };
        string[] lightsaberPool = { "TrSwSwing", "TrSw360" };
        string[] allPool        = { "TrCrescent", "TrChut", "TrRaSwing", "TrRa360", "TrSwSwing", "TrSw360" };

        string[] pool = null;

        switch (tipoCounter)
        {
            case "KICK":       pool = kickPool;       break;
            case "RAPIER":     pool = rapierPool;     break;
            case "LIGHTSABER": pool = lightsaberPool; break;
            default:           pool = allPool;        break;
        }

        bool anyInRange = false;
        foreach (string atk in pool)
        {
            if (AtaqueEnRango(atk, distancia)) { anyInRange = true; break; }
        }

        if (!anyInRange)
        {
            pool = ObtenerPoolPorDistancia(distancia);
            if (pool == null || pool.Length == 0) return null;
        }

        int idx = UnityEngine.Random.Range(0, pool.Length);
        return pool[idx];
    }

    /// <summary>
    /// Procesa el input de estado especial (teclas 7/8/9).
    /// </summary>
    void ProcesarEstadoEspecial()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.Sleep;
            datosJugador.estadoElegido = ESTADO_SUENO;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.Confused;
            datosJugador.estadoElegido = ESTADO_CONFUSO;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.None;
            datosJugador.estadoElegido = ESTADO_NINGUNO;
        }
    }

    /// <summary>
    /// Procesa el input de movimiento.
    /// </summary>
    void ProcesarMovimiento(float horizontal, float vertical)
    {
        if (isLockedOn && lockedEnemy != null && !isAttackingEnemy)
        {
            Vector3 toEnemy = (lockedEnemy.position - transform.position).normalized;
            toEnemy.y = 0;

            Vector3 right = Vector3.Cross(Vector3.up, toEnemy);
            right.y = 0;
            right.Normalize();

            moveDirection = (toEnemy * vertical + right * horizontal).normalized;

            if (toEnemy != Vector3.zero)
            {
                float angleDiff = Vector3.Angle(transform.forward, toEnemy);
                if (angleDiff > rotationThreshold)
                {
                    transform.DOKill();
                    transform.DOLookAt(
                    lockedEnemy.position + Vector3.up * 1.5f,
                    rotationDuration,
                    AxisConstraint.Y,
                    Vector3.up
                    ).SetEase(Ease.OutSine);
                }
            }

            if (moveDirection.magnitude >= 0.1f)
                controller.Move(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

            if (moveDirection.magnitude >= 0.1f && !isAttackingEnemy)
            {
                if (moveDirection != Vector3.zero)
                {
                    transform.DOKill();
                    transform.DORotateQuaternion(
                        Quaternion.LookRotation(moveDirection),
                        rotationDuration
                    );
                }
                controller.Move(moveDirection * speed * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Actualiza los parámetros del Animator.
    /// </summary>
    void ProcesarAnimator()
    {
        Vector3 localMove = transform.InverseTransformDirection(moveDirection);
        animator.SetFloat("VelocityX", localMove.x, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityY", localMove.z, 0.1f, Time.deltaTime);
        animator.SetFloat("WalkAnimSpeed", walkAnimationSpeed);
        animator.SetBool("isWalking", moveDirection.magnitude > 0.1f && !isAttackingEnemy);
    }

    /// <summary>
    /// Mueve al jugador hacia el target durante un ataque.
    /// </summary>
    void MoverHaciaTarget(EnemyScript target, float duration)
    {
        if (target == null) return;

        OnTrajectory?.Invoke(target);
        transform.DOLookAt(target.transform.position + Vector3.up * 1.5f, 0.2f, AxisConstraint.Y, Vector3.up);

        Vector3 offset = Vector3.MoveTowards(target.transform.position, transform.position, 0.95f);
        transform.DOMove(offset, duration);
    }

    /// <summary>
    /// Ejecuta un ataque con un trigger específico.
    /// </summary>
    void EjecutarAtaque(string triggerAtaque, float cooldown, EnemyScript target, float duracionMovimiento)
    {
        animator.SetTrigger(triggerAtaque);

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(
            CorrutinaAtaque(
                EsUltimoGolpe() ? 1.5f : cooldown,
                target,
                duracionMovimiento
            )
        );

        if (EsUltimoGolpe())
            StartCoroutine(CorrutinaGolpeFinal());
    }

    /// <summary>
    /// Corrutina de ataque: espera y restaura estado.
    /// </summary>
    IEnumerator CorrutinaAtaque(float duracion, EnemyScript target, float duracionMovimiento)
    {
        isAttackingEnemy = true;

        if (target != null)
        {
            target.StopMoving();
            MoverHaciaTarget(target, duracionMovimiento);
        }

        yield return new WaitForSeconds(duracion);
        isAttackingEnemy = false;

        if (lockedTarget != null)
            lockedTarget.ReleaseLock();

        speed = 0f;
        yield return new WaitForSeconds(0.2f);
        DOVirtual.Float(0f, 2f, 0.6f, v => speed = v);
    }

    /// <summary>
    /// Corrutina de golpe final (cámara lenta).
    /// </summary>
    IEnumerator CorrutinaGolpeFinal()
    {
        Time.timeScale = 0.5f;
        if (lastHitCamera != null) lastHitCamera.SetActive(true);
        if (lastHitFocusObject != null)
            lastHitFocusObject.position = lockedTarget != null
                ? lockedTarget.transform.position
                : transform.position;

        yield return new WaitForSecondsRealtime(2f);

        if (lastHitCamera != null) lastHitCamera.SetActive(false);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Ejecuta un contraataque.
    /// </summary>
    void EjecutarContraataque(string tipoCounter)
    {
        if (isCountering || isAttackingEnemy) return;

        EnemyScript closest = EnemigoMasCercanoPreparandoAtaque();
        if (closest == null) return;

        lockedTarget = closest;

        Transform targetTransform = closest.transform;
        if (targetTransform != null)
        {
            lockedEnemy = targetTransform;
            isLockedOn = true;

            transform.DOKill();
            transform.DOLookAt(
                lockedEnemy.position + Vector3.up * 1.5f,
                rotationDuration,
                AxisConstraint.Y,
                Vector3.up
            ).SetEase(Ease.OutSine);

            if (lockOnInstance == null)
                CrearIndicadorLockOn(lockedEnemy);
        }

        OnCounterAttack?.Invoke(lockedTarget);

        float dist = DistanciaAEnemy(lockedTarget);
        if (dist > 2f)
        {
            Atacar(lockedTarget, dist);
            return;
        }

        float dodgeDuration = 0.6f;
        animator.SetTrigger("Dodge");

        if (targetTransform != null)
        {
            Vector3 dirToEnemy = (targetTransform.position - transform.position).normalized;
            dirToEnemy.y = 0;
            if (dirToEnemy != Vector3.zero)
                transform.forward = dirToEnemy;
        }

        Vector3 dodgeDir = targetTransform != null ? targetTransform.forward : transform.forward;
        transform.DOMove(transform.position + dodgeDir, dodgeDuration);

        if (counterCoroutine != null) StopCoroutine(counterCoroutine);
        counterCoroutine = StartCoroutine(CorrutinaContraataque(closest, dodgeDuration, tipoCounter));
    }

    /// <summary>
    /// Corrutina de contraataque.
    /// </summary>
    IEnumerator CorrutinaContraataque(EnemyScript originalTarget, float dodgeDur, string tipoCounter)
    {
        isCountering = true;

        yield return new WaitForSeconds(dodgeDur);

        EnemyScript currentTarget = originalTarget;
        if (currentTarget == null || !currentTarget.IsAttackable())
        {
            currentTarget = EnemigoMasCercanoPreparandoAtaque();

            if (currentTarget == null)
            {
                currentTarget = ObtenerEnemigoMasCercano();
                if (currentTarget == null)
                {
                    isCountering = false;
                    yield break;
                }
            }

            lockedTarget = currentTarget;
        }

        float dist = DistanciaAEnemy(currentTarget);
        string ataque = SeleccionarContraataque(tipoCounter, dist);

        if (ataque != null)
        {
            EjecutarAtaque(ataque, attackCooldown, currentTarget, 0.65f);
        }

        isCountering = false;

        if (lockedTarget != null)
            lockedTarget.ReleaseLock();
    }

    /// <summary>
    /// Decide qué ataque ejecutar según la distancia al target.
    /// </summary>
    void Atacar(EnemyScript target, float distancia)
    {
        if (target == null)
        {
            EjecutarAtaque("TrRaSwing", 0.2f, null, 0);
            return;
        }

        string[] kickAttacks       = { "TrCrescent", "TrChut" };
        string[] rapierAttacks     = { "TrRaSwing", "TrRa360" };
        string[] lightSaberAttacks = { "TrSwSwing", "TrSw360" };

        string[] pool;

        if (distancia <= kickRange)
        {
            pool = new string[]
            {
                kickAttacks[0], kickAttacks[1],
                rapierAttacks[0], rapierAttacks[1],
                lightSaberAttacks[0], lightSaberAttacks[1]
            };
        }
        else if (distancia <= rapierRange)
        {
            pool = new string[]
            {
                rapierAttacks[0], rapierAttacks[1],
                lightSaberAttacks[0], lightSaberAttacks[1]
            };
        }
        else if (distancia <= lightSaberRange)
        {
            pool = new string[]
            {
                lightSaberAttacks[0], lightSaberAttacks[1]
            };
        }
        else
        {
            EjecutarAtaque("TrRaSwing", 0.2f, null, 0);
            return;
        }

        int idx = UnityEngine.Random.Range(0, pool.Length);
        EjecutarAtaque(pool[idx], attackCooldown, target, 0.65f);
    }

    /// <summary>
    /// Procesa el input de ataques.
    /// </summary>
    void ProcesarInputAtaques()
    {
        if (isCountering) return;

        if (Input.GetKeyDown(KeyCode.CapsLock)) VerificarYAtaque();
        if (Input.GetKeyDown(KeyCode.LeftShift)) VerificarYAtaquePatada();
        if (Input.GetKeyDown(KeyCode.Mouse1)) VerificarYAtaqueRapier();
        if (Input.GetKeyDown(KeyCode.Mouse0)) VerificarYAtaqueSable();
        if (Input.GetKeyDown(KeyCode.V)) VerificarYAtaqueEspecifico("TrCrescent");
        if (Input.GetKeyDown(KeyCode.B)) VerificarYAtaqueEspecifico("TrChut");
        if (Input.GetKeyDown(KeyCode.N)) VerificarYAtaqueEspecifico("TrRaSwing");
        if (Input.GetKeyDown(KeyCode.F)) VerificarYAtaqueEspecifico("TrRa360");
        if (Input.GetKeyDown(KeyCode.R)) VerificarYAtaqueEspecifico("TrSwSwing");
        if (Input.GetKeyDown(KeyCode.T)) VerificarYAtaqueEspecifico("TrSw360");
    }

    /// <summary>
    /// Procesa el input de contraataques.
    /// </summary>
    void ProcesarInputContraataques()
    {
        if (Input.GetKeyDown(KeyCode.Z)) EjecutarContraataque("KICK");
        if (Input.GetKeyDown(KeyCode.X)) EjecutarContraataque("LIGHTSABER");
        if (Input.GetKeyDown(KeyCode.C)) EjecutarContraataque("RAPIER");
        if (Input.GetKeyDown(KeyCode.Space)) EjecutarContraataque("DEFAULT");
    }

    void VerificarYAtaque()
    {
        if (isAttackingEnemy) return;

        ResolverTarget();
        if (lockedTarget == null)
        {
            EjecutarAtaque("TrRaSwing", 0.2f, null, 0);
            return;
        }

        float distancia = DistanciaAEnemy(lockedTarget);
        Atacar(lockedTarget, distancia);
    }

    void VerificarYAtaquePatada()
    {
        if (isAttackingEnemy) return;

        ResolverTarget();
        if (lockedTarget == null)
        {
            EjecutarAtaque("TrCrescent", 0.2f, null, 0);
            return;
        }

        float distancia = DistanciaAEnemy(lockedTarget);
        string[] kickAttacks = { "TrCrescent", "TrChut" };

        if (distancia <= kickRange)
        {
            string atk = kickAttacks[UnityEngine.Random.Range(0, kickAttacks.Length)];
            EjecutarAtaque(atk, attackCooldown, lockedTarget, 0.65f);
        }
        else
        {
            Atacar(lockedTarget, distancia);
        }
    }

    void VerificarYAtaqueRapier()
    {
        if (isAttackingEnemy) return;

        ResolverTarget();
        if (lockedTarget == null)
        {
            EjecutarAtaque("TrRaSwing", 0.2f, null, 0);
            return;
        }

        float distancia = DistanciaAEnemy(lockedTarget);
        string[] rapierAttacks = { "TrRaSwing", "TrRa360" };

        if (distancia <= rapierRange)
        {
            string atk = rapierAttacks[UnityEngine.Random.Range(0, rapierAttacks.Length)];
            EjecutarAtaque(atk, attackCooldown, lockedTarget, 0.65f);
        }
        else
        {
            Atacar(lockedTarget, distancia);
        }
    }

    void VerificarYAtaqueSable()
    {
        if (isAttackingEnemy) return;

        ResolverTarget();
        if (lockedTarget == null)
        {
            EjecutarAtaque("TrSwSwing", 0.2f, null, 0);
            return;
        }

        float distancia = DistanciaAEnemy(lockedTarget);
        string[] saberAttacks = { "TrSwSwing", "TrSw360" };

        if (distancia <= lightSaberRange)
        {
            string atk = saberAttacks[UnityEngine.Random.Range(0, saberAttacks.Length)];
            EjecutarAtaque(atk, attackCooldown, lockedTarget, 0.65f);
        }
        else
        {
            Atacar(lockedTarget, distancia);
        }
    }

    void VerificarYAtaqueEspecifico(string triggerAtaque)
    {
        if (isAttackingEnemy) return;

        if (isLockedOn && EstaEnemyVivoYActivo(lockedEnemy))
        {
            lockedTarget = lockedEnemy.GetComponent<EnemyScript>();
            if (lockedTarget != null && lockedTarget.IsAttackable())
            {
                EjecutarAtaque(triggerAtaque, 1.3f, lockedTarget, 0.65f);
                return;
            }
        }

        if (enemyDetection != null && enemyDetection.InputMagnitude() > 0.2f)
        {
            EnemyScript detected = enemyDetection.CurrentTarget();
            if (detected != null && detected.IsAttackable())
            {
                lockedTarget = detected;
                EjecutarAtaque(triggerAtaque, 1.3f, lockedTarget, 0.65f);
                return;
            }
        }

        if (enemyManager != null && enemyManager.aliveEnemyCount > 0)
        {
            lockedTarget = ObtenerEnemigoMasCercano();
            if (lockedTarget != null)
            {
                EjecutarAtaque(triggerAtaque, 1.3f, lockedTarget, 0.65f);
                return;
            }
        }

        EjecutarAtaque(triggerAtaque, 1.3f, null, 0);
    }

    /// <summary>
    /// Recibe daño del enemigo.
    /// </summary>
    public void RecibirDanyo()
    {
        animator.SetTrigger("NinjaHit");
        DamageEvent?.Invoke();

        if (damageCoroutine != null) StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(CorrutinaDanyo());
    }

    IEnumerator CorrutinaDanyo()
    {
        speed = 0f;
        yield return new WaitForSeconds(0.5f);
        speed = 2f;
        DOVirtual.Float(0f, 2f, 0.6f, v => speed = v);
    }

    // ══════════════════════════════════════════════════════════
    // UPDATE — Loop principal
    // ══════════════════════════════════════════════════════════

    void Update()
    {
        // ── Rebuscar EnemyManager si fue destruido ──────────
        if (enemyManager == null || enemyManager.gameObject == null || !enemyManager.isActiveAndEnabled)
            enemyManager = FindAnyObjectByType<EnemyManager>();

        // ── Leer input ───────────────────────────────────────
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveAxis = new Vector2(horizontal, vertical);

        // ── Sincronizar datos del alumno ─────────────────────
        SincronizarDatosDelJugador();
        ActualizarDebug();

        // ── Auto-lock ─────────────────────────────────────────
        GestionarAutoLock();
        GestionarAutoSwitchAAttacker();

        // ── Lock-On Toggle (Tab) ──────────────────────────────
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isLockedOn)
                DesbloquearLockManual();
            else
            {
                manualUnlock = false;
                ForzarLockAlMasCercano();
            }
        }

        // ── Cambiar objetivo (Q / E) ─────────────────────────
        if (isLockedOn)
        {
            if (Input.GetKeyDown(KeyCode.E)) CambiarLockADireccion(1);
            else if (Input.GetKeyDown(KeyCode.Q)) CambiarLockADireccion(-1);
        }

        // ── Validar lock actual ──────────────────────────────
        ValidarLockActual();

        // ── Movimiento ────────────────────────────────────────
        ProcesarMovimiento(horizontal, vertical);

        // ── Animator ──────────────────────────────────────────
        ProcesarAnimator();

        // ── Estado especial ───────────────────────────────────
        ProcesarEstadoEspecial();

        // ── Ataques ───────────────────────────────────────────
        ProcesarInputAtaques();

        // ── Contraataques ─────────────────────────────────────
        ProcesarInputContraataques();
    }

    void LateUpdate()
    {
        ActualizarIndicadorLockOn();
    }

    // ══════════════════════════════════════════════════════════
    // API PÚBLICA (para otros scripts)
    // ══════════════════════════════════════════════════════════

    public Damageable.EstadoEspecial GetEstadoSeleccionado()
    {
        return estadoSeleccionado;
    }

    // ── Hitbox Enable/Disable (Animation Events) ─────────────

    public void EnableRapierHitbox()      { ActivarHitboxRapier(); }
    public void EnableLightSaberHitbox()  { ActivarHitboxSable(); }
    public void EnableLeftKickHitbox()    { ActivarHitboxPatadaIzquierda(); }
    public void EnableRightKickHitbox()   { ActivarHitboxPatadaDerecha(); }
    public void DisableRapierHitbox()     { DesactivarHitboxRapier(); }
    public void DisableLightSaberHitbox() { DesactivarHitboxSable(); }
    public void DisableLeftKickHitbox()   { DesactivarHitboxPatadaIzquierda(); }
    public void DisableRightKickHitbox()  { DesactivarHitboxPatadaDerecha(); }

    // ── HitEvent (Animation Event) ───────────────────────────

    public void HitEvent()
    {
        if (lockedTarget == null || enemyManager == null || enemyManager.aliveEnemyCount == 0)
            return;

        OnHit?.Invoke(lockedTarget);
    }

    // ── RecibirDanyo (llamado por EnemyHitbox) ───────────────

    public void RecibirDanyoPublico()
    {
        RecibirDanyo();
    }

    // ── Corrutinas originales (compatibilidad) ───────────────

    IEnumerator DanceSpin()
    {
        for (int i = 0; i < 360; i += 30)
        {
            transform.Rotate(0, 30, 0);
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator JumpCountdown()
    {
        for (int i = 3; i > 0; i--)
        {
            Debug.Log(i);
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("¡Jump!");
        controller.Move(Vector3.up * 2f);
    }

    IEnumerator Ra360()   { isAttackingEnemy = true; animator.SetTrigger("TrRa360");  yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Sw360()   { isAttackingEnemy = true; animator.SetTrigger("TrSw360");  yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator SwSwing() { isAttackingEnemy = true; animator.SetTrigger("TrSwSwing"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator RaSwing() { isAttackingEnemy = true; animator.SetTrigger("TrRaSwing"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Crescent(){ isAttackingEnemy = true; animator.SetTrigger("TrCrescent"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Chut()    { isAttackingEnemy = true; animator.SetTrigger("TrChut");    yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }

    // ══════════════════════════════════════════════════════════
    // FIN DEL ARCHIVO
    // ══════════════════════════════════════════════════════════
}