---
name: tic1-normativas-encapsulamiento
version: 1
description: >
  Skill de normativas de generación de código y filosofía de encapsulamiento
  para el espacio TIC I Andalucía. Define cómo se generan los snippets C++,
  qué partes del proyecto Unity están encapsuladas (el alumno NUNCA las ve ni
  las toca), qué partes son "islas editables" del alumno, y las reglas exactas
  de estilo, estructura y abstracción que todo código de respuesta debe cumplir.
  Basada en los scripts reales del proyecto: SimpleWalk, RapierHitbox,
  LightSaberHitbox, KickCollider, Damageable, DamageTypes, PlayerHealth,
  ChangeColors, ChangeType y Potion (Unity 6000.4.5f1).
agents: [main_agent, general_purpose]
---

# Skill: Normativas de Código y Filosofía de Encapsulamiento

## Para qué sirve esta skill

Esta skill le dice al modelo exactamente:
1. Qué partes del proyecto Unity están **blindadas** (el alumno nunca las ve).
2. Qué partes son **islas editables** (el alumno solo toca esas zonas).
3. Cómo se **genera todo el código** que se le muestra al alumno.
4. Las **reglas de estilo** que nunca se rompen.

---

## Filosofía de Encapsulamiento

### La analogía del coche

El proyecto Unity funciona como un coche de carreras ya montado.
El alumno solo maneja el volante (las islas de código).
El motor, la caja de cambios, el chasis: ya están hechos y el alumno
no necesita abrirlos para aprender a conducir.

El profesor ha construido toda la complejidad de Unity por dentro.
El alumno ve solo funciones con nombre claro en C++ básico.

### Tres capas del proyecto

```
CAPA 1 — MOTOR UNITY (INVISIBLE para el alumno)
  Animator, CharacterController, Collider, Rigidbody,
  MonoBehaviour, Update(), Start(), Awake(), OnTriggerEnter(),
  GetComponent(), transform, Vector3, Quaternion, Coroutines,
  HashSet, UnityEvent, Material instancing, Time.deltaTime

CAPA 2 — SCRIPTS ENCAPSULADOS (el alumno lee el nombre, no el cuerpo)
  SimpleWalk.cs        → mueve el personaje y dispara animaciones
  RapierHitbox.cs      → detecta golpes del rapier y aplica tipo de daño
  LightSaberHitbox.cs  → detecta golpes del sable y aplica color de daño
  KickCollider.cs      → detecta golpes de patada
  Damageable.cs        → gestiona vida del enemigo, veneno, sangrado
  DamageTypes.cs       → enum con los tipos: Red, Blue, Gray, Normal, Poison, Bleed
  PlayerHealth.cs      → vida del jugador, RecibirDanio(), Curar()
  ChangeColors.cs      → cambia el color visual del sable láser (teclas 1/2/3)
  ChangeType.cs        → cambia el modo del rapier (teclas 4/5/6)
  Potion.cs            → poción que cura al jugador al tocarse

CAPA 3 — ISLAS EDITABLES (el alumno SOLO toca estas zonas)
  Los switch/case de cambio de tipo de arma
  Los if/else de detección de teclas para ataques
  Las funciones de mecánicas: AplicarVeneno(), AplicarSangrado(), Curar()
  El struct del personaje y sus variables
  El sistema de XP: SumarXP(), ComprobarSubidaNivel()
  La lógica del boss: ComprobarAturdimiento(), ComprobarConfusion()
  Las condiciones de vulnerabilidad: EsColorVulnerable(), EsEtiquetaPermitida()
```

---

## Los Scripts Reales — Qué Hace Cada Uno

Esta sección es para que el modelo sepa exactamente qué existe ya en el proyecto
y lo abstrae correctamente cuando lo explica al alumno.

### SimpleWalk.cs (playerscript.cs)
```
QUÉ HACE (real): MonoBehaviour con CharacterController.
  - Update() lee WASD y mueve al personaje en espacio mundo.
  - Teclas de ataque: Mouse0=RaSwing, Mouse1=SwSwing, Q=Ra360,
    E=Sw360, K=Chut, M=Martelo, B=DanceSpin
  - Cada ataque lanza una Coroutine que dispara el trigger
    del Animator y luego espera 1.3 segundos.
  - Métodos públicos Enable/DisableXHitbox() activados por
    Animation Events desde los clips de Mixamo.

CÓMO LO EXPLICA EL MODELO AL ALUMNO:
  "El bloque principal del juego. Piensa en él como el árbitro
   del partido: escucha lo que pulsas y le dice al personaje
   qué hacer. Tú no tocas esto."

ABSTRACCIÓN DE SUS FUNCIONES:
  Update()                → SimularTurno() en C++ básico
  Start()                 → IniciarJuego()
  StartCoroutine(X())     → "espera N turnos y luego hace X"
  EnableRapierHitbox()    → ActivarZonaDeGolpeRapier()
  DisableRapierHitbox()   → DesactivarZonaDeGolpeRapier()
```

### RapierHitbox.cs
```
QUÉ HACE (real): Detecta OnTriggerEnter. Comprueba:
  1. EsGolpePropio → ignora si es el propio jugador
  2. BuscarVidaEnEnemigo → busca Damageable en el objeto tocado
  3. YaFueGolpeadoEnEsteSwing → evita doble hit (HashSet)
  4. CanBeDamagedBy(damageType, "Rapier") → filtra vulnerabilidad
  5. TakeDamage(damage, damageType, source) → aplica el daño

TECLAS: Update() detecta 4/5/6 → AplicarModoNormal/Veneno/Sangrado()
  Cada modo cambia damageType y tinta el material de la hoja.

CÓMO LO EXPLICA EL MODELO AL ALUMNO:
  "Es la zona invisible alrededor de la hoja. Cuando toca a un
   enemigo, comprueba si ese enemigo es vulnerable al tipo de
   daño activo. Si sí, le quita vida. Tú no tocas esto."

ABSTRACCIÓN:
  OnTriggerEnter()  → ComprobarColision()
  HashSet           → "memoria de golpes del swing actual"
  damageType        → "el modo activo: Normal, Veneno o Sangrado"
```

### ChangeType.cs (RapierColorController)
```
QUÉ HACE (real): Detecta teclas 4/5/6 en Update().
  - EstablecerModo(tipo) cambia currentDamageType,
    aplica color al material instanciado y notifica a RapierHitbox.
  - Separación de responsabilidades: este script maneja INPUT+COLOR,
    RapierHitbox maneja COLISIONES.

ISLA EDITABLE PARA EL ALUMNO:
  El switch interno ElegirColorSegunModo() es exactamente el tipo de
  ejercicio que el alumno hace en Escena 3 y 5.
  El alumno practica el mismo patrón en C++ básico, luego el profesor
  muestra cómo ese patrón existe ya en el proyecto real.

ABSTRACCIÓN:
  EstablecerModo() → CambiarTipoRapier(int modo)
  Input.GetKeyDown → "si pulsas la tecla X..."
```

### ChangeColors.cs (LightsaberColorController)
```
QUÉ HACE (real): Mismo patrón que ChangeType pero para el sable.
  Teclas 1/2/3 → SetSaberColor(DamageType.Red/Blue/Gray)
  Cambia _BaseColor del material instanciado.
  Notifica a WeaponHitbox el nuevo damageType.

ABSTRACCIÓN:
  SetSaberColor(tipo) → CambiarColorSable(int color)
```

### Damageable.cs
```
QUÉ HACE (real): Vida de enemigos + efectos de estado.
  - maxHealth / currentHealth (int)
  - vulnerableTypes[]: DamageType[] — si vacío, vulnerable a todo
  - allowedWeaponTags[]: string[] — si vacío, cualquier arma daña
  - TakeDamage(amount, damageType, source): aplica daño y efectos
  - CanBeDamagedBy(type, weaponTag): bool — comprueba filtros
  - IsVulnerableToColor(incoming): bool — recorre vulnerableTypes[]
  - IsAllowedWeaponTag(tag): bool — recorre allowedWeaponTags[]
  - Efectos: Poison (-0.5 vida/seg, 6 seg), Bleed (al 5.º golpe, +3)
  - Cooldown entre golpes: 0.25 seg de invencibilidad
  - Die(): Destroy(gameObject) cuando currentHealth <= 0

ISLAS EDITABLES EQUIVALENTES EN C++:
  IsVulnerableToColor() → EsColorVulnerable() en C++ básico con if/else
  IsAllowedWeaponTag()  → EsEtiquetaPermitida() en C++ básico con if/else
  Lógica de veneno      → AplicarVeneno() con while/for y contador
  Lógica de sangrado    → AplicarSangrado() con contador de golpes

ABSTRACCIÓN:
  TakeDamage()      → RecibirGolpe(int cantidad)
  CanBeDamagedBy()  → PuedeSerGolpeado(string tipoAtaque)
  Destroy()         → ElEnemigoCaeCuandoVidaEsCero()
```

### DamageTypes.cs
```
QUÉ HACE (real): Enum público con 6 valores:
  Red, Blue, Gray    → tipos del LightSaber (color)
  Normal             → Rapier sin efecto
  Poison             → Veneno: -0.5 vida/seg durante 6 seg
  Bleed              → Sangrado: al 5.º golpe, +3 daño extra

ABSTRACCIÓN PARA EL ALUMNO:
  En C++ básico se simula con constantes o con un int:
      const int TIPO_NORMAL   = 0;
      const int TIPO_VENENO   = 1;
      const int TIPO_SANGRADO = 2;
      const int COLOR_ROJO    = 3;
      const int COLOR_AZUL    = 4;
      const int COLOR_GRIS    = 5;
```

### PlayerHealth.cs
```
QUÉ HACE (real): float vida = 10, vidaMax = 20.
  Update() hace Mathf.Clamp(vida, 0, vidaMax) cada frame.
  RecibirDanio(float): resta cantidad y loguea.
  Curar(float): suma cantidad y loguea.

ISLA EDITABLE EQUIVALENTE:
  void RecibirDanio(int cantidad) con if(vida < 0) vida = 0;
  void Curar(int cantidad) con if(vida > vidaMaxima) vida = vidaMaxima;
```

### Potion.cs
```
QUÉ HACE (real): OnTriggerEnter detecta al jugador.
  Cura 1 punto/segundo durante 5 segundos (sin coroutine).
  Usa Time.deltaTime + acumulador. Al terminar, Destroy().

ABSTRACCIÓN:
  ProcesarCuracionPorTiempo() → en C++ básico: bucle for de 5 turnos
  que llama a Curar(1) una vez por turno.
```

### KickCollider.cs / LightSaberHitbox.cs
```
QUÉ HACE (real): Mismo patrón que RapierHitbox pero sin cambio
  de modo. Solo detectan colisión, comprueban CanBeDamagedBy
  y llaman a TakeDamage.

ABSTRACCIÓN:
  Mismas funciones: ComprobarColision(), PuedeSerGolpeado(), RecibirGolpe()
```

---

## Reglas de Generación de Código

### Regla G1 — Estructura fija de cada snippet

```
1. Comentario de cabecera: qué hace este bloque
2. #include <iostream> (solo si se imprime algo)
3. Constantes globales (const int, const float)
4. Structs de datos (si hacen falta, sin métodos)
5. Variables globales del juego (pocas, bien comentadas)
6. Firmas de funciones Debug: SOLO declaración, SIN cuerpo
7. Funciones de lógica: CON cuerpo completo y comentado
8. main() o SimularJuego() que lo llama todo en orden
```

### Regla G2 — Firmas Debug, NUNCA el cuerpo

Las siguientes funciones SOLO aparecen como declaración.
El alumno las llama pero nunca ve su interior.

```cpp
// Estas funciones ya están hechas. Solo tienes que llamarlas.
void DebugVida();           // Muestra la vida actual en pantalla
void DebugVeneno();         // Muestra si hay veneno y cuántos turnos
void DebugSangrado();       // Muestra el estado del sangrado
void DebugEstado();         // Muestra todos los datos del personaje
void DebugXP();             // Muestra la experiencia acumulada
void DebugEscena();         // Muestra en qué escena estamos
void DebugTipoRapier();     // Muestra el modo activo del rapier
void DebugColorSable();     // Muestra el color activo del sable
void DebugBoss();           // Muestra el estado del boss (Escena 6)
```

Nunca, bajo ningún concepto, aparece el bloque { ... } de estas funciones.

### Regla G3 — Comentario obligatorio en líneas no triviales

```cpp
// BIEN:
int vida = 100;              // La vida empieza en 100 puntos
bool envenenado = false;     // false = sin veneno activo
int turnosVeneno = 0;        // Cuántos turnos le quedan al veneno

// MAL (sin comentar):
int vida = 100;
bool envenenado = false;
int turnosVeneno = 0;
```

Todo operador compuesto que un principiante pueda no entender lleva comentario:
```cpp
vida = vida - danio;         // Le quitamos el daño a la vida
turnosVeneno = turnosVeneno - 1;  // Un turno menos de veneno
```

### Regla G4 — TODOs con formato exacto

```cpp
// TODO: ¿Cuántos puntos de daño hace el veneno cada turno?
//       Prueba empezando por 5. Ponlo en vez del 0.
int danioVeneno = 0;         // <- cambia este número

// TODO: ¿Cuántos turnos dura el sangrado? Ponlo aquí.
int turnosSangrado = 0;      // <- ponlo tú
```

Regla: el TODO siempre tiene:
- Línea 1: qué tiene que poner el alumno
- Línea 2 (opcional): sugerencia concreta ("prueba con X")
- La variable con valor 0/false/vacío que el alumno cambia

### Regla G5 — Equivalencias de abstracción obligatorias

Cuando algo de Unity aparezca en la explicación, SIEMPRE se mapea así:

| Término Unity/C# | Término C++ básico para el alumno |
|---|---|
| Update() cada frame | SimularTurno() — se llama una vez por turno |
| Start() | IniciarJuego() — se ejecuta una sola vez al inicio |
| OnTriggerEnter() | ComprobarColision() — detecta si hay impacto |
| Destroy(gameObject) | ElEnemigoCaeCuandoVidaEsCero() |
| GetComponent<T>() | BuscarComponente() — busca algo en el objeto |
| MonoBehaviour | El bloque principal del juego |
| GameObject | struct Personaje (ficha de datos) |
| Animator.SetTrigger() | DisparaAnimacion("nombre") |
| Coroutine | "una función que espera N turnos antes de continuar" |
| Time.deltaTime | dt — el tiempo que pasó desde el último turno |
| Debug.Log() | DebugX() — ya hecha, solo la llamas |
| HashSet<T> | "memoria de golpes: recuerda a quién ya golpeaste" |
| UnityEvent | "cuando pasa X, automáticamente se llama Y" |
| [Header("...")] | (ignorar, es decoración del inspector) |
| [SerializeField] | (ignorar, es conexión del inspector) |
| Math.Clamp(v,min,max) | if(v<min) v=min; if(v>max) v=max; |

### Regla G6 — Mecánicas reales en C++ básico

Estas son las implementaciones de referencia canónicas.
El modelo SIEMPRE genera algo coherente con estas plantillas.

**Veneno (Poison) — en el proyecto real: -0.5 vida/seg durante 6 seg:**
```cpp
// En C++ básico lo simulamos como: -1 vida por turno durante 3 turnos
void AplicarVeneno()
{
    if (envenenado == false)    // Si no hay veneno, no hacemos nada
        return;

    vida = vida - danioVeneno;  // El veneno quita vida este turno
    turnosVeneno = turnosVeneno - 1;  // Un turno menos de veneno

    if (turnosVeneno <= 0)      // Si ya no quedan turnos de veneno...
    {
        envenenado = false;     // El veneno desaparece
        turnosVeneno = 0;       // Nos aseguramos de que quede en cero
    }
}
```

**Sangrado (Bleed) — en el proyecto real: al 5.º golpe inflige 3 de daño extra:**
```cpp
// En C++ básico: contamos golpes. Al llegar a 5, daño extra.
void RegistrarGolpeSangrado()
{
    if (sangrando == false)     // Si no hay sangrado, no contamos
        return;

    golpesSangrado = golpesSangrado + 1;  // Un golpe más de sangrado

    if (golpesSangrado >= 5)    // Si llegamos al 5.º golpe...
    {
        vida = vida - danioExtraSangrado; // Daño extra del sangrado
        golpesSangrado = 0;               // Reiniciamos el contador
    }
}
```

**Curación (Potion) — en el proyecto real: +1 vida/seg durante 5 seg:**
```cpp
void Curar(int cantidad)
{
    vida = vida + cantidad;         // Sumamos la curación

    if (vida > vidaMaxima)          // Si nos pasamos del máximo...
        vida = vidaMaxima;          // Nos quedamos justo en el máximo
}
```

**Comprobación de muerte:**
```cpp
void ComprobarMuerte()
{
    if (vida <= 0)          // Si la vida llega a cero o menos...
    {
        vida = 0;           // La vida nunca baja de cero (por si acaso)
        vivo = false;       // El personaje está derrotado
    }
}
```

**EsColorVulnerable — en el proyecto real: IsVulnerableToColor():**
```cpp
// Comprueba si el enemigo es débil al tipo de daño del arma.
// En el proyecto real esto lo hace Damageable.cs automáticamente.
// Aquí lo practicamos a mano para aprender los if/else.
bool EsColorVulnerable(int colorArma, int colorEnemigo)
{
    if (colorArma == colorEnemigo)  // Si el color del arma coincide...
        return true;                // El enemigo es vulnerable

    return false;                   // Si no coincide, no le daña
}
```

**EsEtiquetaPermitida — en el proyecto real: IsAllowedWeaponTag():**
```cpp
// Comprueba si el arma puede dañar a este enemigo.
// Algunos enemigos solo pueden ser dañados con el rapier o con patadas.
bool EsEtiquetaPermitida(string etiquetaArma, string etiquetaEnemigo)
{
    if (etiquetaArma == etiquetaEnemigo)  // Si es el arma correcta...
        return true;

    return false;
}
```

**Sistema de XP (Escena 4):**
```cpp
// Constantes de XP por tipo de enemigo
const int XP_ALIEN    = 1;      // Los aliens de juguete dan poca XP
const int XP_MOUSEY   = 5;      // Los peluches dan XP media
const int XP_ALONSO   = 10;     // Fernando Alonso da mucha XP
const int XP_OTROS    = 15;     // Cualquier otro enemigo da más aún

void SumarXP(string tipoEnemigo)
{
    if (tipoEnemigo == "Alien")
        xpActual = xpActual + XP_ALIEN;
    else if (tipoEnemigo == "Mousey")
        xpActual = xpActual + XP_MOUSEY;
    else if (tipoEnemigo == "Alonso")
        xpActual = xpActual + XP_ALONSO;
    else
        xpActual = xpActual + XP_OTROS;    // Cualquier otro tipo

    DebugXP();   // Mostramos la XP actualizada
}
```

**Struct canónico del personaje:**
```cpp
// Un struct es como una ficha de personaje de un juego de mesa:
// agrupa todos sus datos en un solo sitio
struct Personaje
{
    string nombre;           // El nombre del personaje
    int vida;                // Su vida actual
    int vidaMaxima;          // La vida máxima que puede tener
    bool vivo;               // true = vivo, false = derrotado

    // Efectos de estado
    bool envenenado;         // true = tiene veneno activo
    int turnosVeneno;        // Cuántos turnos dura el veneno
    int danioVeneno;         // Cuánto daño hace por turno

    bool sangrando;          // true = está sangrando
    int golpesSangrado;      // Contador de golpes de sangrado
    int danioExtraSangrado;  // Daño extra al llegar al 5.º golpe

    // Progresión
    int xpActual;            // Experiencia acumulada
    int nivel;               // Nivel actual del personaje
};
```

### Regla G7 — Formato de respuesta siempre en 5 secciones

```
### ¿Qué va a hacer este código?
[3-5 líneas en palabras de niño. Analogía si ayuda.
 Conectar con lo que el alumno ya vio en escenas anteriores.]

### Los datos que necesitamos
[Struct y/o variables globales. TODOS comentados.
 TODOs marcados para los valores que el alumno debe poner.]

### Las funciones
[Una función por bloque lógico. Cabecera comentada.
 Cuerpo completo con comentarios en cada línea no trivial.
 Máximo 3 funciones nuevas por respuesta.]

### Cómo se usa todo junto
[SimularJuego() o main() que llama a todo en orden.
 Con DebugX() en los puntos clave para que el alumno
 pueda comprobar que funciona.]

### Tu turno — cosas que puedes cambiar
[Lista numerada de TODOs. Cada uno con:
 - Número de línea o nombre de función donde está
 - Qué tiene que hacer el alumno
 - Sugerencia de valor para empezar]
```

### Regla G8 — Límite de conceptos nuevos por respuesta

Máximo 3 conceptos nuevos por respuesta. Si la pregunta requiere más,
dividir en pasos y avisar al alumno: "Hoy vemos solo esto. La semana
que viene vemos el siguiente paso."

Orden de introducción de conceptos por escena:

```
Escena 2: if/else simples, variables bool e int básicas
Escena 3: switch/case, structs básicos, funciones con parámetros
Escena 4: funciones que devuelven bool, if/else anidados, XP
Escena 5: while con contador, combinación if+while+switch
Escena 6: condiciones compuestas (&&, ||), orden de cláusulas,
           lógica de estados del boss (ninguno→aturdido→confundido)
```

### Regla G9 — Lo que NUNCA debe aparecer en el código

```
❌ class NombreClase { ... }
❌ *puntero, &referencia, operador ->
❌ std::vector<>, std::map<>, HashSet<>, List<>
❌ template<typename T>
❌ auto variable = ...
❌ [] () { }  (lambdas)
❌ #include <vector>, #include <map>
❌ Console.WriteLine(), Debug.Log() visible
❌ GetComponent(), transform., gameObject., Destroy()
❌ new NombreClase(), delete puntero
❌ using namespace std; (aunque sea inofensivo, confunde)
❌ Cualquier tipo de Unity: Vector3, Color, Collider, Rigidbody
❌ Código sin comentar
```

### Regla G10 — Analogías recomendadas por concepto

```
struct           → "ficha de personaje de un juego de mesa"
variable global  → "pizarra del juego que todos pueden ver y cambiar"
función          → "botón mágico: le pones nombre, lo pulsas, hace su trabajo"
parámetro        → "información que le das al botón para que sepa qué hacer"
bool             → "interruptor de luz: encendido (true) o apagado (false)"
if/else          → "si tienes escudo el daño se reduce; si no, te lo llevas entero"
switch/case      → "un panel de botones: según cuál pulses, pasa una cosa distinta"
for              → "haz esto exactamente N veces seguidas"
while            → "sigue haciendo esto MIENTRAS se cumpla la condición"
do-while         → "haz esto al menos una vez, y repite mientras se cumpla"
SimularTurno()   → "el tic-tac del reloj del juego: cada vez que suena, el mundo avanza"
IniciarJuego()   → "el disparo de salida de la carrera: solo suena una vez"
ComprobarColision() → "el árbitro que mira si dos cosas se han tocado"
DebugX()         → "la pantalla del árbitro que te muestra lo que está pasando"
```

---

## Conexión entre el Código C++ del Alumno y los Scripts Reales

Esta sección explica al modelo cómo mapear lo que el alumno practica en C++
con lo que existe en el proyecto Unity real, para que el modelo pueda decir
"lo que acabas de hacer en C++ es exactamente lo que hace ChangeType.cs
en el proyecto real, solo que Unity lo envuelve con sus herramientas".

| Lo que practica el alumno en C++ | Script real equivalente | Función real equivalente |
|---|---|---|
| switch con teclas 4/5/6 | ChangeType.cs | DetectarCambioDeModo() |
| switch con teclas 1/2/3 | ChangeColors.cs | SetSaberColor() |
| if/else para ataques K/M | SimpleWalk.cs | Update() parte de patadas |
| EsColorVulnerable() | Damageable.cs | IsVulnerableToColor() |
| EsEtiquetaPermitida() | Damageable.cs | IsAllowedWeaponTag() |
| AplicarVeneno() con contador | Damageable.cs | bloque Poison en TakeDamage() |
| AplicarSangrado() con contador | Damageable.cs | bloque Bleed en TakeDamage() |
| Curar() con tope de vida | Potion.cs | CurarUnPunto() |
| SumarXP() con if/else | (por hacer) | EnemyXP.cs (pendiente) |
| ComprobarAturdimiento() | (por hacer) | BossController.cs (pendiente) |
| struct Personaje | Damageable.cs | campos maxHealth, vulnerableTypes |
| ComprobarMuerte() | Damageable.cs | Die() |

---

## Nivel de Dificultad por Escena

El modelo adapta la complejidad del código al nivel esperado en cada escena.

| Escena | Dificultad | Conceptos en juego |
|---|---|---|
| 1 | Exploración libre | Cualquier cosa, sin restricciones |
| 2 | Básico | if/else, variables int/bool, teclas simples |
| 3 | Básico-medio | switch/case, struct, funciones sin retorno |
| 4 | Medio | Funciones con retorno bool, if/else anidados, XP |
| 5 | Medio-avanzado | while con contador, combinación estructuras |
| 6 | Avanzado | Condiciones compuestas, lógica de estados, orden correcto de cláusulas |
