// ============================================================
// RapierHitbox.cs — VERSIÓN ENCAPSULADA COMPLETA
// CON EJERCICIOS DIDÁCTICOS PARA BACHILLERATO TIC 1
// ============================================================
// Gestiona el hitbox del Rapier y su modo de daño activo.
//
// TECLAS DEL ALUMNO:
//   4 → modo Normal   (color base del material, sin efecto)
//   5 → modo Veneno   (tinte morado, activa veneno al impactar)
//   6 → modo Sangrado (tinte rojo extra, acumula golpes)
//
// ORGANIZACIÓN EN TRES BLOQUES:
//   BLOQUE 1 — Inicialización de variables + constantes
//   BLOQUE 2 — Ejercicios en estilo C++ (el alumno trabaja aquí)
//   BLOQUE 3 — Funciones de trabajo sucio Unity/C# (alumno NO toca)
//
// TODA la funcionalidad original se mantiene íntegra.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RapierHitbox : MonoBehaviour
{
    // =====================================================================
    // BLOQUE 1 — INICIALIZACIÓN DE VARIABLES Y ESTRUCTURAS
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │  1A — CONSTANTES DEL ALUMNO                             │
    // └─────────────────────────────────────────────────────────┘

    public const int MODO_NORMAL   = 0;
    public const int MODO_VENENO   = 1;
    public const int MODO_SANGRADO = 2;

    public const int COLOR_ROJO  = 0;
    public const int COLOR_MORADO = 1;
    public const int COLOR_BLANCO = 2;

    public const int ESTADO_NINGUNO = 0;
    public const int ESTADO_SLEEP   = 1;
    public const int ESTADO_CONFUSO = 2;

    public const int GOLPES_PARA_ESTADO = 8;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1B — ESTRUCTURA DE DATOS DE LA FICHA DEL RAPIER        │
    // └─────────────────────────────────────────────────────────┘

    [System.Serializable]
    public struct FichaRapier
    {
        public int modoActual;              // 0=Normal, 1=Veneno, 2=Sangrado
        public int colorActual;             // 0=Rojo, 1=Morado, 2=Blanco
        public int danyoBase;               // Daño base del arma
        public int estadoSeleccionado;      // Estado especial elegido por el jugador
        public int golpesAcumulados;        // Golpes hacia el estado especial
        public bool hitboxActivo;           // true = puede golpear
    }

    public FichaRapier ficha;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1C — VARIABLES DE DEBUG PARA EJERCICIOS [EJ]           │
    // └─────────────────────────────────────────────────────────┘

    [Header("═══ DEBUG EJERCICIOS [EJ] ═══")]
    public int debugEJ_modoProcesado;
    public int debugEJ_colorElegido;
    public int debugEJ_estadoProcesado;
    public int debugEJ_golpesParaEstado;
    public bool debugEJ_estadoAplicado;
    public int debugEJ_tipoEfecto;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1D — INSPECTOR (variables originales)                   │
    // └─────────────────────────────────────────────────────────┘

    [Header("Daño base")]
    public int damage = 1;

    [Tooltip("Transform raíz del personaje que posee este hitbox.")]
    public Transform ownerRoot;

    [Header("Teclas de modo del Rapier")]
    public KeyCode teclaModoNormal   = KeyCode.Alpha4;
    public KeyCode teclaModoVeneno   = KeyCode.Alpha5;
    public KeyCode teclaModoSangrado = KeyCode.Alpha6;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1E — ESTADO INTERNO ORIGINAL                            │
    // └─────────────────────────────────────────────────────────┘

    [HideInInspector] public DamageType damageType = DamageType.Red;

    private Renderer hojaRenderer;
    private Material materialInstanciado;

    private static readonly Color tintNormal   = new Color(1.00f, 1.00f, 1.00f, 1f);
    private static readonly Color tintVeneno   = new Color(0.80f, 0.60f, 1.00f, 1f);
    private static readonly Color tintSangrado = new Color(1.00f, 0.55f, 0.55f, 1f);

    readonly HashSet<Damageable> alreadyHit = new HashSet<Damageable>();

    private SimpleWalk playerCombat;

    // ══════════════════════════════════════════════════════════
    // AWAKE
    // ══════════════════════════════════════════════════════════

    void Awake()
    {
        ObtenerMaterialDeLaHoja();
        AplicarModoNormal();
        if (playerCombat == null)
            playerCombat = FindAnyObjectByType<SimpleWalk>();

        // Inicializar ficha
        ficha.modoActual = MODO_NORMAL;
        ficha.colorActual = COLOR_ROJO;
        ficha.danyoBase = damage;
        ficha.estadoSeleccionado = ESTADO_NINGUNO;
        ficha.golpesAcumulados = 0;
        ficha.hitboxActivo = false;
    }

    // ══════════════════════════════════════════════════════════
    // UPDATE
    // ══════════════════════════════════════════════════════════

    void Update()
    {
        DetectarCambioDeModo();
        SincronizarFicha();
    }

    void SincronizarFicha()
    {
        ficha.modoActual = TipoAConstante(damageType);
        ficha.danyoBase = damage;
        ficha.hitboxActivo = enabled;

        if (playerCombat != null)
        {
            ficha.estadoSeleccionado = EstadoEspecialAConstante(playerCombat.estadoSeleccionado);
        }
    }

    // =====================================================================
    // BLOQUE 2 — EJERCICIOS EN ESTILO C++
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │         HERRAMIENTAS QUE PUEDES USAR LIBREMENTE         │
    // │         (ya están implementadas, no las toques)         │
    // └─────────────────────────────────────────────────────────┘

    /// <summary>
    /// Convierte un DamageType a su constante int correspondiente.
    /// </summary>
    int TipoAConstante(DamageType tipo)
    {
        switch (tipo)
        {
            case DamageType.Normal:  return MODO_NORMAL;
            case DamageType.Poison:  return MODO_VENENO;
            case DamageType.Bleed:   return MODO_SANGRADO;
            default:                 return MODO_NORMAL;
        }
    }

    /// <summary>
    /// Convierte una constante int a su DamageType correspondiente.
    /// </summary>
    DamageType ConstanteATipo(int constante)
    {
        switch (constante)
        {
            case MODO_NORMAL:   return DamageType.Normal;
            case MODO_VENENO:   return DamageType.Poison;
            case MODO_SANGRADO: return DamageType.Bleed;
            default:            return DamageType.Normal;
        }
    }

    /// <summary>
    /// Convierte un EstadoEspecial de Damageable a constante int.
    /// </summary>
    int EstadoEspecialAConstante(Damageable.EstadoEspecial estado)
    {
        switch (estado)
        {
            case Damageable.EstadoEspecial.Sleep:   return ESTADO_SLEEP;
            case Damageable.EstadoEspecial.Confused: return ESTADO_CONFUSO;
            default:                                 return ESTADO_NINGUNO;
        }
    }

    /// <summary>
    /// Devuelve TRUE si la tecla corresponde al modo Normal.
    /// </summary>
    bool EsTeclaNormal(KeyCode tecla)
    {
        return tecla == KeyCode.Alpha4;
    }

    /// <summary>
    /// Devuelve TRUE si la tecla corresponde al modo Veneno.
    /// </summary>
    bool EsTeclaVeneno(KeyCode tecla)
    {
        return tecla == KeyCode.Alpha5;
    }

    /// <summary>
    /// Devuelve TRUE si la tecla corresponde al modo Sangrado.
    /// </summary>
    bool EsTeclaSangrado(KeyCode tecla)
    {
        return tecla == KeyCode.Alpha6;
    }

    /// <summary>
    /// Devuelve TRUE si el jugador tiene un estado especial seleccionado.
    /// </summary>
    bool JugadorTieneEstadoSeleccionado()
    {
        if (playerCombat == null) return false;
        return playerCombat.estadoSeleccionado != Damageable.EstadoEspecial.None;
    }

    /// <summary>
    /// Devuelve el estado especial seleccionado por el jugador como constante int.
    /// </summary>
    int EstadoJugador()
    {
        if (playerCombat == null) return ESTADO_NINGUNO;
        return EstadoEspecialAConstante(playerCombat.estadoSeleccionado);
    }

    /// <summary>
    /// Devuelve los golpes necesarios para aplicar un estado especial.
    /// </summary>
    int GolpesNecesarios()
    {
        return GOLPES_PARA_ESTADO;
    }

    /// <summary>
    /// Devuelve TRUE si el enemigo ya fue golpeado en este swing.
    /// (Usado internamente, el alumno no necesita llamarla)
    /// </summary>
    bool FueGolpeado(Damageable d)
    {
        return alreadyHit.Contains(d);
    }

    /// <summary>
    /// Marca un enemigo como golpeado en este swing.
    /// </summary>
    void MarcarGolpeado(Damageable d)
    {
        alreadyHit.Add(d);
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 1 — DetectarCambioDeModo: ¿Qué modo elegir? (switch/case)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador pulsa 4, 5 o 6 para cambiar el modo del Rapier.
    //  Este ejercicio decide qué modo activar según la tecla pulsada.
    //
    //  EJEMPLO REAL:
    //  - Pulsa 4 → modo Normal (daño directo, sin efecto)
    //  - Pulsa 5 → modo Veneno (aplica veneno al enemigo)
    //  - Pulsa 6 → modo Sangrado (acumula golpes de sangrado)
    //
    //  OBJETIVO: Recibe una tecla y devuelve el modo correspondiente:
    //    Tecla 4 → 0 (Normal)
    //    Tecla 5 → 1 (Veneno)
    //    Tecla 6 → 2 (Sangrado)
    //    Otra tecla → -1 (no cambiar)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: EsTeclaNormal(), EsTeclaVeneno(), EsTeclaSangrado(), if/else
    //  ❌ NO uses: Input, KeyCode directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="tecla">Tecla pulsada.</param>
    /// <returns>Modo a activar: 0=Normal, 1=Veneno, 2=Sangrado, -1=Ninguno</returns>
    int DetectarCambioDeModo(KeyCode tecla)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        if (EsTeclaNormal(tecla))
        {
            return MODO_NORMAL;
        }
        else if (EsTeclaVeneno(tecla))
        {
            return MODO_VENENO;
        }
        else if (EsTeclaSangrado(tecla))
        {
            return MODO_SANGRADO;
        }
        else
        {
            return -1;  // tecla no reconocida
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 2 — ProcesarEstadoEspecial: ¿Aplicar estado al enemigo?
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cuando el jugador golpea a un enemigo con un arma,
    //  se acumulan golpes. Al llegar a 8, se aplica el estado especial
    //  que el jugador haya seleccionado (sueño o confusión).
    //
    //  Este ejercicio es COMPARTIDO por RapierHitbox, KickCollider y
    //  LightSaberHitbox. Todos usan la misma lógica.
    //
    //  EJEMPLO REAL:
    //  - Jugador selecciona SLEEP (tecla 7)
    //  - Golpea al enemigo 7 veces → nada especial
    //  - Golpea la 8ª vez → se aplica SLEEP al enemigo
    //  - El contador se resetea a 0
    //
    //  OBJETIVO: Recibe los golpes actuales del enemigo hacia el estado
    //  y el estado seleccionado por el jugador. Devuelve:
    //    - Si golpes < 8 → devuelve los golpes + 1 (acumula)
    //    - Si golpes >= 8 y estado == SLEEP → devuelve -1 (aplicar sleep)
    //    - Si golpes >= 8 y estado == CONFUSO → devuelve -2 (aplicar confused)
    //    - Si golpes >= 8 y estado == NINGUNO → devuelve 0 (resetear, no aplicar)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: GolpesNecesarios(), EstadoJugador(), if/else, switch/case
    //  ❌ NO uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="golpesActuales">Golpes acumulados hacia el estado.</param>
    /// <returns>
    ///   >= 0: nuevos golpes acumulados (solo se acumuló)
    ///   -1:   aplicar SLEEP
    ///   -2:   aplicar CONFUSED
    /// </returns>
    int ProcesarEstadoEspecial(int golpesActuales)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // PASO 1: acumulamos un golpe
        int nuevosGolpes = golpesActuales + 1;

        // PASO 2: ¿llegó al umbral?
        int umbral = GolpesNecesarios();  // 8

        if (nuevosGolpes < umbral)
        {
            // No llegó al umbral, solo acumulamos
            return nuevosGolpes;
        }

        // PASO 3: llegó al umbral, ¿qué estado aplicar?
        int estadoJugador = EstadoJugador();

        switch (estadoJugador)
        {
            case ESTADO_SLEEP:
                return -1;  // aplicar SLEEP

            case ESTADO_CONFUSO:
                return -2;  // aplicar CONFUSED

            case ESTADO_NINGUNO:
            default:
                return 0;   // resetear, no aplicar nada
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 3 — ¿Qué color según el modo? (switch/case)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cada modo del Rapier tiene un color asociado:
    //   - Normal → Blanco (color base de la hoja)
    //   - Veneno → Morado (tinte venenoso)
    //   - Sangrado → Rojo (tinte de sangre)
    //
    //  OBJETIVO: Recibe un modo y devuelve el color correspondiente:
    //    Modo 0 (Normal)   → 2 (Blanco)
    //    Modo 1 (Veneno)   → 1 (Morado)
    //    Modo 2 (Sangrado) → 0 (Rojo)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: switch/case
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="modo">Modo del Rapier (0=Normal, 1=Veneno, 2=Sangrado).</param>
    /// <returns>Color correspondiente (0=Rojo, 1=Morado, 2=Blanco)</returns>
    int ColorSegunModo(int modo)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        switch (modo)
        {
            case MODO_NORMAL:
                return COLOR_BLANCO;   // Blanco = color base

            case MODO_VENENO:
                return COLOR_MORADO;   // Morado = veneno

            case MODO_SANGRADO:
                return COLOR_ROJO;     // Rojo = sangre

            default:
                return COLOR_BLANCO;   // por defecto, blanco
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 4 — ¿El golpe es válido? (condiciones compuestas &&)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Antes de aplicar daño, el juego comprueba varias cosas:
    //   1. El golpe NO es del propio jugador (no autolesionarse)
    //   2. El enemigo tiene componente Damageable
    //   3. El enemigo NO fue ya golpeado en este swing
    //   4. El enemigo acepta este tipo de daño y arma
    //
    //  OBJETIVO: Recibe varias condiciones como bool y devuelve true
    //  si TODAS se cumplen (el golpe es válido).
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: operadores && (AND), if/else
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="esGolpePropio">true si el golpe es del propio jugador.</param>
    /// <param name="tieneDamageable">true si el enemigo tiene Damageable.</param>
    /// <param name="yaFueGolpeado">true si ya fue golpeado este swing.</param>
    /// <param name="aceptaTipoDanyo">true si acepta este tipo de daño.</param>
    /// <returns>true si el golpe es válido y se debe aplicar daño.</returns>
    bool EsGolpeValido(bool esGolpePropio, bool tieneDamageable, bool yaFueGolpeado, bool aceptaTipoDanyo)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // El golpe es válido si:
        // - NO es golpe propio (no autolesión)
        // - TIENE Damageable (puede recibir daño)
        // - NO fue golpeado ya (no doble hit)
        // - ACEPTA el tipo de daño
        if (!esGolpePropio && tieneDamageable && !yaFueGolpeado && aceptaTipoDanyo)
            return true;
        else
            return false;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // =====================================================================
    // BLOQUE 3 — TRABAJO SUCIO UNITY/C# (alumno NO toca)
    // =====================================================================

    // ══════════════════════════════════════════════════════════
    // DETECCIÓN DE MODO (llama al ejercicio 1)
    // ══════════════════════════════════════════════════════════

    void DetectarCambioDeModo()
    {
        // Comprobamos las teclas
        bool pulsaNormal   = Input.GetKeyDown(teclaModoNormal);
        bool pulsaVeneno   = Input.GetKeyDown(teclaModoVeneno);
        bool pulsaSangrado = Input.GetKeyDown(teclaModoSangrado);

        // ── USAMOS LA FUNCIÓN DEL EJERCICIO 1 ──
        if (pulsaNormal)
        {
            int modo = DetectarCambioDeModo(teclaModoNormal);
            EstablecerModo(modo);
        }
        else if (pulsaVeneno)
        {
            int modo = DetectarCambioDeModo(teclaModoVeneno);
            EstablecerModo(modo);
        }
        else if (pulsaSangrado)
        {
            int modo = DetectarCambioDeModo(teclaModoSangrado);
            EstablecerModo(modo);
        }
    }

    /// <summary>
    /// Establece el modo del Rapier según la constante.
    /// </summary>
    void EstablecerModo(int modo)
    {
        debugEJ_modoProcesado = modo;

        // El tipo de daño se sigue asignando directamente
        damageType = ConstanteATipo(modo);

        // ── USAMOS LA FUNCIÓN DEL EJERCICIO 3 ──
        int colorConst = ColorSegunModo(modo);   // el alumno decide el color
        debugEJ_colorElegido = colorConst;       // lo mostramos en el inspector

        // Convertimos la constante del alumno a un Color de Unity
        Color tinte;
        switch (colorConst)
        {
            case COLOR_MORADO: tinte = tintVeneno;   break;
            case COLOR_ROJO:   tinte = tintSangrado; break;
            default:           tinte = tintNormal;   break;  // COLOR_BLANCO u otros
        }

        AplicarTinteAlMaterial(tinte);

        Debug.Log("[Rapier] Modo: " + damageType + " | Color: " + colorConst);
    }

    // ══════════════════════════════════════════════════════════
    // APLICACIÓN DE MODOS (se mantienen originales)
    // ══════════════════════════════════════════════════════════

    void AplicarModoNormal()
    {
        damageType = DamageType.Normal;
        AplicarTinteAlMaterial(tintNormal);
        Debug.Log("[Rapier] Modo: Normal");
    }

    void AplicarModoVeneno()
    {
        damageType = DamageType.Poison;
        AplicarTinteAlMaterial(tintVeneno);
        Debug.Log("[Rapier] Modo: Veneno");
    }

    void AplicarModoSangrado()
    {
        damageType = DamageType.Bleed;
        AplicarTinteAlMaterial(tintSangrado);
        Debug.Log("[Rapier] Modo: Sangrado");
    }

    void AplicarTinteAlMaterial(Color tinte)
    {
        if (materialInstanciado == null) return;
        if (materialInstanciado.HasProperty("_BaseColor"))
            materialInstanciado.SetColor("_BaseColor", tinte);
        else if (materialInstanciado.HasProperty("_Color"))
            materialInstanciado.SetColor("_Color", tinte);
    }

    void ObtenerMaterialDeLaHoja()
    {
        hojaRenderer = GetComponent<Renderer>();
        if (hojaRenderer != null)
        {
            materialInstanciado = hojaRenderer.material;
        }
    }

    public void Reactivar()
    {
        Debug.Log("OnEnable de Rapier ha sido activado");
        alreadyHit.Clear();
    }

    // ══════════════════════════════════════════════════════════
    // DETECCIÓN DE COLISIÓN (llama a los ejercicios 2 y 4)
    // ══════════════════════════════════════════════════════════

    void OnTriggerEnter(Collider other)
    {
        // Recopilamos los datos para pasárselos al ejercicio
        bool esPropio       = EsGolpePropio(other);
        Damageable damageable = BuscarVidaEnEnemigo(other);
        bool tieneDamageable  = damageable != null;
        // YaFueGolpeadoEnEsteSwing marca al enemigo como golpeado (efecto lateral)
        bool yaGolpeado       = tieneDamageable ? YaFueGolpeadoEnEsteSwing(damageable) : false;
        string etiquetaArma   = gameObject.tag;
        bool aceptaTipo       = tieneDamageable ? damageable.CanBeDamagedBy(damageType, etiquetaArma) : false;

        // ── USAMOS LA FUNCIÓN DEL EJERCICIO 4 ──
        // El alumno decide si el golpe es válido con estas condiciones
        if (!EsGolpeValido(esPropio, tieneDamageable, yaGolpeado, aceptaTipo))
            return;   // el alumno ha dictaminado que no se debe aplicar daño

        // Si llegamos aquí, el golpe es válido (damageable seguro que no es null)
        GameObject fuente = ownerRoot != null ? ownerRoot.gameObject : gameObject;
        damageable.TakeDamage(damage, damageType, fuente);

        // Procesar estado especial (ya integrado con el ejercicio 2)
        ProcesarEstadoEspecial(damageable);
    }

    /// <summary>
    /// Procesa el estado especial tras golpear a un enemigo.
    /// USA la función del ejercicio 2 (ProcesarEstadoEspecial).
    /// </summary>
    void ProcesarEstadoEspecial(Damageable damageable)
    {
        if (playerCombat == null)
            playerCombat = FindAnyObjectByType<SimpleWalk>();

        int estadoSeleccionado = EstadoJugador();

        // Si no hay estado seleccionado, no hacer nada
        if (estadoSeleccionado == ESTADO_NINGUNO)
            return;

        // Si el enemigo ya está en un estado especial, no acumular
        if (damageable.GetEstadoEspecial() != Damageable.EstadoEspecial.None)
            return;

        // ── Llamamos a la función del ejercicio 2 ──
        int resultado = ProcesarEstadoEspecial(damageable.golpesRecibidosParaEstado);

        debugEJ_golpesParaEstado = resultado;
        debugEJ_estadoAplicado = (resultado == -1 || resultado == -2);

        if (resultado >= 0)
        {
            // Solo se acumuló un golpe (o se reseteó)
            damageable.golpesRecibidosParaEstado = resultado;
        }
        else if (resultado == -1)
        {
            // Aplicar SLEEP
            if (damageable.AplicarSleep())
            {
                EnemyScript enemy = damageable.GetComponent<EnemyScript>();
                if (enemy != null)
                    enemy.ActivarSleep();
                Debug.Log($"[RapierHitbox] SLEEP aplicado a '{damageable.name}' tras {GOLPES_PARA_ESTADO} golpes.");
            }
            damageable.golpesRecibidosParaEstado = 0;
        }
        else if (resultado == -2)
        {
            // Aplicar CONFUSED
            if (damageable.AplicarConfused())
            {
                EnemyScript enemy = damageable.GetComponent<EnemyScript>();
                if (enemy != null)
                    enemy.ActivarConfused();
                Debug.Log($"[RapierHitbox] CONFUSED aplicado a '{damageable.name}' tras {GOLPES_PARA_ESTADO} golpes.");
            }
            damageable.golpesRecibidosParaEstado = 0;
        }
    }

    // ══════════════════════════════════════════════════════════
    // HELPERS DE COLISIÓN (se mantienen originales)
    // ══════════════════════════════════════════════════════════

    bool EsGolpePropio(Collider other)
    {
        return ownerRoot != null && other.transform.root == ownerRoot;
    }

    Damageable BuscarVidaEnEnemigo(Collider other)
    {
        return other.GetComponentInParent<Damageable>();
    }

    bool YaFueGolpeadoEnEsteSwing(Damageable damageable)
    {
        return !alreadyHit.Add(damageable);
    }

    string ObtenerEtiquetaPropia()
    {
        return gameObject.tag;
    }
}