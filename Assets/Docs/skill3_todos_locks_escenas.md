---
name: tic1-todos-locks-escenas
version: 1
description: >
  Skill de TODOs obligatorios y sistema de locks por escena para el espacio
  TIC I Andalucía. Define con sumo detalle qué debe completar el alumno en
  cada escena para poder avanzar, cómo se presenta cada ejercicio bloqueante
  en C++ básico (con el snippet esqueleto), qué comprueba el juego para
  desbloquear el paso a la siguiente escena, y cómo el modelo guía al alumno
  sin darle la solución directa. Basada en los scripts reales del proyecto
  Unity 6000.4.5f1: SimpleWalk, ChangeType, ChangeColors, Damageable,
  DamageTypes, PlayerHealth, KickCollider, LightSaberHitbox, RapierHitbox.
agents: [main_agent, general_purpose]
---

# Skill: TODOs Obligatorios y Locks por Escena

## Para qué sirve esta skill

Esta skill le dice al modelo exactamente:

1. **Qué tiene que hacer el alumno** en cada escena para poder avanzar.
2. **El snippet esqueleto** que se le entrega: con TODO marcados, sin solución.
3. **Cómo se valida** en Unity que el alumno lo ha completado correctamente
   (la mecánica de lock que impide pasar de escena hasta que funciona).
4. **Cómo guía el modelo** al alumno cuando pide ayuda: pistas, no soluciones.
5. **Las pistas graduadas**: nivel 1 (pista mínima), nivel 2 (más detalle),
   nivel 3 (solo si el alumno lleva mucho rato atascado).

---

## Filosofía del Lock

### Por qué existe el lock

El objetivo no es castigar al alumno, sino garantizar que el código
que escribe realmente funciona antes de avanzar. Si el enemigo no muere
es porque el código no está bien. Esa consecuencia inmediata y visual
es la retroalimentación más potente que existe en programación.

### Cómo funciona el lock en Unity (el alumno no lo ve)

El profesor tiene preparado en cada escena un script de lock encapsulado
que comprueba en tiempo real si las condiciones del ejercicio se cumplen:
- Si el alumno no ha rellenado los valores por defecto (0, false, ""),
  el enemigo tiene vida infinita o no recibe daño.
- Si el alumno ha puesto valores incorrectos, el enemigo no muere
  (o no aplica el efecto correcto) y no aparece la salida de escena.
- Solo cuando la lógica funciona (el enemigo muere como debe),
  se activa el trigger de paso a la siguiente escena.

### Cómo lo explica el modelo al alumno

```
"El ninja no puede avanzar porque los enemigos son inmortales.
 ¿Por qué? Porque el código que activa su daño aún no está completo.
 Cuando rellenes los TODO correctamente, los enemigos empezarán a
 recibir daño y podrás derrotarlos. Solo cuando los derrotes todos,
 aparecerá la salida de la sala."
```

---

## Sistema de Pistas Graduadas

Cuando el alumno pide ayuda, el modelo responde en 3 niveles según
cuántas veces haya pedido ayuda ya en esa misma tarea:

### Nivel 1 — Pista mínima (primera petición de ayuda)
Solo señalar dónde está el problema sin decir la solución:
```
"Fíjate en la función CambiarTipoRapier(). Tienes un switch con tres
casos. El case 5 debería activar el veneno. ¿Qué valor le has puesto
a tipoActual cuando entras en ese case?"
```

### Nivel 2 — Más detalle (segunda petición)
Mostrar la estructura correcta con el hueco todavía vacío:
```
"El switch tiene que verse así, pero faltan los valores:

case 5:
    tipoActual = ____;   // <- aquí va el número del veneno
    break;

En el enum de tipos que tienes arriba, el veneno es el número 1.
¿Qué pondrías?"
```

### Nivel 3 — Casi la solución (tercera petición o más)
Mostrar el fragmento correcto con explicación completa:
```
"El case completo es este:

case 5:
    tipoActual = 1;  // 1 = veneno (según el enum de arriba)
    break;

Ahora entiende por qué: el switch mira qué tecla pulsaste (5),
y según eso cambia tipoActual. El juego luego lee tipoActual
para saber qué daño hacer. Si tipoActual sigue siendo 0 (Normal),
el veneno nunca se activa."
```

---

## ESCENA 1 — Sala de Pruebas

### Descripción
Sala libre de testing. No hay lock obligatorio.
El alumno puede explorar el proyecto, probar los ataques
y familiarizarse con los controles.

### Condición para avanzar
Romper un cubo específico de la escena.
El cubo tiene un Damageable con vulnerableTypes vacío
(vulnerable a cualquier ataque) y baja vida.

### Ejercicio sugerido (no obligatorio)
```cpp
// Prueba libre: cambia estos números y mira qué pasa en el juego.
// No hay respuesta incorrecta, solo experimenta.

int vidaDelCubo = 3;    // TODO: ¿cuántos golpes necesita el cubo?
                         // <- prueba con 1, luego con 5, luego con 10

int danioDelGolpe = 1;  // TODO: ¿cuánto daño hace cada golpe?
                         // <- prueba cambiar este número también
```

### Mensaje en pantalla al entrar (texto negro sobre blanco)
```
SALA DE PRUEBAS
Aquí puedes probar todo lo que quieras sin miedo a romper nada.
Para pasar a la siguiente sala... rompe ese cubo.
Controles:
  WASD        → moverse
  Clic izq    → rapier
  Clic der    → sable láser
  Q / E       → media vuelta
  K           → patada Chut
  M           → patada Martelo
  1/2/3       → color del sable (Rojo/Azul/Gris)
  4/5/6       → modo del rapier (Normal/Veneno/Sangrado)
[ENTER para continuar]
```

---

## ESCENA 2 — Aliens de Juguete

### Descripción
Llegan olas de aliens de juguete. El alumno debe configurar
los ataques básicos usando if/else y asignar valores a variables.

### Lock: condición para avanzar
Los aliens tienen vida = 0 por defecto (son inmortales hasta que
el alumno complete los ejercicios). Solo cuando el código esté
correcto, los aliens morirán y aparecerá la salida.

### TIP en pantalla antes de la batalla
```
CONSEJO DE COMBATE
Los aliens de juguete son resistentes... por ahora.
Para vencerlos necesitas:
  1. Decirle al juego cuánta vida tienen (Ejercicio A)
  2. Configurar qué teclas hacen qué ataque (Ejercicio B)
  3. Decirle cuánto daño hace cada ataque (Ejercicio C)
Cuando lo tengas, los aliens podrán ser derrotados.
[ENTER para empezar]
```

### EJERCICIO A — Variables básicas del enemigo
Snippet que se entrega al alumno:

```cpp
// ============================================================
// EJERCICIO A — Escena 2: Datos del alien de juguete
// ============================================================
// Un alien de juguete es como una ficha de juego de mesa.
// Tiene vida, un nombre y puede estar vivo o muerto.
// Rellena los TODO para que el alien tenga estadísticas reales.

struct Alien
{
    string nombre;   // El nombre del alien
    int vida;        // Su vida actual
    int vidaMaxima;  // La vida máxima que puede tener
    bool vivo;       // true = vivo, false = derrotado
};

// Esta función crea un alien con sus datos iniciales.
// Es como rellenar la ficha antes de empezar a jugar.
Alien CrearAlien()
{
    Alien a;

    a.nombre = "AlienJuguete";  // El alien ya tiene nombre

    // TODO: ¿Cuánta vida tiene el alien? Ponlo aquí.
    //       Prueba con 3. Si es muy fácil sube a 5.
    a.vida = 0;              // <- cambia el 0

    // TODO: La vida máxima tiene que ser igual que la inicial.
    //       Ponlo aquí.
    a.vidaMaxima = 0;        // <- cambia el 0

    a.vivo = true;           // El alien empieza vivo

    return a;                // Devolvemos la ficha rellena
}
```

**Validación en Unity**: `Damageable.maxHealth` del alien prefab
se lee desde el valor que el alumno ponga en `a.vida`. Si sigue
siendo 0, el alien no puede morir (vida nunca baja de 0).

### EJERCICIO B — if/else de ataques (patadas)
Snippet que se entrega al alumno:

```cpp
// ============================================================
// EJERCICIO B — Escena 2: Configurar los ataques con if/else
// ============================================================
// El ninja puede dar dos tipos de patadas.
// Tienes que decirle al juego qué pasa cuando el jugador
// pulsa cada tecla.
//
// Teclas disponibles:
//   "K" → quieres que haga el Chut (patada frontal)
//   "M" → quieres que haga el Martelo (patada alta)

// Esta función se llama una vez por turno (como el Update del juego).
// Comprueba qué tecla se ha pulsado y ejecuta el ataque correcto.
void ComprobarTeclasDeAtaque(string teclaPulsada)
{
    // TODO: Escribe aquí el if/else para los dos ataques.
    //
    // Pista: si teclaPulsada es igual a "K", hay que hacer Chut.
    //        Si teclaPulsada es igual a "M", hay que hacer Martelo.
    //
    // Las funciones de ataque ya están hechas, solo tienes que
    // llamarlas en el sitio correcto:
    //   EjecutarChut();      <- llama a esto para el Chut
    //   EjecutarMartelo();   <- llama a esto para el Martelo

    // Escribe tu if/else aquí:


}

// Estas funciones ya están hechas. No las toques.
void EjecutarChut();      // Lanza la animación de patada Chut
void EjecutarMartelo();   // Lanza la animación de patada Martelo
```

**Validación en Unity**: `SimpleWalk.Update()` reemplaza la detección
de `KeyCode.K` y `KeyCode.M` por la función del alumno. Si el if/else
no está escrito, las patadas no disparan animación ni hitbox.

### EJERCICIO C — Daño del ataque
Snippet que se entrega al alumno:

```cpp
// ============================================================
// EJERCICIO C — Escena 2: ¿Cuánto daño hace cada ataque?
// ============================================================
// Un ataque sin daño no sirve de nada. Tienes que decirle
// al juego cuánta vida le quita cada tipo de golpe al enemigo.

// TODO: ¿Cuánto daño hace una patada Chut? Ponlo aquí.
//       Prueba empezando por 1. Si quieres que sea más fuerte, sube.
int danioChut = 0;       // <- cambia el 0

// TODO: ¿Cuánto daño hace una patada Martelo? Ponlo aquí.
//       El Martelo es más lento pero más fuerte. Prueba con 2.
int danioMartelo = 0;    // <- cambia el 0

// Esta función aplica el daño al enemigo.
// Ya está hecha, solo necesita que danioChut y danioMartelo
// tengan valores distintos de 0.
void AplicarDanioPatada(int danio, int vidaEnemigo)
{
    vidaEnemigo = vidaEnemigo - danio;  // Le quitamos el daño

    if (vidaEnemigo <= 0)               // Si la vida llega a cero...
    {
        vidaEnemigo = 0;                // No puede bajar de cero
        DebugEstado();                  // Mostramos que ha sido derrotado
    }
}
```

**Validación en Unity**: `KickHitbox.damage` se inicializa con el
valor de `danioChut` o `danioMartelo` según el collider. Si siguen
en 0, los golpes no restan vida.

---

## ESCENA 3 — Peluches Mousey

### Lock: condición para avanzar
Los Mousey son inmortales hasta que:
1. El switch del rapier funciona (cambia entre Normal/Veneno/Sangrado).
2. El switch del sable funciona (cambia entre Rojo/Azul/Gris).
3. El struct del personaje está rellenado con datos reales.
4. La función `EsEtiquetaPermitida()` devuelve true correctamente.

### TIP en pantalla antes de la batalla
```
CONSEJO DE COMBATE
Los peluches Mousey son más resistentes que los aliens.
Para vencerlos necesitas:
  1. Configurar el switch del rapier (teclas 4, 5, 6)
  2. Configurar el switch del sable (teclas 1, 2, 3)
  3. Crear la ficha del personaje con un struct
  4. Decirle al juego qué armas pueden dañar a cada Mousey
Cada Mousey tiene un color. Usa el arma del color correcto.
[ENTER para empezar]
```

### EJERCICIO A — Switch del rapier (teclas 4/5/6)

```cpp
// ============================================================
// EJERCICIO A — Escena 3: Switch para cambiar el modo del rapier
// ============================================================
// El rapier puede estar en 3 modos distintos.
// Según qué tecla pulses, cambia su tipo de daño.
// Un switch es como un panel de 3 botones: pulsas uno y pasa
// una cosa distinta.
//
// Modos disponibles:
//   tecla 4 → modo Normal   (tipo 0): daño directo, sin efecto
//   tecla 5 → modo Veneno   (tipo 1): envenena al enemigo
//   tecla 6 → modo Sangrado (tipo 2): hace sangrar al enemigo

// Tipos de daño (ya están definidos como constantes):
const int TIPO_NORMAL   = 0;  // Sin efecto extra
const int TIPO_VENENO   = 1;  // Activa veneno al golpear
const int TIPO_SANGRADO = 2;  // Activa sangrado al golpear

// El tipo activo del rapier (empieza en Normal)
int tipoRapierActual = TIPO_NORMAL;

// Esta función se llama cuando el jugador pulsa 4, 5 o 6.
void CambiarTipoRapier(int teclaNumerica)
{
    // TODO: Escribe aquí el switch que cambia tipoRapierActual.
    //
    // Pista: el switch mira el valor de teclaNumerica.
    //   Si teclaNumerica es 4 → tipoRapierActual = TIPO_NORMAL
    //   Si teclaNumerica es 5 → tipoRapierActual = ????
    //   Si teclaNumerica es 6 → tipoRapierActual = ????
    //
    // Estructura de un switch:
    //   switch (variable)
    //   {
    //       case valor:
    //           // lo que pasa
    //           break;
    //   }

    // Escribe tu switch aquí:


    DebugTipoRapier();  // Muestra qué tipo está activo ahora
}
```

### EJERCICIO B — Switch del sable (teclas 1/2/3)

```cpp
// ============================================================
// EJERCICIO B — Escena 3: Switch para cambiar el color del sable
// ============================================================
// El sable láser cambia de color y de tipo de daño.
// Funciona igual que el rapier pero con colores.
//
//   tecla 1 → color Rojo  (tipo 3)
//   tecla 2 → color Azul  (tipo 4)
//   tecla 3 → color Gris  (tipo 5)

const int COLOR_ROJO = 3;   // Sable rojo
const int COLOR_AZUL = 4;   // Sable azul
const int COLOR_GRIS = 5;   // Sable gris

int colorSableActual = COLOR_ROJO;  // Empieza en rojo

// Esta función se llama cuando el jugador pulsa 1, 2 o 3.
void CambiarColorSable(int teclaNumerica)
{
    // TODO: Escribe aquí el switch que cambia colorSableActual.
    //
    // Es igual que el ejercicio anterior, pero con los colores.
    // Si teclaNumerica es 1 → colorSableActual = COLOR_ROJO
    // Si teclaNumerica es 2 → colorSableActual = ????
    // Si teclaNumerica es 3 → colorSableActual = ????

    // Escribe tu switch aquí:


    DebugColorSable();  // Muestra qué color está activo ahora
}
```

### EJERCICIO C — Struct del personaje + EsEtiquetaPermitida

```cpp
// ============================================================
// EJERCICIO C — Escena 3: Ficha del personaje y armas permitidas
// ============================================================
// Cada Mousey solo puede ser dañado por ciertas armas.
// Algunos solo reciben daño del rapier, otros del sable,
// otros de patadas. Tienes que comprobarlo.

struct Personaje
{
    string nombre;
    int vida;
    int vidaMaxima;
    bool vivo;

    // TODO: ¿Qué arma tiene equipada el personaje?
    //       Escribe "Rapier", "LightSaber" o "Kick"
    string armaEquipada;  // <- escribe el nombre del arma entre comillas
};

// Esta función comprueba si el arma puede dañar a este Mousey.
// Devuelve true si puede, false si no puede.
bool EsEtiquetaPermitida(string etiquetaArma, string etiquetaEnemigo)
{
    // TODO: Escribe aquí el if que compara las dos etiquetas.
    //
    // Pista: si etiquetaArma es igual a etiquetaEnemigo,
    //        el arma sí puede dañar al enemigo (devuelve true).
    //        Si no son iguales, no puede (devuelve false).

    // Escribe tu if aquí:


    return false;  // <- esto cambiará cuando escribas el if correcto
}
```

---

## ESCENA 4 — Fernando Alonso

### Lock: condición para avanzar
Los Alonso son inmortales hasta que:
1. El sistema de XP esté implementado con los valores correctos
   (Alien=1, Mousey=5, Alonso=10, otros=15).
2. La función `EsColorVulnerable()` funcione correctamente.

### TIP en pantalla antes de la batalla
```
CONSEJO DE COMBATE
Fernando Alonso tiene muchos fans. Y todos llevan mono de piloto.
Para vencerlos necesitas:
  1. Implementar el sistema de experiencia (XP)
     Alien: 1 XP | Mousey: 5 XP | Alonso: 10 XP | Otros: 15 XP
  2. Implementar EsColorVulnerable():
     el color del mono del piloto determina qué sable le daña
Lock: no podrás avanzar hasta haber acumulado suficiente XP
derrotando a todos los Alonso de la sala.
[ENTER para empezar]
```

### EJERCICIO A — Sistema de XP

```cpp
// ============================================================
// EJERCICIO A — Escena 4: Sistema de experiencia (XP)
// ============================================================
// Cada vez que derrotes a un enemigo ganas experiencia.
// Diferente tipo de enemigo = diferente cantidad de XP.
// Acumulas XP y subes de nivel.

// XP que da cada tipo de enemigo (ya definidas, no las cambies)
const int XP_ALIEN  = 1;   // Los aliens dan poca XP
const int XP_MOUSEY = 5;   // Los peluches dan XP media
const int XP_ALONSO = 10;  // Alonso da bastante XP

// TODO: ¿Cuánta XP dan los enemigos que no son ninguno de los anteriores?
//       Ponlo aquí. Que sea más que Alonso.
const int XP_OTROS = 0;    // <- cambia el 0

// La XP acumulada del jugador (empieza en 0)
int xpActual = 0;

// Esta función se llama cada vez que derrotes un enemigo.
// Suma la XP correspondiente según el tipo de enemigo.
void SumarXP(string tipoEnemigo)
{
    // TODO: Escribe aquí el if/else-if que suma la XP correcta.
    //
    // Pista: compara tipoEnemigo con "Alien", "Mousey", "Alonso".
    // Si es "Alien"  → xpActual = xpActual + XP_ALIEN
    // Si es "Mousey" → xpActual = xpActual + ????
    // Si es "Alonso" → xpActual = xpActual + ????
    // Si es cualquier otro → xpActual = xpActual + ????

    // Escribe tu if/else-if aquí:


    DebugXP();  // Muestra la XP actualizada en pantalla
}
```

### EJERCICIO B — EsColorVulnerable

```cpp
// ============================================================
// EJERCICIO B — Escena 4: ¿Es el enemigo vulnerable a este color?
// ============================================================
// Cada Fernando Alonso lleva un mono de un color.
// Solo el sable del color correcto le puede dañar.
// Tienes que comprobarlo con un if.

// Esta función recibe el color del arma y el color del enemigo.
// Devuelve true si el color del arma coincide con la debilidad.
bool EsColorVulnerable(int colorArma, int colorEnemigo)
{
    // TODO: Escribe aquí el if que compara colorArma con colorEnemigo.
    //
    // Pista: si colorArma es igual a colorEnemigo,
    //        el enemigo es vulnerable (devuelve true).
    //        Si no son iguales, no le afecta (devuelve false).

    // Escribe tu if aquí:


    return false;  // <- esto cambiará cuando escribas el if correcto
}
```

---

## ESCENA 5 — Maniquís

### Lock: condición para avanzar
Los maniquís son inmortales hasta que:
1. El switch de cambio de tipo del rapier funcione para los 3 modos.
2. El while de aplicación de veneno funcione con contador de turnos.
3. El while de aplicación de sangrado funcione con contador de golpes.
4. Las debilidades de cada maniquí estén configuradas correctamente.

### TIP en pantalla antes de la batalla
```
CONSEJO DE COMBATE
Los maniquís de cada color solo pueden ser dañados
con el tipo de daño correcto del rapier.
  Maniquí rojo     → vulnerable a Sangrado (rapier modo 6)
  Maniquí morado   → vulnerable a Veneno   (rapier modo 5)
  Maniquí blanco   → vulnerable a Normal   (rapier modo 4)
Para vencerlos necesitas implementar:
  1. El while del veneno (quita vida varios turnos)
  2. El while del sangrado (cuenta golpes hasta el 5.º)
  3. Elegir el modo correcto según el color del maniquí
[ENTER para empezar]
```

### EJERCICIO A — While del veneno

```cpp
// ============================================================
// EJERCICIO A — Escena 5: El while del veneno
// ============================================================
// El veneno no mata de golpe. Va quitando vida poco a poco,
// un poco cada turno, durante varios turnos seguidos.
// Es como si alguien te picara una avispa: el dolor dura un rato.

bool envenenado    = false;  // ¿Tiene veneno el enemigo ahora mismo?
int turnosVeneno   = 0;      // Cuántos turnos le quedan de veneno

// TODO: ¿Cuánto daño hace el veneno por turno?
//       El veneno del rapier en el juego real quita 0.5 por segundo.
//       En C++ básico usamos enteros: prueba con 1.
int danioVeneno = 0;         // <- cambia el 0

// Esta función activa el veneno en el enemigo.
// Se llama cuando el rapier en modo Veneno golpea al enemigo.
void ActivarVeneno(int numeroTurnos)
{
    envenenado   = true;         // El veneno se activa
    turnosVeneno = numeroTurnos; // Le damos los turnos que durará

    // TODO: ¿Cuántos turnos dura el veneno cuando se activa?
    //       Prueba con 3. Ponlo en la llamada a esta función.
}

// Esta función aplica el daño del veneno cada turno.
// Se llama una vez por turno mientras el veneno esté activo.
void AplicarVeneno(int vidaEnemigo)
{
    // TODO: Escribe aquí el while que aplica el veneno turno a turno.
    //
    // Pista: el while debe repetirse MIENTRAS turnosVeneno sea mayor
    //        que 0 Y el enemigo esté envenenado.
    //
    // Dentro del while, por cada turno:
    //   1. Resta danioVeneno a vidaEnemigo
    //   2. Resta 1 a turnosVeneno
    //   3. Si turnosVeneno llega a 0, pon envenenado = false
    //
    // Estructura:
    //   while (condicion)
    //   {
    //       // lo que pasa cada turno
    //   }

    // Escribe tu while aquí:


    DebugVeneno();  // Muestra el estado del veneno en pantalla
}
```

### EJERCICIO B — While del sangrado

```cpp
// ============================================================
// EJERCICIO B — Escena 5: El while del sangrado
// ============================================================
// El sangrado funciona diferente al veneno.
// No quita vida cada turno. En cambio, cuenta golpes.
// Cuando llevas 5 golpes de sangrado... ¡BAM! Daño extra.
// Es como si cada corte se fuera acumulando hasta explotar.

bool sangrando       = false;  // ¿Tiene sangrado el enemigo?
int golpesSangrado   = 0;      // Cuántos golpes de sangrado lleva

// TODO: ¿Cuánto daño extra hace al llegar al 5.º golpe?
//       En el juego real son 3 puntos de daño extra. Prueba con 3.
int danioExplSangrado = 0;     // <- cambia el 0

// Esta función registra un golpe de sangrado.
// Se llama cada vez que el rapier en modo Sangrado golpea al enemigo.
void RegistrarGolpeSangrado(int vidaEnemigo)
{
    if (sangrando == false)    // Si no hay sangrado activo, nada que hacer
        return;

    golpesSangrado = golpesSangrado + 1;  // Un golpe más

    // TODO: Escribe aquí el if que comprueba si llegamos al 5.º golpe.
    //
    // Pista: si golpesSangrado es mayor o igual a 5...
    //   1. Resta danioExplSangrado a vidaEnemigo
    //   2. Reinicia golpesSangrado a 0
    //   3. Llama a DebugSangrado() para ver qué pasó

    // Escribe tu if aquí:


}
```

---

## ESCENA 6 — Boss Dummy

### Lock: condición para vencer al boss
El boss tiene una secuencia obligatoria de 3 fases.
Solo puede ser derrotado si se sigue el orden exacto:
1. **8 golpes de aturdimiento** → boss pasa a estado ATURDIDO (blanco)
2. **8 golpes de confusión** → boss pasa a estado CONFUNDIDO (negro)
3. **1 golpe final** después de la confusión → boss derrotado

Si el alumno aplica confusión antes del aturdimiento, no funciona.
Si aplica el golpe final sin la confusión previa, no funciona.

### TIP en pantalla antes de la batalla
```
CONSEJO DE COMBATE — BOSS DUMMY
El Dummy no puede ser derrotado con ataques normales.
Tienes que seguir una secuencia exacta:
  FASE 1: 8 golpes de aturdimiento (tecla 8)
          → el boss se pone blanco y se queda quieto 5 segundos
  FASE 2: 8 golpes de confusión    (tecla 9)
          → el boss se pone negro y ataca a objetos en vez de a ti
  FASE 3: 1 golpe final            (tecla 7 o clic)
          → el boss es derrotado
IMPORTANTE: el orden importa. Si no lo haces en orden, no funciona.
Configura las condiciones en el orden correcto dentro de las funciones.
[ENTER para comenzar]
```

### EJERCICIO A — Estado del boss y teclas (teclas 7/8/9)

```cpp
// ============================================================
// EJERCICIO A — Escena 6: Estado del boss y teclas especiales
// ============================================================
// El boss tiene 3 estados posibles:
//   0 = Normal    (sin efecto)
//   1 = Aturdido  (se queda quieto 5 segundos, se ve blanco)
//   2 = Confundido(ataca a objetos en vez de a ti, se ve negro)
//
// Las teclas especiales del boss son:
//   tecla 7 → golpe normal (sin efecto especial)
//   tecla 8 → golpe de aturdimiento
//   tecla 9 → golpe de confusión

int estadoBoss        = 0;   // El boss empieza en estado Normal
int golpesAturdimiento = 0;  // Cuántos golpes de aturdimiento lleva
int golpesConfusion    = 0;  // Cuántos golpes de confusión lleva

// Esta función se llama cuando el jugador pulsa 7, 8 o 9.
void AplicarEfectoBoss(int teclaNumerica)
{
    // TODO: Escribe aquí el switch para las teclas 7, 8 y 9.
    //
    // case 7 → llama a GolpeNormal()
    // case 8 → llama a AplicarAturdimiento()
    // case 9 → llama a AplicarConfusion()

    // Escribe tu switch aquí:


    DebugBoss();  // Muestra el estado del boss en pantalla
}
```

### EJERCICIO B — Lógica del aturdimiento (el orden importa)

```cpp
// ============================================================
// EJERCICIO B — Escena 6: Aturdimiento del boss
// ============================================================
// Cada golpe de aturdimiento suma al contador.
// Cuando llegamos a 8 golpes, el boss se aturde.
// IMPORTANTE: el aturdimiento solo funciona si el boss
//             está en estado Normal (estadoBoss == 0).
//             Si ya está aturdido o confundido, no suma.

void AplicarAturdimiento()
{
    // TODO: Escribe aquí las condiciones en el orden correcto.
    //
    // El orden correcto es:
    //   1. Primero comprobar si el boss YA está aturdido o confundido
    //      → si es así, salir sin hacer nada (return)
    //   2. Si el boss está en estado Normal, sumar un golpe
    //   3. Si golpesAturdimiento llega a 8...
    //      → cambiar estadoBoss a 1 (Aturdido)
    //      → reiniciar golpesAturdimiento a 0
    //
    // Pista: las cláusulas if van en ESE ORDEN. Si las cambias
    //        de sitio, la lógica falla y el boss no se aturde.

    // Escribe tus if aquí, en el orden correcto:


    DebugBoss();
}
```

### EJERCICIO C — Lógica de la confusión (depende del aturdimiento)

```cpp
// ============================================================
// EJERCICIO C — Escena 6: Confusión del boss
// ============================================================
// La confusión solo funciona si el boss YA estuvo aturdido antes.
// No puedes confundirle si primero no le aturdiste.
// IMPORTANTE: el orden de los if dentro de esta función
//             es la clave del ejercicio.

void AplicarConfusion()
{
    // TODO: Escribe aquí las condiciones en el orden correcto.
    //
    // El orden correcto es:
    //   1. Primero comprobar si el boss YA está confundido
    //      → si es así, salir sin hacer nada (return)
    //   2. Comprobar si el boss ha pasado por el aturdimiento
    //      → solo si estadoBoss fue 1 en algún momento se puede confundir
    //      → usa una variable bool: bool yaEstuvoAturdido = false;
    //   3. Si el boss no estuvo aturdido antes → salir sin hacer nada
    //   4. Si pasó el aturdimiento, sumar golpe de confusión
    //   5. Si golpesConfusion llega a 8 → cambiar estadoBoss a 2
    //
    // Pista: si pones el paso 4 antes que el paso 2,
    //        podrás confundirle sin haberle aturdido. ¡Eso es un bug!

    // Escribe tus if aquí, en el orden correcto:


    DebugBoss();
}
```

### EJERCICIO D — Golpe final (solo funciona tras confusión)

```cpp
// ============================================================
// EJERCICIO D — Escena 6: El golpe final
// ============================================================
// Solo puedes derrotar al boss con el golpe final
// si ya ha pasado por AMBAS fases: aturdimiento Y confusión.
// Si intentas el golpe final sin haberle confundido, no funciona.

bool yaEstuvoAturdido   = false;  // true cuando completes el aturdimiento
bool yaEstuvoConfundido = false;  // true cuando completes la confusión

void GolpeNormal()
{
    // TODO: Escribe aquí el if que comprueba si el boss puede
    //       ser derrotado con este golpe.
    //
    // Condición para derrotar al boss:
    //   yaEstuvoAturdido == true
    //   Y ADEMÁS yaEstuvoConfundido == true
    //   Y ADEMÁS estadoBoss == 2 (está en estado confundido ahora mismo)
    //
    // Si se cumplen las tres... llamar a DerrotarBoss()
    // Si no se cumplen... el golpe normal no hace nada especial.
    //
    // Pista: usa el operador && para combinar las condiciones.
    //   if (condicion1 && condicion2 && condicion3)

    // Escribe tu if aquí:


    DebugBoss();
}

// Esta función ya está hecha. Solo se llama cuando las condiciones
// del TODO anterior se cumplen.
void DerrotarBoss();   // Activa la animación de derrota del boss
```

---

## Tabla Resumen de Locks por Escena

| Escena | Ejercicios obligatorios | Variable/función que valida el lock |
|--------|------------------------|-------------------------------------|
| 1 | Ninguno (romper el cubo) | `Damageable.maxHealth > 0` en el cubo |
| 2A | `CrearAlien()` con vida real | `a.vida != 0` |
| 2B | `ComprobarTeclasDeAtaque()` con if/else | Patadas disparan animación y hitbox |
| 2C | `danioChut != 0` y `danioMartelo != 0` | `KickHitbox.damage > 0` |
| 3A | `CambiarTipoRapier()` con switch | `ChangeType.DetectarCambioDeModo()` funciona |
| 3B | `CambiarColorSable()` con switch | `ChangeColors.SetSaberColor()` funciona |
| 3C | `EsEtiquetaPermitida()` devuelve true | `Damageable.IsAllowedWeaponTag()` válido |
| 4A | `SumarXP()` con valores correctos | `xpActual` acumula correctamente |
| 4B | `EsColorVulnerable()` devuelve true | `Damageable.IsVulnerableToColor()` válido |
| 5A | `AplicarVeneno()` con while funcional | Veneno resta vida varios turnos |
| 5B | `RegistrarGolpeSangrado()` con if correcto | Sangrado explota al 5.º golpe |
| 6A | Switch teclas 7/8/9 | `AplicarEfectoBoss()` redirige bien |
| 6B | `AplicarAturdimiento()` orden correcto | Boss llega a `estadoBoss == 1` |
| 6C | `AplicarConfusion()` orden correcto | Boss llega a `estadoBoss == 2` |
| 6D | `GolpeNormal()` con && correcto | `DerrotarBoss()` se llama |

---

## Cómo Responde el Modelo Cuando el Alumno Pide Ayuda en un Lock

### Patrón de respuesta estándar

```
1. Identificar en qué lock está: "Estás en el Ejercicio B de la Escena 3."

2. Recordar qué tiene que hacer sin dar la solución:
   "El switch tiene que mirar qué tecla pulsaste y cambiar
    tipoRapierActual según el número. ¿Qué número pusiste
    en el case 5?"

3. Si pide más ayuda (segunda vez): mostrar la estructura vacía.

4. Si pide más ayuda (tercera vez): mostrar el fragmento correcto
   con explicación línea a línea de por qué funciona.

5. Siempre cerrar con:
   "Cuando lo tengas, pruébalo: pulsa la tecla 5 en el juego
    y el rapier debería ponerse morado. Si se pone morado,
    ¡está funcionando!"
```

### Lo que NUNCA hace el modelo

```
❌ Dar la solución completa en la primera petición de ayuda
❌ Decir "simplemente pon X en la línea Y" sin explicación
❌ Explicar por qué funciona la solución después de darla
   (hay que explicarlo ANTES, para que el alumno lo entienda)
❌ Usar terminología de Unity al explicar el lock
❌ Dar más de una pista a la vez
```
