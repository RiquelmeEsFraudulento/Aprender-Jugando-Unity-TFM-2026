// ═══════════════════════════════════════════════════════════════════════════
//  PlayerHUDController.cs — VERSIÓN ENCAPSULADA COMPLETA
//  CON EJERCICIOS DIDÁCTICOS PARA BACHILLERATO TIC 1
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
//  ORGANIZACIÓN EN TRES BLOQUES:
//    BLOQUE 1 — Inicialización de variables + constantes + estructuras
//    BLOQUE 2 — Ejercicios en estilo C++ (el alumno trabaja aquí)
//    BLOQUE 3 — Funciones de trabajo sucio Unity/C# (alumno NO toca)
//
//  TODA la funcionalidad original se mantiene íntegra.
//  Los Debug originales se respetan tal cual.
//  Nuevos debugs se marcan con prefijo [EJ] para filtrar fácil.
// ═══════════════════════════════════════════════════════════════════════════

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUDController : MonoBehaviour
{
    // =====================================================================
    // BLOQUE 1 — INICIALIZACIÓN DE VARIABLES Y ESTRUCTURAS
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │  1A — CONSTANTES DEL ALUMNO (puede modificarlas)        │
    // └─────────────────────────────────────────────────────────┘

    public const int ITEM_LIFE_STEAL = 0;
    public const int ITEM_POTION     = 1;
    public const int ITEM_LIFE_STONE = 2;
    public const int ITEM_CHAOS      = 3;

    public const int SLOT_COUNT = 4;

    public const int ESTADO_NINGUNO  = 0;
    public const int ESTADO_SLEEP    = 1;
    public const int ESTADO_CONFUSO  = 2;

    public const int LOOT_NO_DROP     = -1;
    public const int LOOT_LIFE_STEAL  = 0;
    public const int LOOT_POTION      = 1;
    public const int LOOT_LIFE_STONE  = 2;
    public const int LOOT_CHAOS       = 3;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1B — ESTRUCTURA DE DATOS DEL INVENTARIO (ficha)        │
    // └─────────────────────────────────────────────────────────┘
    //
    // Piensa en esta estructura como la mochila del jugador.
    // Cada item tiene un nombre, una cantidad y un estado.

    [System.Serializable]
    public struct FichaInventario
    {
        // ── Datos de cada item ─────────────────────────────────
        public int lifeStealCount;       // Cantidad de Life Steal (0-5)
        public int potionCount;          // Cantidad de Pociones (0-5)
        public int lifeStoneCount;       // Cantidad de Life Stones (0-1)
        public int chaosCount;           // Cantidad de Chaos (0-2)

        // ── Estados activos ───────────────────────────────────
        public bool lifeStealActivo;     // true = efecto activo
        public bool potionActivo;        // true = efecto activo
        public bool lifeStoneActivo;     // true = efecto activo
        public bool chaosActivo;         // true = efecto activo

        // ── Tiempos restantes ─────────────────────────────────
        public float lifeStealTiempoRestante;  // Segundos que quedan
        public float potionTiempoRestante;
        public float lifeStoneTiempoRestante;
        public float chaosTiempoRestante;

        // ── Life Stone especial ───────────────────────────────
        public bool lifeStoneDisponibleEscena;  // true = no se usó esta escena

        // ── Estado del jugador ────────────────────────────────
        public float vidaJugador;         // Vida actual
        public float vidaMaximaJugador;   // Vida máxima
        public float xpJugador;           // XP actual
        public float nivelJugador;        // Nivel actual

        // ── Estado del círculo (7/8/9) ───────────────────────
        public int estadoCirculo;         // 0=ninguno, 1=sleep, 2=confused

        // ── Selección de item ─────────────────────────────────
        public int itemSeleccionado;      // Índice del item seleccionado (0-3)
    }

    // ── Instancia visible para el alumno ──────────────────────
    public FichaInventario ficha;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1C — VARIABLES DE DEBUG PARA EJERCICIOS [EJ]           │
    // └─────────────────────────────────────────────────────────┘

    [Header("═══ DEBUG EJERCICIOS [EJ] ═══")]
    public int debugEJ_itemSeleccionado;
    public bool debugEJ_lifeStealActivo;
    public bool debugEJ_potionActivo;
    public bool debugEJ_lifeStoneActivo;
    public bool debugEJ_chaosActivo;
    public float debugEJ_lifeStealTiempo;
    public float debugEJ_potionTiempo;
    public float debugEJ_lifeStoneTiempo;
    public float debugEJ_chaosTiempo;
    public int debugEJ_estadoCirculo;
    public int debugEJ_resultadoLoot;
    public string debugEJ_nombreItemLoot;
    public int debugEJ_itemAplicado;
    public float debugEJ_vidaAntesChaos;
    public float debugEJ_vidaDespuesChaos;
    public bool debugEJ_chaosPenalizacion;
    public int debugEJ_lifeStoneCountAntes;
    public int debugEJ_lifeStoneCountDespues;
    public float debugEJ_vidaAntesLifeStone;
    public float debugEJ_vidaDespuesLifeStone;
    public bool debugEJ_lifeStoneDisponible;
    public int debugEJ_itemsAnyadidos;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1D — REFERENCIAS (se mantienen originales)             │
    // └─────────────────────────────────────────────────────────┘

    [Header("Core References")]
    public PlayerHealth playerHealth;
    public EnemyManager enemyManager;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1E — INVENTARIO (se mantiene original)                  │
    // └─────────────────────────────────────────────────────────┘

    [Header("Item Inventory")]
    public const int MAX_STACKABLE = 5;

    public int lifeStealCount = 5;
    public int potionCount = 5;
    public int lifeStoneCount = 1;
    public int chaosCount = 2;

    //[HideInInspector] public int savedLifeStealCount;
    //[HideInInspector] public int savedPotionCount;
    //[HideInInspector] public int savedLifeStoneCount;
    //[HideInInspector] public int savedChaosCount;

    [HideInInspector] public bool lifeStoneAvailableThisScene = true;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1F — LIFE STEAL SETTINGS (se mantiene original)         │
    // └─────────────────────────────────────────────────────────┘

    [Header("Life Steal Settings")]
    public float lifeStealDuration = 15f;
    public float lifeStealHealFraction = 2f;

    [HideInInspector] public bool lifeStealIsActive = false;
    [HideInInspector] public float lifeStealRemainingTime = 0f;

    private Coroutine _lifeStealCoroutine;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1G — POTION SETTINGS (se mantiene original)             │
    // └─────────────────────────────────────────────────────────┘

    [Header("Potion Settings")]
    public float potionHotDuration = 5f;
    public float potionHealPercent = 0.15f;

    [HideInInspector] public bool potionIsActive = false;
    public float potionRemainingTime = 0f;

    private int _activePotionHots = 0;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1H — LIFE STONE SETTINGS (se mantiene original)         │
    // └─────────────────────────────────────────────────────────┘

    [Header("Life Stone Settings")]
    public float lifeStoneTriggerThreshold = 0.25f;
    public float lifeStoneInstantHeal = 0.20f;
    public float lifeStoneHotHeal = 0.10f;
    public float lifeStoneHotDuration = 5f;

    [HideInInspector] public bool lifeStoneIsActive = false;
    [HideInInspector] public float lifeStoneRemainingTime = 0f;

    private Coroutine _lifeStoneCoroutine;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1I — CHAOS SETTINGS (se mantiene original)              │
    // └─────────────────────────────────────────────────────────┘

    [Header("Chaos Settings")]
    public float chaosDotDuration = 3f;
    public float chaosDotPercentPerS = 0.03f;
    public float chaosPenaltyInstant = 0.10f;

    [HideInInspector] public bool chaosIsActive = false;
    [HideInInspector] public float chaosRemainingTime = 0f;

    private Coroutine _chaosDotCoroutine;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1J — USS CLASS CONSTANTS (se mantienen originales)      │
    // └─────────────────────────────────────────────────────────┘

    private const string STATE_NONE = "state-none";
    private const string STATE_SLEEP = "state-sleep";
    private const string STATE_CONFUSED = "state-confused";

    private const string SLOT_ACTIVE = "slot-active";
    private const string SLOT_INACTIVE = "slot-inactive";
    private const string SLOT_SELECTED = "slot-selected";
    private const string SLOT_USED = "slot-used";

    // ┌─────────────────────────────────────────────────────────┐
    // │  1K — VISUAL ELEMENT CACHE (se mantiene original)        │
    // └─────────────────────────────────────────────────────────┘

    private VisualElement _healthFill;
    private VisualElement _xpFill;
    private Label _levelLabel;
    private VisualElement _stateCircle;
    private VisualElement _lootPopup;
    private Label _lootLabel;

    private VisualElement[] _slots;
    private Label[] _labels;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1L — ITEM WHEEL STATE (se mantiene original)            │
    // └─────────────────────────────────────────────────────────┘

    private int _selectedIndex = 0;

    private readonly string[] _slotVENames = {
        "slot-lifesteal", "slot-potion", "slot-lifestone", "slot-chaos"
    };
    private readonly string[] _labelVENames = {
        "label-lifesteal", "label-potion", "label-lifestone", "label-chaos"
    };

    private float _scrollAccum = 0f;
    private const float SCROLL_THRESHOLD = 0.1f;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1M — LOOT TABLE (se mantiene original)                  │
    // └─────────────────────────────────────────────────────────┘

    private readonly int[] _lootItems = { -1, 0, 1, 2, 3 };
    private readonly float[] _lootWeightT1 = { 60f, 25f, 12f, 2f, 1f };
    private readonly float[] _lootWeightT5 = { 20f, 20f, 20f, 20f, 20f };

    private Coroutine _lootCoroutine;

    // ══════════════════════════════════════════════════════════
    // AWAKE / ONENABLE / UPDATE (se mantienen originales)
    // ══════════════════════════════════════════════════════════

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
        SincronizarFicha();
        ActualizarDebugEjercicios();
    }

    // ══════════════════════════════════════════════════════════
    // AUTO-RESOLVE REFERENCES (se mantiene original)
    // ══════════════════════════════════════════════════════════

    private void AutoResolveReferences()
    {
        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (enemyManager == null)
            enemyManager = FindAnyObjectByType<EnemyManager>();
    }

    private EnemyManager GetActiveEnemyManager()
    {
        if (enemyManager != null && enemyManager.gameObject != null
            && enemyManager.isActiveAndEnabled)
            return enemyManager;

        enemyManager = FindAnyObjectByType<EnemyManager>();
        return enemyManager;
    }

    // ══════════════════════════════════════════════════════════
    // VISUAL ELEMENT CACHING (se mantiene original)
    // ══════════════════════════════════════════════════════════

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

    // ══════════════════════════════════════════════════════════
    // BARS (se mantiene original)
    // ══════════════════════════════════════════════════════════

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

    // ══════════════════════════════════════════════════════════
    // SCROLL WHEEL (se mantiene original)
    // ══════════════════════════════════════════════════════════

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

    // ══════════════════════════════════════════════════════════
    // ITEM SLOT VISUALS (se mantiene original)
    // ══════════════════════════════════════════════════════════

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

            _slots[i].RemoveFromClassList(SLOT_ACTIVE);
            _slots[i].RemoveFromClassList(SLOT_INACTIVE);
            _slots[i].RemoveFromClassList(SLOT_SELECTED);
            _slots[i].RemoveFromClassList(SLOT_USED);

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

    // ══════════════════════════════════════════════════════════
    // LOOT POPUP (se mantiene original)
    // ══════════════════════════════════════════════════════════

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

    // ══════════════════════════════════════════════════════════
    // SAVE / RESTORE (se mantiene original)
    // ══════════════════════════════════════════════════════════
    /*
    public void SavePlayerHUDStats()
    {
        if (playerHealth == null) return;

        savedLifeStealCount = lifeStealCount;
        savedPotionCount = potionCount;
        savedLifeStoneCount = lifeStoneCount;
        savedChaosCount = chaosCount;
    }
    */
    public void RestorePlayerHUDStats(int savedLifeStealCount, int savedPotionCount, int savedLifeStoneCount, int savedChaosCount)
    {
        lifeStealCount = savedLifeStealCount;
        potionCount = savedPotionCount;
        lifeStoneCount = savedLifeStoneCount;
        chaosCount = savedChaosCount;
        lifeStoneAvailableThisScene = true;
    }

    // =====================================================================
    // SINCRONIZAR FICHA Y DEBUG
    // =====================================================================

    /// <summary>
    /// Sincroniza la ficha visible del alumno con el estado real.
    /// </summary>
    void SincronizarFicha()
    {
        ficha.lifeStealCount = lifeStealCount;
        ficha.potionCount = potionCount;
        ficha.lifeStoneCount = lifeStoneCount;
        ficha.chaosCount = chaosCount;

        ficha.lifeStealActivo = lifeStealIsActive;
        ficha.potionActivo = potionIsActive;
        ficha.lifeStoneActivo = lifeStoneIsActive;
        ficha.chaosActivo = chaosIsActive;

        ficha.lifeStealTiempoRestante = lifeStealRemainingTime;
        ficha.potionTiempoRestante = potionRemainingTime;
        ficha.lifeStoneTiempoRestante = lifeStoneRemainingTime;
        ficha.chaosTiempoRestante = chaosRemainingTime;

        ficha.lifeStoneDisponibleEscena = lifeStoneAvailableThisScene;

        if (playerHealth != null)
        {
            ficha.vidaJugador = playerHealth.vida;
            ficha.vidaMaximaJugador = playerHealth.vidaMax;
            ficha.xpJugador = playerHealth.XP;
            ficha.nivelJugador = playerHealth.level;
        }

        ficha.itemSeleccionado = _selectedIndex;
    }

    /// <summary>
    /// Actualiza las variables de debug de ejercicios.
    /// </summary>
    void ActualizarDebugEjercicios()
    {
        debugEJ_itemSeleccionado = _selectedIndex;
        debugEJ_lifeStealActivo = lifeStealIsActive;
        debugEJ_potionActivo = potionIsActive;
        debugEJ_lifeStoneActivo = lifeStoneIsActive;
        debugEJ_chaosActivo = chaosIsActive;
        debugEJ_lifeStealTiempo = lifeStealRemainingTime;
        debugEJ_potionTiempo = potionRemainingTime;
        debugEJ_lifeStoneTiempo = lifeStoneRemainingTime;
        debugEJ_chaosTiempo = chaosRemainingTime;
        debugEJ_lifeStoneDisponible = lifeStoneAvailableThisScene;
    }

    // =====================================================================
    // BLOQUE 2 — EJERCICIOS EN ESTILO C++
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │         HERRAMIENTAS QUE PUEDES USAR LIBREMENTE         │
    // │         (ya están implementadas, no las toques)         │
    // └─────────────────────────────────────────────────────────┘

    /// <summary>
    /// Devuelve el tiempo que pasó desde el último frame.
    /// En Unity esto es Time.deltaTime.
    /// </summary>
    float ObtenerTiempoFrame()
    {
        return Time.deltaTime;
    }

    /// <summary>
    /// Redondea un float hacia arriba al entero más cercano.
    /// En Unity esto es Mathf.CeilToInt.
    /// </summary>
    int RedondearArriba(float valor)
    {
        return Mathf.CeilToInt(valor);
    }

    /// <summary>
    /// Devuelve el porcentaje como decimal (ej: 15% → 0.15).
    /// </summary>
    float PorcentajeADecimal(float porcentaje)
    {
        return porcentaje / 100f;
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
    /// Devuelve el nivel del jugador.
    /// </summary>
    float NivelJugador()
    {
        if (playerHealth == null) return 1f;
        return playerHealth.level;
    }

    /// <summary>
    /// Devuelve TRUE si el Life Steal está activo.
    /// </summary>
    bool LifeStealEstaActivo()
    {
        return lifeStealIsActive;
    }

    /// <summary>
    /// Devuelve TRUE si la Poción está activa.
    /// </summary>
    bool PotionEstaActiva()
    {
        return potionIsActive;
    }

    /// <summary>
    /// Devuelve TRUE si el Life Stone está activo.
    /// </summary>
    bool LifeStoneEstaActivo()
    {
        return lifeStoneIsActive;
    }

    /// <summary>
    /// Devuelve TRUE si el Chaos está activo.
    /// </summary>
    bool ChaosEstaActivo()
    {
        return chaosIsActive;
    }

    /// <summary>
    /// Devuelve TRUE si el Life Stone está disponible esta escena.
    /// </summary>
    bool LifeStoneDisponible()
    {
        return lifeStoneAvailableThisScene;
    }

    /// <summary>
    /// Devuelve la cantidad de un item según su índice.
    /// 0=Life Steal, 1=Potion, 2=Life Stone, 3=Chaos
    /// </summary>
    int CantidadItem(int indiceItem)
    {
        switch (indiceItem)
        {
            case ITEM_LIFE_STEAL: return lifeStealCount;
            case ITEM_POTION:     return potionCount;
            case ITEM_LIFE_STONE: return lifeStoneCount;
            case ITEM_CHAOS:      return chaosCount;
            default:              return 0;
        }
    }

    /// <summary>
    /// Devuelve TRUE si un item está activo según su índice.
    /// </summary>
    bool ItemEstaActivo(int indiceItem)
    {
        switch (indiceItem)
        {
            case ITEM_LIFE_STEAL: return lifeStealIsActive;
            case ITEM_POTION:     return potionIsActive;
            case ITEM_LIFE_STONE: return lifeStoneIsActive;
            case ITEM_CHAOS:      return chaosIsActive;
            default:              return false;
        }
    }

    /// <summary>
    /// Devuelve el tiempo restante de un item según su índice.
    /// </summary>
    float TiempoRestanteItem(int indiceItem)
    {
        switch (indiceItem)
        {
            case ITEM_LIFE_STEAL: return lifeStealRemainingTime;
            case ITEM_POTION:     return potionRemainingTime;
            case ITEM_LIFE_STONE: return lifeStoneRemainingTime;
            case ITEM_CHAOS:      return chaosRemainingTime;
            default:              return 0f;
        }
    }

    /// <summary>
    /// Devuelve el nombre de un item según su índice.
    /// </summary>
    string NombreItem(int indiceItem)
    {
        switch (indiceItem)
        {
            case ITEM_LIFE_STEAL: return "Life Steal";
            case ITEM_POTION:     return "Potion";
            case ITEM_LIFE_STONE: return "Life Stone";
            case ITEM_CHAOS:      return "Chaos";
            default:              return "Unknown";
        }
    }

    /// <summary>
    /// Devuelve el nombre de un item de loot según su código.
    /// -1=No drop, 0=Life Steal, 1=Potion, 2=Life Stone, 3=Chaos
    /// </summary>
    string NombreItemLoot(int codigoLoot)
    {
        switch (codigoLoot)
        {
            case LOOT_LIFE_STEAL: return "Life Steal";
            case LOOT_POTION:     return "Potion";
            case LOOT_LIFE_STONE: return "Life Stone";
            case LOOT_CHAOS:      return "Chaos";
            case LOOT_NO_DROP:    return "No drop";
            default:              return "Nothing";
        }
    }

    /// <summary>
    /// Devuelve el estado del círculo como string.
    /// </summary>
    string EstadoCirculoComoString(int estado)
    {
        switch (estado)
        {
            case ESTADO_SLEEP:   return STATE_SLEEP;
            case ESTADO_CONFUSO: return STATE_CONFUSED;
            default:             return STATE_NONE;
        }
    }

    /// <summary>
    /// Devuelve el peso de loot para un item en el nivel dado.
    /// Interpola entre nivel 1 y nivel 5.
    /// </summary>
    float PesoNivel1(int indiceItem)
    {
        if (indiceItem < 0 || indiceItem >= _lootWeightT1.Length) return 0f;
        return _lootWeightT1[indiceItem];
    }

    /// <summary>
    /// Devuelve el peso de loot para un item en el nivel 5.
    /// </summary>
    float PesoNivel5(int indiceItem)
    {
        if (indiceItem < 0 || indiceItem >= _lootWeightT5.Length) return 0f;
        return _lootWeightT5[indiceItem];
    }

    /// <summary>
    /// Devuelve la duración del Life Steal.
    /// </summary>
    float DuracionLifeSteal()
    {
        return lifeStealDuration;
    }

    /// <summary>
    /// Devuelve el factor de curación del Life Steal.
    /// </summary>
    float FactorCuracionLifeSteal()
    {
        return lifeStealHealFraction;
    }

    /// <summary>
    /// Devuelve la duración de la Poción.
    /// </summary>
    float DuracionPotion()
    {
        return potionHotDuration;
    }

    /// <summary>
    /// Devuelve el porcentaje de curación de la Poción.
    /// </summary>
    float PorcentajeCuracionPotion()
    {
        return potionHealPercent;
    }

    /// <summary>
    /// Devuelve el umbral de vida del Life Stone (como decimal).
    /// </summary>
    float UmbralLifeStone()
    {
        return lifeStoneTriggerThreshold;
    }

    /// <summary>
    /// Devuelve el porcentaje de curación instantánea del Life Stone.
    /// </summary>
    float PorcentajeCuracionInstantLifeStone()
    {
        return lifeStoneInstantHeal;
    }

    /// <summary>
    /// Devuelve el porcentaje de curación HoT del Life Stone.
    /// </summary>
    float PorcentajeCuracionHoTLifeStone()
    {
        return lifeStoneHotHeal;
    }

    /// <summary>
    /// Devuelve la duración del Life Stone HoT.
    /// </summary>
    float DuracionLifeStoneHoT()
    {
        return lifeStoneHotDuration;
    }

    /// <summary>
    /// Devuelve la duración del Chaos DoT.
    /// </summary>
    float DuracionChaosDoT()
    {
        return chaosDotDuration;
    }

    /// <summary>
    /// Devuelve el porcentaje de daño por segundo del Chaos.
    /// </summary>
    float PorcentajeDanyoChaosPorSegundo()
    {
        return chaosDotPercentPerS;
    }

    /// <summary>
    /// Devuelve el porcentaje de penalización instantánea del Chaos.
    /// </summary>
    float PorcentajePenalizacionChaos()
    {
        return chaosPenaltyInstant;
    }

    /// <summary>
    /// Devuelve el número máximo de items apilables.
    /// </summary>
    int MaxApilable()
    {
        return MAX_STACKABLE;
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 1 — PotionHoTRoutine: Calcular curación por frame
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: La Poción cura un porcentaje de la vida máxima durante
    //  varios segundos. Cada frame, calcula cuánto curar este frame.
    //
    //  EJEMPLO REAL:
    //  - Vida máxima: 100
    //  - Porcentaje de curación: 15% (0.15)
    //  - Duración: 5 segundos
    //  - Curación total: 100 * 0.15 = 15
    //  - Curación por segundo: 15 / 5 = 3
    //  - Curación este frame: 3 * dt
    //
    //  OBJETIVO: Calcula cuánto curar este frame.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: VidaMaximaJugador(), PorcentajeCuracionPotion(), DuracionPotion(), ObtenerTiempoFrame()
    //  ❌ NO uses: playerHealth directamente, Time.deltaTime directamente
    //
    // ══════════════════════════════════════════════════════════════════════
    //
    //  PASOS A SEGUIR:
    //
    //  PASO 1 — Calcular la curación total
    //  curacionTotal = vidaMaxima * porcentajeCuracion
    //
    //  PASO 2 — Calcular curación por segundo
    //  curacionPorSegundo = curacionTotal / duracion
    //
    //  PASO 3 — Calcular curación este frame
    //  curacionEsteFrame = curacionPorSegundo * tiempoFrame
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>Cuánto curar este frame.</returns>
    float CalcularCuracionPotionEsteFrame()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: calculamos la curación total
        float vidaMax = VidaMaximaJugador();
        float porcentaje = PorcentajeCuracionPotion();  // 0.15
        float curacionTotal = vidaMax * porcentaje;

        // PASO 2: calculamos curación por segundo
        float duracion = DuracionPotion();  // 5
        float curacionPorSegundo = curacionTotal / duracion;

        // PASO 3: calculamos curación este frame
        float dt = ObtenerTiempoFrame();
        float curacionEsteFrame = curacionPorSegundo * dt;

        return curacionEsteFrame;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 2 — OnLifeStealDamageDealt: Calcular curación por daño
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cuando el Life Steal está activo, cada vez que el
    //  jugador hace daño a un enemigo, se cura un múltiplo de ese daño.
    //
    //  EJEMPLO REAL:
    //  - Life Steal activo: true
    //  - Factor de curación: 2.0
    //  - Daño hecho al enemigo: 5
    //  - Curación: 5 * 2.0 = 10
    //
    //  OBJETIVO: Calcula cuánto curar según el daño hecho.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: LifeStealEstaActivo(), FactorCuracionLifeSteal()
    //  ❌ NO uses: lifeStealIsActive directamente
    //
    // ══════════════════════════════════════════════════════════════════════
    //
    //  PASOS A SEGUIR:
    //
    //  PASO 1 — Comprobar si Life Steal está activo
    //  Si no está activo, devuelve 0 (no se cura nada).
    //
    //  PASO 2 — Calcular la curación
    //  curacion = danyo * factorCuracion
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="danyo">Daño hecho al enemigo.</param>
    /// <returns>Cuánto curar al jugador.</returns>
    float CalcularCuracionLifeSteal(float danyo)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: ¿está activo el Life Steal?
        if (!LifeStealEstaActivo())
            return 0f;

        // PASO 2: calculamos la curación
        float factor = FactorCuracionLifeSteal();  // 2.0
        float curacion = danyo * factor;

        return curacion;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 3 — ApplyChaosToManager: Calcular nuevo HP tras Chaos
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El item Chaos reduce a la mitad la vida de todos los
    //  enemigos. Si la vida es impar, primero la redondea arriba al
    //  par más cercano. El resultado mínimo es 1 (nunca mata directamente).
    //
    //  EJEMPLO REAL:
    //  - Vida actual: 7 → redondea a 8 → mitad = 4
    //  - Vida actual: 6 → ya es par → mitad = 3
    //  - Vida actual: 1 → redondea a 2 → mitad = 1 (mínimo)
    //
    //  OBJETIVO: Calcula la nueva vida de un enemigo tras aplicar Chaos.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: if/else, operadores módulo (%), funciones básicas
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════
    //
    //  PASOS A SEGUIR:
    //
    //  PASO 1 — Comprobar si la vida es impar
    //  Usa el operador % (módulo). Si vida % 2 != 0, es impar.
    //
    //  PASO 2 — Si es impar, redondear arriba al par
    //  vida = vida + 1
    //
    //  PASO 3 — Dividir entre 2
    //  nuevaVida = vida / 2
    //
    //  PASO 4 — Asegurar mínimo de 1
    //  Si nuevaVida < 1, ponla a 1.
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="vidaActual">Vida actual del enemigo.</param>
    /// <returns>Nueva vida tras aplicar Chaos.</returns>
    int CalcularVidaTrasChaos(int vidaActual)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: comprobamos si es impar
        int vida = vidaActual;

        if (vida % 2 != 0)
        {
            // PASO 2: si es impar, redondear arriba al par
            vida = vida + 1;
        }

        // PASO 3: dividimos entre 2
        int nuevaVida = vida / 2;

        // PASO 4: aseguramos mínimo de 1
        if (nuevaVida < 1)
            nuevaVida = 1;

        return nuevaVida;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 4 — ChaosDotRoutine: Calcular daño DoT este frame
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Después de usar Chaos, el jugador recibe daño continuo
    //  (DoT = Damage over Time) durante varios segundos. Cada frame
    //  pierde un porcentaje de su vida máxima.
    //
    //  EJEMPLO REAL:
    //  - Vida máxima: 100
    //  - Porcentaje por segundo: 3% (0.03)
    //  - Duración: 3 segundos
    //  - Daño por segundo: 100 * 0.03 = 3
    //  - Daño este frame: 3 * dt
    //
    //  OBJETIVO: Calcula cuánto daño recibir este frame por el DoT.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: VidaMaximaJugador(), PorcentajeDanyoChaosPorSegundo(), ObtenerTiempoFrame()
    //  ❌ NO uses: nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════
    //
    //  PASOS A SEGUIR:
    //
    //  PASO 1 — Calcular daño por segundo
    //  danyoPorSegundo = vidaMaxima * porcentajePorSegundo
    //
    //  PASO 2 — Calcular daño este frame
    //  danyoEsteFrame = danyoPorSegundo * tiempoFrame
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>Daño que recibe el jugador este frame por Chaos DoT.</returns>
    float CalcularDanyoChaosEsteFrame()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: calculamos daño por segundo
        float vidaMax = VidaMaximaJugador();
        float porcentaje = PorcentajeDanyoChaosPorSegundo();  // 0.03
        float danyoPorSegundo = vidaMax * porcentaje;

        // PASO 2: calculamos daño este frame
        float dt = ObtenerTiempoFrame();
        float danyoEsteFrame = danyoPorSegundo * dt;

        return danyoEsteFrame;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 5 — LifeStoneRoutine: Calcular curación total
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El Life Stone tiene dos fases de curación:
    //   1. Curación instantánea: un porcentaje de la vida máxima
    //   2. Curación HoT (Heal over Time): otro porcentaje durante varios segundos
    //
    //  EJEMPLO REAL:
    //  - Vida máxima: 100
    //  - Curación instantánea: 20% → 20 puntos inmediatos
    //  - Curación HoT: 10% → 10 puntos durante 5 segundos
    //
    //  OBJETIVO: Calcula la curación instantánea y la curación por segundo del HoT.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: VidaMaximaJugador(), PorcentajeCuracionInstantLifeStone(),
    //          PorcentajeCuracionHoTLifeStone(), DuracionLifeStoneHoT()
    //  ❌ NO uses: nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Resultado de la curación del Life Stone.
    /// </summary>
    public struct ResultadoLifeStone
    {
        public float curacionInstantanea;    // Puntos de vida inmediatos
        public float curacionHoTTotal;       // Puntos totales del HoT
        public float curacionHoTPorSegundo;  // Puntos por segundo del HoT
    }

    /// <returns>Resultado con curación instantánea y HoT.</returns>
    ResultadoLifeStone CalcularCuracionLifeStone()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: calculamos la curación instantánea
        float vidaMax = VidaMaximaJugador();
        float porcentajeInstant = PorcentajeCuracionInstantLifeStone();  // 0.20
        float curacionInstantanea = vidaMax * porcentajeInstant;

        // PASO 2: calculamos la curación HoT total
        float porcentajeHoT = PorcentajeCuracionHoTLifeStone();  // 0.10
        float curacionHoTTotal = vidaMax * porcentajeHoT;

        // PASO 3: calculamos la curación HoT por segundo
        float duracionHoT = DuracionLifeStoneHoT();  // 5
        float curacionHoTPorSegundo = curacionHoTTotal / duracionHoT;

        // PASO 4: devolvemos el resultado
        ResultadoLifeStone resultado;
        resultado.curacionInstantanea = curacionInstantanea;
        resultado.curacionHoTTotal = curacionHoTTotal;
        resultado.curacionHoTPorSegundo = curacionHoTPorSegundo;
        return resultado;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 6 — CheckLifeStoneTrigger: ¿Se debe activar Life Stone?
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El Life Stone se activa automáticamente cuando la vida
    //  del jugador baja de un porcentaje umbral de su vida máxima.
    //  Solo se activa una vez por escena.
    //
    //  EJEMPLO REAL:
    //  - Umbral: 25% (0.25)
    //  - Vida máxima: 100
    //  - Vida actual: 20 → 20/100 = 0.20 < 0.25 → ¡SE ACTIVA!
    //  - Vida actual: 30 → 30/100 = 0.30 >= 0.25 → no se activa
    //
    //  OBJETIVO: Comprobar si el Life Stone debe activarse.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: VidaJugador(), VidaMaximaJugador(), UmbralLifeStone(), LifeStoneDisponible()
    //  ❌ NO uses: nada de Unity directamente
    //
    // ══════════════════════════════════════════════════════════════════════
    //
    //  PASOS A SEGUIR:
    //
    //  PASO 1 — Comprobar si está disponible esta escena
    //  Si no está disponible, devuelve false.
    //
    //  PASO 2 — Calcular el porcentaje de vida actual
    //  porcentajeVida = vidaActual / vidaMaxima
    //
    //  PASO 3 — Comparar con el umbral
    //  Si porcentajeVida < umbral, devuelve true.
    //  Si no, devuelve false.
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>true si el Life Stone debe activarse.</returns>
    bool DebeActivarseLifeStone()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: ¿está disponible esta escena?
        if (!LifeStoneDisponible())
            return false;

        // PASO 2: calculamos el porcentaje de vida actual
        float vida = VidaJugador();
        float vidaMax = VidaMaximaJugador();
        float porcentajeVida = vida / vidaMax;

        // PASO 3: comparamos con el umbral
        float umbral = UmbralLifeStone();  // 0.25

        if (porcentajeVida < umbral)
            return true;   // ¡se activa!
        else
            return false;  // no se activa

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 7 — UpdateStateCircle: ¿Qué estado mostrar? (switch/case)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador pulsa 7, 8 o 9 para elegir un estado especial.
    //  El círculo de la UI debe mostrar el estado correspondiente.
    //
    //  EJEMPLO REAL:
    //  - Pulsa 7 → muestra "sleep"
    //  - Pulsa 8 → muestra "confused"
    //  - Pulsa 9 → muestra "none" (círculo vacío)
    //
    //  OBJETIVO: Recibe una tecla (7, 8, 9) y devuelve el estado como string.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: switch/case
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="tecla">Tecla pulsada (7, 8, 9).</param>
    /// <returns>Nombre del estado: "sleep", "confused" o "none"</returns>
    string EstadoSegunTecla(int tecla)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        switch (tecla)
        {
            case 7:
                return "sleep";

            case 8:
                return "confused";

            case 9:
                return "none";

            default:
                return "none";
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 8 — GetSlotActive: ¿Está activo un slot? (switch/case)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cada slot del inventario (Life Steal, Potion, Life Stone, Chaos)
    //  puede estar activo o inactivo. Este ejercicio comprueba si un slot
    //  específico está activo.
    //
    //  OBJETIVO: Recibe el índice del slot (0-3) y devuelve true si está activo.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: ItemEstaActivo(), switch/case
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="indiceSlot">Índice del slot (0=Life Steal, 1=Potion, 2=Life Stone, 3=Chaos).</param>
    /// <returns>true si el slot está activo.</returns>
    bool SlotEstaActivo(int indiceSlot)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        switch (indiceSlot)
        {
            case ITEM_LIFE_STEAL:
                return ItemEstaActivo(ITEM_LIFE_STEAL);

            case ITEM_POTION:
                return ItemEstaActivo(ITEM_POTION);

            case ITEM_LIFE_STONE:
                return ItemEstaActivo(ITEM_LIFE_STONE);

            case ITEM_CHAOS:
                return ItemEstaActivo(ITEM_CHAOS);

            default:
                return false;
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 9 — AddItemsOnSceneChange: ¿Cuántos items añadir?
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Al cambiar de escena, el jugador recibe items gratis.
    //  Pero no puede superar el máximo apilable (5). Este ejercicio
    //  calcula cuántos items se añaden realmente.
    //
    //  EJEMPLO REAL:
    //  - Life Steal actual: 4, se añaden 2, máximo: 5
    //  - Resultado: 4 + 2 = 6, pero el máximo es 5 → se añade 1
    //
    //  OBJETIVO: Calcula cuántos items se añaden realmente sin superar el máximo.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: CantidadItem(), MaxApilable(), if/else
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="indiceItem">Índice del item (0-3).</param>
    /// <param name="cantidadAAnyadir">Cantidad que se intenta añadir.</param>
    /// <returns>Cantidad real que se puede añadir sin superar el máximo.</returns>
    int CalcularItemsAAnyadir(int indiceItem, int cantidadAAnyadir)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: obtenemos la cantidad actual
        int cantidadActual = CantidadItem(indiceItem);

        // PASO 2: calculamos cuánto sería el total
        int total = cantidadActual + cantidadAAnyadir;

        // PASO 3: obtenemos el máximo
        int maximo = MaxApilable();  // 5

        // PASO 4: si supera el máximo, calculamos cuánto se puede añadir
        if (total > maximo)
        {
            int sePuedeAnyadir = maximo - cantidadActual;
            return sePuedeAnyadir;
        }

        // PASO 5: si no supera, se añade todo
        return cantidadAAnyadir;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 10 — ApplyLoot: ¿Qué item se aplica? (switch/case)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cuando un enemigo muere, puede soltar un item.
    //  Este ejercicio decide qué item se aplica según el código de loot.
    //
    //  EJEMPLO REAL:
    //  - Código 0 → Life Steal (si no está lleno)
    //  - Código 1 → Potion (si no está lleno)
    //  - Código 2 → Life Stone (siempre)
    //  - Código 3 → Chaos (siempre)
    //
    //  OBJETIVO: Recibe un código de loot y devuelve el índice del item
    //  que se debe aplicar. Si no se puede aplicar (inventario lleno),
    //  devuelve -1.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: CantidadItem(), MaxApilable(), switch/case, if/else
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="codigoLoot">Código del loot (-1, 0, 1, 2, 3).</param>
    /// <returns>Índice del item a aplicar, o -1 si no se puede.</returns>
    int AplicarLoot(int codigoLoot)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        int maximo = MaxApilable();  // 5

        switch (codigoLoot)
        {
            case LOOT_LIFE_STEAL:  // 0
                // Solo se añade si no está lleno
                if (CantidadItem(ITEM_LIFE_STEAL) < maximo)
                    return ITEM_LIFE_STEAL;
                else
                    return -1;  // lleno, no se puede

            case LOOT_POTION:  // 1
                if (CantidadItem(ITEM_POTION) < maximo)
                    return ITEM_POTION;
                else
                    return -1;

            case LOOT_LIFE_STONE:  // 2
                // Life Stone siempre se añade (no tiene límite de apilamiento)
                return ITEM_LIFE_STONE;

            case LOOT_CHAOS:  // 3
                // Chaos siempre se añade
                return ITEM_CHAOS;

            case LOOT_NO_DROP:  // -1
            default:
                return -1;  // no se aplica nada
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 11 — RollLoot: ¿Qué item toca? (for + acumulador + if)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cuando un enemigo muere, se hace un "sorteo" para ver
    //  qué item suelta. Cada item tiene un peso (probabilidad).
    //  El nivel del jugador afecta los pesos.
    //
    //  EJEMPLO REAL (nivel 1):
    //  - No drop: 60% de probabilidad
    //  - Life Steal: 25%
    //  - Potion: 12%
    //  - Life Stone: 2%
    //  - Chaos: 1%
    //
    //  A nivel 5, todos tienen 20% (equilibrado).
    //
    //  OBJETIVO: Simula el sorteo de loot. Recibe un número aleatorio
    //  entre 0 y 100, y devuelve el código del item que tocó.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: PesoNivel1(), PesoNivel5(), NivelJugador(), for, if
    //  ❌ No uses: Random.Range, nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════
    //
    //  PASOS A SEGUIR:
    //
    //  PASO 1 — Calcular el factor de interpolación
    //  t = (nivel - 1) / 4  (nivel 1 → t=0, nivel 5 → t=1)
    //
    //  PASO 2 — Para cada item, calcular su peso interpolado
    //  peso = pesoNivel1 + (pesoNivel5 - pesoNivel1) * t
    //
    //  PASO 3 — Calcular el peso total (suma de todos)
    //
    //  PASO 4 — Recorrer los items acumulando peso
    //  Si el número aleatorio <= peso acumulado, ese item ganó.
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="numeroAleatorio">Número aleatorio entre 0 y 100.</param>
    /// <returns>Código del item que tocó (-1, 0, 1, 2, 3).</returns>
    int RollLoot(float numeroAleatorio)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: calculamos el factor de interpolación
        float nivel = NivelJugador();
        float t = (nivel - 1f) / 4f;
        if (t < 0f) t = 0f;
        if (t > 1f) t = 1f;

        // PASO 2: calculamos los pesos interpolados para cada item
        // Items: 0=No drop, 1=Life Steal, 2=Potion, 3=Life Stone, 4=Chaos
        float[] pesos = new float[5];
        int totalItems = 5;  // _lootItems tiene 5 elementos: -1, 0, 1, 2, 3

        for (int i = 0; i < totalItems; i++)
        {
            float peso1 = PesoNivel1(i);
            float peso5 = PesoNivel5(i);
            pesos[i] = peso1 + (peso5 - peso1) * t;
        }

        // PASO 3: calculamos el peso total
        float pesoTotal = 0f;
        for (int i = 0; i < totalItems; i++)
        {
            pesoTotal = pesoTotal + pesos[i];
        }

        // PASO 4: recorremos acumulando peso para ver qué item tocó
        float pesoAcumulado = 0f;
        for (int i = 0; i < totalItems; i++)
        {
            pesoAcumulado = pesoAcumulado + pesos[i];

            if (numeroAleatorio <= pesoAcumulado)
            {
                // ¡Este item ganó!
                // Convertir índice de array a código de loot
                // Índice 0 → LOOT_NO_DROP (-1)
                // Índice 1 → LOOT_LIFE_STEAL (0)
                // Índice 2 → LOOT_POTION (1)
                // Índice 3 → LOOT_LIFE_STONE (2)
                // Índice 4 → LOOT_CHAOS (3)
                if (i == 0) return LOOT_NO_DROP;
                return i - 1;  // 1→0, 2→1, 3→2, 4→3
            }
        }

        // Si llegamos aquí, devolvemos el último item (por seguridad)
        return LOOT_CHAOS;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 12 — ¿Cuántos enemigos tienen 1 HP tras Chaos? (for + if)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Después de aplicar Chaos, algunos enemigos pueden quedar
    //  con exactamente 1 HP. El juego necesita saber cuántos hay así
    //  para aplicar una penalización al jugador.
    //
    //  OBJETIVO: Recibe un array de vidas actuales de enemigos y devuelve
    //  cuántos tienen exactamente 1 HP después de aplicar Chaos.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: CalcularVidaTrasChaos(), for, if
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="vidasEnemigos">Array con las vidas actuales de los enemigos.</param>
    /// <returns>Cuántos enemigos quedan con exactamente 1 HP tras Chaos.</returns>
    int ContarEnemigosConUnHPTrasChaos(int[] vidasEnemigos)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: contador empieza en 0
        int contador = 0;

        // PASO 2: recorremos todos los enemigos
        for (int i = 0; i < vidasEnemigos.Length; i++)
        {
            // PASO 3: calculamos la nueva vida tras Chaos
            int nuevaVida = CalcularVidaTrasChaos(vidasEnemigos[i]);

            // PASO 4: ¿quedó con exactamente 1 HP?
            if (nuevaVida == 1)
            {
                contador = contador + 1;
            }
        }

        // PASO 5: devolvemos el contador
        return contador;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 13 — ¿Cuántos segundos quedan de un efecto? (if + operador ternario)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: La UI necesita mostrar cuántos segundos quedan de un
    //  efecto activo. Si el efecto no está activo, muestra 0.
    //
    //  OBJETIVO: Recibe el índice de un item y devuelve cuántos segundos
    //  enteros quedan (redondeados arriba). Si no está activo, devuelve 0.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: ItemEstaActivo(), TiempoRestanteItem(), RedondearArriba()
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="indiceItem">Índice del item (0-3).</param>
    /// <returns>Segundos enteros que quedan del efecto (0 si no está activo).</returns>
    int SegundosRestantesItem(int indiceItem)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: ¿está activo?
        if (!ItemEstaActivo(indiceItem))
            return 0;

        // PASO 2: obtenemos el tiempo restante
        float tiempo = TiempoRestanteItem(indiceItem);

        // PASO 3: redondeamos arriba a entero
        int segundos = RedondearArriba(tiempo);

        return segundos;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 14 — ¿Cuántos items en total tiene el jugador? (for + acumulador)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El juego necesita saber cuántos items en total tiene
    //  el jugador para mostrar estadísticas o logros.
    //
    //  OBJETIVO: Suma la cantidad de todos los items del inventario.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: CantidadItem(), for, acumulador
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <returns>Número total de items en el inventario.</returns>
    int TotalItemsEnInventario()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: acumulador
        int total = 0;

        // PASO 2: recorremos todos los slots
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            total = total + CantidadItem(i);
        }

        // PASO 3: devolvemos el total
        return total;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 15 — ¿Qué clase de slot mostrar en UI? (switch/case)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: La UI necesita saber qué clase CSS aplicar a cada slot:
    //   - "active" = el efecto está activo
    //   - "selected" = el jugador lo tiene seleccionado (pero no activo)
    //   - "inactive" = no seleccionado y no activo
    //   - "used" = Life Stone ya usado esta escena
    //
    //  OBJETIVO: Recibe el índice del slot y su estado, y devuelve
    //  la clase CSS correspondiente.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: switch/case, if/else
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Clase CSS para un slot de la UI.
    /// </summary>
    public struct ClaseSlot
    {
        public string clase;  // "active", "selected", "inactive", "used"
    }

    /// <param name="indiceSlot">Índice del slot (0-3).</param>
    /// <param name="estaActivo">true si el efecto está activo.</param>
    /// <param name="estaSeleccionado">true si el jugador lo tiene seleccionado.</param>
    /// <param name="esLifeStoneUsado">true si es Life Stone y ya se usó esta escena.</param>
    /// <returns>Clase CSS correspondiente.</returns>
    string ClaseCSSParaSlot(int indiceSlot, bool estaActivo, bool estaSeleccionado, bool esLifeStoneUsado)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: si está activo, siempre es "active"
        if (estaActivo)
            return "active";

        // PASO 2: si es Life Stone y ya se usó, es "used"
        if (esLifeStoneUsado)
            return "used";

        // PASO 3: si está seleccionado (pero no activo), es "selected"
        if (estaSeleccionado)
            return "selected";

        // PASO 4: por defecto, "inactive"
        return "inactive";

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // =====================================================================
    // BLOQUE 3 — TRABAJO SUCIO UNITY/C# (alumno NO toca)
    // =====================================================================
    //
    // Las funciones de los ejercicios de arriba LLAMAN a estas internamente
    // a través de las funciones auxiliares del Bloque 2.
    // Estas funciones SÍ usan las funciones del alumno.
    // ════════════════════════════════════════════════════════════


    // ══════════════════════════════════════════════════════════
    // LIFE STEAL SYSTEM (llama al ejercicio 2)
    // ══════════════════════════════════════════════════════════

    public bool TryActivateLifeSteal()
    {
        if (lifeStealCount <= 0) return false;
        if (playerHealth == null) return false;

        lifeStealCount--;

        if (_lifeStealCoroutine != null) StopCoroutine(_lifeStealCoroutine);
        _lifeStealCoroutine = StartCoroutine(LifeStealRoutine());
        return true;
    }

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

        // ── USAMOS LA FUNCIÓN DEL EJERCICIO 2 ──
        float heal = CalcularCuracionLifeSteal(damage);

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

    // ══════════════════════════════════════════════════════════
    // POTION SYSTEM (llama al ejercicio 1)
    // ══════════════════════════════════════════════════════════

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

            // ── USAMOS LA FUNCIÓN DEL EJERCICIO 1 ──
            float curacionFrame = CalcularCuracionPotionEsteFrame();
            playerHealth.vida = Mathf.Min(playerHealth.vida + curacionFrame, playerHealth.vidaMax);

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

    // ══════════════════════════════════════════════════════════
    // LIFE STONE SYSTEM (llama a los ejercicios 5 y 6)
    // ══════════════════════════════════════════════════════════

    private void CheckLifeStoneTrigger()
    {
        if (!lifeStoneAvailableThisScene) return;
        if (lifeStoneCount <= 0) return;
        if (lifeStoneIsActive) return;
        if (playerHealth == null) return;

        // ── USAMOS LA FUNCIÓN DEL EJERCICIO 6 ──
        if (DebeActivarseLifeStone())
        {
            if (_lifeStoneCoroutine != null) StopCoroutine(_lifeStoneCoroutine);
            _lifeStoneCoroutine = StartCoroutine(LifeStoneRoutine());
        }
    }

    private IEnumerator LifeStoneRoutine()
    {
        lifeStoneCount--;
        lifeStoneAvailableThisScene = false;

        // ── USAMOS LA FUNCIÓN DEL EJERCICIO 5 ──
        ResultadoLifeStone curacion = CalcularCuracionLifeStone();

        // Aplicar curación instantánea
        float vidaAntesInstant = playerHealth.vida;
        playerHealth.vida = Mathf.Min(playerHealth.vida + curacion.curacionInstantanea, playerHealth.vidaMax);
        Debug.Log($"[LifeStone] Instant heal: +{curacion.curacionInstantanea:F1} | Vida: {vidaAntesInstant:F1}→{playerHealth.vida:F1}");

        // Aplicar HoT
        lifeStoneIsActive = true;
        lifeStoneRemainingTime = lifeStoneHotDuration;

        float elapsed = 0f;
        while (elapsed < lifeStoneHotDuration)
        {
            elapsed += Time.deltaTime;
            lifeStoneRemainingTime = Mathf.Max(lifeStoneHotDuration - elapsed, 0f);

            // Curación HoT este frame
            float curacionHoTFrame = curacion.curacionHoTPorSegundo * Time.deltaTime;
            playerHealth.vida = Mathf.Min(playerHealth.vida + curacionHoTFrame, playerHealth.vidaMax);

            yield return null;
        }

        lifeStoneIsActive = false;
        lifeStoneRemainingTime = 0f;
    }

    // ══════════════════════════════════════════════════════════
    // CHAOS SYSTEM (llama a los ejercicios 3, 4 y 12)
    // ══════════════════════════════════════════════════════════

    public bool TryActivateChaos()
    {
        if (chaosCount <= 0) return false;
        if (playerHealth == null) return false;

        EnemyManager activeManager = GetActiveEnemyManager();
        if (activeManager == null)
        {
            Debug.LogWarning("[Chaos] No hay EnemyManager activo en la escena.");
            return false;
        }

        chaosCount--;
        ApplyChaosToManager(activeManager);
        return true;
    }

    private void ApplyChaosToManager(EnemyManager mgr)
    {
        bool anyAtOne = false;

        // ── Recolectamos las vidas para el ejercicio 12 ──
        int totalEnemigos = 0;
        foreach (EnemyManager.EnemyStruct entry in mgr.allEnemies)
        {
            if (entry.enemyScript != null && entry.enemyScript.IsAttackable())
                totalEnemigos++;
        }

        int[] vidasEnemigos = new int[totalEnemigos];
        int idx = 0;

        foreach (EnemyManager.EnemyStruct entry in mgr.allEnemies)
        {
            EnemyScript e = entry.enemyScript;

            if (!EstaEnemyAlive(e)) continue;

            int hp = e.currentHealth;

            // ── USAMOS LA FUNCIÓN DEL EJERCICIO 3 ──
            int nuevaVida = CalcularVidaTrasChaos(hp);
            int damageToDeal = e.currentHealth - nuevaVida;

            // Guardamos la vida original para el ejercicio 12
            if (idx < vidasEnemigos.Length)
                vidasEnemigos[idx++] = hp;

            if (damageToDeal > 0)
            {
                if (!EstaEnemyAlive(e)) continue;
                e.TakeDamage(damageToDeal, DamageType.Normal, gameObject);
            }

            if (!EstaEnemyAlive(e)) continue;

            if (e.currentHealth == 1)
                anyAtOne = true;
        }

        // ── USAMOS LA FUNCIÓN DEL EJERCICIO 12 para debug ──
        int enemigosConUno = ContarEnemigosConUnHPTrasChaos(vidasEnemigos);
        Debug.Log($"[Chaos] Enemigos que quedarían con 1 HP (simulación): {enemigosConUno}");

        if (_chaosDotCoroutine != null) StopCoroutine(_chaosDotCoroutine);

        if (anyAtOne)
        {
            playerHealth.vida = Mathf.Max(playerHealth.vida - playerHealth.vidaMax * chaosPenaltyInstant, 1f);
            Debug.Log($"[Chaos] ¡Penalización! -{chaosPenaltyInstant * 100}% vida | Vida: {playerHealth.vida:F1}");
        }
        else
        {
            _chaosDotCoroutine = StartCoroutine(ChaosDotRoutine());
        }
    }

    private bool EstaEnemyAlive(EnemyScript enemy)
    {
        if (enemy == null) return false;
        if (!enemy.isActiveAndEnabled) return false;
        if (!enemy.IsAttackable()) return false;
        return true;
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

            // ── USAMOS LA FUNCIÓN DEL EJERCICIO 4 ──
            float danyoFrame = CalcularDanyoChaosEsteFrame();
            playerHealth.vida = Mathf.Max(playerHealth.vida - danyoFrame, 1f);

            yield return null;
        }

        chaosIsActive = false;
        chaosRemainingTime = 0f;
    }

    // ══════════════════════════════════════════════════════════
    // STATE CIRCLE (llama al ejercicio 7)
    // ══════════════════════════════════════════════════════════

    private void UpdateStateCircle()
    {
        if (_stateCircle == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha7)) SetState(ESTADO_SLEEP);
        else if (Input.GetKeyDown(KeyCode.Alpha8)) SetState(ESTADO_CONFUSO);
        else if (Input.GetKeyDown(KeyCode.Alpha9)) SetState(ESTADO_NINGUNO);
    }

    private void SetState(int estado)
    {
        if (_stateCircle == null) return;

        // ── USAMOS LA FUNCIÓN DEL EJERCICIO 7 ──
        string claseCSS = "state-" + EstadoSegunTecla( estado==ESTADO_SLEEP?7 : (estado==ESTADO_CONFUSO?8:9) );
        _stateCircle.RemoveFromClassList(STATE_NONE);
        _stateCircle.RemoveFromClassList(STATE_SLEEP);
        _stateCircle.RemoveFromClassList(STATE_CONFUSED);
        _stateCircle.AddToClassList(claseCSS);
    }

    // ══════════════════════════════════════════════════════════
    // LOOT BOX SYSTEM (llama a los ejercicios 10 y 11)
    // ══════════════════════════════════════════════════════════

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

    /// <summary>
    /// USAMOS LA FUNCIÓN DEL EJERCICIO 11 (RollLoot).
    /// </summary>
    private int RollLoot()
    {
        float nivel = Mathf.Clamp(playerHealth.level, 1f, 5f);
        float numeroAleatorio = Random.Range(0f, 100f);

        // ── Llamamos a la función del ejercicio 11 ──
        int resultado = RollLoot(numeroAleatorio);

        // Actualizamos debug
        debugEJ_resultadoLoot = resultado;
        debugEJ_nombreItemLoot = NombreItemLoot(resultado);

        return resultado;
    }

    /// <summary>
    /// USAMOS LA FUNCIÓN DEL EJERCICIO 10 (AplicarLoot).
    /// </summary>
    private string ApplyLoot(int itemIndex)
    {
        // ── Llamamos a la función del ejercicio 10 ──
        int itemAplicado = AplicarLoot(itemIndex);

        // Actualizamos debug
        debugEJ_itemAplicado = itemAplicado;

        if (itemAplicado < 0)
            return "Nothing";

        switch (itemAplicado)
        {
            case ITEM_LIFE_STEAL:
                if (lifeStealCount < MAX_STACKABLE)
                    lifeStealCount++;
                return "Life Steal";

            case ITEM_POTION:
                if (potionCount < MAX_STACKABLE)
                    potionCount++;
                return "Potion";

            case ITEM_LIFE_STONE:
                lifeStoneCount++;
                if (lifeStoneCount > 0 && !lifeStoneAvailableThisScene)
                    lifeStoneAvailableThisScene = true;
                return "Life Stone";

            case ITEM_CHAOS:
                chaosCount++;
                return "Chaos";

            default:
                return "Nothing";
        }
    }

    // ══════════════════════════════════════════════════════════
    // ADD ITEMS ON SCENE CHANGE (llama al ejercicio 9)
    // ══════════════════════════════════════════════════════════

    public void AddItemsOnSceneChange()
    {
        int itemsAnyadidos = 0;

        // ── USAMOS LA FUNCIÓN DEL EJERCICIO 9 ──
        int aAnyadirLS = CalcularItemsAAnyadir(ITEM_LIFE_STEAL, 2);
        lifeStealCount += aAnyadirLS;
        itemsAnyadidos += aAnyadirLS;

        int aAnyadirP = CalcularItemsAAnyadir(ITEM_POTION, 2);
        potionCount += aAnyadirP;
        itemsAnyadidos += aAnyadirP;

        //lifeStoneCount++;
        //itemsAnyadidos++;

        int aAnyadirC = CalcularItemsAAnyadir(ITEM_CHAOS, 1);
        chaosCount += aAnyadirC;
        itemsAnyadidos += aAnyadirC;

        lifeStoneAvailableThisScene = true;

        debugEJ_itemsAnyadidos = itemsAnyadidos;
        Debug.Log($"[HUD] Items escena: LS={lifeStealCount}, P={potionCount}, St={lifeStoneCount}, Ch={chaosCount}");
    }

    // ══════════════════════════════════════════════════════════
    // FIN DEL ARCHIVO
    // ══════════════════════════════════════════════════════════
}