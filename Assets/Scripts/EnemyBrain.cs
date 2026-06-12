// ============================================================
// EnemyManager.cs — MENTE COLMENA v5.2 COMPLETA
// ============================================================
// TODAS las funcionalidades de v5.0 + compatibilidad con:
//   - allEnemies (List<EnemyStruct>) 
//   - aliveEnemyCount (int)
//   - RemoveEnemy(EnemyScript)
//   - SetEnemyAvailability(EnemyScript, bool)
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════
    // COMPATIBILIDAD — Struct PÚBLICO que los scripts usan
    // PlayerUIComponent, GameManager, SimpleWalk... todos acceden a:
    //   enemyManager.allEnemies[i].enemyScript
    //   enemyManager.allEnemies[i].enemyAvailability
    // ══════════════════════════════════════════════════════════

    [System.Serializable]
    public struct EnemyStruct
    {
        public EnemyScript enemyScript;
        public bool enemyAvailability;
    }

    // Lista pública de compatibilidad
    public List<EnemyStruct> allEnemies = new List<EnemyStruct>();

    // ══════════════════════════════════════════════════════════
    // ESTADO INTERNO — Datos extra por enemigo (no visible)
    // ══════════════════════════════════════════════════════════

    private struct EnemyInternalState
    {
        public int consecutiveFailures;
        public float lastAttackTime;
    }

    private Dictionary<EnemyScript, EnemyInternalState> _states = new Dictionary<EnemyScript, EnemyInternalState>();

    // ══════════════════════════════════════════════════════════
    // COMPATIBILIDAD — Propiedad vida para scripts viejos
    // SpawnManager usa: enemyManager.aliveEnemyCount
    // ══════════════════════════════════════════════════════════
    public int aliveEnemyCount => AliveCount();

    // ══════════════════════════════════════════════════════════
    // INSPECTOR — Identidad
    // ══════════════════════════════════════════════════════════

    [Header("═══ COLMENA ═══")]
    public string hiveName = "Hive";
    [SerializeField] private bool _showDebugLogs = true;

    [Header("═══ RONDAS ═══")]
    public int totalRounds = 3;
    [SerializeField] private int currentRound = 1;

    [Header("═══ TIMING: ENJAMBRE (5+) ═══")]
    public float pauseSwarm = 1.2f;
    public float pauseSwarmVariance = 0.4f;

    [Header("═══ TIMING: PACK (3-4) ═══")]
    public float pausePack = 0.7f;
    public float pausePackVariance = 0.2f;

    [Header("═══ TIMING: DUO (2) ═══")]
    public float pauseDuo = 0.3f;
    public float duoCoordinationDelay = 0.8f;

    [Header("═══ TIMING: LAST STAND (1) ═══")]
    public float pauseLast = 0.15f;
    public float lastAggroRange = 2f;

    [Header("═══ ANTI-BLOQUEO ═══")]
    public float maxReadyWait = 3f;
    public float maxAttackWait = 6f;
    public float maxRetreatWait = 3f;
    public int maxConsecutiveFailures = 3;
    public float failureCooldown = 2f;
    public float minReuseCooldown = 2f;

    // ══════════════════════════════════════════════════════════
    // ESTADO INTERNO
    // ══════════════════════════════════════════════════════════

    private Coroutine _brain;
    private bool _alive = false;
    private EnemyScript _lastAttacker = null;
    private float _roundStartTime;
    private int _attacksThisRound = 0;
    private string _currentPhase = "Dormida";

    // Público para UI
    public int aliveCount => AliveCount();
    public int totalCount => allEnemies.Count;
    public bool hiveAlive => _alive && AliveCount() > 0;
    public string hivePhase => _currentPhase;

    // ══════════════════════════════════════════════════════════
    // LIFECYCLE
    // ══════════════════════════════════════════════════════════

    void Start()
    {
        RegisterAllEnemiesInChildren();
        WakeUp();
    }

    void OnDestroy()
    {
        _alive = false;
        StopAllCoroutines();
        _brain = null;
    }

    // ══════════════════════════════════════════════════════════
    // API PÚBLICA — WakeUp / Shutdown / NotifyDeath
    // ══════════════════════════════════════════════════════════

    public void WakeUp()
    {
        if (_alive) return;
        _alive = true;
        _lastAttacker = null;
        _roundStartTime = Time.time;
        _attacksThisRound = 0;
        _states.Clear();

        for (int i = 0; i < allEnemies.Count; i++)
        {
            var es = allEnemies[i].enemyScript;
            if (es != null)
                _states[es] = new EnemyInternalState { consecutiveFailures = 0, lastAttackTime = -99f };
        }

        _brain = StartCoroutine(Brain());
        Log($"🐛 ¡Colmena DESPIERTA! {AliveCount()} agentes.");
    }

    public void Shutdown()
    {
        _alive = false;
        StopAllCoroutines();
        _brain = null;
        Log("💀 Colmena apagando.");
    }

    public void NotifyDeath(EnemyScript e)
    {
        int before = AliveCount();
        RemoveEnemy(e);
        allEnemies.RemoveAll(entry => entry.enemyScript == e);

        if (_lastAttacker == e) _lastAttacker = null;
        _states.Remove(e);

        int after = AliveCount();
        if (before != after)
            Log($"⚱ '{e.name}' cayó. {after} restantes. Fase: {GetPhaseName(after)}");
    }

    // ══════════════════════════════════════════════════════════
    // REGISTRO
    // ══════════════════════════════════════════════════════════

    public void RegisterAllEnemiesInChildren()
    {
        // No limpiar si ya hay enemigos registrados (evita sobrescribir en runtime)
        var found = GetComponentsInChildren<EnemyScript>(true);

        foreach (var e in found)
        {
            if (e == null) continue;

            // Verificar si ya está registrado
            bool alreadyRegistered = false;
            for (int i = 0; i < allEnemies.Count; i++)
            {
                if (allEnemies[i].enemyScript == e)
                {
                    alreadyRegistered = true;
                    break;
                }
            }

            if (!alreadyRegistered)
            {
                allEnemies.Add(new EnemyStruct { enemyScript = e, enemyAvailability = true });
                _states[e] = new EnemyInternalState { consecutiveFailures = 0, lastAttackTime = -99f };
            }
        }

        Log($"🐝 Colmena: {allEnemies.Count} agentes registrados.");
    }

    // ══════════════════════════════════════════════════════════
    // MENTE COLMENA — BRAIN LOOP COMPLETO
    // ══════════════════════════════════════════════════════════

    IEnumerator Brain()
    {
        yield return new WaitForSeconds(0.5f);

        while (_alive)
        {
            int vivos = AliveCount();
            UpdatePhase(vivos);

            // ── EXTINCIÓN ─────────────────────────────────────
            if (vivos <= 0)
            {
                Log("💀 COLMENA EXTINTA. Fin de la mente.");
                _alive = false;
                _brain = null;
                yield break;
            }

            if (!HasLivingEnemies())
            {
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            // ── SELECCIONAR ATACANTE ──────────────────────────
            EnemyScript attacker = SelectAttacker();

            if (attacker == null)
            {
                LogVerbose("⏳ Nadie listo. Esperando...");
                yield return new WaitForSeconds(0.4f);
                continue;
            }

            Log($"🐛→⚔ '{attacker.name}' seleccionado. Fase: {_currentPhase} ({vivos} vivos)");

            // ── ESPERAR A QUE ESTÉ LISTO ──────────────────────
            float timer = 0f;
            bool started = false;

            if (IsReady(attacker))
            {
                started = true;
            }
            else
            {
                float waitLimit = GetReadyWaitByPhase(vivos);

                while (!IsReady(attacker))
                {
                    if (!IsAlive(attacker))
                    {
                        Log($"⚱ '{attacker.name}' murió esperando.");
                        break;
                    }

                    if (AliveCount() == 0) { _alive = false; yield break; }

                    timer += 0.1f;
                    if (timer >= waitLimit)
                    {
                        LogWarning($"⏰ '{attacker.name}' atascado ({timer:F1s}). Force reset.");
                        FreeAgent(attacker);
                        break;
                    }

                    yield return new WaitForSeconds(0.1f);
                }

                if (IsAlive(attacker))
                    started = true;
            }

            if (!started || !IsAlive(attacker))
            {
                MarkFailure(attacker);
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            // ── EJECUTAR ATAQUE ───────────────────────────────
            string enemyName = attacker.name;
            attacker.SetAttack();
            _attacksThisRound++;

            LogVerbose($"⚔ '{enemyName}' atacando...");

            float atkTimer = 0f;
            while (attacker.IsPreparingAttack())
            {
                if (!IsAlive(attacker))
                {
                    Log($"⚱ '{enemyName}' cayó atacando.");
                    break;
                }

                if (AliveCount() == 0) { _alive = false; yield break; }

                atkTimer += 0.1f;
                if (atkTimer >= maxAttackWait)
                {
                    LogWarning($"⏰ Ataque timeout '{enemyName}'.");
                    FreeAgent(attacker);
                    attacker.StopMoving();
                    break;
                }

                yield return new WaitForSeconds(0.1f);
            }

            if (!IsAlive(attacker))
            {
                MarkFailure(attacker);
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            // ── RETIRADA (excepto Last Stand) ─────────────────
            bool isLastStand = (AliveCount() <= 1);

            if (!isLastStand && IsAlive(attacker))
            {
                LogVerbose($"🏃 '{enemyName}' retirándose.");
                attacker.SetRetreat();

                float retTimer = 0f;
                while (attacker.IsRetreating())
                {
                    if (!IsAlive(attacker))
                    {
                        Log($"⚱ '{enemyName}' cayó retirándose.");
                        break;
                    }

                    if (AliveCount() == 0) { _alive = false; yield break; }

                    retTimer += 0.1f;
                    if (retTimer >= maxRetreatWait)
                    {
                        LogWarning($"⏰ Retirada larga '{enemyName}'.");
                        FreeAgent(attacker);
                        attacker.StopMoving();
                        break;
                    }

                    yield return new WaitForSeconds(0.1f);
                }

                if (IsAlive(attacker))
                    attacker.ResetAllStates();
            }
            else if (isLastStand && IsAlive(attacker))
            {
                Log($"🔥 '{enemyName}' ÚLTIMO. Sin retirada.");
            }

            // ── POST-ATAQUE ──────────────────────────────────
            if (IsAlive(attacker))
            {
                ResetInternalState(attacker);
                _lastAttacker = attacker;
            }
            else
            {
                MarkFailure(attacker);
            }

            // ── PAUSA ESTRATÉGICA ─────────────────────────────
            float pause = GetPauseByPhase(AliveCount());

            // Asegurar cooldown mínimo para el atacante
            if (_states.ContainsKey(attacker))
            {
                var st = _states[attacker];
                float timeSince = Time.time - st.lastAttackTime;
                float remaining = minReuseCooldown - timeSince;
                if (remaining > 0)
                    pause = Mathf.Max(pause, remaining);
            }

            LogVerbose($"⏳ Pausa: {pause:F1s}");

            float pauseElapsed = 0f;
            while (pauseElapsed < pause)
            {
                if (AliveCount() == 0) { _alive = false; yield break; }
                pauseElapsed += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
        }

        _brain = null;
        Log("💤 Brain terminado.");
    }

    // ══════════════════════════════════════════════════════════
    // SELECCIÓN — INTELIGENCIA COLECTIVA
    // ══════════════════════════════════════════════════════════

    EnemyScript SelectAttacker()
    {
        int vivos = AliveCount();
        if (vivos <= 0) return null;

        // ── Candidatos sanos ────────────────────────────────
        List<EnemyScript> candidates = new List<EnemyScript>();

        for (int i = 0; i < allEnemies.Count; i++)
        {
            var data = allEnemies[i];
            if (!data.enemyAvailability) continue;
            if (!IsAlive(data.enemyScript)) continue;
            if (!IsReady(data.enemyScript)) continue;

            // Cooldown
            if (_states.ContainsKey(data.enemyScript))
            {
                float timeSince = Time.time - _states[data.enemyScript].lastAttackTime;
                if (timeSince < minReuseCooldown && vivos > 1) continue;
            }

            candidates.Add(data.enemyScript);
        }

        // ── Fallback: cualquiera no retirando ───────────────
        if (candidates.Count == 0)
        {
            LogVerbose("⚠ Sin candidatos limpios. Fallback...");
            for (int i = 0; i < allEnemies.Count; i++)
            {
                var data = allEnemies[i];
                if (!data.enemyAvailability) continue;
                if (!IsAlive(data.enemyScript)) continue;
                if (data.enemyScript.IsRetreating()) continue;
                candidates.Add(data.enemyScript);
            }
        }

        // ── Fallback nuclear: rescatar al primero ───────────
        if (candidates.Count == 0)
        {
            LogVerbose("⚠ Nadie disponible. Rescatando...");
            for (int i = 0; i < allEnemies.Count; i++)
            {
                var data = allEnemies[i];
                if (!IsAlive(data.enemyScript)) continue;
                FreeAgent(data.enemyScript);
                candidates.Add(data.enemyScript);
                break;
            }
        }

        if (candidates.Count == 0) return null;

        // ── Por fase ────────────────────────────────────────
        if (vivos <= 1 && candidates.Count >= 1)
            return candidates[0];

        if (vivos <= 2 && candidates.Count > 1 && _lastAttacker != null)
        {
            var filtered = FilterOut(candidates, _lastAttacker);
            if (filtered.Count > 0) return filtered[Random.Range(0, filtered.Count)];
        }

        if (vivos <= 4 && candidates.Count > 1 && _lastAttacker != null)
        {
            var filtered = FilterOut(candidates, _lastAttacker);
            filtered = FilterZeroFailures(filtered);
            if (filtered.Count > 0) return filtered[Random.Range(0, filtered.Count)];
        }

        // ── Aleatorio ponderado por fallos ──────────────────
        return WeightedRandom(candidates);
    }

    // ══════════════════════════════════════════════════════════
    // HELPERS DE ESTADO
    // ══════════════════════════════════════════════════════════

    bool IsAlive(EnemyScript e)
    {
        if (e == null) return false;
        if (!e.gameObject.activeInHierarchy) return false;
        if (!e.IsAttackable()) return false;
        return true;
    }

    bool IsReady(EnemyScript e)
    {
        if (!IsAlive(e)) return false;
        if (e.IsRetreating()) return false;
        if (e.IsLockedTarget()) return false;
        if (e.IsStunned()) return false;
        if (e.EstaDormido()) return false;
        if (e.EstaConfuso()) return false;
        
        // NUEVO: Si lleva mucho tiempo bloqueado, marcar como no listo
        // temporalmente para que otro enemigo ataque
        if (e.IsPathBlocked() && e.GetBlockedTime() > 3f)
        {
            LogVerbose($"⏳ '{e.name}' lleva mucho tiempo bloqueado. Saltando...");
            return false;
        }
        
        return true;
    }

    bool HasLivingEnemies()
    {
        for (int i = 0; i < allEnemies.Count; i++)
            if (IsAlive(allEnemies[i].enemyScript)) return true;
        return false;
    }

    // ══════════════════════════════════════════════════════════
    // HELPERS DE FASE
    // ══════════════════════════════════════════════════════════

    string GetPhaseName(int vivos)
    {
        if (vivos <= 0) return "💀 Extinta";
        if (vivos <= 1) return "🔥 Last Stand";
        if (vivos <= 2) return "⚔ Duo";
        if (vivos <= 4) return "🐺 Pack";
        return "🐝 Enjambre";
    }

    void UpdatePhase(int vivos)
    {
        string newPhase = GetPhaseName(vivos);
        if (newPhase != _currentPhase)
        {
            _currentPhase = newPhase;
            if (vivos <= 1) Log("🔥¡¡LAST STAND!!");
            else if (vivos <= 2) Log("⚔ Fase DUO.");
            else if (vivos <= 4) Log($"🐺 Fase PACK ({vivos}).");
        }
    }

    float GetPauseByPhase(int vivos)
    {
        if (vivos <= 1) return pauseLast;
        if (vivos <= 2) return pauseDuo;
        if (vivos <= 4) return pausePack + Random.Range(-pausePackVariance, pausePackVariance);
        return pauseSwarm + Random.Range(-pauseSwarmVariance, pauseSwarmVariance);
    }

    float GetReadyWaitByPhase(int vivos)
    {
        if (vivos <= 1) return maxReadyWait * 0.5f;
        if (vivos <= 2) return maxReadyWait * 0.7f;
        return maxReadyWait;
    }

    // ══════════════════════════════════════════════════════════
    // GESTIÓN DE FALLOS
    // ══════════════════════════════════════════════════════════

    void MarkFailure(EnemyScript e)
    {
        if (e == null || !_states.ContainsKey(e)) return;

        var st = _states[e];
        st.consecutiveFailures++;
        _states[e] = st;

        LogVerbose($"⚠ '{e.name}' fallo #{st.consecutiveFailures}");

        if (st.consecutiveFailures >= maxConsecutiveFailures)
        {
            LogWarning($"🚫 '{e.name}' falla {maxConsecutiveFailures}x. Cooldown {failureCooldown}s.");
            SetEnemyAvailability(e, false);
            StartCoroutine(ReactivateAfterCooldown(e));
        }
    }

    IEnumerator ReactivateAfterCooldown(EnemyScript e)
    {
        yield return new WaitForSeconds(failureCooldown);

        if (IsAlive(e) && _states.ContainsKey(e))
        {
            SetEnemyAvailability(e, true);
            ResetInternalState(e);
            e.ResetAllStates();
            Log($"✅ '{e.name}' reactivado.");
        }
    }

    void FreeAgent(EnemyScript e)
    {
        if (e == null) return;
        LogVerbose($"🔧 Rescatando '{e.name}'");
        e.ResetAllStates();
        e.ReleaseLock();
        e.StopMoving();
    }

    void ResetInternalState(EnemyScript e)
    {
        if (e == null || !_states.ContainsKey(e)) return;
        var st = _states[e];
        st.consecutiveFailures = 0;
        st.lastAttackTime = Time.time;
        _states[e] = st;
    }

    // ══════════════════════════════════════════════════════════
    // QUERIES PÚBLICAS
    // ══════════════════════════════════════════════════════════

    public int AliveCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Count; i++)
        {
            var e = allEnemies[i].enemyScript;
            if (e == null) continue;
            if (!e.gameObject.activeInHierarchy) continue;
            if (e.IsAttackable()) count++;
        }
        return count;
    }

    public bool AnyPreparingAttack()
    {
        for (int i = 0; i < allEnemies.Count; i++)
        {
            var e = allEnemies[i].enemyScript;
            if (e == null || !IsAlive(e)) continue;
            if (!allEnemies[i].enemyAvailability) continue;
            if (e.IsPreparingAttack()) return true;
        }
        return false;
    }

    public bool IsExtinct() => AliveCount() <= 0;

    public int GetTotalRounds() => totalRounds;
    public int GetCurrentRound() => currentRound;
    public void SetCurrentRound(int round) => currentRound = round;
    public int GetAttacksThisRound() => _attacksThisRound;

    // ══════════════════════════════════════════════════════════
    // MUTACIÓN — API que otros scripts llaman
    // ══════════════════════════════════════════════════════════

    public void SetEnemyAvailability(EnemyScript enemy, bool state)
    {
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyScript == enemy)
            {
                var entry = allEnemies[i];
                entry.enemyAvailability = state;
                allEnemies[i] = entry;
                break;
            }
        }

        if (_lastAttacker == enemy) _lastAttacker = null;

        var det = FindAnyObjectByType<EnemyDetection>();
        if (det != null && det.CurrentTarget() == enemy)
            det.SetCurrentTarget(null);
    }

    public void RemoveEnemy(EnemyScript enemy)
    {
        allEnemies.RemoveAll(entry => entry.enemyScript == enemy);

        if (_lastAttacker == enemy) _lastAttacker = null;
        _states.Remove(enemy);

        LogVerbose($"🗑 '{enemy.name}' removido. Restantes: {allEnemies.Count}");
    }

    // ══════════════════════════════════════════════════════════
    // DIAGNÓSTICO
    // ══════════════════════════════════════════════════════════

    [ContextMenu("🔍 Diagnóstico")]
    public void Diagnose()
    {
        Log("🔍 ═══ DIAGNÓSTICO ═══");
        for (int i = 0; i < allEnemies.Count; i++)
        {
            var e = allEnemies[i].enemyScript;
            if (e == null) { Log($"  NULL entry at {i}"); continue; }
            Log($"  {(IsAlive(e) ? "🟢" : "💀")} {e.name} | Health: {e.currentHealth} | Available: {allEnemies[i].enemyAvailability}");
        }
        Log($"═══ Vivos: {AliveCount()} | Total: {allEnemies.Count} ═══");
    }

    // ══════════════════════════════════════════════════════════
    // HELPERS PRIVADOS
    // ══════════════════════════════════════════════════════════

    List<EnemyScript> FilterOut(List<EnemyScript> list, EnemyScript exclude)
    {
        var result = new List<EnemyScript>();
        foreach (var e in list) if (e != exclude) result.Add(e);
        return result;
    }

    List<EnemyScript> FilterZeroFailures(List<EnemyScript> list)
    {
        var result = new List<EnemyScript>();
        foreach (var e in list)
        {
            if (_states.ContainsKey(e) && _states[e].consecutiveFailures == 0)
                result.Add(e);
        }
        return result.Count > 0 ? result : list;
    }

    EnemyScript WeightedRandom(List<EnemyScript> candidates)
    {
        var weighted = new List<EnemyScript>();
        foreach (var c in candidates)
        {
            int weight = 1;
            if (_states.ContainsKey(c))
                weight = Mathf.Max(1, 4 - _states[c].consecutiveFailures);
            for (int w = 0; w < weight; w++) weighted.Add(c);
        }
        return weighted[Random.Range(0, weighted.Count)];
    }

    // ══════════════════════════════════════════════════════════
    // LOGGING
    // ══════════════════════════════════════════════════════════

    void Log(string msg) { if (_showDebugLogs) Debug.Log($"[Colmena] {msg}"); }
    void LogWarning(string msg) { Debug.LogWarning($"[Colmena] {msg}"); }

    void LogVerbose(string msg)
    {
        #if UNITY_EDITOR
        if (_showDebugLogs) Debug.Log($"[Colmena] {msg}");
        #endif
    }
}