// ============================================================
// PlayerHealth.cs — VERSIÓN ENCAPSULADA PARA TIC I BACHILLERATO
// ============================================================
// ESTE ARCHIVO TIENE 3 PARTES BIEN DIFERENCIADAS:
//   (1) INICIALIZACIÓN DE VARIABLES
//   (2) FUNCIONES ESTILO C++ — EL ALUMNO ESCRIBE AQUÍ
//   (3) TRABAJO SUCIO UNITY/C# — NO TOCAR, YA HECHO
// ============================================================

using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════
    // PARTE 1 — INICIALIZACIÓN DE VARIABLES (VISIBLES EN INSPECTOR)
    // ══════════════════════════════════════════════════════════════

    [Header("Vida del jugador")]
    public float vida = 10f;
    public float vidaMax = 20f;

    [Header("Progresión")]
    public float level = 1f;           // Nivel actual (empezamos en 1)
    public float XP = 0f;              // Experiencia acumulada
    public float xpParaSiguienteNivel = 20f;  // Umbral para subir (se calcula solo)

    [Header("Referencias")]
    public PlayerHUDController hud;
    public GameManager gameManager;    // Se auto-busca si está null

    // ── Estado interno de muerte ──────────────────────────────
    private bool isDead = false;
    private bool muerteYaNotificada = false;

    // ══════════════════════════════════════════════════════════════
    // PARTE 2 — HERRAMIENTAS QUE YA ESTÁN HECHAS (NO TOCAR)
    // ══════════════════════════════════════════════════════════════
    // El alumno SOLO llama a estas funciones. Su interior es "magia Unity".

    /// <summary>
    /// Devuelve el GameManager de la escena. Lo busca si no está asignado.
    /// </summary>
    GameManager ObtenerGameManager()
    {
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();
        return gameManager;
    }

    /// <summary>
    /// Devuelve el EnemyManager activo. Lo rebusca cada vez porque
    /// los managers se destruyen y recrean entre rondas.
    /// </summary>
    EnemyManager ObtenerEnemyManager()
    {
        var gm = ObtenerGameManager();
        if (gm == null) return null;
        return gm.enemySpawner?.GetSpawnedManager();
    }

    /// <summary>
    /// Notifica la muerte al GameManager, con información de escena
    /// y ronda para estadísticas.
    /// </summary>
    void RegistrarMuerte()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        ObtenerNombreEscenaActual();
        ObtenerRondaActual();
        NotificarMuerteAGameManager();

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }

    /// <summary>
    /// Devuelve el índice de la escena actual (0 = Testing, 1 = Scene2...).
    /// </summary>
    int ObtenerIndiceEscenaActual()
    {
        var gm = ObtenerGameManager();
        return gm != null ? gm.GetCurrentSceneIndex() : 0;
    }

    /// <summary>
    /// Devuelve el número de ronda actual (1-based).
    /// </summary>
    int ObtenerRondaActual()
    {
        var gm = ObtenerGameManager();
        return gm != null ? gm.GetCurrentRound() : 1;
    }

    /// <summary>
    /// Devuelve el nombre de la escena actual.
    /// </summary>
    string ObtenerNombreEscenaActual()
    {
        var gm = ObtenerGameManager();
        return gm != null ? gm.GetCurrentSceneName() : "Desconocida";
    }

    /// <summary>
    /// Notifica al GameManager que el jugador ha muerto.
    /// </summary>
    void NotificarMuerteAGameManager()
    {
        var gm = ObtenerGameManager();
        if (gm != null)
            gm.OnPlayerDied();
    }

    /// <summary>
    /// Clampa la vida entre 0 y vidaMax. Se llama automáticamente.
    /// </summary>
    void ClampearVida()
    {
        if (vida < 0f) vida = 0f;
        if (vida > vidaMax) vida = vidaMax;
    }

    /// <summary>
    /// Actualiza la HUD si existe.
    /// </summary>
    void ActualizarHUD()
    {
        if (hud != null)
        {
            // La HUD se actualiza sola en su Update, pero forzamos si hace falta
        }
    }

    // ══════════════════════════════════════════════════════════════
    // PARTE 2 — 🎯 EJERCICIOS DEL ALUMNO (ESTILO C++ PURO)
    // ══════════════════════════════════════════════════════════════
    // REGLAS:
    //  ✅ Solo C++ básico: int, float, bool, if, else, while, for, return
    //  ✅ Solo puedes usar las variables de arriba y las funciones "herramienta"
    //  ❌ NO uses: GameObject, Transform, Vector3, Corrutinas, GetComponent...
    //  ❌ NO uses: Debug.Log, Time.deltaTime, Mathf, arrays de C#...
    //
    // CADA FUNCIÓN TIENE SU ENUNCIADO DETALLADO EN EL SUMMARY.
    // EL ALUMNO ESCRIBE EL CUERPO. LAS FIRMAS YA ESTÁN PUESTAS.
    // ══════════════════════════════════════════════════════════════

    // ┌─────────────────────────────────────────────────────────┐
    // │  EJERCICIO 1: ¿ES PRIMO?                                │
    // └─────────────────────────────────────────────────────────┘
    /// <summary>
    /// EJERCICIO 1 — EsPrimo
    /// 
    /// OBJETIVO:
    /// Determinar si un número entero positivo es primo. Un número primo
    /// SOLO es divisible entre 1 y sí mismo. Por convención, 0 y 1 NO son primos.
    /// El 2 ES primo (es el único número par que lo es).
    /// 
    /// ESTRATEGIA RECOMENDADA:
    /// - Para números ≤ 1: devolver false directamente.
    /// - Para el 2: devolver true directamente.
    /// - Para pares mayores que 2: devolver false (son divisibles por 2).
    /// - Para impares ≥ 3: comprobar si tienen algún divisor desde 3 hasta la
    ///   raíz cuadrada del número, saltando de 2 en 2 (solo impares).
    ///   Si encuentras alguno que divida exacto (resto 0), NO es primo.
    ///   Si terminas el bucle sin encontrar ninguno, SÍ es primo.
    /// 
    /// VARIABLES DISPONIBLES:
    ///   • int n — el número a comprobar (parámetro de entrada, n ≥ 0)
    /// 
    /// HERRAMIENTAS A TU DISPOSICIÓN:
    ///   • Estructuras de control: if, else, for, while, return
    ///   • Operador módulo (%) para saber si una división es exacta
    ///   • Comparaciones: ==, !=, <, >, <=, >=
    ///   • Aritmética básica: +, -, *, /
    ///   • La función se llama a sí misma recursivamente NO está permitido
    /// 
    /// EJEMPLOS DE COMPORTAMIENTO ESPERADO:
    ///   EsPrimo(0)   → false
    ///   EsPrimo(1)   → false
    ///   EsPrimo(2)   → true
    ///   EsPrimo(3)   → true
    ///   EsPrimo(4)   → false  (2 × 2)
    ///   EsPrimo(5)   → true
    ///   EsPrimo(9)   → false  (3 × 3)
    ///   EsPrimo(11)  → true
    ///   EsPrimo(15)  → false  (3 × 5)
    ///   EsPrimo(17)  → true
    ///   EsPrimo(100) → false
    /// 
    /// PISTA: Para comprobar divisores hasta la raíz, la condición del bucle
    /// puede ser: i * i <= n
    /// </summary>
    /// <param name="n">Número a comprobar (n ≥ 0)</param>
    /// <returns>true si es primo, false en caso contrario</returns>
    bool EsPrimo(float n)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        // CASO BASE: 0 y 1 no son primos
        if (n <= 1) return false;

        // 2 es el único primo par
        if (n == 2) return true;

        // Pares mayores que 2 no son primos
        if (n % 2 == 0) return false;

        // Comprobar divisores impares desde 3 hasta raíz(n)
        for (int i = 3; i * i <= n; i += 2)
        {
            if (n % i == 0) return false;  // Encontrado divisor → no es primo
        }

        // Nadie lo dividió → es primo
        return true;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // ┌─────────────────────────────────────────────────────────┐
    // │  EJERCICIO 2: SIGUIENTE PRIMO                           │
    // └─────────────────────────────────────────────────────────┘
    /// <summary>
    /// EJERCICIO 2 — SiguientePrimo
    /// 
    /// OBJETIVO:
    /// Encontrar el menor número primo ESTRICTAMENTE MAYOR que el número
    /// recibido como parámetro. Es decir, dado 'actual', devolver el primer
    /// primo que esté por encima.
    /// 
    /// ESTRATEGIA RECOMENDADA:
    /// - Empieza a buscar en el número siguiente (actual + 1).
    /// - Comprueba si ese número es primo usando la función EsPrimo()
    ///   que ya has implementado en el ejercicio anterior.
    /// - Si lo es, devuélvelo inmediatamente.
    /// - Si no lo es, pasa al siguiente número y repite.
    /// - Esto es un bucle "hasta que se cumpla una condición".
    /// 
    /// VARIABLES DISPONIBLES:
    ///   • int actual — número de partida (parámetro, actual ≥ 0)
    /// 
    /// HERRAMIENTAS A TU DISPOSICIÓN:
    ///   • Estructuras de control: while, if, return
    ///   • Operador ++ o suma + 1 para avanzar al siguiente número
    ///   • Llamada a tu propia función: EsPrimo(candidato)
    ///   • Variables locales auxiliares (int candidato, etc.)
    /// 
    /// EJEMPLOS DE COMPORTAMIENTO ESPERADO:
    ///   SiguientePrimo(1)   → 2
    ///   SiguientePrimo(2)   → 3
    ///   SiguientePrimo(3)   → 5
    ///   SiguientePrimo(5)   → 7
    ///   SiguientePrimo(7)   → 11
    ///   SiguientePrimo(10)  → 11
    ///   SiguientePrimo(13)  → 17
    ///   SiguientePrimo(20)  → 23
    /// 
    /// PISTA: Un bucle while(true) con un return dentro cuando se cumple
    /// la condición es un patrón muy común para este tipo de búsquedas.
    /// </summary>
    /// <param name="actual">Número de partida (busca el primo > actual)</param>
    /// <returns>El siguiente número primo</returns>
    float SiguientePrimo(float actual)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        float candidato = actual + 1;

        while (true)
        {
            if (EsPrimo(candidato))
                return candidato;

            candidato = candidato + 1;
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // ┌─────────────────────────────────────────────────────────┐
    // │  EJERCICIO 3: XP NECESARIA PARA NIVEL                   │
    // └─────────────────────────────────────────────────────────┘
    /// <summary>
    /// EJERCICIO 3 — CalcularXPNecesariaParaNivel
    /// 
    /// OBJETIVO:
    /// Calcular cuánta experiencia total se necesita para alcanzar un nivel
    /// concreto. La fórmula del juego es:
    /// 
    ///     XP_necesaria = SiguientePrimo(nivel) × 10
    /// 
    /// Donde "SiguientePrimo(nivel)" significa el primer número primo
    /// estrictamente mayor que el nivel indicado.
    /// 
    /// ESTRATEGIA RECOMENDADA:
    /// - Llama a la función SiguientePrimo() que ya has hecho, pasándole
    ///   el nivel como parámetro.
    /// - Multiplica el resultado por 10.
    /// - Devuelve ese valor.
    /// 
    /// VARIABLES DISPONIBLES:
    ///   • int nivel — nivel objetivo para el que calcular el umbral (parámetro)
    /// 
    /// HERRAMIENTAS A TU DISPOSICIÓN:
    ///   • Llamada a función propia: SiguientePrimo(nivel)
    ///   • Multiplicación: *
    ///   • Return directo del cálculo
    /// 
    /// EJEMPLOS DE COMPORTAMIENTO ESPERADO:
    ///   Nivel 1 → SiguientePrimo(1) = 2 → 2 × 10 = 20 XP
    ///   Nivel 2 → SiguientePrimo(2) = 3 → 3 × 10 = 30 XP
    ///   Nivel 3 → SiguientePrimo(3) = 5 → 5 × 10 = 50 XP
    ///   Nivel 4 → SiguientePrimo(4) = 5 → 5 × 10 = 50 XP
    ///   Nivel 5 → SiguientePrimo(5) = 7 → 7 × 10 = 70 XP
    ///   Nivel 6 → SiguientePrimo(6) = 7 → 7 × 10 = 70 XP
    ///   Nivel 7 → SiguientePrimo(7) = 11 → 11 × 10 = 110 XP
    /// 
    /// NOTA: Esta función será usada por el sistema para saber cuándo
    /// el jugador tiene XP suficiente para subir de nivel.
    /// </summary>
    /// <param name="nivel">Nivel para el que calcular el umbral de XP</param>
    /// <returns>XP total necesaria para alcanzar ese nivel</returns>
    float CalcularXPNecesariaParaNivel(float nivel)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        float siguientePrimo = SiguientePrimo(nivel);
        return siguientePrimo * 10;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // ┌─────────────────────────────────────────────────────────┐
    // │  EJERCICIO 4: MULTIPLICADOR ESCENA/RONDA                │
    // └─────────────────────────────────────────────────────────┘
    /// <summary>
    /// EJERCICIO 4 — ObtenerMultiplicadorEscenaRonda
    /// 
    /// OBJETIVO:
    /// Calcular el multiplicador de experiencia que se aplica en este momento,
    /// basándote en dos factores: la escena donde está el jugador y la ronda
    /// actual dentro de esa escena.
    /// 
    /// REGLAS DEL JUEGO:
    /// 
    /// 1. MULTIPLICADOR BASE POR ESCENA (índice de escena):
    ///    • Escena 0 (Testing)     → ×1.0
    ///    • Escena 1 (Aliens)      → ×2.0
    ///    • Escena 2 (Peluches)    → ×3.0
    ///    • Escena 3 (Alonso)      → ×4.0
    ///    • Escena 4 (Maniquís)    → ×5.0
    ///    • Escena 5 (Boss)        → ×10.0
    ///    • Escena 6 (Final)       → ×1.0
    ///    • Cualquier otra         → ×1.0
    /// 
    /// 2. BONUS POR RONDA:
    ///    • Cada ronda completada añade +0.2 al multiplicador.
    ///    • Ronda 1 → +0.0 (base)
    ///    • Ronda 2 → +0.2
    ///    • Ronda 3 → +0.4
    ///    • Ronda 4 → +0.6
    ///    • Ronda 5+ → +0.6 (TOPE: las rondas extra por muerte no
    ///      aumentan el bonus más allá de la ronda 3)
    /// 
    /// 3. MULTIPLICADOR FINAL = multiplicadorBase + bonusRonda
    /// 
    /// ESTRATEGIA RECOMENDADA:
    /// - Obtén el índice de escena actual con ObtenerIndiceEscenaActual().
    /// - Obtén la ronda actual con ObtenerRondaActual().
    /// - Usa una estructura de selección múltiple (switch) para asignar
    ///   el multiplicador base según la escena.
    /// - Calcula la "ronda efectiva": si la ronda actual > 3, úsala como 3.
    /// - Calcula el bonus: (rondaEfectiva - 1) × 0.2
    /// - Suma base + bonus y devuelve el resultado.
    /// 
    /// VARIABLES DISPONIBLES (las obtienes llamando a funciones herramienta):
    ///   • int escena = ObtenerIndiceEscenaActual();
    ///   • int ronda  = ObtenerRondaActual();
    /// 
    /// HERRAMIENTAS A TU DISPOSICIÓN:
    ///   • Estructuras: switch, if, else
    ///   • Variables locales: float, int
    ///   • Aritmética con decimales: *, -, +
    ///   • Llamadas a funciones herramienta (ya hechas)
    /// 
    /// EJEMPLOS DE COMPORTAMIENTO ESPERADO:
    ///   Escena 1 (Aliens, ×2.0), Ronda 1  → 2.0 + 0.0 = 2.0
    ///   Escena 1 (Aliens, ×2.0), Ronda 2  → 2.0 + 0.2 = 2.2
    ///   Escena 3 (Alonso, ×4.0), Ronda 3  → 4.0 + 0.4 = 4.4
    ///   Escena 5 (Boss, ×10.0), Ronda 5   → 10.0 + 0.6 = 10.6 (ronda tratada como 3)
    ///   Escena 0 (Testing, ×1.0), Ronda 2 → 1.0 + 0.2 = 1.2
    /// </summary>
    /// <returns>Multiplicador float (ej: 2.4 = 240% de XP base)</returns>
    float ObtenerMultiplicadorEscenaRonda()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        int escena = ObtenerIndiceEscenaActual();
        int ronda = ObtenerRondaActual();

        // Multiplicador base según escena
        float multiplicadorBase = 1f;

        switch (escena)
        {
            case 0: multiplicadorBase = 1f; break;   // Testing
            case 1: multiplicadorBase = 2f; break;   // Aliens
            case 2: multiplicadorBase = 3f; break;   // Peluches
            case 3: multiplicadorBase = 4f; break;   // Alonso
            case 4: multiplicadorBase = 5f; break;   // Maniquís
            case 5: multiplicadorBase = 10f; break;  // Boss
            case 6: multiplicadorBase = 1f; break;   // Final
            default: multiplicadorBase = 1f; break;
        }

        // Bonus por ronda (máx ronda 3)
        int rondaEfectiva = ronda;
        if (rondaEfectiva > 3) rondaEfectiva = 3;

        float bonusRonda = (rondaEfectiva - 1) * 0.2f;

        return multiplicadorBase + bonusRonda;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // ┌─────────────────────────────────────────────────────────┐
    // │  EJERCICIO 5: CALCULAR XP GANADA REAL                   │
    // └─────────────────────────────────────────────────────────┘
    /// <summary>
    /// EJERCICIO 5 — CalcularXPGanada
    /// 
    /// OBJETIVO:
    /// Aplicar el multiplicador de escena/ronda al valor base de experiencia
    /// que otorga un enemigo, para obtener la experiencia final real que
    /// recibe el jugador.
    /// 
    /// REGLAS:
    /// - Recibe xpBase (lo que vale el enemigo en bruto, ej: 5, 10, 15...).
    /// - Obtiene el multiplicador actual llamando a ObtenerMultiplicadorEscenaRonda().
    /// - Multiplica xpBase × multiplicador.
    /// - Garantiza un mínimo de 1 XP (incluso si el cálculo diera 0).
    /// 
    /// VARIABLES DISPONIBLES:
    ///   • float xpBase — XP base del enemigo (parámetro, siempre ≥ 1)
    /// 
    /// HERRAMIENTAS A TU DISPOSICIÓN:
    ///   • Llamada a función propia: ObtenerMultiplicadorEscenaRonda()
    ///   • Multiplicación float × int
    ///   • Conversión (cast) de float a int
    ///   • Comparación y asignación para forzar mínimo
    ///   • Return del resultado final
    /// 
    /// EJEMPLOS DE COMPORTAMIENTO ESPERADO:
    ///   Enemigo base 5 XP, multiplicador 2.0  → 10 XP
    ///   Enemigo base 5 XP, multiplicador 3.2  → 16 XP (5 × 3.2 = 16.0)
    ///   Enemigo base 3 XP, multiplicador 1.2  → 3 XP (3 × 1.2 = 3.6 → 3)
    ///   Enemigo base 1 XP, multiplicador 0.5  → 1 XP (mínimo garantizado)
    /// </summary>
    /// <param name="xpBase">XP base que da el enemigo en bruto</param>
    /// <returns>XP final tras aplicar multiplicadores (mínimo 1)</returns>
    float CalcularXPGanada(float xpBase)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        float multiplicador = ObtenerMultiplicadorEscenaRonda();
        float xpFinal = xpBase * multiplicador;

        if (xpFinal < 1) xpFinal = 1;

        return xpFinal;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // ┌─────────────────────────────────────────────────────────┐
    // │  EJERCICIO 6: COMPROBAR SUBIDA DE NIVEL                 │
    // └─────────────────────────────────────────────────────────┘
    /// <summary>
    /// EJERCICIO 6 — ComprobarSubidaNivel
    /// 
    /// OBJETIVO:
    /// Comprobar si la experiencia acumulada (XP) ha alcanzado o superado
    /// el umbral necesario para subir de nivel (xpParaSiguienteNivel).
    /// Si es así, subir TODOS los niveles que correspondan de golpe
    /// (puede pasar que una gran ganancia de XP suba varios niveles).
    /// 
    /// ACCIONES AL SUBIR DE NIVEL (cada vez):
    /// 1. Incrementar level en 1.
    /// 2. Incrementar vidaMax en 2.
    /// 3. Curar un 5% de la vida máxima sin superar vidaMax
    /// (recuerda usar f por que son floats ej. 25f).
    /// 4. Recalcular el nuevo umbral: xpParaSiguienteNivel =
    ///    CalcularXPNecesariaParaNivel(nivelActual).
    /// 
    /// VARIABLES GLOBALES QUE PUEDES LEER Y MODIFICAR:
    ///   • float level            — nivel actual
    ///   • float XP               — experiencia total acumulada
    ///   • float vidaMax          — vida máxima
    ///   • float vida             — vida actual
    ///   • float xpParaSiguienteNivel — umbral para el siguiente nivel
    /// 
    /// HERRAMIENTAS A TU DISPOSICIÓN:
    ///   • Bucle que se repite MIENTRAS se cumpla una condición
    ///   • Llamada a tu función: CalcularXPNecesariaParaNivel((int)level)
    ///   • Asignaciones y aritmética básica
    ///   • Conversión (int)level para pasar float a int en la llamada
    /// 
    /// EJEMPLO DE FLUJO:
    ///   Situación: level=1, XP=25, xpParaSiguienteNivel=20
    ///   → XP ≥ 20, subir a level=2, vidaMax+2, vida=vidaMax
    ///   → Nuevo umbral = CalcularXPNecesariaParaNivel(2) = 30
    ///   → XP=25 < 30, terminar
    /// 
    /// NOTA: Esta función NO da experiencia, solo comprueba y aplica
    /// las subidas de nivel que correspondan en ese momento.
    /// </summary>
    void ComprobarSubidaNivel()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        while (XP >= xpParaSiguienteNivel)
        {
            level = level + 1;
            vidaMax = vidaMax + 2;
            vida += 0.05f * vidaMax;
            
            if (vida > vidaMax) {
                vida = vidaMax;
            }
            xpParaSiguienteNivel = CalcularXPNecesariaParaNivel((int)level);
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // ┌─────────────────────────────────────────────────────────┐
    // │  EJERCICIO 7: PROCESAR GANANCIA DE XP (ORQUESTADOR)     │
    // └─────────────────────────────────────────────────────────┘
    /// <summary>
    /// EJERCICIO 7 — ProcesarGananciaXP
    /// 
    /// OBJETIVO:
    /// Esta es la función principal que el juego llama cuando el jugador
    /// derrota a un enemigo. Orquesta todo el proceso de ganancia de
    /// experiencia: calcular lo que realmente se gana, sumarlo al total,
    /// y comprobar si hay subida de nivel.
    /// 
    /// FLUJO COMPLETO:
    /// 1. Calcula la XP final real llamando a CalcularXPGanada(xpBase).
    /// 2. Suma esa XP final al total acumulado (variable global XP).
    /// 3. Llama a ComprobarSubidaNivel() para procesar subidas si toca.
    /// 4. Devuelve la XP final ganada (útil para mostrar en HUD/logs).
    /// 
    /// VARIABLES DISPONIBLES:
    ///   • float xpBase — XP base del enemigo derrotado (parámetro)
    ///   • float XP   — experiencia total del jugador (variable global, se modifica)
    /// 
    /// HERRAMIENTAS A TU DISPOSICIÓN:
    ///   • Llamadas a tus propias funciones: CalcularXPGanada, ComprobarSubidaNivel
    ///   • Suma y asignación: XP = XP + cantidad
    ///   • Return de un valor calculado
    ///   • Variables locales para guardar resultados intermedios
    /// 
    /// EJEMPLO:
    ///   Enemigo base 10 XP, multiplicador actual 2.5
    ///   → CalcularXPGanada(10) devuelve 25
    ///   → XP = XP + 25
    ///   → ComprobarSubidaNivel() procesa niveles si XP ≥ umbral
    ///   → Devuelve 25
    /// </summary>
    /// <param name="xpBase">XP base del enemigo derrotado</param>
    /// <returns>XP real ganada (tras multiplicadores)</returns>
    float ProcesarGananciaXP(float xpBase)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        float xpFinal = CalcularXPGanada(xpBase);
        XP = XP + xpFinal;
        ComprobarSubidaNivel();
        return xpFinal;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // ┌─────────────────────────────────────────────────────────┐
    // │  EJERCICIO 8: ¿ESTÁ MUERTO?                             │
    // └─────────────────────────────────────────────────────────┘
    /// <summary>
    /// EJERCICIO 8 — EstaMuerto
    /// 
    /// OBJETIVO:
    /// Función de consulta simple: devolver true si la vida del jugador
    /// es 0 o menos, false en caso contrario.
    /// 
    /// VARIABLES DISPONIBLES:
    ///   • float vida — vida actual del jugador (variable global, solo lectura)
    bool EstaMuerto()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        return vida <= 0f;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // ┌─────────────────────────────────────────────────────────┐
    // │  EJERCICIO 9: ACTUALIZAR ESTADO DE MUERTE (UPDATE)     │
    // └─────────────────────────────────────────────────────────┘
    /// <summary>
    /// EJERCICIO 9 — ActualizarEstadoMuerte
    /// 
    /// OBJETIVO:
    /// Esta función se llama CADA FRAME (desde Update). Su misión es
    /// detectar el momento exacto en que el jugador PASA de estar vivo
    /// a estar muerto (la transición), y en ese instante ejecutar
    /// RegistrarMuerte() una sola vez.
    /// 
    /// VARIABLES GLOBALES:
    /// • bool isDead — Es estado anterior de donde indica si el jugador 
    ///  estaba muerto o no. Se lee y se tiene que actualizar.
    /// 
    /// LÓGICA DE TRANSICIÓN:
    /// - Usa EstaMuerto() para saber el estado ACTUAL (ahora mismo).
    /// - Usa la variable global 'isDead' para recordar el estado ANTERIOR
    ///   (en el frame previo).
    /// - CASO 1: El estado ACTUAL indica que esta muerto y el estado ANTERIOR dice que no estaba muerto
    ///     → ACABA de morir ESTE frame
    ///     → Pon isDead = true
    ///     → Llama a RegistrarMuerte()
    /// - CASO 2: El estado ACTUAL indica que esta vivo, sin importar el estado ANTERIOR
    ///     → Está vivo (o ha respawned)
    ///     → Pon isDead = false (reset para próxima muerte)
    /// - CASO 3: El estado ACTUAL indica que esta muerto y el estado ANTERIOR también lo indicaba
    ///     → Ya estaba muerto, no hacer nada
    /// 
    /// HERRAMIENTAS A TU DISPOSICIÓN:
    ///   • Llamada a tu función: EstaMuerto()
    ///   • Estructuras: if, else if, else
    ///   • Operadores lógicos: && (y), ! (no)
    ///   • Asignación de booleanos
    ///   • Llamada a tu función: RegistrarMuerte()
    /// 
    /// PISTA: Piensa en los tres casos mutuamente excluyentes y
    /// escríbelos en orden lógico.
    /// </summary>
    void ActualizarEstadoMuerte()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        bool muertoAhora = EstaMuerto();

        if (muertoAhora && !isDead)
        {
            isDead = true;
            RegistrarMuerte();
        }
        else if (!muertoAhora)
        {
            isDead = false;
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // ══════════════════════════════════════════════════════════════
    // PARTE 3 — TRABAJO SUCIO UNITY/C# (NO TOCAR — YA HECHO)
    // ══════════════════════════════════════════════════════════════
    // Aquí está todo lo que usa APIs de Unity: Update, Start, Clamp,
    // FindAnyObjectByType, GameManager, EnemyManager, etc.
    // EL ALUMNO NO VE ESTA PARTE. SOLO LLAMA A SUS FUNCIONES DE ARRIBA.
    // ══════════════════════════════════════════════════════════════

    void Awake()
    {
        // Auto-buscar referencias si no están en Inspector
        if (hud == null)
            hud = FindAnyObjectByType<PlayerHUDController>();
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        // Inicializar umbral del primer nivel (nivel 1 → siguiente primo de 1 = 2 → 20 XP)
        xpParaSiguienteNivel = CalcularXPNecesariaParaNivel(level);
    }

    void Start()
    {
        // Asegurar vida inicial coherente
        ClampearVida();
        
        // Sincronizar HUD si existe
        if (hud != null)
        {
            // La HUD leerá nuestras variables públicas en su Update
        }
    }

    void Update()
    {
        // 1. Clampear vida siempre (seguridad)
        ClampearVida();

        // 2. Detectar muerte (llama al ejercicio del alumno)
        ActualizarEstadoMuerte();

        // 3. Actualizar HUD (la HUD se encarga sola de leer level, XP, vida...)
    }

    // ══════════════════════════════════════════════════════════════
    // API PÚBLICA — LA QUE USAN OTROS SCRIPTS (EnemyHitbox, Potion, GameManager...)
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Recibe daño directo. Lo llaman los hitboxes de enemigos.
    /// </summary>
    public void TakeDamage(float cantidad)
    {
        vida -= cantidad;
        ClampearVida();
        // Debug interno si hace falta
    }

    /// <summary>
    /// Recibe daño (alias compatibilidad código antiguo).
    /// </summary>
    public void RecibirDanio(float cantidad)
    {
        TakeDamage(cantidad);
    }

    /// <summary>
    /// Cura al jugador. Lo llaman pociones, LifeSteal, LifeStone...
    /// </summary>
    public void Curar(float cantidad)
    {
        vida += cantidad;
        ClampearVida();
    }

    /// <summary>
    /// Ganar XP por matar enemigo. Lo llama GameManager / EnemyManager.
    /// xpBase = valor base del enemigo (ej: 5, 10, 15...).
    /// Devuelve el XP real ganada (tras multiplicadores) para HUD/logs.
    /// </summary>
    public float GanarXP(float xpBase)
    {
        float xpFinal = ProcesarGananciaXP(xpBase);
        return xpFinal;
    }

    /// <summary>
    /// Lootbox drop. Lo llama GameManager al matar enemigo.
    /// </summary>
    public void Lootbox()
    {
        if (hud != null)
            hud.GrantLoot();
    }

    /// <summary>
    /// Resetear flag de muerte para nuevo intento (lo llama GameManager al respawn).
    /// </summary>
    public void ResetDeath()
    {
        isDead = false;
        muerteYaNotificada = false;
        vida = vidaMax;  // Vida completa al respawn
    }

    /// <summary>
    /// Info para HUD/debug: XP necesaria para siguiente nivel.
    /// </summary>
    public float GetXPParaSiguienteNivel() => xpParaSiguienteNivel;

    /// <summary>
    /// Info para HUD: multiplicador actual escena/ronda.
    /// </summary>
    public float GetMultiplicadorActual() => ObtenerMultiplicadorEscenaRonda();
}