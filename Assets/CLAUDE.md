'''# CLAUDE.md — Aprender-Jugando-Unity-TFM-2026
> Rama activa: **NORAGDOLLS** | Motor: Unity 6000.4.5f1 | Lenguaje de enseñanza: C++ básico

---

## ¿Qué es este proyecto?

TFM de educación gamificada para alumnos de **TIC I (1.º Bachillerato, Andalucía)**.
El profesor monta el proyecto Unity; el alumno SOLO rellena funciones concretas
dentro de los scripts, escritas y explicadas en **C++ básico pedagógico**.

El videojuego es un ninja con rapier, sable láser y patadas.
Hay 6 escenas de historia + sala de pruebas, cada una enseña un concepto nuevo de programación.

---

## Arquitectura de scripts (NO TOCAR sin permiso)

| Script | Responsabilidad |
|---|---|
| `SimpleWalk.cs` | Movimiento, teclas, animaciones |
| `RapierHitbox.cs` | Hitbox rapier, cambio de modo |
| `LightSaberHitbox.cs` | Hitbox sable, cambio de color |
| `KickCollider.cs` | Hitbox patadas |
| `Damageable.cs` | Vida enemigo, veneno, sangrado, muerte |
| `DamageTypes.cs` | Enum: Red, Blue, Gray, Normal, Poison, Bleed |
| `PlayerHealth.cs` | Vida jugador, recibir daño, curar |
| `ChangeColors.cs` | Color visual sable láser |
| `ChangeType.cs` | Tipo de daño rapier/sable |
| `Potion.cs` | Curación al jugador |

**Regla crítica**: Claude nunca modifica estos scripts salvo que el profesor lo pida explícitamente.
El alumno SOLO toca los bloques marcados como `// TODO:` en los scripts de ejercicio.

---

## Mecánicas del juego (referencia rápida)

### RAPIER (clic izq = swing, Q = media vuelta)
- Tecla 4 → Modo Normal (daño directo)
- Tecla 5 → Modo Veneno (0.5 vida/seg, 6 segundos)
- Tecla 6 → Modo Sangrado (al 5.º golpe: +3 daño)

### LIGHTSABER (clic der = swing, E = media vuelta)
- Tecla 1 → Color Rojo (daño Red)
- Tecla 2 → Color Azul (daño Blue)
- Tecla 3 → Color Gris (daño Gray)

### PATADAS
- Tecla K → Chut (frontal)
- Tecla M → Martelo (alta)

---

## Escenas y conceptos enseñados

| Escena | Enemigo | Concepto pedagógico |
|---|---|---|
| 1 | Sala de pruebas | Testing libre |
| 2 | Aliens de juguete | if/else básico, patadas |
| 3 | Peluches Mousey | switch espadas, struct personaje |
| 4 | Fernando Alonso | Sistema XP, EsColorVulnerable |
| 5 | Maniquís | Cambio tipo rapier, veneno, sangrado |
| 6 | Boss Dummy | Aturdimiento, confusión, lógica boss |

Cada escena: pantalla intro → batalla → pantalla outro.
El alumno no avanza hasta completar los ejercicios de código obligatorios.

---

## Reglas absolutas de generación de código

### SINTAXIS: C++ básico ÚNICAMENTE
El alumno solo conoce:
- Variables: `int`, `float`, `bool`, `string`
- Condicionales: `if`, `if/else`, `switch/case`
- Bucles: `for`, `while`, `do-while`
- Funciones con y sin retorno, parámetros de tipo básico
- Structs como fichas de datos (sin métodos internos)
- Operadores: `+`, `-`, `*`, `/`, `%`, `==`, `!=`, `<`, `>`, `<=`, `>=`, `&&`, `||`, `!`
- Constantes: `const int MAX_VIDA = 100;`

### PROHIBIDO siempre
- `class`, herencia, polimorfismo, interfaces
- Punteros (`*`), referencias (`&`), operador `->`
- `std::vector`, `std::map`, `HashSet`, `List`, contenedores
- Templates, lambdas, `auto`, `using`, `namespace`
- Sintaxis de C# o Unity nombrada directamente
- Corrutinas, eventos, delegados, callbacks
- Tipos de Unity: `Vector3`, `Color`, `Collider`, etc.
- `GetComponent`, `transform`, `Animator`, `CharacterController`

### Abstracción del motor (equivalencias obligatorias)
| Unity real | Lo que escribe el alumno |
|---|---|
| `Start()` | `IniciarJuego()` |
| `Update()` | `SimularTurno()` |
| `OnTriggerEnter()` | `ComprobarColision()` |
| `Debug.Log()` | `DebugX();` (solo firma, nunca cuerpo) |
| `GameObject` | `struct Personaje` |
| Coroutine | "función que espera N turnos" |

---

## Funciones Debug — SOLO SE MUESTRA LA FIRMA

```cpp
void DebugVida();          // Muestra la vida actual
void DebugVeneno();        // Muestra turnos de veneno restantes
void DebugSangrado();      // Muestra estado del sangrado
void DebugEstado();        // Muestra todos los datos del personaje
void DebugXP();            // Muestra la experiencia actual
void DebugEscena();        // Muestra la escena actual
```
**Nunca** incluir el cuerpo `{ ... }` de estas funciones en ninguna respuesta.

---

## Estructura obligatoria de cada snippet de código

1. `#include` y constantes globales
2. Structs de datos (si hacen falta)
3. Variables globales del juego (pocas, bien comentadas)
4. Firmas de funciones Debug (SIN cuerpo)
5. Funciones de lógica (CON cuerpo completo y comentado)
6. `SimularJuego()` o `main()` que lo llama todo

---

## Estilo de comentarios (obligatorio en cada línea no trivial)

```cpp
int vida = 100;            // La vida empieza en 100 puntos
bool envenenado = false;   // false = sin veneno activo
int turnosVeneno = 0;      // Cuántos turnos dura el veneno
```

---

## Formato TODO para el alumno

```cpp
// TODO: ¿Cuántos turnos dura el veneno? Ponlo aquí
int turnosVeneno = 0;      // <- cambia el 0
```

---

## Estructura obligatoria de cada respuesta

```
### ¿Qué va a hacer este código?
[3-5 líneas en palabras de niño, con analogía si ayuda]

### Los datos que necesitamos
[Struct o variables globales, todos comentados]

### Las funciones
[Una función por bloque, cabecera + cuerpo comentado]

### Cómo se usa todo junto
[SimularJuego() que llama a todo en orden]

### Tu turno — cosas que puedes cambiar
[Lista de TODOs que el alumno puede completar]
```

---

## Tono y nivel pedagógico

- Explica como a un niño de 12 años listo
- Máximo 3 conceptos nuevos por respuesta
- Usa analogías: struct = ficha de personaje de mesa, bool = interruptor de luz
- Respuestas cortas con ejemplos concretos
- Nunca mencionar C# o Unity directamente en el código

---

## Flujo de trabajo Git (ramas activas)

- Rama principal de trabajo: `NORAGDOLLS`
- Nunca mergear a main sin revisión del profesor
- Un commit por ejercicio completado
- Mensajes de commit en español, descriptivos: `feat: escena3 switch espadas completado`

---

## Errores frecuentes a evitar

| Error | Solución |
|---|---|
| Usar `List<>` o arrays de C# | Usar arrays estáticos `int arr[10]` |
| Escribir `Debug.Log()` con cuerpo | Solo declarar la firma |
| Usar `class` en vez de `struct` | Siempre `struct` sin métodos |
| Nombrar Unity directamente | Usar equivalencias de abstracción |
| Dar solución completa sin TODOs | Dejar siempre algo para el alumno |

---

## Contexto curricular

- Asignatura: TIC I, 1.º Bachillerato, Junta de Andalucía
- Currículo de referencia: Decreto 318/2023 (programación básica, pensamiento computacional)
- El alumno YA sabe: variables, condicionales, bucles, funciones, structs
- El alumno NO sabe aún: POO, memoria dinámica, STL, patrones de diseño
'''
