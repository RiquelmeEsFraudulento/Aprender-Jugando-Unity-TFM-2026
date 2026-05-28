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

    [Header("Rondas por Escena")]
    public int roundsPerScene = 3;

    private int currentSceneIndex = 0;
    private int currentRound = 0;
    private bool isInBattle = false;
    private bool isInCutscene = false;
    private bool hasShownIntro = false;

    private float savedPlayerHealth = -1f;
    private float savedPlayerMaxHealth = -1f;
    private float savedPlayerXP = -1f;
    private float savedPlayerLevel = -1f;

    // ══════════════════════════════════════════════════════════
    // SINGLETON
    // ══════════════════════════════════════════════════════════

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        AutoFindReferences();
    }

    void AutoFindReferences()
    {
        if (player == null)
        {
            SimpleWalk sw = FindAnyObjectByType<SimpleWalk>();
            if (sw != null) player = sw.gameObject;
        }
        if (enemySpawner == null)
            enemySpawner = FindAnyObjectByType<EnemySpawner>();
        if (spawnPoint == null && player != null)
            spawnPoint = player.transform;
    }

    // ══════════════════════════════════════════════════════════
    // SCENE LOAD
    // ══════════════════════════════════════════════════════════

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(AfterSceneLoaded());
    }

    IEnumerator AfterSceneLoaded()
    {
        Debug.Log("[GameManager] === ESCENA CARGADA ===");
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

        // ── CHECK IF TESTING SCENE (has TestCube) ────────────
        if (IsTestingScene())
        {
            Debug.Log("[GameManager] Escena de testing detectada.");
            yield return StartCoroutine(HandleTestingScene());
        }
        else if (currentSceneIndex > 0 && currentSceneIndex < sceneOrder.Count - 1)
        {
            // ── BATTLE SCENE ──────────────────────────────────
            // 1) SPAWN ENEMIES FIRST
            Debug.Log("[GameManager] === PRIMER SPAWN DE ENEMIGOS ===");
            yield return StartCoroutine(SpawnFirstEnemies());

            // 2) PLAYER APPEARS
            Debug.Log("[GameManager] Jugador aparece en la escena.");
            SetPlayerActive(true);

            // 3) PLAY INTRO
            if (!hasShownIntro)
            {
                hasShownIntro = true;
                yield return StartCoroutine(ShowIntroCutscene());
            }

            // 4) START BATTLE
            yield return StartCoroutine(StartBattleRound());
        }
    }

    // ══════════════════════════════════════════════════════════
    // TESTING SCENE — Check for TestCube
    // ══════════════════════════════════════════════════════════

    bool IsTestingScene()
    {
        GameObject testCube = GameObject.FindGameObjectWithTag("TestCube");
        if (testCube != null)
        {
            Debug.Log("[GameManager] TestCube encontrado → escena de testing.");
            return true;
        }
        return false;
    }

    IEnumerator HandleTestingScene()
    {
        Debug.Log("[GameManager] === MODO TESTING ===");

        // Make sure player is active
        SetPlayerActive(true);

        // Wait until TestCube is destroyed
        Debug.Log("[GameManager] Esperando que TestCube sea destruido...");

        while (true)
        {
            GameObject testCube = GameObject.FindGameObjectWithTag("TestCube");
            if (testCube == null)
            {
                Debug.Log("[GameManager] TestCube destruido → avanzando a siguiente escena.");
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }

        // Advance to next scene
        yield return StartCoroutine(AdvanceToNextScene());
    }

    // ══════════════════════════════════════════════════════════
    // FIRST SPAWN — Spawn enemies BEFORE player appears
    // ══════════════════════════════════════════════════════════

    IEnumerator SpawnFirstEnemies()
    {
        Debug.Log("[GameManager] === SPAWN PRIMERA OLEADA ===");

        // Keep player disabled during spawn
        SetPlayerActive(false);

        // Find all EnemySpawners with tag "Respawn"
        GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        Debug.Log($"[GameManager] Encontrados {respawnPoints.Length} spawn points.");

        if (respawnPoints.Length == 0)
        {
            Debug.LogError("[GameManager] NO HAY SPAWN POINTS CON TAG 'Respawn'!");
            SetPlayerActive(true);
            yield break;
        }

        // Spawn from each point
        foreach (GameObject rp in respawnPoints)
        {
            EnemySpawner spawner = rp.GetComponent<EnemySpawner>();
            if (spawner != null)
            {
                Debug.Log($"[GameManager] Spawneando desde '{rp.name}'...");
                EnemyManager spawned = spawner.Spawn();
                if (spawned != null)
                {
                    Debug.Log($"[GameManager] Spawned: '{spawned.name}' con {spawned.allEnemies.Count} hijos.");
                }
                else
                {
                    Debug.LogError($"[GameManager] Spawner '{rp.name}' devolvió NULL!");
                }
            }
            else
            {
                Debug.LogWarning($"[GameManager] '{rp.name}' no tiene EnemySpawner!");
            }
        }

        // Wait for Start() to run on new prefabs
        Debug.Log("[GameManager] Esperando inicialización de Managers...");
        yield return new WaitForSeconds(0.5f);

        // Find all spawned EnemyManagers
        EnemyManager[] allManagers = FindObjectsByType<EnemyManager>();
        Debug.Log($"[GameManager] {allManagers.Length} EnemyManager(s) en escena.");

        if (allManagers.Length > 0)
        {
            // Count total enemies
            int totalEnemies = 0;
            foreach (EnemyManager mgr in allManagers)
            {
                totalEnemies += mgr.allEnemies.Count;
            }
            Debug.Log($"[GameManager] Total enemigos: {totalEnemies}");

            StartAIOnAllManagers();
        }
        else
        {
            Debug.Log("[GameManager] CRITICAL: No se encontró EnemyManager después del spawn!");
        }

        Debug.Log("[GameManager] === PRIMERA OLEADA COMPLETADA ===");
    }

    // ══════════════════════════════════════════════════════════
    // INTRO / OUTRO
    // ══════════════════════════════════════════════════════════

    IEnumerator ShowIntroCutscene()
    {
        isInCutscene = true;
        Time.timeScale = 0f;
        currentRound = 0;
        SetPlayerActive(false);

        StoryTextData.SceneTexts sceneTexts = storyData.GetSceneTexts(currentSceneIndex);
        if (sceneTexts != null && sceneTexts.introTexts.Count > 0)
        {
            yield return StartCoroutine(DisplayTextSequence(sceneTexts.introTexts.ToArray()));
        }

        Time.timeScale = 1f;
        isInCutscene = false;
        SetPlayerActive(true);

        Debug.Log("[GameManager] Intro terminada.");
    }

    IEnumerator ShowOutroCutscene()
    {
        isInCutscene = true;
        Time.timeScale = 0f;
        SetPlayerActive(false);

        StoryTextData.SceneTexts sceneTexts = storyData.GetSceneTexts(currentSceneIndex);
        if (sceneTexts != null && sceneTexts.outroTexts.Count > 0)
        {
            yield return StartCoroutine(DisplayTextSequence(sceneTexts.outroTexts.ToArray()));
        }

        Time.timeScale = 1f;
        isInCutscene = false;
        SetPlayerActive(true);

        Debug.Log("[GameManager] Outro terminada.");
    }

    // ══════════════════════════════════════════════════════════
    // BATTLE ROUNDS
    // ══════════════════════════════════════════════════════════

    IEnumerator StartBattleRound()
    {
        currentRound++;
        Debug.Log($"[GameManager] ███ RONDA {currentRound}/{roundsPerScene} ███");

        isInBattle = true;

        // Wait for all enemies to die
        yield return StartCoroutine(WaitForAllEnemiesDead());

        Debug.Log($"[GameManager] ███ RONDA {currentRound} COMPLETADA ███");
        isInBattle = false;

        // Check if more rounds needed
        if (currentRound < roundsPerScene)
        {
            Debug.Log($"[GameManager] Preparando ronda {currentRound + 1}...");

            // ── DESTROY OLD MANAGERS ──────────────────────────
            yield return StartCoroutine(DestroyAllManagers());

            // ── SPAWN NEW WAVE ────────────────────────────────
            yield return StartCoroutine(SpawnNextWave());

            // ── START NEXT ROUND ──────────────────────────────
            yield return StartCoroutine(StartBattleRound());
        }
        else
        {
            // All rounds complete — show outro
            Debug.Log("[GameManager] Todas las rondas completadas!");
            yield return StartCoroutine(ShowOutroCutscene());
            yield return StartCoroutine(AdvanceToNextScene());
        }
    }

    // ══════════════════════════════════════════════════════════
    // WAIT FOR ALL ENEMIES DEAD
    // ══════════════════════════════════════════════════════════

    IEnumerator WaitForAllEnemiesDead()
    {
        Debug.Log("[GameManager] Esperando que mueran todos los enemigos...");

        while (true)
        {
            if (player != null)
            {
                EnemyManager[] managers = FindObjectsByType<EnemyManager>();

                if (managers.Length == 0)
                {
                    Debug.Log("[GameManager] No hay managers → todos muertos.");
                    yield break;
                }

                int totalAlive = 0;
                foreach (EnemyManager mgr in managers)
                {
                    totalAlive += mgr.AliveEnemyCount();
                }

                if (totalAlive <= 0)
                {
                    Debug.Log("[GameManager] ¡Todos los enemigos muertos!");
                    yield break;
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    // ══════════════════════════════════════════════════════════
    // DESTROY ALL MANAGERS
    // ══════════════════════════════════════════════════════════

    IEnumerator DestroyAllManagers()
    {
        Debug.Log("[GameManager] === DESTRUYENDO MANAGERS ANTIGUOS ===");

        EnemyManager[] managers = FindObjectsByType<EnemyManager>();
        Debug.Log($"[GameManager] Destruyendo {managers.Length} manager(s).");

        foreach (EnemyManager mgr in managers)
        {
            if (mgr != null)
            {
                Debug.Log($"[GameManager] Destruyendo: '{mgr.name}'");
                Destroy(mgr.gameObject);
            }
        }

        // Wait for destruction to process
        yield return null;
        yield return null;

        // Clear spawner references
        GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        foreach (GameObject rp in respawnPoints)
        {
            EnemySpawner spawner = rp.GetComponent<EnemySpawner>();
            if (spawner != null)
            {
                spawner.ClearCurrentManager();
            }
        }

        enemySpawner = FindAnyObjectByType<EnemySpawner>();

        Debug.Log("[GameManager] Managers destruidos.");
    }

    // ══════════════════════════════════════════════════════════
    // SPAWN NEXT WAVE
    // ══════════════════════════════════════════════════════════

    IEnumerator SpawnNextWave()
    {
        Debug.Log("[GameManager] === SPAWNANDO NUEVA OLEADA ===");

        yield return new WaitForSeconds(0.5f);

        // Find all Respawn points
        GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        Debug.Log($"[GameManager] {respawnPoints.Length} spawn points encontrados.");

        foreach (GameObject rp in respawnPoints)
        {
            EnemySpawner spawner = rp.GetComponent<EnemySpawner>();
            if (spawner != null)
            {
                Debug.Log($"[GameManager] Spawneando desde '{rp.name}'...");
                EnemyManager newMgr = spawner.Spawn();
                if (newMgr != null)
                {
                    Debug.Log($"[GameManager] Spawned: '{newMgr.name}' con {newMgr.allEnemies.Count} enemigos.");
                }
                else
                {
                    Debug.LogError($"[GameManager] Spawner '{rp.name}' devolvió NULL!");
                }
            }
        }

        // Wait for initialization
        yield return new WaitForSeconds(0.5f);

        // Start AI on new managers
        EnemyManager[] newManagers = FindObjectsByType<EnemyManager>();
        Debug.Log($"[GameManager] {newManagers.Length} manager(s) nuevos en escena.");

        if (newManagers.Length > 0)
        {
            StartAIOnAllManagers();
            Debug.Log($"[GameManager] Nueva oleada lista.");
        }
        else
        {
            Debug.LogError("[GameManager] CRITICAL: No managers después del spawn!");
        }

        Debug.Log("[GameManager] === NUEVA OLEADA COMPLETADA ===");
    }

    // ══════════════════════════════════════════════════════════
    // PLAYER DEATH
    // ══════════════════════════════════════════════════════════

    public void OnPlayerDied()
    {
        StartCoroutine(PlayerDeathSequence());
    }

    IEnumerator PlayerDeathSequence()
    {
        Debug.Log("[GameManager] === JUGADOR MURIÓ ===");

        isInBattle = false;
        isInCutscene = true;
        Time.timeScale = 0f;
        SetPlayerActive(false);

        string deathText = storyData != null ? storyData.deathText :
            "No me rendiré...\n\n[ENTER para continuar]";

        yield return StartCoroutine(DisplaySingleText(deathText));

        Time.timeScale = 1f;
        isInCutscene = false;

        // Destroy old managers
        yield return StartCoroutine(DestroyAllManagers());

        // Respawn enemies
        yield return StartCoroutine(SpawnNextWave());

        // Respawn player
        RespawnPlayer();
        SetPlayerActive(true);

        // Restart current round
        yield return StartCoroutine(StartBattleRound());
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
            if (savedPlayerHealth > 0)
                ph.vida = savedPlayerHealth;
            ph.ResetDeath();
        }

        Debug.Log("[GameManager] Jugador respawneado.");
    }

    // ══════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════

    void StartAIOnAllManagers()
    {
        EnemyManager[] managers = FindObjectsByType<EnemyManager>();
        foreach (EnemyManager mgr in managers)
        {
            if (mgr != null)
            {
                mgr.StartAI();
                Debug.Log($"[GameManager] StartAI en '{mgr.name}'.");
            }
        }
    }

    void SetPlayerActive(bool active)
    {
        if (player == null) return;

        SimpleWalk sw = player.GetComponent<SimpleWalk>();
        if (sw != null) sw.enabled = active;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = active;

        Animator anim = player.GetComponent<Animator>();
        if (anim != null) anim.enabled = active;

        Debug.Log($"[GameManager] Jugador {(active ? "ACTIVADO" : "DESACTIVADO")}.");
    }

    // ══════════════════════════════════════════════════════════
    // SAVE / RESTORE
    // ══════════════════════════════════════════════════════════

    public void SavePlayerStats()
    {
        if (player == null) return;
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null) return;

        savedPlayerHealth = ph.vida;
        savedPlayerMaxHealth = ph.vidaMax;
        savedPlayerXP = ph.XP;
        savedPlayerLevel = ph.level;
    }

    void RestorePlayerStats()
    {
        if (player == null) return;
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null) return;

        if (savedPlayerHealth > 0)
        {
            ph.vida = savedPlayerHealth;
            ph.vidaMax = savedPlayerMaxHealth;
            ph.XP = savedPlayerXP;
            ph.level = savedPlayerLevel;
        }
    }

    // ══════════════════════════════════════════════════════════
    // TEXT DISPLAY
    // ══════════════════════════════════════════════════════════
    IEnumerator DisplayTextSequence(string[] texts)
    {
        Debug.Log($"[GameManager] === TEXT SEQUENCE START: {texts.Length} pages ===");

        // Find or create StoryTextUI
        StoryTextUI_Canvas storyUI = FindAnyObjectByType<StoryTextUI_Canvas>();
        if (storyUI == null)
        {
            Debug.Log("[GameManager] Creating StoryTextUI...");
            GameObject uiGO = new GameObject("StoryTextUI");
            storyUI = uiGO.AddComponent<StoryTextUI_Canvas>();
            DontDestroyOnLoad(uiGO);
        }

        for (int i = 0; i < texts.Length; i++)
        {
            Debug.Log($"[GameManager] --- Page {i + 1}/{texts.Length} ---");

            storyUI.ShowText(texts[i]);

            // Wait for player to press ENTER/SPACE/CLICK
            yield return StartCoroutine(storyUI.WaitForInput());

            Debug.Log($"[GameManager] Page {i + 1} complete, hiding...");

            storyUI.HideText();

            // Small pause between pages for smooth transition
            yield return new WaitForSecondsRealtime(0.3f);
        }

        Debug.Log("[GameManager] === TEXT SEQUENCE COMPLETE ===");
    }

    IEnumerator DisplaySingleText(string text)
    {
        Debug.Log("[GameManager] === SINGLE TEXT START ===");

        StoryTextUI_Canvas storyUI = FindAnyObjectByType<StoryTextUI_Canvas>();
        if (storyUI == null)
        {
            GameObject uiGO = new GameObject("StoryTextUI");
            storyUI = uiGO.AddComponent<StoryTextUI_Canvas>();
            DontDestroyOnLoad(uiGO);
        }

        storyUI.ShowText(text);
        yield return StartCoroutine(storyUI.WaitForInput());
        storyUI.HideText();

        Debug.Log("[GameManager] === SINGLE TEXT COMPLETE ===");
    }

    IEnumerator WaitForEnterOrClick()
    {
        yield return new WaitUntil(() => !Input.GetKeyDown(KeyCode.Return) &&
                                          !Input.GetMouseButtonDown(0));
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) ||
                                          Input.GetKeyDown(KeyCode.Space) ||
                                          Input.GetMouseButtonDown(0));
    }

    TextScreenManager CreateTextScreenManager()
    {
        GameObject go = new GameObject("TextScreenManager");
        TextScreenManager tsm = go.AddComponent<TextScreenManager>();
        tsm.Initialize();
        return tsm;
    }

    // ══════════════════════════════════════════════════════════
    // SCENE ADVANCE
    // ══════════════════════════════════════════════════════════

    IEnumerator AdvanceToNextScene()
    {
        Debug.Log("[GameManager] Avanzando a siguiente escena...");

        SavePlayerStats();
        currentSceneIndex++;
        hasShownIntro = false;
        currentRound = 0;

        if (currentSceneIndex < sceneOrder.Count)
        {
            SceneManager.LoadScene(sceneOrder[currentSceneIndex]);
        }
        else
        {
            currentSceneIndex = 0;
            SceneManager.LoadScene(sceneOrder[currentSceneIndex]);
            Debug.Log("[GameManager] ¡Juego completado!");
        }

        yield return null;
    }

    // ══════════════════════════════════════════════════════════
    // API
    // ══════════════════════════════════════════════════════════

    public int GetCurrentSceneIndex() => currentSceneIndex;
    public int GetCurrentRound() => currentRound;
    public int GetRoundsPerScene() => roundsPerScene;
    public bool IsInBattle() => isInBattle;
    public bool IsInCutscene() => isInCutscene;
}