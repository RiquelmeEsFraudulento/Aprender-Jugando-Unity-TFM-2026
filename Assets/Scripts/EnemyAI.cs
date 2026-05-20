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
/*using UnityEngine;

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

*/

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class EnemyScript : Damageable
{
    //Declarations
    private Animator animator;
    private SimpleWalk playerCombat;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    public CharacterController characterController;

    [Header("Stats")]
    //public int health = 3;
    private float moveSpeed = 1;
    private Vector3 moveDirection;

    [Header("States")]
    [SerializeField] private bool isPreparingAttack;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isRetreating;
    [SerializeField] private bool isLockedTarget;
    [SerializeField] private bool isStunned;
    [SerializeField] private bool isWaiting = true;

    [Header("Polish")]
    //[SerializeField] private ParticleSystem counterParticle;

    private Coroutine PrepareAttackCoroutine;
    private Coroutine RetreatCoroutine;
    private Coroutine DamageCoroutine;
    private Coroutine MovementCoroutine;
    private Coroutine DeathCoroutine;

    //Events
    public UnityEvent<EnemyScript> OnDamage;
    public UnityEvent<EnemyScript> OnStopMoving;
    public UnityEvent<EnemyScript> OnRetreat;
    public int   danyoAtaque        = 10;    // Puntos de daño por golpe

    // ── Debug switch ──────────────────────────────────────────
    private const bool LOG_IA = true;
    void Start()
    {
        enemyManager = GetComponentInParent<EnemyManager>();
        base.InicializarVida();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        playerCombat = FindAnyObjectByType<SimpleWalk>();
        enemyDetection = playerCombat.GetComponentInChildren<EnemyDetection>();

        //playerCombat.OnHit.AddListener((x) => OnPlayerHit(x));
        playerCombat.OnCounterAttack.AddListener((x) => OnPlayerCounter(x));
        playerCombat.OnTrajectory.AddListener((x) => OnPlayerTrajectory(x));

        MovementCoroutine = StartCoroutine(EnemyMovement());

    }

    IEnumerator EnemyMovement()
    {
        //Waits until the enemy is not assigned to no action like attacking or retreating
        yield return new WaitUntil(() => isWaiting == true);

        int randomChance = Random.Range(0, 2);

        if (randomChance == 1)
        {
            int randomDir = Random.Range(0, 2);
            moveDirection = randomDir == 1 ? Vector3.right : Vector3.left;
            isMoving = true;
        }
        else
        {
            StopMoving();
        }

        yield return new WaitForSeconds(1);

        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    void Update()
    {
        //Constantly look at player
        transform.LookAt(new Vector3(playerCombat.transform.position.x, transform.position.y, playerCombat.transform.position.z));

        //Only moves if the direction is set
        MoveEnemy(moveDirection);

        base.AvanzarCooldown();
        if (estaEnvenenado) ProcesarVenenoPorTiempo();
    }

    //Listened event from Player Animation
    void OnPlayerHit(EnemyScript target)
    {
        if (target == this)
        {
            StopEnemyCoroutines();
            DamageCoroutine = StartCoroutine(HitCoroutine());

            enemyDetection.SetCurrentTarget(null);
            isLockedTarget = false;
            OnDamage.Invoke(this);

            currentHealth--;

            if (currentHealth <= 0)
            {
                Death();
                return;
            }

            animator.SetTrigger("Hit");
            transform.DOMove(transform.position - (transform.forward / 2), .3f).SetDelay(.1f);

            StopMoving();
        }

        IEnumerator HitCoroutine()
        {
            isStunned = true;
            yield return new WaitForSeconds(.5f);
            isStunned = false;
        }
    }

    void OnPlayerCounter(EnemyScript target)
    {
        if (target == this)
        {
            PrepareAttack(false);
        }
    }

    void OnPlayerTrajectory(EnemyScript target)
    {
        if (target == this)
        {
            StopEnemyCoroutines();
            isLockedTarget = true;
            PrepareAttack(false);
            StopMoving();
        }
    }

    void Death()
    {
        StopEnemyCoroutines();

        this.enabled = false;
        characterController.enabled = false;
        animator.SetTrigger("Death");
        enemyManager.SetEnemyAvailiability(this, false);
    }


    public override void Morir()
    {
        StopEnemyCoroutines();

        this.enabled = false;
        characterController.enabled = false;
        animator.SetTrigger("Death");
        enemyManager.SetEnemyAvailiability(this, false);
        enemyManager.RemoveEnemy(this);   // <--- NUEVO

        DeathCoroutine = StartCoroutine(MuerteCooldown());

        IEnumerator MuerteCooldown()
        {
            yield return new WaitForSeconds(1.4f);
            onDeath?.Invoke();
            Destroy(gameObject);
        }
    }

    public void SetRetreat()
    {
        StopEnemyCoroutines();

        RetreatCoroutine = StartCoroutine(PrepRetreat());

        IEnumerator PrepRetreat()
        {
            yield return new WaitForSeconds(1.4f);
            OnRetreat.Invoke(this);
            isRetreating = true;
            moveDirection = -Vector3.forward;
            isMoving = true;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, playerCombat.transform.position) > 4);
            isRetreating = false;
            StopMoving();

            //Free 
            isWaiting = true;
            MovementCoroutine = StartCoroutine(EnemyMovement());
        }
    }

    public void SetAttack()
    {

        isLockedTarget = false;
        isWaiting = false;

        PrepareAttackCoroutine = StartCoroutine(PrepAttack());

        IEnumerator PrepAttack()
        {
            PrepareAttack(true);
            yield return new WaitForSeconds(.2f);
            moveDirection = Vector3.forward;
            isMoving = true;
        }
    }


    void PrepareAttack(bool active)
    {
        isPreparingAttack = active;

        if (active)
        {
            //counterParticle.Play();
        }
        else
        {
            StopMoving();
            //counterParticle.Clear();
            //counterParticle.Stop();
        }
    }

    void MoveEnemy(Vector3 direction)
    {
        //Set movespeed based on direction
        moveSpeed = 1;

        if (direction == Vector3.forward)
            moveSpeed = 5;
        if (direction == -Vector3.forward)
            moveSpeed = 2;

        //Set Animator values
        animator.SetFloat("InputMagnitude", (characterController.velocity.normalized.magnitude * direction.z) / (5 / moveSpeed), .2f, Time.deltaTime);
        animator.SetBool("Strafe", (direction == Vector3.right || direction == Vector3.left));
        animator.SetFloat("StrafeDirection", direction.normalized.x, .2f, Time.deltaTime);

        //Don't do anything if isMoving is false
        if (!isMoving)
            return;

        Vector3 dir = (playerCombat.transform.position - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir; //Vector perpendicular to direction
        Vector3 movedir = Vector3.zero;

        Vector3 finalDirection = Vector3.zero;

        if (direction == Vector3.forward)
            finalDirection = dir;
        if (direction == Vector3.right || direction == Vector3.left)
            finalDirection = (pDir * direction.normalized.x);
        if (direction == -Vector3.forward)
            finalDirection = -transform.forward;

        if (direction == Vector3.right || direction == Vector3.left)
            moveSpeed /= 1.5f;

        movedir += finalDirection * moveSpeed * Time.deltaTime;

        characterController.Move(movedir);

        if (!isPreparingAttack)
            return;

        if(Vector3.Distance(transform.position, playerCombat.transform.position) < 2)
        {
            StopMoving();
            if (!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
                Attack();
            else
                PrepareAttack(false);
        }
    }

    public void ReleaseLock()
    {
        isLockedTarget = false;
    }


    private void Attack()
    {
        transform.DOMove(transform.position + (transform.forward / 1), .5f);
        animator.SetTrigger("AirPunch");
    }

    public void HitEvent()
    {
        if(!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
        {
            GolpearNinja(playerCombat.GetComponent<PlayerHealth>());
            playerCombat.RecibirDanyo();        
        }

        PrepareAttack(false);
    }

    public void StopMoving()
    {
        isMoving = false;
        moveDirection = Vector3.zero;
        if(characterController.enabled)
            characterController.Move(moveDirection);
    }

    void StopEnemyCoroutines()
    {
        PrepareAttack(false);

        if (isRetreating)
        {
            if (RetreatCoroutine != null)
                StopCoroutine(RetreatCoroutine);
        }

        if (PrepareAttackCoroutine != null)
            StopCoroutine(PrepareAttackCoroutine);

        if(DamageCoroutine != null)
            StopCoroutine(DamageCoroutine);

        if (MovementCoroutine != null)
            StopCoroutine(MovementCoroutine);
    }

    #region Public Booleans

    public bool IsAttackable()
    {
        return currentHealth > 0;
    }

    public bool IsPreparingAttack()
    {
        return isPreparingAttack;
    }

    public bool IsRetreating()
    {
        return isRetreating;
    }

    public bool IsLockedTarget()
    {
        return isLockedTarget;
    }

    public bool IsStunned()
    {
        return isStunned;
    }

    public void GolpearNinja(PlayerHealth vidaNinja)
    {
        if (vidaNinja == null) return;
        //if (estadoActual != Estado.Attack && estadoActual != Estado.Berserker) return;

        // PlayerHealth.cs ya existe en el proyecto, lo llamamos igual que siempre
        vidaNinja.TakeDamage(danyoAtaque);
        DebugIA($"[IA] '{name}' golpeó al ninja: -{danyoAtaque}");
    }


    public override void TakeDamage(int amount, DamageType damageType, GameObject source){
        // --- Daño base, cooldown, veneno/sangrado ---
        base.TakeDamage(amount, damageType, source);

        // --- Si ha muerto, la clase base ya disparó Morir() y onDeath.
        //     No hacemos nada más aquí para no interferir con la secuencia de muerte.
        if (currentHealth <= 0)
            return;

        // --- REACCIÓN DE IMPACTO (antes en OnPlayerHit) ---
        StopEnemyCoroutines();
        DamageCoroutine = StartCoroutine(HitCoroutine());

        enemyDetection.SetCurrentTarget(null);
        isLockedTarget = false;
        OnDamage.Invoke(this);      // Evento público de daño

        animator.SetTrigger("Hit");
        transform.DOMove(transform.position - (transform.forward / 2), .3f).SetDelay(.1f);
        StopMoving();

        // La corrutina de aturdimiento se mantiene igual que en OnPlayerHit
        IEnumerator HitCoroutine()
        {
            isStunned = true;
            yield return new WaitForSeconds(.5f);
            isStunned = false;
        }
    }    
    void DebugIA(string msg) { if (LOG_IA) Debug.Log(msg); }

    #endregion
}
