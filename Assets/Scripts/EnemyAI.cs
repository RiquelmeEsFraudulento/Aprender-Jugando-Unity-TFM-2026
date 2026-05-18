using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : Damageable
{
    public enum Estado
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee,
        Stunned,
        Dead
    }

    [Header("Referencias")]
    public Transform objetivo;
    public Animator animator;
    public CharacterController controller;
    public EnemyHitbox hitbox;
    public Renderer[] renderersVisuales;

    [Header("Movimiento")]
    public float velocidadPatrulla = 1.8f;
    public float velocidadPersecucion = 4.2f;
    public float velocidadHuida = 4.8f;
    public float radioDeteccion = 10f;
    public float radioAtaque = 2f;
    public float radioReposicion = 3.5f;
    public float giroSuave = 12f;

    [Header("Combate")]
    public int danoAtaque = 8;
    public float cooldownAtaque = 1.1f;
    public float tiempoStunGolpe = 0.22f;
    public float fuerzaRetroceso = 2.2f;
    public float umbralHuidaVida = 0.25f;
    public int danoExtraBerserker = 3;

    [Header("Ragdoll")]
    public bool usarRagdoll = true;
    public float tiempoRagdollGolpe = 0.18f;
    public float tiempoRagdollMuerte = 999f;
    public float impulsoRagdollGolpe = 1.3f;

    [Header("Animator Params")]
    public string pWalk = "isWalking";
    public string pRun = "isRunning";
    public string pAttack = "isAttacking";
    public string pStun = "isStunned";
    public string pDead = "isDead";
    public string tAttack = "Attack";
    public string pHitReact = "HitReact";

    public Estado estadoActual = Estado.Idle;
    public bool modoBerserker = false;
    public bool vivo = true;

    float tAtaque;
    float tStun;
    float tPatrulla;
    float tHitReact;
    float dirPatrolX;
    float dirPatrolZ;
    bool ragdollActivo;
    bool puedeRecibirGolpes = true;
    Rigidbody[] ragdollBodies;
    Collider[] ragdollColliders;

    protected void Awake()
    {
        InicializarVida();
        if (animator == null) animator = GetComponent<Animator>();
        if (controller == null) controller = GetComponent<CharacterController>();
        ragdollBodies = GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = GetComponentsInChildren<Collider>(true);
        PrepararRagdoll(false);
    }

    void Start()
    {
        if (objetivo == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) objetivo = p.transform;
        }
        if (hitbox != null) hitbox.Enlazar(this);
        CambiarEstado(Estado.Patrol);
    }

    void Update()
    {
        if (!vivo) return;
        ActualizarTimers();
        if (ragdollActivo) return;
        EjecutarEstado();
        ActualizarAnimatorBase();
        base.AvanzarCooldown();
        if (estaEnvenenado) base.ProcesarVenenoPorTiempo();
    }

    void ActualizarTimers()
    {
        if (tAtaque > 0f) tAtaque -= Time.deltaTime;
        if (tStun > 0f) tStun -= Time.deltaTime;
        if (tPatrulla > 0f) tPatrulla -= Time.deltaTime;
        if (tHitReact > 0f) tHitReact -= Time.deltaTime;
    }

    void EjecutarEstado()
    {
        if (estadoActual == Estado.Stunned)
        {
            if (tStun <= 0f) CambiarEstado(modoBerserker ? Estado.Chase : Estado.Chase);
            return;
        }
        if (estadoActual == Estado.Dead) return;
        if (DebeHuir()) { CambiarEstado(Estado.Flee); return; }
        float d = DistanciaObjetivo();
        if (estadoActual == Estado.Idle)
        {
            if (d <= radioDeteccion) CambiarEstado(Estado.Chase);
            return;
        }
        if (estadoActual == Estado.Patrol)
        {
            HacerPatrulla();
            if (d <= radioDeteccion) CambiarEstado(Estado.Chase);
            return;
        }
        if (estadoActual == Estado.Chase)
        {
            Perseguir();
            if (d <= radioAtaque) CambiarEstado(Estado.Attack);
            return;
        }
        if (estadoActual == Estado.Attack)
        {
            Atacar();
            if (d > radioAtaque * 1.25f) CambiarEstado(Estado.Chase);
            return;
        }
        if (estadoActual == Estado.Flee)
        {
            Huir();
            if (d > radioReposicion) CambiarEstado(Estado.Patrol);
            return;
        }
    }

    void HacerPatrulla()
    {
        if (tPatrulla <= 0f)
        {
            dirPatrolX = Random.Range(-1f, 1f);
            dirPatrolZ = Random.Range(-1f, 1f);
            tPatrulla = 2.8f;
        }
        Vector3 dir = new Vector3(dirPatrolX, 0f, dirPatrolZ).normalized;
        Mover(dir, velocidadPatrulla);
    }

    void Perseguir()
    {
        if (objetivo == null) return;
        Vector3 dir = (objetivo.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();
        Mover(dir, modoBerserker ? velocidadPersecucion * 1.15f : velocidadPersecucion);
        Mirar(objetivo.position);
    }

    void Huir()
    {
        if (objetivo == null) return;
        Vector3 dir = (transform.position - objetivo.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();
        Mover(dir, velocidadHuida);
        Mirar(transform.position + dir);
    }

    void Atacar()
    {
        if (objetivo == null) return;
        Mirar(objetivo.position);
        if (tAtaque > 0f) return;
        tAtaque = modoBerserker ? cooldownAtaque * 0.6f : cooldownAtaque;
        if (animator != null)
        {
            animator.SetTrigger(tAttack);
            animator.SetBool(pAttack, true);
            animator.SetBool(pRun, false);
            animator.SetBool(pWalk, false);
        }
    }

    void Mover(Vector3 dir, float speed)
    {
        if (controller == null) { transform.position += dir * speed * Time.deltaTime; return; }
        Vector3 move = dir * speed;
        move.y = -2f;
        controller.Move(move * Time.deltaTime);
    }

    void Mirar(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion q = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, giroSuave * Time.deltaTime);
    }

    float DistanciaObjetivo()
    {
        if (objetivo == null) return 999f;
        return Vector3.Distance(transform.position, objetivo.position);
    }

    bool DebeHuir()
    {
        if (maxHealth <= 0) return false;
        return ((float)currentHealth / (float)maxHealth) <= umbralHuidaVida && estadoActual != Estado.Attack;
    }

    void CambiarEstado(Estado nuevo)
    {
        if (estadoActual == nuevo) return;
        estadoActual = nuevo;
        if (animator != null)
        {
            animator.SetBool(pWalk, nuevo == Estado.Patrol);
            animator.SetBool(pRun, nuevo == Estado.Chase || nuevo == Estado.Flee || nuevo == Estado.Attack || nuevo == Estado.Stunned || nuevo == Estado.Dead);
            animator.SetBool(pAttack, nuevo == Estado.Attack);
            animator.SetBool(pStun, nuevo == Estado.Stunned);
            animator.SetBool(pDead, nuevo == Estado.Dead);
        }
        if (nuevo == Estado.Attack && hitbox != null) hitbox.SetActivo(true);
    }

    void ActualizarAnimatorBase()
    {
        if (animator == null) return;
        if (estadoActual != Estado.Attack && hitbox != null) hitbox.SetActivo(false);
        if (estadoActual == Estado.Stunned) animator.SetBool(pStun, true);
        if (estadoActual == Estado.Dead) animator.SetBool(pDead, true);
    }

    public void ActivarBerserker()
    {
        modoBerserker = true;
        if (estadoActual != Estado.Dead && estadoActual != Estado.Stunned) CambiarEstado(Estado.Chase);
    }

    public void AplicarGolpeRecibido(int dano, Vector3 puntoImpacto, Vector3 direccion)
    {
        if (!vivo) return;
        if (!puedeRecibirGolpes) return;
        int danoFinal = dano;
        if (modoBerserker) danoFinal = Mathf.Max(1, dano - danoExtraBerserker / 2);
        currentHealth -= danoFinal;
        if (currentHealth < 0) currentHealth = 0;
        puedeRecibirGolpes = false;
        tStun = tiempoStunGolpe;
        tHitReact = tiempoRagdollGolpe;
        if (animator != null) animator.SetTrigger(pHitReact);
        if (usarRagdoll)
        {
            ActivarRagdollTemporal(puntoImpacto, direccion);
        }
        if (currentHealth <= 0)
        {
            MorirConRagdoll();
            return;
        }
        CambiarEstado(Estado.Stunned);
        Invoke(nameof(RecuperarGolpe), tiempoStunGolpe);
    }

    void RecuperarGolpe()
    {
        puedeRecibirGolpes = true;
        if (vivo && estadoActual == Estado.Stunned && !ragdollActivo)
            CambiarEstado(modoBerserker ? Estado.Chase : Estado.Patrol);
    }

    void ActivarRagdollTemporal(Vector3 puntoImpacto, Vector3 direccion)
    {
        if (ragdollBodies == null || ragdollBodies.Length == 0) return;
        PrepararRagdoll(true);
        ragdollActivo = true;
        if (ragdollBodies.Length > 0 && ragdollBodies[0] != null)
            ragdollBodies[0].AddForce((direccion + Vector3.up * 0.35f).normalized * impulsoRagdollGolpe, ForceMode.Impulse);
        CancelInvoke(nameof(DesactivarRagdollTemporal));
        Invoke(nameof(DesactivarRagdollTemporal), tiempoRagdollGolpe);
    }

    void DesactivarRagdollTemporal()
    {
        if (!vivo) return;
        PrepararRagdoll(false);
        ragdollActivo = false;
        RecuperarGolpe();
    }

    void MorirConRagdoll()
    {
        vivo = false;
        estadoActual = Estado.Dead;
        if (animator != null) animator.SetBool(pDead, true);
        if (hitbox != null) hitbox.SetActivo(false);
        PrepararRagdoll(true);
        ragdollActivo = true;
        CancelInvoke();
        Invoke(nameof(DesactivarRagdollMuerte), tiempoRagdollMuerte);
    }

    void DesactivarRagdollMuerte()
    {
        // Si quieres destruir el cadáver, descomenta la siguiente línea.
        // Destroy(gameObject);
    }

    void PrepararRagdoll(bool activo)
    {
        if (ragdollBodies != null)
        {
            for (int i = 0; i < ragdollBodies.Length; i++)
            {
                if (ragdollBodies[i] == null) continue;
                if (ragdollBodies[i].gameObject == gameObject) continue;
                ragdollBodies[i].isKinematic = !activo;
                ragdollBodies[i].detectCollisions = activo;
            }
        }
        if (ragdollColliders != null)
        {
            for (int i = 0; i < ragdollColliders.Length; i++)
            {
                if (ragdollColliders[i] == null) continue;
                if (ragdollColliders[i].gameObject == gameObject) continue;
                ragdollColliders[i].enabled = activo;
            }
        }
        if (controller != null) controller.enabled = !activo;
        if (animator != null) animator.enabled = !activo;
    }

    public void AnimationHitFrame()
    {
        if (hitbox != null) hitbox.SetActivo(true);
    }

    public void AnimationHitEnd()
    {
        if (hitbox != null) hitbox.SetActivo(false);
        if (estadoActual == Estado.Attack) animator.SetBool(pAttack, false);
    }
}
