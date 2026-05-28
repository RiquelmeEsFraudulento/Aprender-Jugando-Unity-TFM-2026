// ============================================================
// Boss.cs — VERSIÓN DEFINITIVA
// ============================================================
// Boss: hereda de EnemyScript → Damageable.
//
// REGLAS:
//   1. Boss INVULNERABLE hasta recibir Sleep Y Confused.
//   2. Sleep: 8 golpes → boss duerme. Al terminar (5s) o al
//      recibir 1 hit → _yaRecibioSleep = true, boss despierta.
//   3. Confused: 8 golpes → boss confuso. Al terminar (4s) o
//      al recibir 1 hit → _yaRecibioConfused = true, boss OK.
//   4. Ambos true → boss vulnerable → siguiente golpe = muerte.
//   5. Mientras invulnerable: animación de hit pero 0 daño.
//   6. El boss NUNCA se queda parado: su AI (EnemyScript) sigue
//      funcionando normalmente entre estados.
//
// CAMBIOS NECESARIOS EN Damageable.cs:
//   AplicarSleep()         → public virtual
//   AplicarConfused()      → public virtual
//   LimpiarEstadoEspecial() → protected virtual
// ============================================================

using System.Collections;
using UnityEngine;

public class Boss : EnemyScript
{
    private bool _yaRecibioSleep = false;
    private bool _yaRecibioConfused = false;
    private bool _esVulnerable = false;
    private bool _derrotado = false;

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
        Debug.Log($"[Boss] Iniciado | Vida:{currentHealth} | Dmg:{danyoAtaque}");
    }

    // ══════════════════════════════════════════════════════════
    // OVERRIDE: AplicarSleep
    // ══════════════════════════════════════════════════════════

    public override bool AplicarSleep()
    {
        if (_derrotado) return false;

        bool ok = base.AplicarSleep();

        if (ok)
        {
            Debug.Log($"[Boss] ✦ SLEEP aplicado | S:{_yaRecibioSleep} C:{_yaRecibioConfused}");
        }

        return ok;
    }

    // ══════════════════════════════════════════════════════════
    // OVERRIDE: AplicarConfused
    // ══════════════════════════════════════════════════════════

    public override bool AplicarConfused()
    {
        if (_derrotado) return false;

        bool ok = base.AplicarConfused();

        if (ok)
        {
            Debug.Log($"[Boss] ✦ CONFUSED aplicado | S:{_yaRecibioSleep} C:{_yaRecibioConfused}");
        }

        return ok;
    }

    // ══════════════════════════════════════════════════════════
    // OVERRIDE: TakeDamage
    // ══════════════════════════════════════════════════════════

    public override void TakeDamage(int amount, DamageType damageType, GameObject source)
    {
        if (_derrotado) return;

        // ── A) INVULNERABLE: 0 daño + gestión de estados ──────
        if (!_esVulnerable)
        {
            Debug.Log($"[Boss] ✖ BLOQUEADO (-{amount}→0) | S:{_yaRecibioSleep} C:{_yaRecibioConfused}");

            // Si el boss está dormido → el hit lo despierta
            if (EstaDormido())
            {
                LimpiarEstadoEspecial();  // Despierta al boss (limpia estado Sleep)
                if (!_yaRecibioSleep)
                {
                    _yaRecibioSleep = true;
                    Debug.Log($"[Boss] ✦ SLEEP → registrado por hit | S:{_yaRecibioSleep} C:{_yaRecibioConfused}");
                    ComprobarVulnerabilidad();
                }
            }

            // Si el boss está confuso → el hit lo desconfunde
            if (EstaConfuso())
            {
                LimpiarEstadoEspecial();  // Desconfunde al boss
                if (!_yaRecibioConfused)
                {
                    _yaRecibioConfused = true;
                    Debug.Log($"[Boss] ✦ CONFUSED → registrado por hit | S:{_yaRecibioSleep} C:{_yaRecibioConfused}");
                    ComprobarVulnerabilidad();
                }
            }

            // Feedback visual (animación de hit, sin daño real)
            animator.SetTrigger("Hit");

            return;
        }

        // ── B) VULNERABLE: GOLPE FINAL ────────────────────────
        Debug.Log("[Boss] ★★★ GOLPE FINAL ★★★");

        currentHealth = 0;
        _derrotado = true;
        EjecutarMuerte();
    }

    // ══════════════════════════════════════════════════════════
    // VULNERABILIDAD
    // ══════════════════════════════════════════════════════════

    void ComprobarVulnerabilidad()
    {
        if (_yaRecibioSleep && _yaRecibioConfused && !_esVulnerable)
        {
            _esVulnerable = true;
            Debug.Log("╔══════════════════════════════════════╗");
            Debug.Log("║  [Boss] ¡VULNERABLE AL GOLPE FINAL!  ║");
            Debug.Log("╚══════════════════════════════════════╝");
        }
    }

    // ══════════════════════════════════════════════════════════
    // MUERTE
    // ══════════════════════════════════════════════════════════

    void EjecutarMuerte()
    {
        Debug.Log("[Boss] ████ DERROTADO ████");
        StopAllCoroutines();
        animator.SetTrigger("Death");
        this.enabled = false;
        characterController.enabled = false;
        enemyManager?.SetEnemyAvailability(this, false);
        enemyManager?.RemoveEnemy(this);
        playerCombat?.GetComponent<PlayerHealth>()?.GanarXP(XPEarned);
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
    // API
    // ══════════════════════════════════════════════════════════

    public bool RecibioSleep()    => _yaRecibioSleep;
    public bool RecibioConfused() => _yaRecibioConfused;
    public bool EsVulnerable()    => _esVulnerable;
    public bool EstaDerrotado()   => _derrotado;
}