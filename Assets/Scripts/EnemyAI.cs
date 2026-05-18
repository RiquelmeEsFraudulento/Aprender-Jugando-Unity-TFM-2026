// ============================================================
// EnemyAI.cs  —  COMPORTAMIENTO DEL ENEMIGO
// ============================================================
// Pon este script en cada prefab de enemigo.
// Hereda de Damageable, así que NO pongas Damageable aparte:
// este script ya tiene vida, veneno, sangrado, etc.
//
// ESTADOS DEL ENEMIGO (máquina de estados simple):
//   Idle       → espera quieto mirando al ninja
//   Patrol     → camina por la escena sin objetivo concreto
//   Chase      → corre hacia el ninja al detectarlo
//   Attack     → golpea al ninja cuando está cerca
//   Flee       → huye cuando le queda poca vida
//   Stunned    → aturdido, no puede hacer nada
//   Berserker  → como Chase/Attack pero más rápido y agresivo
//
// ANIMATOR — parámetros que usa este script:
//   bool  "isWalking"    → true cuando se mueve despacio
//   bool  "isRunning"    → true cuando corre (Chase/Berserker)
//   bool  "isAttacking"  → true durante el ataque
//   bool  "isStunned"    → true mientras está aturdido
//   bool  "isDead"       → true cuando muere (dispara animación)
//   trigger "Attack"     → dispara el golpe puntual
//
// JERARQUÍA DE COMPONENTES EN EL PREFAB:
//   EnemyPrefab (Root)
//   ├─ EnemyAI.cs         ← este script (hereda Damageable)
//   ├─ Animator           ← Controller: EnemyAnimator.controller
//   ├─ CharacterController← para mover el personaje
//   └─ [Hitbox hijo]      ← collider trigger del puño/arma
// ============================================================
using UnityEngine;

public class EnemyAI : Damageable           // Hereda vida + efectos de Damageable
{
    // ══════════════════════════════════════════════════════════
    // ENUMERACIÓN DE ESTADOS
    // En C++ sería: enum Estado { IDLE, PATROL, CHASE, ... };
    // ══════════════════════════════════════════════════════════
    public enum Estado
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee,
        Stunned,
        Berserker
    }

    // ══════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════
    [Header("IA — Distancias")]
    [Tooltip("El enemigo detecta al ninja si está a menos de esta distancia.")]
    public float distanciaDeteccion = 10f;   // Rango de visión

    [Tooltip("El enemigo ataca cuando está a menos de esta distancia.")]
    public float distanciaAtaque    = 2f;    // Rango cuerpo a cuerpo

    [Tooltip("El enemigo huye si su vida cae por debajo de este % (0-1). Ej: 0.25 = 25%")]
    public float porcentajeHuida    = 0.25f; // Huye al 25% de vida

    [Header("IA — Velocidades")]
    public float velocidadPatrulla  = 2f;    // Velocidad de paseo
    public float velocidadPersecucion = 5f; // Velocidad de carrera
    public float velocidadBerserker = 7f;   // Velocidad en modo rabia

    [Header("IA — Ataque")]
    [Tooltip("Daño que hace el enemigo al golpear al ninja.")]
    public int   danyoAtaque        = 10;    // Puntos de daño por golpe
    public float cooldownAtaque     = 1.5f; // Segundos entre golpes

    [Header("IA — Aturdimiento")]
    public float duracionAturdido   = 2f;   // Segundos que dura el stun

    [Header("Estado actual (solo lectura)")]
    public Estado estadoActual      = Estado.Idle;
    public bool   esBerserker       = false; // Lo activa EnemyBrain

    // ── Referencias ───────────────────────────────────────────
    private Transform        _ninja;           // Transform del jugador
    private Animator         _animator;        // Componente Animator
    private CharacterController _controller;  // Para mover el GameObject

    // ── Timers internos ───────────────────────────────────────
    private float _timerAtaque    = 0f;   // Cuenta el cooldown entre golpes
    private float _timerStun      = 0f;   // Cuenta cuánto lleva aturdido
    private float _timerPatrulla  = 0f;   // Cuánto lleva en la misma dirección
    private Vector3 _dirPatrulla  = Vector3.forward; // Dirección actual de patrulla

    // ── Hashes de Animator (más rápido que usar strings) ─────
    private static readonly int _hashWalking   = Animator.StringToHash("isWalking");
    private static readonly int _hashRunning   = Animator.StringToHash("isRunning");
    private static readonly int _hashAttacking = Animator.StringToHash("isAttacking");
    private static readonly int _hashStunned   = Animator.StringToHash("isStunned");
    private static readonly int _hashDead      = Animator.StringToHash("isDead");
    private static readonly int _hashAttackTrigger = Animator.StringToHash("Attack");

    // ── Debug switch ──────────────────────────────────────────
    private const bool LOG_IA = true;

    // ══════════════════════════════════════════════════════════
    // CICLO DE VIDA UNITY
    // ══════════════════════════════════════════════════════════
    void Awake()     // 'override' porque Damageable tiene Awake
    {
        base.InicializarVida();

        //base.Awake();                   // Llama a Damageable.Awake() → inicializa vida

        _animator   = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        // Buscamos al ninja por su Tag "Player"
        GameObject ninjaGO = GameObject.FindGameObjectWithTag("Player");
        if (ninjaGO != null)
            _ninja = ninjaGO.transform;

        // Nos registramos en la Mente Colmena
        if (EnemyBrain.Instancia != null)
            EnemyBrain.Instancia.RegistrarEnemigo(this);
        else
            DebugIA($"[IA] '{name}': No hay EnemyBrain en la escena. Crea un GameObject con EnemyBrain.cs");

        CambiarEstado(Estado.Patrol);   // Empezamos patrullando
    }

    void Update()
    {
        // Damageable.Update() ya se ejecuta en la clase padre (veneno, cooldown)
        // Aquí solo gestionamos la IA

        ActualizarTimers();
        EjecutarEstadoActual();
    }

    // ══════════════════════════════════════════════════════════
    // MÁQUINA DE ESTADOS PRINCIPAL
    // ══════════════════════════════════════════════════════════

    // Avanza los timers de cooldown y stun cada frame
    void ActualizarTimers()
    {
        if (_timerAtaque > 0f)   _timerAtaque  -= Time.deltaTime;
        if (_timerStun   > 0f)   _timerStun    -= Time.deltaTime;
        if (_timerPatrulla > 0f) _timerPatrulla -= Time.deltaTime;
    }

    // Decide qué función ejecutar según el estado actual
    void EjecutarEstadoActual()
    {
        switch (estadoActual)
        {
            case Estado.Idle:       EstadoIdle();      break;
            case Estado.Patrol:     EstadoPatrol();    break;
            case Estado.Chase:      EstadoChase();     break;
            case Estado.Attack:     EstadoAttack();    break;
            case Estado.Flee:       EstadoFlee();      break;
            case Estado.Stunned:    EstadoStunned();   break;
            case Estado.Berserker:  EstadoBerserker(); break;
        }
    }

    // ── IDLE: espera quieto ────────────────────────────────────
    void EstadoIdle()
    {
        ActualizarAnimator(false, false, false, false);

        // Si el ninja está cerca, empieza a perseguirle
        if (DistanciaAlNinja() <= distanciaDeteccion)
            CambiarEstado(Estado.Chase);
    }

    // ── PATROL: camina en una dirección aleatoria ─────────────
    void EstadoPatrol()
    {
        ActualizarAnimator(true, false, false, false);

        // Si el ninja entra en rango, lo perseguimos
        if (_ninja != null && DistanciaAlNinja() <= distanciaDeteccion)
        {
            CambiarEstado(Estado.Chase);
            return;
        }

        // Cada 3 segundos cambiamos de dirección de patrulla
        if (_timerPatrulla <= 0f)
        {
            _dirPatrulla   = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            _timerPatrulla = 3f;
        }

        Mover(_dirPatrulla, velocidadPatrulla);
    }

    // ── CHASE: corre hacia el ninja ────────────────────────────
    void EstadoChase()
    {
        ActualizarAnimator(false, true, false, false);

        if (_ninja == null) { CambiarEstado(Estado.Idle); return; }

        // ¿Tiene poca vida? Huye
        if (DeberiaHuir()) { CambiarEstado(Estado.Flee); return; }

        float dist = DistanciaAlNinja();

        // ¿Lo perdió de vista?
        if (dist > distanciaDeteccion * 1.5f)
        {
            CambiarEstado(Estado.Patrol);
            return;
        }

        // ¿Ya está en rango de ataque?
        if (dist <= distanciaAtaque)
        {
            CambiarEstado(Estado.Attack);
            return;
        }

        // Se acerca al ninja
        Vector3 dir = (_ninja.position - transform.position).normalized;
        dir.y = 0f;                        // Evita que el enemigo "vuele"
        Mover(dir, velocidadPersecucion);
        MirarHacia(_ninja.position);
    }

    // ── ATTACK: golpea al ninja ────────────────────────────────
    void EstadoAttack()
    {
        ActualizarAnimator(false, false, true, false);

        if (_ninja == null) { CambiarEstado(Estado.Idle); return; }

        MirarHacia(_ninja.position);       // Siempre mira al ninja mientras ataca

        float dist = DistanciaAlNinja();

        // Si se alejó, volvemos a perseguirle
        if (dist > distanciaAtaque * 1.2f)
        {
            CambiarEstado(Estado.Chase);
            return;
        }

        // ¿Tiene poca vida? Huye aunque esté atacando
        if (DeberiaHuir()) { CambiarEstado(Estado.Flee); return; }

        // Golpe con cooldown
        if (_timerAtaque <= 0f)
        {
            _animator.SetTrigger(_hashAttackTrigger); // Dispara la animación del golpe
            _timerAtaque = cooldownAtaque;             // Reinicia el cooldown

            // El daño real lo aplica el collider del puño (EnemyHitbox.cs)
            // Este trigger solo lanza la animación
            DebugIA($"[IA] '{name}' ATACA | dist={dist:F2}");
        }
    }

    // ── FLEE: huye del ninja ───────────────────────────────────
    void EstadoFlee()
    {
        ActualizarAnimator(false, true, false, false); // Corre (animación de run)

        if (_ninja == null) { CambiarEstado(Estado.Idle); return; }

        // Se aleja en dirección opuesta al ninja
        Vector3 dir = (transform.position - _ninja.position).normalized;
        dir.y = 0f;
        Mover(dir, velocidadPersecucion);
        MirarHacia(transform.position + dir); // Mira hacia donde huye
    }

    // ── STUNNED: no puede moverse ni atacar ────────────────────
    void EstadoStunned()
    {
        ActualizarAnimator(false, false, false, true);

        // Cuando se le acaba el tiempo de stun, vuelve a perseguir
        if (_timerStun <= 0f)
        {
            DebugIA($"[IA] '{name}' se recupera del aturdimiento");
            CambiarEstado(esBerserker ? Estado.Berserker : Estado.Chase);
        }
    }

    // ── BERSERKER: como Chase pero mucho más rápido ────────────
    void EstadoBerserker()
    {
        ActualizarAnimator(false, true, false, false); // Corre

        if (_ninja == null) { CambiarEstado(Estado.Idle); return; }

        float dist = DistanciaAlNinja();

        if (dist <= distanciaAtaque)
        {
            // Ataca con cooldown reducido a la mitad (más agresivo)
            if (_timerAtaque <= 0f)
            {
                _animator.SetTrigger(_hashAttackTrigger);
                _timerAtaque = cooldownAtaque * 0.5f;   // Doble velocidad de ataque
                DebugIA($"[IA] '{name}' ATAQUE BERSERKER");
            }
        }
        else
        {
            Vector3 dir = (_ninja.position - transform.position).normalized;
            dir.y = 0f;
            Mover(dir, velocidadBerserker);             // Más rápido que Chase normal
            MirarHacia(_ninja.position);
        }
    }

    // ══════════════════════════════════════════════════════════
    // CAMBIO DE ESTADO
    // ══════════════════════════════════════════════════════════
    void CambiarEstado(Estado nuevo)
    {
        if (estadoActual == nuevo) return; // Evita cambios innecesarios

        DebugIA($"[IA] '{name}' {estadoActual} → {nuevo}");
        estadoActual = nuevo;

        // Acciones al entrar en el nuevo estado
        switch (nuevo)
        {
            case Estado.Stunned:
                _timerStun = duracionAturdido;      // Carga el timer de stun
                break;
            case Estado.Patrol:
                _timerPatrulla = 0f;                // Elige dirección inmediatamente
                break;
        }
    }

    // ══════════════════════════════════════════════════════════
    // API PÚBLICA — llamada por EnemyBrain y EnemyHitbox
    // ══════════════════════════════════════════════════════════

    // EnemyBrain llama a esto cuando la colmena entra en rabia
    public void ActivarBerserker()
    {
        esBerserker = true;
        DebugIA($"[IA] '{name}' entra en MODO BERSERKER");

        // Solo cambia de estado si no está ya muriendo o aturdido
        if (estadoActual != Estado.Stunned)
            CambiarEstado(Estado.Berserker);
    }

    // El collider del puño del enemigo llama a esto al tocar al ninja
    // (equivalente a OnTriggerEnter de EnemyHitbox.cs)
    public void GolpearNinja(PlayerHealth vidaNinja)
    {
        if (vidaNinja == null) return;
        if (estadoActual != Estado.Attack && estadoActual != Estado.Berserker) return;

        // PlayerHealth.cs ya existe en el proyecto, lo llamamos igual que siempre
        vidaNinja.TakeDamage(danyoAtaque);
        DebugIA($"[IA] '{name}' golpeó al ninja: -{danyoAtaque}");
    }

    // ══════════════════════════════════════════════════════════
    // OVERRIDE DE MUERTE — reemplaza el Morir() de Damageable
    // Damageable.cs llama a onDeath cuando la vida llega a 0.
    // Añadimos el UnityEvent onDeath en el Inspector al método OnMuerte.
    // ══════════════════════════════════════════════════════════
    public void OnMuerte()
    {
        // Desactivamos la IA para que no siga ejecutándose
        estadoActual = Estado.Idle;
        enabled      = false;            // Desactiva este script

        // Avisamos a la Mente Colmena
        if (EnemyBrain.Instancia != null)
            EnemyBrain.Instancia.DesregistrarEnemigo(this);

        // Lanzamos la animación de muerte
        if (_animator != null)
            _animator.SetBool(_hashDead, true);

        DebugIA($"[IA] '{name}' ha muerto. Avisando a la colmena.");
        // Destroy lo hace Damageable.cs con su evento onDeath → no lo repetimos
    }

    // ══════════════════════════════════════════════════════════
    // HELPERS DE MOVIMIENTO Y LÓGICA
    // ══════════════════════════════════════════════════════════

    // Mueve el enemigo en una dirección a una velocidad dada
    void Mover(Vector3 direccion, float velocidad)
    {
        if (_controller == null) return;

        Vector3 movimiento = direccion * velocidad * Time.deltaTime;
        movimiento.y -= 9.81f * Time.deltaTime; // Gravedad simple
        _controller.Move(movimiento);
    }

    // Hace que el enemigo mire hacia un punto (rotación suave)
    void MirarHacia(Vector3 objetivo)
    {
        Vector3 dir = (objetivo - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return; // Demasiado cerca, no girar

        Quaternion rotObjetivo = Quaternion.LookRotation(dir);
        transform.rotation     = Quaternion.Slerp(
            transform.rotation, rotObjetivo, 10f * Time.deltaTime
        );
    }

    // Devuelve la distancia real al ninja (o un número muy grande si no hay ninja)
    float DistanciaAlNinja()
    {
        if (_ninja == null) return 9999f;
        return Vector3.Distance(transform.position, _ninja.position);
    }

    // Comprueba si debe huir según el porcentaje de vida
    // En C++: bool deberiaHuir() { return vida < vidaMax * 0.25; }
    bool DeberiaHuir()
    {
        return (float)currentHealth / (float)maxHealth <= porcentajeHuida;
    }

    // Actualiza todos los parámetros bool del Animator de una sola vez
    void ActualizarAnimator(bool walking, bool running, bool attacking, bool stunned)
    {
        if (_animator == null) return;

        _animator.SetBool(_hashWalking,   walking);
        _animator.SetBool(_hashRunning,   running);
        _animator.SetBool(_hashAttacking, attacking);
        _animator.SetBool(_hashStunned,   stunned);
    }

    // ── Debug ─────────────────────────────────────────────────
    void DebugIA(string msg) { if (LOG_IA) Debug.Log(msg); }
}