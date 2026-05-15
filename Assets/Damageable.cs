/*using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 4;
    public int currentHealth = 4;

    [Header("Color damage types that can hurt this enemy")]
    [Tooltip("If empty, enemy is vulnerable to ALL colors (Red/Blue/Gray).")]
    public DamageType[] vulnerableTypes;

    [Header("Weapon tags that can hurt this enemy")]
    [Tooltip("Use tags like Kick, Rapier, LightSaber. If empty, any tag is allowed.")]
    public string[] allowedWeaponTags;

    [Header("Events")]
    public UnityEvent onHit;
    public UnityEvent onDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    // Called by WeaponHitbox AFTER checking this enemy can be damaged
    public void TakeDamage(int amount, DamageType damageType, GameObject source)
    {
        if (amount <= 0) return;

        currentHealth -= amount;
        Debug.Log(currentHealth);
        onHit?.Invoke();

        if (currentHealth <= 1)
        {
            currentHealth = 1;
            Die();
        }
    }

    // Called by WeaponHitbox BEFORE applying damage
    public bool CanBeDamagedBy(DamageType type, string weaponTag)
    {
        if (weaponTag != "LightSaber" || weaponTag != "Rapier")
        {
            return IsAllowedWeaponTag(weaponTag);
        }

        Debug.Log(IsVulnerableToColor(type) && IsAllowedWeaponTag(weaponTag));

        return IsVulnerableToColor(type) && IsAllowedWeaponTag(weaponTag);
    }

    bool IsVulnerableToColor(DamageType incoming)
    {
        if (vulnerableTypes == null || vulnerableTypes.Length == 0)
            return true; // no filter = any color hurts

        for (int i = 0; i < vulnerableTypes.Length; i++)
        {
            if (vulnerableTypes[i] == incoming)
                return true;
        }
        return false;
    }

    bool IsAllowedWeaponTag(string tag)
    {
        if (allowedWeaponTags == null || allowedWeaponTags.Length == 0)
            return true; // no filter = any weapon tag hurts

        for (int i = 0; i < allowedWeaponTags.Length; i++)
        {
            if (allowedWeaponTags[i] == tag)
                return true;
        }
        return false;
    }

    void Die()
    {
        Debug.Log("memueropaco");
        //onDeath?.Invoke();
        // TODO: play animation, drop loot, pooling, etc.
        Destroy(gameObject);
    }
}
*/
// ============================================================
// Damageable.cs
// ============================================================
// Gestiona la vida de un enemigo y los efectos de estado.
//
// EFECTOS DISPONIBLES:
//   · Normal  → daño directo sin efecto secundario
//   · Poison  → -0.5 vida/segundo durante 6 segundos
//   · Bleed   → al 5.º golpe inflige 3 de daño extra
//   · Red / Blue / Gray → daño de LightSaber (solo color)
//
// COOLDOWN DE IMPACTO:
//   El enemigo no puede recibir daño más de una vez cada
//   0.25 segundos (invencibilidad momentánea entre golpes).
//   Esto reemplaza el HashSet alreadyHit de los hitboxes,
//   que bloqueaba permanentemente al enemigo.
//
// NOTA PARA ALUMNO DE C++:
//   · Time.deltaTime  ≡  dt  del bucle de tu motor.
//   · UnityEvent      ≡  callback / puntero a función.
//   · [Header] y [Tooltip] solo son etiquetas del Inspector,
//     no afectan a la lógica en tiempo de ejecución.
// ============================================================

using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════
    // DEBUG SWITCHES
    // Pon en false la categoría que quieras silenciar.
    // En producción ponlos todos a false sin tocar nada más.
    // ══════════════════════════════════════════════════════════
    private const bool LOG_INIT      = true;
    private const bool LOG_COOLDOWN  = true;
    private const bool LOG_DANIO     = true;
    private const bool LOG_VENENO    = true;
    private const bool LOG_SANGRADO  = true;
    private const bool LOG_MUERTE    = true;

    // ══════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════

    [Header("Vida")]
    public int maxHealth     = 10;
    public int currentHealth = 10;

    [Header("Cooldown entre golpes")]
    [Tooltip("Segundos de invencibilidad tras recibir un impacto.")]
    public float cooldownEntreGolpes = 0.25f;

    [Header("Filtros de vulnerabilidad")]
    [Tooltip("Vacío = vulnerable a todo color (Red/Blue/Gray).")]
    public DamageType[] vulnerableTypes;

    [Tooltip("Vacío = cualquier arma puede dañar. Ej: Rapier, LightSaber, Kick")]
    public string[] allowedWeaponTags;

    [Header("Eventos Unity")]
    public UnityEvent onHit;
    public UnityEvent onDeath;

    // ══════════════════════════════════════════════════════════
    // ESTADO PRIVADO
    // ══════════════════════════════════════════════════════════

    // ── Cooldown ─────────────────────────────────────────────
    private float tiempoDesdeUltimoGolpe = 999f; // empieza listo para recibir daño

    // ── Veneno ───────────────────────────────────────────────
    private bool  estaEnvenenado     = false;
    private float tiempoVeneno       = 0f;
    private float acumuladorVeneno   = 0f;

    private const float DURACION_VENENO       = 6f;
    private const float INTERVALO_VENENO      = 1f;
    private const float DANIO_VENENO_POR_TICK = 0.5f;

    // ── Sangrado ─────────────────────────────────────────────
    private int golpesDeSangrado = 0;

    private const int GOLPES_PARA_EXPLOTAR   = 5;
    private const int DANIO_EXPLOSION_BLEED  = 3;

    // ══════════════════════════════════════════════════════════
    // INICIALIZACIÓN
    // ══════════════════════════════════════════════════════════

    void Awake()
    {
        InicializarVida();
    }

    void InicializarVida()
    {
        currentHealth = maxHealth;
        DebugInit(
            $"[Init] '{gameObject.name}' | Vida: {currentHealth}/{maxHealth}" +
            $" | Cooldown: {cooldownEntreGolpes}s" +
            $" | Tags: [{FormatArray(allowedWeaponTags)}]" +
            $" | Tipos: [{FormatArray(vulnerableTypes)}]"
        );
    }

    // ══════════════════════════════════════════════════════════
    // UPDATE — acumuladores de tiempo (sin coroutines)
    // En C++: void update(float dt) { ... }
    // ══════════════════════════════════════════════════════════

    void Update()
    {
        AvanzarCooldown();
        if (estaEnvenenado) ProcesarVenenoPorTiempo();
    }

    // ══════════════════════════════════════════════════════════
    // API PÚBLICA  ← los hitboxes solo llaman a estas dos
    // ══════════════════════════════════════════════════════════

    // ── CanBeDamagedBy ───────────────────────────────────────
    // La hitbox pregunta ANTES de aplicar daño.
    // Devuelve true  → el golpe pasa.
    // Devuelve false → el golpe se ignora.
    public bool CanBeDamagedBy(DamageType tipo, string weaponTag)
    {
        DebugCooldown(
            $"[Filtro] '{gameObject.name}' | tag='{weaponTag}' | tipo={tipo}"
        );

        if (EstaEnCooldown())          return false;
        if (!EsEtiquetaPermitida(weaponTag)) return false;
        if (weaponTag == "LightSaber" && !EsColorVulnerable(tipo)) return false;

        return true;
    }

    // ── TakeDamage ───────────────────────────────────────────
    // La hitbox llama DESPUÉS de que CanBeDamagedBy devuelva true.
    public void TakeDamage(int amount, DamageType damageType, GameObject source)
    {
        if (amount <= 0) return;

        DebugDanio(
            $"[Hit] '{gameObject.name}' -{amount} | tipo={damageType}" +
            $" | fuente='{source?.name}' | Vida antes: {currentHealth}"
        );

        ReiniciarCooldown();
        AplicarDanioDirecto(amount);
        onHit?.Invoke();
        ProcesarEfectoDeEstado(damageType);

        DebugDanio($"[Hit] Vida después: {currentHealth}/{maxHealth}");
    }

    // ══════════════════════════════════════════════════════════
    // COOLDOWN
    // ══════════════════════════════════════════════════════════

    void AvanzarCooldown()
    {
        // Acumula tiempo entre frames igual que un timer en C++.
        if (tiempoDesdeUltimoGolpe < cooldownEntreGolpes)
            tiempoDesdeUltimoGolpe += Time.deltaTime;
    }

    void ReiniciarCooldown()
    {
        tiempoDesdeUltimoGolpe = 0f;
        DebugCooldown(
            $"[Cooldown] '{gameObject.name}' bloqueado por {cooldownEntreGolpes}s"
        );
    }

    bool EstaEnCooldown()
    {
        bool enCooldown = tiempoDesdeUltimoGolpe < cooldownEntreGolpes;
        if (enCooldown)
            DebugCooldown(
                $"[Cooldown] '{gameObject.name}' aún en cooldown" +
                $" ({tiempoDesdeUltimoGolpe:F3}/{cooldownEntreGolpes}s) → BLOQUEADO"
            );
        return enCooldown;
    }

    // ══════════════════════════════════════════════════════════
    // EFECTOS DE ESTADO
    // En C++: switch(damageType) { case POISON: ... }
    // ══════════════════════════════════════════════════════════

    void ProcesarEfectoDeEstado(DamageType tipo)
    {
        switch (tipo)
        {
            case DamageType.Poison:
                ActivarVeneno();
                break;

            case DamageType.Bleed:
                RegistrarGolpeDeSangrado();
                break;

            case DamageType.Normal:
            case DamageType.Red:
            case DamageType.Blue:
            case DamageType.Gray:
                // Sin efecto de estado adicional.
                break;

            default:
                Debug.LogWarning(
                    $"[Damageable] ⚠ DamageType '{tipo}' sin caso en el switch. " +
                    "Añádelo en DamageTypes.cs y trátalo aquí."
                );
                break;
        }
    }

    // ══════════════════════════════════════════════════════════
    // VENENO
    // ══════════════════════════════════════════════════════════

    void ActivarVeneno()
    {
        estaEnvenenado  = true;
        tiempoVeneno    = 0f;
        acumuladorVeneno = 0f;
        DebugVeneno(
            $"[Veneno] ACTIVADO en '{gameObject.name}'" +
            $" | {DURACION_VENENO}s | -{DANIO_VENENO_POR_TICK}/tick"
        );
    }

    void ProcesarVenenoPorTiempo()
    {
        tiempoVeneno     += Time.deltaTime;
        acumuladorVeneno += Time.deltaTime;

        if (acumuladorVeneno >= INTERVALO_VENENO)
        {
            acumuladorVeneno -= INTERVALO_VENENO;
            AplicarDanioDirecto(Mathf.CeilToInt(DANIO_VENENO_POR_TICK));
            DebugVeneno(
                $"[Veneno] Tick -{Mathf.CeilToInt(DANIO_VENENO_POR_TICK)}" +
                $" | {tiempoVeneno:F1}/{DURACION_VENENO}s" +
                $" | Vida: {currentHealth}/{maxHealth}"
            );
        }

        if (tiempoVeneno >= DURACION_VENENO)
            DesactivarVeneno();
    }

    void DesactivarVeneno()
    {
        estaEnvenenado  = false;
        tiempoVeneno    = 0f;
        acumuladorVeneno = 0f;
        DebugVeneno($"[Veneno] TERMINADO en '{gameObject.name}'");
    }

    // ══════════════════════════════════════════════════════════
    // SANGRADO
    // ══════════════════════════════════════════════════════════

    void RegistrarGolpeDeSangrado()
    {
        golpesDeSangrado++;
        DebugSangrado(
            $"[Sangrado] Golpe {golpesDeSangrado}/{GOLPES_PARA_EXPLOTAR}" +
            $" en '{gameObject.name}'"
        );

        if (golpesDeSangrado >= GOLPES_PARA_EXPLOTAR)
            ExplotarSangrado();
    }

    void ExplotarSangrado()
    {
        golpesDeSangrado = 0;
        DebugSangrado(
            $"[Sangrado] ¡EXPLOSIÓN! -{DANIO_EXPLOSION_BLEED}" +
            $" en '{gameObject.name}' | Vida antes: {currentHealth}"
        );
        AplicarDanioDirecto(DANIO_EXPLOSION_BLEED);
        DebugSangrado($"[Sangrado] Vida después: {currentHealth}/{maxHealth}");
    }

    // ══════════════════════════════════════════════════════════
    // DAÑO DIRECTO — única función que baja currentHealth
    // En C++: enemy.vida -= cantidad;
    // ══════════════════════════════════════════════════════════

    void AplicarDanioDirecto(int cantidad)
    {
        currentHealth -= cantidad;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Morir();
        }
    }

    // ══════════════════════════════════════════════════════════
    // FILTROS DE VULNERABILIDAD
    // ══════════════════════════════════════════════════════════

    bool EsColorVulnerable(DamageType tipo)
    {
        if (vulnerableTypes == null || vulnerableTypes.Length == 0)
        {
            DebugCooldown("[Filtro→Color] Lista vacía → vulnerable a todo");
            return true;
        }

        for (int i = 0; i < vulnerableTypes.Length; i++)
        {
            if (vulnerableTypes[i] == tipo)
            {
                DebugCooldown($"[Filtro→Color] {tipo} ENCONTRADO → PASA");
                return true;
            }
        }

        DebugCooldown(
            $"[Filtro→Color] {tipo} NO está en [{FormatArray(vulnerableTypes)}] → BLOQUEADO"
        );
        return false;
    }

    bool EsEtiquetaPermitida(string etiqueta)
    {
        if (allowedWeaponTags == null || allowedWeaponTags.Length == 0)
        {
            DebugCooldown("[Filtro→Tag] Lista vacía → cualquier arma pasa");
            return true;
        }

        for (int i = 0; i < allowedWeaponTags.Length; i++)
        {
            if (allowedWeaponTags[i] == etiqueta)
            {
                DebugCooldown($"[Filtro→Tag] '{etiqueta}' ENCONTRADO → PASA");
                return true;
            }
        }

        DebugCooldown(
            $"[Filtro→Tag] '{etiqueta}' NO está en [{FormatArray(allowedWeaponTags)}] → BLOQUEADO"
        );
        return false;
    }

    // ══════════════════════════════════════════════════════════
    // MUERTE
    // ══════════════════════════════════════════════════════════

    void Morir()
    {
        DebugMuerte($"[Muerte] '{gameObject.name}' ha muerto.");
        onDeath?.Invoke();
        Destroy(gameObject);
    }

    // ══════════════════════════════════════════════════════════
    // HELPERS DE DEBUG — encapsulados por categoría
    // El alumnado no necesita saber cómo funcionan por dentro.
    // Cambiar el switch de LOG_* es suficiente para activarlos.
    // ══════════════════════════════════════════════════════════

    void DebugInit     (string msg) { if (LOG_INIT)     Debug.Log(msg); }
    void DebugCooldown (string msg) { if (LOG_COOLDOWN) Debug.Log(msg); }
    void DebugDanio    (string msg) { if (LOG_DANIO)    Debug.Log(msg); }
    void DebugVeneno   (string msg) { if (LOG_VENENO)   Debug.Log(msg); }
    void DebugSangrado (string msg) { if (LOG_SANGRADO) Debug.Log(msg); }
    void DebugMuerte   (string msg) { if (LOG_MUERTE)   Debug.Log(msg); }

    // Convierte cualquier array a string legible para los logs.
    // En C++: template<typename T> std::string formatArray(T* arr, int size)
    string FormatArray<T>(T[] arr)
    {
        if (arr == null || arr.Length == 0) return "cualquiera";
        return string.Join(", ", arr);
    }
}