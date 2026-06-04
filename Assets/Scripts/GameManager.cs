// ══════════════════════════════════════════════════════════
// GameManager.cs — FIX TOTAL: while loop, no recursion
// ══════════════════════════════════════════════════════════

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Jugador")]
    public GameObject player;
    public Transform spawnPoint;

    [Header("Enemy Spawner")]
    public EnemySpawner enemySpawner;

    [Header("Story Data")]
    public StoryTextData storyData;

    [Header("Orden de Escenas")]
    public List<string> sceneOrder = new List<string>()
    {
        "TestingScene",
        "Scene2_Aliens",
        "Scene3_Peluches",
        "Scene4_Alonso",
        "Scene5_Maniquis",
        "Scene6_Boss",
        "FinalScene"
    };

    [Header("Rondas por Escena Normal")]
    public int roundsPerScene = 3;

    private int currentSceneIndex = 0;
    private int currentRound = 0;
    private bool isInBattle = false;
    private bool isInCutscene = false;
    private bool hasShownIntro = false;
    private int extraRoundsFromDeath = 0;

    private Coroutine activeBattleCoroutine;

    private float savedPlayerHealth = -1f;
    private float savedPlayerMaxHealth = -1f;
    private float savedPlayerXP = -1f;
    private float savedPlayerLevel = -1f;

    public int savedlifeStealCount;       // Cantidad de Life Steal (0-5)
    public int savedPotionCount;          // Cantidad de Pociones (0-5)
    public int savedLifeStoneCount;       // Cantidad de Life Stones (0-1)
    public int savedChaosCount;           // Cantidad de Chaos (0-2)

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() => AutoFindReferences();

    void AutoFindReferences()
    {
        if (player == null) { SimpleWalk sw = FindAnyObjectByType<SimpleWalk>(); if (sw != null) player = sw.gameObject; }
        if (enemySpawner == null) enemySpawner = FindAnyObjectByType<EnemySpawner>();
        if (spawnPoint == null && player != null) spawnPoint = player.transform;
    }

    bool IsTestingScene() => GameObject.FindGameObjectWithTag("TestCube") != null;
    bool IsBossScene() => currentSceneIndex == sceneOrder.IndexOf("Scene6_Boss");
    bool IsFinalScene() => currentSceneIndex == sceneOrder.Count - 1;

    int GetTotalRoundsForThisScene()
    {
        return (IsBossScene() ? 1 : roundsPerScene) + extraRoundsFromDeath;
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(AfterSceneLoaded());

    IEnumerator AfterSceneLoaded()
    {
        Debug.Log($"[GM] === ESCENA [{currentSceneIndex}] {sceneOrder[currentSceneIndex]} ===");
        yield return null;

        AutoFindReferences();

        if (player != null && spawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
            if (cc != null) cc.enabled = true;
        }

        RestorePlayerStats();

        if (IsTestingScene()) yield return StartCoroutine(HandleTestingScene());
        else if (IsFinalScene()) yield return StartCoroutine(HandleFinalScene());
        else if (IsBossScene()) yield return StartCoroutine(HandleBossScene());
        else if (currentSceneIndex > 0) yield return StartCoroutine(HandleNormalBattleScene());
        else SetPlayerActive(true);
    }

    // ══════════════════════════════════════════════════════════
    // TESTING
    // ══════════════════════════════════════════════════════════

    IEnumerator HandleTestingScene()
    {
        SetPlayerActive(true);
        while (GameObject.FindGameObjectWithTag("TestCube") != null) yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(AdvanceToNextScene());
    }

    // ══════════════════════════════════════════════════════════
    // FINAL
    // ══════════════════════════════════════════════════════════

    IEnumerator HandleFinalScene()
    {
        SetPlayerActive(true);
        if (!hasShownIntro) { hasShownIntro = true; yield return StartCoroutine(ShowIntroCutscene()); }
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(ShowOutroCutscene());
        Debug.Log("[GM] ████ COMPLETADO ████");
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(AdvanceToNextScene());
    }

    // ══════════════════════════════════════════════════════════
    // BOSS — while loop, no recursion
    // ══════════════════════════════════════════════════════════

    IEnumerator HandleBossScene()
    {
        extraRoundsFromDeath = 0;
        int total = GetTotalRoundsForThisScene();
        Debug.Log($"[GM] === BOSS — {total} rondas (extra:{extraRoundsFromDeath}) ===");

        yield return StartCoroutine(SpawnFirstEnemies());
        SetPlayerActive(true);
        if (!hasShownIntro) { hasShownIntro = true; yield return StartCoroutine(ShowIntroCutscene()); }

        StopActiveBattle();
        activeBattleCoroutine = StartCoroutine(BattleLoop(total, true));
        yield return activeBattleCoroutine;
        activeBattleCoroutine = null;

        yield return StartCoroutine(ShowOutroCutscene());
        yield return StartCoroutine(AdvanceToNextScene());
    }

    // ══════════════════════════════════════════════════════════
    // NORMAL — while loop, no recursion
    // ══════════════════════════════════════════════════════════

    IEnumerator HandleNormalBattleScene()
    {
        extraRoundsFromDeath = 0;
        int total = GetTotalRoundsForThisScene();
        Debug.Log($"[GM] === BATALLA NORMAL — {total} rondas (base:{roundsPerScene} + extra:{extraRoundsFromDeath}) ===");

        yield return StartCoroutine(SpawnFirstEnemies());
        SetPlayerActive(true);
        if (!hasShownIntro) { hasShownIntro = true; yield return StartCoroutine(ShowIntroCutscene()); }

        StopActiveBattle();
        currentRound = 0;
        activeBattleCoroutine = StartCoroutine(BattleLoop(total, false));
        yield return activeBattleCoroutine;
        activeBattleCoroutine = null;
    }

    // ══════════════════════════════════════════════════════════
    // BATTLE LOOP — WHILE, NO RECURSION
    // ══════════════════════════════════════════════════════════

    IEnumerator BattleLoop(int totalRounds, bool isBoss)
    {
        while (currentRound < totalRounds)
        {
            currentRound++;
            Debug.Log($"[GM] ███ RONDA {currentRound}/{totalRounds} ███");
            isInBattle = true;

            yield return StartCoroutine(WaitForAllEnemiesDead());

            Debug.Log($"[GM] ███ RONDA {currentRound}/{totalRounds} COMPLETADA ███");
            isInBattle = false;

            if (currentRound < totalRounds)
            {
                Debug.Log($"[GM] Preparando ronda {currentRound + 1}/{totalRounds}...");
                yield return StartCoroutine(DestroyAllManagers());
                yield return StartCoroutine(SpawnNextWave());
            }
        }

        Debug.Log($"[GM] ███ TODAS LAS RONDAS COMPLETADAS ({totalRounds}) ███");
        activeBattleCoroutine = null;
        yield return StartCoroutine(ShowOutroCutscene());
        yield return StartCoroutine(AdvanceToNextScene());
    }

    // ══════════════════════════════════════════════════════════
    // STOP battle — kills the single coroutine
    // ══════════════════════════════════════════════════════════

    void StopActiveBattle()
    {
        if (activeBattleCoroutine != null)
        {
            Debug.Log("[GM] ■ DETENIENDO batalla activa.");
            StopCoroutine(activeBattleCoroutine);
            activeBattleCoroutine = null;
        }
        isInBattle = false;
    }

    // ══════════════════════════════════════════════════════════
    // SPAWN FIRST ENEMIES
    // ══════════════════════════════════════════════════════════

    // ══════════════════════════════════════════════════════════
// FIX: SpawnFirstEnemies — Usar nueva API
// ══════════════════════════════════════════════════════════

IEnumerator SpawnFirstEnemies()
{
    Debug.Log("[GM] === SPAWN PRIMERA OLEADA ===");
    SetPlayerActive(false);

    GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

    if (respawnPoints.Length == 0)
    {
        Debug.LogError("[GM] NO HAY SPAWN POINTS!");
        SetPlayerActive(true);
        yield break;
    }

    foreach (GameObject rp in respawnPoints)
    {
        EnemySpawner spawner = rp.GetComponent<EnemySpawner>();
        if (spawner != null)
        {
            EnemyManager spawned = spawner.Spawn();
            if (spawned != null)
            {
                Debug.Log($"[GM] Spawned: '{spawned.hiveName}' ({spawned.totalCount} agentes).");
                spawned.WakeUp();  // Despertar la colmena
            }
        }
    }

    yield return new WaitForSeconds(1f); // Dar tiempo a que se registren

    Debug.Log("[GM] === SPAWN COMPLETADO ===");
}

// ══════════════════════════════════════════════════════════
// FIX: SpawnNextWave — Resetear contadores de ronda
// ══════════════════════════════════════════════════════════

IEnumerator SpawnNextWave()
{
    Debug.Log("[GM] === SPAWN NUEVA OLEADA ===");
    yield return new WaitForSeconds(0.5f);

    foreach (var rp in GameObject.FindGameObjectsWithTag("Respawn"))
    {
        EnemySpawner sp = rp.GetComponent<EnemySpawner>();
        if (sp != null)
        {
            EnemyManager nm = sp.Spawn();
            if (nm != null)
            {
                Debug.Log($"[GM] Oleada: '{nm.hiveName}' ({nm.totalCount} agentes).");
                nm.WakeUp();
            }
        }
    }

    yield return new WaitForSeconds(1f);
    Debug.Log("[GM] === NUEVA OLEADA COMPLETADA ===");
}
    // ══════════════════════════════════════════════════════════
    // WAIT FOR ALL ENEMIES DEAD
    // ══════════════════════════════════════════════════════════

    // REEMPLAZA la función WaitForAllEnemiesDead en GameManager.cs:

IEnumerator WaitForAllEnemiesDead()
{
    float timeout = 60f;
    float elapsed = 0f;

    while (true)
    {
        if (player == null) yield break;

        var managers = FindObjectsByType<EnemyManager>();

        if (managers.Length == 0)
        {
            Debug.Log("[GM] Sin managers → enemigos muertos.");
            yield break;
        }

        int vivos = 0;
        foreach (var m in managers)
        {
            if (m != null)
                vivos += m.aliveEnemyCount;
        }

        if (vivos <= 0)
        {
            Debug.Log("[GM] 💀 ¡Todos muertos!");
            yield break;
        }

        elapsed += 0.5f;

        if (elapsed >= timeout)
        {
            Debug.LogWarning($"[GM] ⏰ TIMEOUT. Ejecutando {vivos} restantes.");
            foreach (var m in managers)
            {
                if (m == null) continue;
                m.Shutdown();

                // Matar agentes restantes
                var agents = new List<EnemyScript>();
                foreach (var entry in m.allEnemies)
                {
                    if (entry.enemyScript != null && entry.enemyScript.IsAttackable())
                        agents.Add(entry.enemyScript);
                }

                foreach (var agent in agents)
                {
                    try
                    {
                        if (agent != null && agent.IsAttackable())
                            agent.Morir();
                    }
                    catch
                    {
                        if (agent != null && agent.gameObject != null)
                            Destroy(agent.gameObject);
                    }
                }
            }
            yield return new WaitForSeconds(2f);
            yield break;
        }

        yield return new WaitForSeconds(0.5f);
    }
}

    // ══════════════════════════════════════════════════════════
    // DESTROY ALL MANAGERS
    // ══════════════════════════════════════════════════════════

    IEnumerator DestroyAllManagers()
    {
        Debug.Log("[GM] Despertando managers...");

        // Primero shutdown limpio
        foreach (var m in FindObjectsByType<EnemyManager>())
        {
            if (m != null)
                m.Shutdown();
        }

        yield return null;
        yield return null;

        // Luego destruir GameObjects
        foreach (var m in FindObjectsByType<EnemyManager>())
        {
            if (m != null)
                Destroy(m.gameObject);
        }

        yield return null;
        yield return null;

        // Limpiar spawners
        foreach (var rp in GameObject.FindGameObjectsWithTag("Respawn"))
        {
            var sp = rp.GetComponent<EnemySpawner>();
            sp?.ClearCurrentManager();
        }

        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        Debug.Log("[GM] Managers destruidos.");
    }

    // ══════════════════════════════════════════════════════════
    // INTRO / OUTRO
    // ══════════════════════════════════════════════════════════

    IEnumerator ShowIntroCutscene()
    {
        isInCutscene = true; Time.timeScale = 0f; SetPlayerActive(false);
        var st = storyData.GetSceneTexts(currentSceneIndex);
        if (st != null && st.introTexts.Count > 0) yield return StartCoroutine(DisplayTextSequence(st.introTexts.ToArray()));
        else { Debug.Log("[GM] Sin intro."); yield return new WaitForSecondsRealtime(1f); }
        Time.timeScale = 1f; isInCutscene = false; SetPlayerActive(true);
        Debug.Log("[GM] Intro terminada.");
    }

    IEnumerator ShowOutroCutscene()
    {
        isInCutscene = true; Time.timeScale = 0f; SetPlayerActive(false);
        var st = storyData.GetSceneTexts(currentSceneIndex);
        if (st != null && st.outroTexts.Count > 0) yield return StartCoroutine(DisplayTextSequence(st.outroTexts.ToArray()));
        else { Debug.Log("[GM] Sin outro."); yield return new WaitForSecondsRealtime(1f); }
        Time.timeScale = 1f; isInCutscene = false; SetPlayerActive(true);
        Debug.Log("[GM] Outro terminada.");
    }

    // ══════════════════════════════════════════════════════════
    // PLAYER DEATH — SIMPLE
    // ══════════════════════════════════════════════════════════

    public void OnPlayerDied() => StartCoroutine(ShowDeathScreenAndSequence());

    private bool _deathActive = false;
    private bool _pendingDeath = false;

    IEnumerator ShowDeathScreenAndSequence()
    {
        int total = GetTotalRoundsForThisScene();
        Debug.Log($"[GM] ☠☠☠ MUERTE — Ronda {currentRound}/{total} ☠☠☠");

        isInBattle = false; isInCutscene = true; Time.timeScale = 0f; SetPlayerActive(false);

        string txt = storyData != null ? storyData.deathText : "No me rendiré...\n\n[ENTER para continuar]";
        yield return StartCoroutine(DisplaySingleText(txt));

        Time.timeScale = 1f; isInCutscene = false;

        if (_deathActive) { _pendingDeath = true; yield break; }

        yield return StartCoroutine(PlayerDeathSequence());

        while (_pendingDeath)
        {
            _pendingDeath = false;
            Debug.Log("[GM] ☠ Muerte encolada.");
            yield return StartCoroutine(ShowDeathScreenForPending());
            yield return StartCoroutine(PlayerDeathSequence());
        }
    }

    IEnumerator ShowDeathScreenForPending()
    {
        isInCutscene = true; Time.timeScale = 0f; SetPlayerActive(false);
        Debug.Log($"[GM] ☠☠☠ MUERTE (encolado) — Ronda {currentRound}/{GetTotalRoundsForThisScene()} ☠☠☠");
        yield return StartCoroutine(DisplaySingleText(storyData != null ? storyData.deathText : "No me rendiré..."));
        Time.timeScale = 1f; isInCutscene = false;
    }

    IEnumerator PlayerDeathSequence()
    {
        _deathActive = true;

        int before = GetTotalRoundsForThisScene();
        extraRoundsFromDeath++;
        int after = GetTotalRoundsForThisScene();

        Debug.Log($"[GM] ☀ Extra: {before}→{after} rondas (extraRounds={extraRoundsFromDeath}). Reintentando ronda {currentRound}.");

        // ■ DETENER batalla activa (la while loop)
        StopActiveBattle();

        yield return StartCoroutine(DestroyAllManagers());
        yield return StartCoroutine(SpawnNextWave());
        RespawnPlayer();
        SetPlayerActive(true);

        // Ajustar: BattleLoop hace currentRound++ al entrar al while
        // Queremos que vuelva a la MISMA ronda
        currentRound--;
        if (currentRound < 0) currentRound = 0;

        Debug.Log($"[GM] ☀ currentRound={currentRound}, BattleLoop sube a {currentRound + 1}");

        _deathActive = false;

        // ■ LANZAR nueva BattleLoop con nuevo total
        activeBattleCoroutine = StartCoroutine(BattleLoop(after, IsBossScene()));
        yield return activeBattleCoroutine;
        activeBattleCoroutine = null;
    }

    void RespawnPlayer()
    {
        if (player == null || spawnPoint == null) return;
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;
        if (cc != null) cc.enabled = true;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.vida = savedPlayerHealth > 0 ? savedPlayerHealth : ph.vidaMax;
            ph.ResetDeath();
        }
        Debug.Log($"[GM] Respawn. Vida: {ph?.vida}/{ph?.vidaMax}");
    }

    // ══════════════════════════════════════════════════════════
    // TEXT DISPLAY
    // ══════════════════════════════════════════════════════════

    IEnumerator DisplayTextSequence(string[] texts)
    {
        var ui = GetOrCreateStoryUI();
        for (int i = 0; i < texts.Length; i++)
        {
            ui.ShowText(texts[i]);
            yield return StartCoroutine(ui.WaitForInput());
            ui.HideText();
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }

    IEnumerator DisplaySingleText(string text)
    {
        var ui = GetOrCreateStoryUI();
        ui.ShowText(text);
        yield return StartCoroutine(ui.WaitForInput());
        ui.HideText();
    }

    StoryTextUI_Canvas GetOrCreateStoryUI()
    {
        var ui = FindAnyObjectByType<StoryTextUI_Canvas>();
        if (ui == null) { var go = new GameObject("StoryTextUI"); ui = go.AddComponent<StoryTextUI_Canvas>(); DontDestroyOnLoad(go); }
        return ui;
    }

    // ══════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════

    void StartAIOnAllManagers()
{
    foreach (var m in FindObjectsByType<EnemyManager>())
    {
        if (m != null)
        {
            m.WakeUp();
            Debug.Log($"[GM] WakeUp '{m.hiveName}' ({m.AliveCount()} vivos).");
        }
    }
}

    void SetPlayerActive(bool active)
    {
        if (player == null) return;
        var sw = player.GetComponent<SimpleWalk>(); if (sw != null) sw.enabled = active;
        var cc = player.GetComponent<CharacterController>(); if (cc != null) cc.enabled = active;
        var an = player.GetComponent<Animator>(); if (an != null) an.enabled = active;
    }

    public void SavePlayerStats()
    {
        if (player == null) return;
        var ph = player.GetComponent<PlayerHealth>(); if (ph == null) return;
        savedPlayerHealth = ph.vida; savedPlayerMaxHealth = ph.vidaMax; savedPlayerXP = ph.XP; savedPlayerLevel = ph.level;
        Debug.Log($"[GM] Stats guardados: {savedPlayerHealth}/{savedPlayerMaxHealth}");

        var hud = player.GetComponentInChildren<PlayerHUDController>();
        if (hud != null)
        {
            //hud.SavePlayerHUDStats();
            savedlifeStealCount = hud.lifeStealCount;
            savedPotionCount = hud.potionCount;  
            savedLifeStoneCount = hud.lifeStoneCount;
            savedChaosCount = hud.chaosCount;       
        }
    }

    void RestorePlayerStats()
    {
        if (player == null) return;
        var ph = player.GetComponent<PlayerHealth>(); if (ph == null) return;
        if (savedPlayerHealth > 0) { ph.vida = savedPlayerHealth; ph.vidaMax = savedPlayerMaxHealth; ph.XP = savedPlayerXP; ph.level = savedPlayerLevel; }
        Debug.Log($"[GM] Stats restaurados: {ph.vida}/{ph.vidaMax}");

        var hud = player.GetComponentInChildren<PlayerHUDController>();
        if (hud != null)
        {
            hud.RestorePlayerHUDStats(savedlifeStealCount, savedPotionCount, savedLifeStoneCount, savedChaosCount);
            hud.AddItemsOnSceneChange();
        }
    }

    // ══════════════════════════════════════════════════════════
    // SCENE ADVANCE
    // ══════════════════════════════════════════════════════════

    IEnumerator AdvanceToNextScene()
    {
        Debug.Log("[GM] Avanzando...");
        SavePlayerStats();
        StopActiveBattle();
        currentSceneIndex++; hasShownIntro = false; currentRound = 0; extraRoundsFromDeath = 0;

        if (currentSceneIndex < sceneOrder.Count)
        {
            Debug.Log($"[GM] → [{currentSceneIndex}] {sceneOrder[currentSceneIndex]}");
            SceneManager.LoadScene(sceneOrder[currentSceneIndex]);
        }
        else
        {
            currentSceneIndex = 0;
            Debug.Log("[GM] ¡Completado! Volviendo al inicio.");
            SceneManager.LoadScene(sceneOrder[0]);
        }
        yield return null;
    }

    // ══════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════

    public int GetCurrentSceneIndex() => currentSceneIndex;
    public int GetCurrentRound() => currentRound;
    public int GetRoundsPerScene() => roundsPerScene;
    public int GetTotalRoundsThisScene() => GetTotalRoundsForThisScene();
    public int GetExtraRoundsFromDeath() => extraRoundsFromDeath;
    public bool IsInBattle() => isInBattle;
    public bool IsInCutscene() => isInCutscene;
    public string GetCurrentSceneName() => (currentSceneIndex >= 0 && currentSceneIndex < sceneOrder.Count) ? sceneOrder[currentSceneIndex] : "Unknown";
}