// ═══════════════════════════════════════════════════════════════════════════
//  PlayerHUDController.cs  —  PREMIUM v3
//
//  Single self-contained class. Handles:
//    • Health / XP / Level bars
//    • State circle (keys 7/8/9)
//    • Item wheel: scroll to select, middle-click to use
//    • Per-item visual states: inactive → selected → active → used
//    • Loot box system on enemy death
//    • Life Steal, Potion, Life Stone, Chaos item logic
//    • ItemInventory data
//
//  SETUP:
//    1. Attach to a GameObject with a UIDocument component.
//    2. Assign references in Inspector (or they auto-resolve):
//       - playerHealth  → PlayerHealth on the player
//       - enemyManager  → EnemyManager in the scene
//    3. Ensure the UXML file is assigned to the UIDocument.
//    4. Ensure the USS file is referenced in the UXML.
// ═══════════════════════════════════════════════════════════════════════════

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUDController : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 1 — INSPECTOR REFERENCES
    // ═══════════════════════════════════════════════════════════════════

    [Header("Core References")]
    [Tooltip("PlayerHealth on the player GameObject. Auto-found if null.")]
    public PlayerHealth playerHealth;

    [Tooltip("EnemyManager in the scene. Auto-found if null.")]
    public EnemyManager enemyManager;

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 2 — ITEM INVENTORY (was ItemInventory.cs)
    // ═══════════════════════════════════════════════════════════════════

    [Header("Item Inventory")]
    [Tooltip("Max stack for Life Steal and Potion.")]
    public const int MAX_STACKABLE = 5;

    public int lifeStealCount = 5;
    public int potionCount = 5;
    public int lifeStoneCount = 1;
    public int chaosCount = 2;

    [HideInInspector] public int savedLifeStealCount;
    [HideInInspector] public int savedPotionCount;
    [HideInInspector] public int savedLifeStoneCount;
    [HideInInspector] public int savedChaosCount;


    /// <summary>
    /// True  = Life Stone is available this scene.
    /// False = already triggered this scene.
    /// Resets to true at scene start.
    /// </summary>
    [HideInInspector] public bool lifeStoneAvailableThisScene = true;

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 3 — LIFE STEAL SYSTEM (was LifeStealSystem.cs)
    // ═══════════════════════════════════════════════════════════════════

    [Header("Life Steal Settings")]
    public float lifeStealDuration = 15f;
    public float lifeStealHealFraction = 2f;

    [HideInInspector] public bool lifeStealIsActive = false;
    [HideInInspector] public float lifeStealRemainingTime = 0f;

    private Coroutine _lifeStealCoroutine;

    /// <summary>
    /// Try to activate Life Steal. Returns true if a charge was consumed.
    /// </summary>
    public bool TryActivateLifeSteal()
    {
        if (lifeStealCount <= 0) return false;
        if (playerHealth == null) return false;

        lifeStealCount--;

        if (_lifeStealCoroutine != null) StopCoroutine(_lifeStealCoroutine);
        _lifeStealCoroutine = StartCoroutine(LifeStealRoutine());
        return true;
    }

    /// <summary>
    /// Call from damage-dealing logic: heals player for a fraction of damage.
    /// </summary>
    

// AFTER:
    public void OnLifeStealDamageDealt(float damage)
    {
        if (!lifeStealIsActive)
        {
            Debug.Log("[LifeSteal] OnDamageDealt called but isActive=false → ignored.");
            return;
        }
        if (playerHealth == null)
        {
            Debug.LogWarning("[LifeSteal] OnDamageDealt called but playerHealth is null!");
            return;
        }
        float heal = damage * lifeStealHealFraction;
        float vidaAntes = playerHealth.vida;
        playerHealth.vida = Mathf.Min(playerHealth.vida + heal, playerHealth.vidaMax);
        float healed = playerHealth.vida - vidaAntes;
        Debug.Log($"[LifeSteal] DamageDealt={damage} | HealMult={lifeStealHealFraction} | Healed={healed:F1} | Vida: {vidaAntes:F1}→{playerHealth.vida:F1}/{playerHealth.vidaMax:F1}");
    }
        private IEnumerator LifeStealRoutine()
    {
        lifeStealIsActive = true;
        lifeStealRemainingTime = lifeStealDuration;

        while (lifeStealRemainingTime > 0f)
        {
            lifeStealRemainingTime -= Time.deltaTime;
            yield return null;
        }

        lifeStealRemainingTime = 0f;
        lifeStealIsActive = false;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 4 — POTION SYSTEM (was PotionSystem.cs)
    // ═══════════════════════════════════════════════════════════════════

    [Header("Potion Settings")]
    public float potionHotDuration = 5f;
    public float potionHealPercent = 0.15f;

    [HideInInspector] public bool potionIsActive = false;
    [HideInInspector] public float potionRemainingTime = 0f;

    private int _activePotionHots = 0;

    public bool TryActivatePotion()
    {
        if (potionCount <= 0) return false;
        if (playerHealth == null) return false;

        potionCount--;
        StartCoroutine(PotionHoTRoutine());
        return true;
    }

    private IEnumerator PotionHoTRoutine()
    {
        _activePotionHots++;
        potionIsActive = true;

        float totalHeal = playerHealth.vidaMax * potionHealPercent;
        float healPerSec = totalHeal / potionHotDuration;
        float elapsed = 0f;

        while (elapsed < potionHotDuration)
        {
            elapsed += Time.deltaTime;
            potionRemainingTime = Mathf.Max(potionHotDuration - elapsed, 0f);
            playerHealth.vida = Mathf.Min(
                playerHealth.vida + healPerSec * Time.deltaTime,
                playerHealth.vidaMax);
            yield return null;
        }

        _activePotionHots--;
        if (_activePotionHots <= 0)
        {
            _activePotionHots = 0;
            potionIsActive = false;
            potionRemainingTime = 0f;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 5 — LIFE STONE SYSTEM (was LifeStoneSystem.cs)
    // ═══════════════════════════════════════════════════════════════════

    [Header("Life Stone Settings")]
    public float lifeStoneTriggerThreshold = 0.25f;
    public float lifeStoneInstantHeal = 0.20f;
    public float lifeStoneHotHeal = 0.10f;
    public float lifeStoneHotDuration = 5f;

    [HideInInspector] public bool lifeStoneIsActive = false;
    [HideInInspector] public float lifeStoneRemainingTime = 0f;

    private Coroutine _lifeStoneCoroutine;

    private void CheckLifeStoneTrigger()
    {
        if (!lifeStoneAvailableThisScene) return;
        if (lifeStoneCount <= 0) return;
        if (lifeStoneIsActive) return;
        if (playerHealth == null) return;

        float hpPct = playerHealth.vida / playerHealth.vidaMax;
        if (hpPct < lifeStoneTriggerThreshold)
        {
            if (_lifeStoneCoroutine != null) StopCoroutine(_lifeStoneCoroutine);
            _lifeStoneCoroutine = StartCoroutine(LifeStoneRoutine());
        }
    }

    private IEnumerator LifeStoneRoutine()
    {
        lifeStoneCount--;
        lifeStoneAvailableThisScene = false;

        // Instant heal
        playerHealth.vida = Mathf.Min(
            playerHealth.vida + playerHealth.vidaMax * lifeStoneInstantHeal,
            playerHealth.vidaMax);

        // HoT
        lifeStoneIsActive = true;
        lifeStoneRemainingTime = lifeStoneHotDuration;

        float totalHeal = playerHealth.vidaMax * lifeStoneHotHeal;
        float healPerSec = totalHeal / lifeStoneHotDuration;
        float elapsed = 0f;

        while (elapsed < lifeStoneHotDuration)
        {
            elapsed += Time.deltaTime;
            lifeStoneRemainingTime = Mathf.Max(lifeStoneHotDuration - elapsed, 0f);
            playerHealth.vida = Mathf.Min(
                playerHealth.vida + healPerSec * Time.deltaTime,
                playerHealth.vidaMax);
            yield return null;
        }

        lifeStoneIsActive = false;
        lifeStoneRemainingTime = 0f;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 6 — CHAOS SYSTEM (was ChaosSystem.cs)
    // ═══════════════════════════════════════════════════════════════════

    [Header("Chaos Settings")]
    public float chaosDotDuration = 3f;
    public float chaosDotPercentPerS = 0.03f;
    public float chaosPenaltyInstant = 0.10f;

    [HideInInspector] public bool chaosIsActive = false;
    [HideInInspector] public float chaosRemainingTime = 0f;

    private Coroutine _chaosDotCoroutine;

    public bool TryActivateChaos()
    {
        if (chaosCount <= 0) return false;
        if (playerHealth == null) return false;

        // ── REBUSCAR el manager activo en cada uso ──
        EnemyManager activeManager = GetActiveEnemyManager();
        if (activeManager == null)
        {
            Debug.LogWarning("[Chaos] No hay EnemyManager activo en la escena.");
            return false;
        }

        chaosCount--;
        
        // Aplicar usando el manager ACTIVO, no la referencia cacheada
        ApplyChaosToManager(activeManager);
        return true;
    }
    private void ApplyChaos()
    {
        // Wrapper que busca el manager actual automáticamente
        EnemyManager activeManager = GetActiveEnemyManager();
        if (activeManager == null)
        {
            Debug.LogWarning("[Chaos] No hay EnemyManager activo.");
            return;
        }
        ApplyChaosToManager(activeManager);
    }

    

    /// <summary>
    /// Devuelve true si el enemigo existe, está activo, y tiene vida > 0.
    /// Se llama antes de CADA operación que pueda fallar si el enemigo murió.
    /// </summary>
    private bool IsEnemyAlive(EnemyScript enemy)
    {
        if (enemy == null) return false;
        if (!enemy.isActiveAndEnabled) return false;
        if (!enemy.IsAttackable()) return false;
        return true;
    }

    private void ApplyChaosToManager(EnemyManager mgr)
    {
        bool anyAtOne = false;

        foreach (EnemyManager.EnemyStruct entry in mgr.allEnemies)
        {
            EnemyScript e = entry.enemyScript;

            // ── Antes de calcular ──
            if (!IsEnemyAlive(e)) continue;

            int hp = e.currentHealth;
            if (hp % 2 != 0) hp += 1;

            int newHp = hp / 2;
            newHp = Mathf.Max(newHp, 1);
            int damageToDeal = e.currentHealth - newHp;

            if (damageToDeal > 0)
            {
                // ── ANTES de golpear (el DoT pudo haberlo matado en este mismo frame) ──
                if (!IsEnemyAlive(e)) continue;

                e.TakeDamage(damageToDeal, DamageType.Normal, gameObject);
            }

            // ── DESPUÉS de golpear: ¿existe y tiene 1 HP? ──
            if (!IsEnemyAlive(e)) continue;

            if (e.currentHealth == 1)
                anyAtOne = true;
        }

        if (_chaosDotCoroutine != null) StopCoroutine(_chaosDotCoroutine);

        if (anyAtOne)
        {
            playerHealth.vida = Mathf.Max(
                playerHealth.vida - playerHealth.vidaMax * chaosPenaltyInstant, 1f);
        }
        else
        {
            _chaosDotCoroutine = StartCoroutine(ChaosDotRoutine());
        }
    }

    private IEnumerator ChaosDotRoutine()
    {
        chaosIsActive = true;
        chaosRemainingTime = chaosDotDuration;

        float elapsed = 0f;
        while (elapsed < chaosDotDuration)
        {
            elapsed += Time.deltaTime;
            chaosRemainingTime = Mathf.Max(chaosDotDuration - elapsed, 0f);

            float dmg = playerHealth.vidaMax * chaosDotPercentPerS * Time.deltaTime;
            playerHealth.vida = Mathf.Max(playerHealth.vida - dmg, 1f);

            yield return null;
        }

        chaosIsActive = false;
        chaosRemainingTime = 0f;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 7 — USS CLASS CONSTANTS
    // ═══════════════════════════════════════════════════════════════════

    private const string STATE_NONE = "state-none";
    private const string STATE_SLEEP = "state-sleep";
    private const string STATE_CONFUSED = "state-confused";

    private const string SLOT_ACTIVE = "slot-active";
    private const string SLOT_INACTIVE = "slot-inactive";
    private const string SLOT_SELECTED = "slot-selected";
    private const string SLOT_USED = "slot-used";

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 8 — VISUAL ELEMENT CACHE
    // ═══════════════════════════════════════════════════════════════════

    private VisualElement _healthFill;
    private VisualElement _xpFill;
    private Label _levelLabel;
    private VisualElement _stateCircle;
    private VisualElement _lootPopup;
    private Label _lootLabel;

    private VisualElement[] _slots;
    private Label[] _labels;

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 9 — ITEM WHEEL STATE
    // ═══════════════════════════════════════════════════════════════════

    private int _selectedIndex = 0;
    private const int SLOT_COUNT = 4;

    private readonly string[] _slotVENames = {
        "slot-lifesteal", "slot-potion", "slot-lifestone", "slot-chaos"
    };
    private readonly string[] _labelVENames = {
        "label-lifesteal", "label-potion", "label-lifestone", "label-chaos"
    };

    private float _scrollAccum = 0f;
    private const float SCROLL_THRESHOLD = 0.1f;

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 10 — LOOT TABLE
    // ═══════════════════════════════════════════════════════════════════

    private readonly int[] _lootItems = { -1, 0, 1, 2, 3 };
    private readonly float[] _lootWeightT1 = { 60f, 25f, 12f, 2f, 1f };
    private readonly float[] _lootWeightT5 = { 20f, 20f, 20f, 20f, 20f };

    private Coroutine _lootCoroutine;

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 11 — UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════

    void Awake()
    {
        AutoResolveReferences();
    }

    void OnEnable()
    {
        CacheVisualElements();
    }

    void Update()
    {
        if (playerHealth == null) return;

        if (enemyManager == null || enemyManager.gameObject == null || !enemyManager.isActiveAndEnabled)
        {
            enemyManager = FindAnyObjectByType<EnemyManager>();
        }

        UpdateBars();
        UpdateStateCircle();
        UpdateScrollWheel();
        UpdateItemSlots();
        CheckLifeStoneTrigger();
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 12 — AUTO-RESOLVE REFERENCES
    // ═══════════════════════════════════════════════════════════════════

    private void AutoResolveReferences()
    {
        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (enemyManager == null)
            enemyManager = FindAnyObjectByType<EnemyManager>();
    }


    /// <summary>
    /// Devuelve el EnemyManager ACTIVO en la escena.
    /// Lo rebusca cada vez porque los managers se destruyen y
    /// se recrean dinámicamente entre rondas.
    /// Mantiene la referencia cacheada si sigue viva.
    /// </summary>
    private EnemyManager GetActiveEnemyManager()
    {
        // Si la referencia cacheada sigue viva, úsala
        if (enemyManager != null && enemyManager.gameObject != null 
            && enemyManager.isActiveAndEnabled)
            return enemyManager;

        // Si no, rebuscar
        enemyManager = FindAnyObjectByType<EnemyManager>();
        return enemyManager;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 13 — VISUAL ELEMENT CACHING
    // ═══════════════════════════════════════════════════════════════════


    private void CacheVisualElements()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _healthFill  = root.Q<VisualElement>("health-fill");
        _xpFill      = root.Q<VisualElement>("xp-fill");
        _levelLabel  = root.Q<Label>("level-label");
        _stateCircle = root.Q<VisualElement>("state-circle");
        _lootPopup   = root.Q<VisualElement>("loot-popup");
        _lootLabel   = root.Q<Label>("loot-label");

        _slots  = new VisualElement[SLOT_COUNT];
        _labels = new Label[SLOT_COUNT];
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            _slots[i]  = root.Q<VisualElement>(_slotVENames[i]);
            _labels[i] = root.Q<Label>(_labelVENames[i]);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 14 — BARS
    // ═══════════════════════════════════════════════════════════════════

    private void UpdateBars()
    {
        if (_healthFill == null || _xpFill == null || _levelLabel == null) return;

        float hp = Mathf.Clamp01(playerHealth.vida / playerHealth.vidaMax);
        _healthFill.style.width = new StyleLength(Length.Percent(hp * 100f));

        float xpNeeded = Mathf.Max(playerHealth.level * 10f, 1f);
        float xp = Mathf.Clamp01(playerHealth.XP / xpNeeded);
        _xpFill.style.width = new StyleLength(Length.Percent(xp * 100f));

        _levelLabel.text = ((int)playerHealth.level).ToString();
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 15 — STATE CIRCLE
    // ═══════════════════════════════════════════════════════════════════

    private void UpdateStateCircle()
    {
        if (_stateCircle == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha7)) SetState(STATE_SLEEP);
        else if (Input.GetKeyDown(KeyCode.Alpha8)) SetState(STATE_CONFUSED);
        else if (Input.GetKeyDown(KeyCode.Alpha9)) SetState(STATE_NONE);
    }

    private void SetState(string newClass)
    {
        if (_stateCircle == null) return;

        _stateCircle.RemoveFromClassList(STATE_NONE);
        _stateCircle.RemoveFromClassList(STATE_SLEEP);
        _stateCircle.RemoveFromClassList(STATE_CONFUSED);
        _stateCircle.AddToClassList(newClass);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 16 — SCROLL WHEEL SELECTION & ACTIVATION
    // ═══════════════════════════════════════════════════════════════════

    private void UpdateScrollWheel()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > SCROLL_THRESHOLD)
        {
            _scrollAccum += scroll;

            if (_scrollAccum > SCROLL_THRESHOLD)
            {
                _selectedIndex = (_selectedIndex - 1 + SLOT_COUNT) % SLOT_COUNT;
                _scrollAccum = 0f;
            }
            else if (_scrollAccum < -SCROLL_THRESHOLD)
            {
                _selectedIndex = (_selectedIndex + 1) % SLOT_COUNT;
                _scrollAccum = 0f;
            }
        }

        if (Input.GetMouseButtonDown(2))
        {
            TryUseSelectedItem();
        }
    }

    private void TryUseSelectedItem()
    {
        switch (_selectedIndex)
        {
            case 0: TryActivateLifeSteal(); break;
            case 1: TryActivatePotion(); break;
            case 2: /* Life Stone activates automatically */ break;
            case 3: TryActivateChaos(); break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 17 — ITEM SLOT VISUALS
    // ═══════════════════════════════════════════════════════════════════

    private void UpdateItemSlots()
    {
        if (_slots == null || _labels == null) return;

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (_slots[i] == null || _labels[i] == null) continue;

            bool isSelected = (i == _selectedIndex);
            bool isActive = GetSlotActive(i);
            float remaining = GetSlotRemaining(i);
            int stock = GetSlotStock(i);

            // Clear all state classes
            _slots[i].RemoveFromClassList(SLOT_ACTIVE);
            _slots[i].RemoveFromClassList(SLOT_INACTIVE);
            _slots[i].RemoveFromClassList(SLOT_SELECTED);
            _slots[i].RemoveFromClassList(SLOT_USED);

            // ── Life Stone special case ──
            if (i == 2)
            {
                bool usedThisScene = !lifeStoneAvailableThisScene;
                if (isActive)
                {
                    _slots[i].AddToClassList(SLOT_ACTIVE);
                    if (isSelected) _slots[i].AddToClassList(SLOT_SELECTED);
                    _labels[i].text = Mathf.CeilToInt(remaining) + "s";
                }
                else if (usedThisScene)
                {
                    _slots[i].AddToClassList(SLOT_USED);
                    _labels[i].text = stock.ToString();
                }
                else
                {
                    _slots[i].AddToClassList(isSelected ? SLOT_SELECTED : SLOT_INACTIVE);
                    _labels[i].text = stock.ToString();
                }
                continue;
            }

            // ── Standard slots ──
            if (isActive)
            {
                _slots[i].AddToClassList(SLOT_ACTIVE);
                if (isSelected) _slots[i].AddToClassList(SLOT_SELECTED);
                _labels[i].text = Mathf.CeilToInt(remaining) + "s";
            }
            else
            {
                _slots[i].AddToClassList(isSelected ? SLOT_SELECTED : SLOT_INACTIVE);
                _labels[i].text = stock.ToString();
            }
        }
    }

    private bool GetSlotActive(int i)
    {
        return i switch
        {
            0 => lifeStealIsActive,
            1 => potionIsActive,
            2 => lifeStoneIsActive,
            3 => chaosIsActive,
            _ => false
        };
    }

    private float GetSlotRemaining(int i)
    {
        return i switch
        {
            0 => lifeStealRemainingTime,
            1 => potionRemainingTime,
            2 => lifeStoneRemainingTime,
            3 => chaosRemainingTime,
            _ => 0f
        };
    }

    private int GetSlotStock(int i)
    {
        return i switch
        {
            0 => lifeStealCount,
            1 => potionCount,
            2 => lifeStoneCount,
            3 => chaosCount,
            _ => 0
        };
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 18 — LOOT BOX SYSTEM
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Call from enemy death:  FindObjectOfType<PlayerHUDController>()?.GrantLoot();
    /// </summary>
    public void GrantLoot()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("[HUD] GrantLoot called but playerHealth is null.");
            return;
        }

        int itemIndex = RollLoot();
        if (itemIndex < 0)
        {
            ShowLootPopup("No drop");
            return;
        }

        string itemName = ApplyLoot(itemIndex);
        ShowLootPopup("+ " + itemName);
    }

    private int RollLoot()
    {
        float level = Mathf.Clamp(playerHealth.level, 1f, 5f);
        float t = (level - 1f) / 4f;

        float totalWeight = 0f;
        float[] weights = new float[_lootItems.Length];
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = Mathf.Lerp(_lootWeightT1[i], _lootWeightT5[i], t);
            totalWeight += weights[i];
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative)
                return _lootItems[i];
        }
        return -1;
    }

    private string ApplyLoot(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0:
                if (lifeStealCount < MAX_STACKABLE)
                    lifeStealCount++;
                return "Life Steal";

            case 1:
                if (potionCount < MAX_STACKABLE)
                    potionCount++;
                return "Potion";

            case 2:
                lifeStoneCount++;
                if (lifeStoneCount > 0 && !lifeStoneAvailableThisScene)
                    lifeStoneAvailableThisScene = true;
                return "Life Stone";

            case 3:
                chaosCount++;
                return "Chaos";

            default:
                return "Nothing";
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SECTION 19 — LOOT POPUP
    // ═══════════════════════════════════════════════════════════════════

    private void ShowLootPopup(string text)
    {
        if (_lootCoroutine != null) StopCoroutine(_lootCoroutine);
        _lootCoroutine = StartCoroutine(LootPopupRoutine(text));
    }

    private IEnumerator LootPopupRoutine(string text)
    {
        _lootLabel.text = text;
        _lootPopup.RemoveFromClassList("loot-hidden");
        _lootPopup.AddToClassList("loot-visible");

        yield return new WaitForSeconds(2.2f);

        _lootPopup.RemoveFromClassList("loot-visible");
        _lootPopup.AddToClassList("loot-hidden");
    }


    public void SavePlayerHUDStats()
{
    if (playerHealth == null) return;

    savedLifeStealCount = lifeStealCount;
    savedPotionCount = potionCount;
    savedLifeStoneCount = lifeStoneCount;
    savedChaosCount = chaosCount;

    Debug.Log($"[HUD] Stats guardados: LS={lifeStealCount}, P={potionCount}, St={lifeStoneCount}, Ch={chaosCount}");
}

public void RestorePlayerHUDStats()
{
    lifeStealCount = savedLifeStealCount;
    potionCount = savedPotionCount;
    lifeStoneCount = savedLifeStoneCount;
    chaosCount = savedChaosCount;
    lifeStoneAvailableThisScene = true;

    Debug.Log($"[HUD] Stats restaurados: LS={lifeStealCount}, P={potionCount}, St={lifeStoneCount}, Ch={chaosCount}");
}

public void AddItemsOnSceneChange()
{
    lifeStealCount = Mathf.Min(lifeStealCount + 2, MAX_STACKABLE);
    potionCount = Mathf.Min(potionCount + 2, MAX_STACKABLE);
    lifeStoneCount++;
    chaosCount++;
    lifeStoneAvailableThisScene = true;

    Debug.Log($"[HUD] Items escena: LS={lifeStealCount}, P={potionCount}, St={lifeStoneCount}, Ch={chaosCount}");
}
}