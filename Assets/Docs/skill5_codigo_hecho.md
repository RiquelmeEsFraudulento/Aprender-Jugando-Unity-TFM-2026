---
name: tic1-codigo-hecho
version: 1
description: >
  Skill con el inventario completo del código existente en el proyecto
  de Unity (Unity 6000.4.5f1). Para cada script se explica qué hace,
  función por función, qué puede modificar el alumno y qué NO debe tocar.
  Importante: aunque el código esté hecho, algunas funciones están 
  deliberadamente simplificadas y el alumno puede mejorarlas como parte 
  de los ejercicios de cada escena.
agents: [main_agent, general_purpose]
---

# Skill: Código Existente — Inventario Completo

## Principio fundamental de esta skill

**"Hecho" no significa "perfecto ni intocable".**

El código base existe para que el alumno no empiece desde cero.
Pero en cada escena hay zonas marcadas como editables.
Cuando el alumno mejora una función existente, eso también es programar.

El modelo debe:
- Explicar qué hace cada función con analogías de C++ puro
- Señalar qué parte el alumno puede modificar en cada ejercicio
- Nunca mostrar la solución completa de un ejercicio activo
- Sí mostrar el cuerpo completo de funciones que son "infraestructura"
  (las que el alumno no tiene que tocar para avanzar)

---

## SCRIPT 1 — SimpleWalk.cs (playerscript.cs)
**Dónde está en Unity**: en el GameObject del personaje ninja.
**Analogía C++**: es la función `main()` del bucle del juego para el jugador.

### Qué hace este script completo
Controla todo lo que puede hacer el personaje: moverse, girar,
lanzar animaciones de ataque, y activar/desactivar las hitboxes
en el momento correcto durante cada animación.

### Función por función

#### `void Start()`
```
Equivalente en C++: constructor de la clase.
Se ejecuta una sola vez al empezar el juego.
Busca el CharacterController y el Animator del personaje.
El alumno NO necesita tocarla.
```

#### `void Update()`
```
Equivalente en C++: el bucle principal while(juegoCorriendo).
Se ejecuta una vez por cada fotograma.
Lee las teclas del teclado y llama a la función correspondiente.

ZONA EDITABLE (Escena 2):
  Los if/else de las teclas de patada están aquí.
  El alumno tiene que rellenar qué animación se lanza
  y qué hitbox se activa al pulsar K (Chut) y M (Martelo).

Teclas ya implementadas:
  WASD     → moverse
  Clic izq → ataque rapier (RaSwing)
  Clic der → ataque sable  (SwSwing)
  Q        → media vuelta rapier (Ra360)
  E        → media vuelta sable  (Sw360)
  K        → Chut (EDITABLE en escena 2)
  M        → Martelo (EDITABLE en escena 2)
```

#### `IEnumerator RaSwing()` / `SwSwing()` / `Ra360()` / `Sw360()`
```
Son coroutines. Una coroutine es una función que puede
"pausarse" y "seguir" después de esperar un tiempo.

Analogía C++: imagina un hilo que duerme X segundos y luego sigue.
  StartCoroutine(RaSwing())  ≡  thread.start(raSwingFunc)

Lo que hace:
1. Activa la animación (SetTrigger).
2. Espera 1.3 segundos (la duración de la animación).
El alumno NO toca estas funciones.
La hitbox se activa/desactiva desde el Animator (Animation Events),
no desde aquí.
```

#### `IEnumerator Chut()` / `Martelo()`
```
Igual que las anteriores pero para las patadas.
Activan la animación y esperan.
El alumno NO toca el cuerpo de estas funciones.
Lo que sí toca: el if/else en Update() que decide
cuándo llamarlas y qué hitbox activar.
```

#### `void EnableRapierHitbox()` / `DisableRapierHitbox()`
#### `void EnableLightSaberHitbox()` / `DisableLightSaberHitbox()`
#### `void EnableLeftKickHitbox()` / `DisableLeftKickHitbox()`
#### `void EnableRightKickHitbox()` / `DisableRightKickHitbox()`
```
Activan o desactivan el Collider de cada arma.
Son llamadas desde los Animation Events del Animator.
El alumno NO las toca.
Analogía C++: son callbacks que el motor llama en un instante concreto.

IMPORTANTE: También llaman a Rapier.Reactivar() para limpiar
el registro de "ya golpeé a este enemigo en este swing".
Esto permite que el mismo enemigo pueda ser golpeado en el
siguiente swing aunque ya lo hubiera sido en el anterior.
```

#### `IEnumerator DanceSpin()`
```
Hace girar al personaje 360 grados pulsando B.
Es un bucle for que rota 30 grados cada 0.05 segundos.
Analogía C++:
  for (int i = 0; i < 360; i += 30) {
    rotar(30);
    esperar(0.05);
  }
El alumno NO la toca. Está para que vean un for en acción.
```

### Qué puede mejorar el alumno en este script
- Escena 2: rellenar el if/else de patadas en Update()
- Escena 2: asignar valores de daño a las hitboxes de patada
- Escenas futuras (mejora opcional): añadir más if/else para
  acciones adicionales del personaje

---

## SCRIPT 2 — Damageable.cs (Damageable-8.cs)
**Dónde está en Unity**: en cada enemigo del juego.
**Analogía C++**: es la struct/clase "Enemigo" con toda su lógica de vida.

### Qué hace este script completo
Gestiona la vida de un enemigo, los efectos de estado (veneno, sangrado),
el cooldown entre golpes, y decide si un golpe puede afectar al enemigo.

### Función por función

#### `void Awake()` → llama a `InicializarVida()`
```
Se ejecuta antes que Start(). Equivale al constructor en C++.
Pone currentHealth = maxHealth al empezar.
Muestra en consola los parámetros del enemigo (DEBUG activado).
```

#### `void Update()`
```
Cada fotograma:
1. Avanza el temporizador del cooldown (AvanzarCooldown).
2. Si hay veneno activo, procesa el daño por tiempo (ProcesarVenenoPorTiempo).
```

#### `bool CanBeDamagedBy(DamageType tipo, string weaponTag)`
```
La hitbox llama a esta función ANTES de aplicar daño.
Devuelve true → el golpe pasa. Devuelve false → el golpe se ignora.

Lógica encadenada (el alumno verá este patrón en Escena 3):
1. Si está en cooldown → false (acaba de recibir un golpe)
2. Si la etiqueta del arma no está permitida → false
3. Si es LightSaber y el color no es la debilidad → false
4. Si pasa todo → true

ZONA EDITABLE:
  EsEtiquetaPermitida()  → el alumno la completa en Escena 3
  EsColorVulnerable()    → el alumno la completa en Escena 4
```

#### `void TakeDamage(int amount, DamageType damageType, GameObject source)`
```
Aplica el daño. Solo se llama después de que CanBeDamagedBy devuelva true.
Pasos:
1. Reinicia el cooldown.
2. Resta la vida (AplicarDanioDirecto).
3. Lanza el evento onHit (si hay animación de impacto asignada).
4. Procesa el efecto de estado (veneno, sangrado, o nada).
El alumno NO toca esta función.
```

#### `bool EsEtiquetaPermitida(string etiqueta)` ← ZONA EDITABLE Escena 3
```
Comprueba si el arma que golpea está en la lista allowedWeaponTags.
Si la lista está vacía → cualquier arma puede dañar (devuelve true).
Si tiene tags → solo los tags de la lista pueden dañar.

ESTADO ACTUAL: implementada con bucle for.
EJERCICIO Escena 3: el alumno tiene que entender el bucle for
y verificar que los tags del Inspector coincidan con los del arma.

Ejemplo de uso en Inspector:
  allowedWeaponTags = ["Rapier"]    → solo el rapier daña
  allowedWeaponTags = ["LightSaber"] → solo el sable daña
  allowedWeaponTags = []            → cualquier arma daña
```

#### `bool EsColorVulnerable(DamageType tipo)` ← ZONA EDITABLE Escena 4
```
Comprueba si el DamageType del golpe está en vulnerableTypes.
Misma lógica que EsEtiquetaPermitida pero con tipos de daño.

ESTADO ACTUAL: implementada con bucle for.
EJERCICIO Escena 4: el alumno tiene que asignar correctamente
los vulnerableTypes en el Inspector de cada enemigo Alonso
según el color de su mono.

Ejemplo:
  Alonso rojo   → vulnerableTypes = [Red]
  Alonso azul   → vulnerableTypes = [Blue]
  Alonso gris   → vulnerableTypes = [Gray]
```

#### `void ActivarVeneno()` / `void ProcesarVenenoPorTiempo()` ← ZONA EDITABLE Escena 5
```
ESTADO ACTUAL:
  ActivarVeneno() pone estaEnvenenado = true y resetea los timers.
  ProcesarVenenoPorTiempo() descuenta vida cada segundo durante 6s.

EJERCICIO Escena 5:
  El alumno tiene que rellenar el while conceptual que simula
  el efecto del veneno tick a tick.
  El while ya está implementado con Time.deltaTime en Update(),
  pero el alumno debe entender la lógica y configurar los valores:
    DURACION_VENENO       = 6f  (segundos que dura)
    INTERVALO_VENENO      = 1f  (cada cuánto hace daño)
    DANIO_VENENO_POR_TICK = 0.5f (cuánto daño por tick)
```

#### `void ActivarSangrado()` / `void ProcesarExplosionDeSangrado()` ← ZONA EDITABLE Escena 5
```
ESTADO ACTUAL:
  Cada golpe de tipo Bleed incrementa golpesDeSangrado.
  Al llegar a 5 golpes, llama a ProcesarExplosionDeSangrado()
  que inflige 3 de daño directo y resetea el contador.

EJERCICIO Escena 5:
  El alumno tiene que colocar el if que detecta que
  golpesDeSangrado ha llegado a GOLPES_PARA_EXPLOTAR.

  Valores configurados:
    GOLPES_PARA_EXPLOTAR  = 5
    DANIO_EXPLOSION_BLEED = 3
```

#### `void Morir()`
```
Destruye el GameObject del enemigo.
Antes llama al evento onDeath (para animaciones de muerte).
El alumno NO la toca.
Analogía C++: delete this; (destruye el objeto de la escena)
```

#### Funciones de Debug (encapsuladas — el alumno NO las ve)
```
DebugInit()     → se llama en InicializarVida()
DebugCooldown() → se llama en EstaEnCooldown() y ReiniciarCooldown()
DebugDanio()    → se llama en TakeDamage()
DebugVeneno()   → se llama en ProcesarVenenoPorTiempo()
DebugSangrado() → se llama en ProcesarExplosionDeSangrado()
DebugMuerte()   → se llama en Morir()

El alumno llama a estas funciones por nombre en sus zonas editables.
Nunca ve Debug.Log() directamente.
```

### Variables configurables desde el Inspector
```
maxHealth           → vida máxima del enemigo (editable en escena 2)
cooldownEntreGolpes → invencibilidad entre golpes (0.25s por defecto)
vulnerableTypes[]   → tipos de daño que le afectan (escena 4)
allowedWeaponTags[] → armas que pueden dañarle (escena 3)
```

---

## SCRIPT 3 — RapierHitbox.cs (RapierHitBox-4.cs)
**Dónde está en Unity**: en el GameObject de la hoja del Rapier.
**Analogía C++**: struct con callback de colisión.

### Qué hace
Detecta cuándo la hoja del rapier toca a un enemigo durante un swing.
Consulta al Damageable si puede dañarle y si puede, aplica el daño.
Gestiona que el mismo enemigo no reciba dos golpes en el mismo swing.

### Funciones clave

#### `void DetectarCambioDeModo()` + `AplicarModoNormal/Veneno/Sangrado()`
```
ESTADO ACTUAL: implementado con if/if/if en DetectarCambioDeModo().
Lee las teclas 4/5/6 y llama a la función correspondiente.

MEJORA POSIBLE (Escena 5):
  El alumno podría refactorizarlo con switch en vez de if/if/if.
  No es obligatorio pero se menciona como buena práctica.
```

#### `void OnTriggerEnter(Collider other)` (encapsulada)
```
Callback que Unity llama cuando la hitbox toca algo.
El alumno NO la ve. Internamente llama a:
  EsGolpePropio() → ignora golpes al propio personaje
  BuscarVidaEnEnemigo() → busca el Damageable del enemigo
  YaFueGolpeadoEnEsteSwing() → evita doble golpe en el mismo swing
  damageable.CanBeDamagedBy() → pregunta si puede dañar
  damageable.TakeDamage() → aplica el daño
```

#### `void Reactivar()`
```
Limpia el registro de "ya golpeé a este enemigo".
Se llama desde SimpleWalk cada vez que se activa una hitbox nueva.
El alumno NO la toca.
```

### Variables configurables
```
damage    → daño por golpe (EDITABLE en escena 2 con valor real > 0)
ownerRoot → referencia al personaje raíz (asignada en Inspector)
```

---

## SCRIPT 4 — RapierColorController.cs (changetype-7.cs)
**Dónde está en Unity**: en el GameObject de la hoja del Rapier (mismo que RapierHitbox).
**Analogía C++**: clase separada que solo gestiona el estado visual + input.

### Separación de responsabilidades (patrón importante)
```
RapierColorController → SOLO se preocupa del color visible y del input.
RapierHitbox          → SOLO se preocupa de detectar golpes.
Ambos se comunican a través del campo damageType de RapierHitbox.
```

### Funciones clave

#### `void EstablecerModo(DamageType tipo)` ← PUNTO DE ENTRADA
```
Función pública que cualquier script puede llamar para forzar un cambio.
Pasos internos (todos encapsulados para el alumno):
1. Guarda el tipo en currentDamageType.
2. Llama a AplicarColorActual() → cambia el color de la hoja.
3. Llama a NotificarHitbox()    → sincroniza con RapierHitbox.
4. Llama a MostrarMensajeDeModo() → DebugRapier().
```

#### `Color ElegirColorSegunModo(DamageType tipo)`
```
switch sobre el tipo → devuelve el Color correspondiente.
Normal   → gris acero  (0.78, 0.78, 0.78)
Veneno   → morado suave (0.72, 0.52, 0.95)
Sangrado → rojo suave   (0.95, 0.42, 0.42)

ZONA EDITABLE (Escena 5 opcional):
  El alumno puede cambiar los colores ajustando los valores RGB.
  No cambia la lógica, solo el aspecto visual.
```

---

## SCRIPT 5 — LightsaberColorController.cs (ChangeColors-6.cs)
**Dónde está en Unity**: en el GameObject de la hoja del sable láser.

### Qué hace
Igual que RapierColorController pero para el sable láser.
Teclas 1/2/3 → Rojo/Azul/Gris.
Cambia el color de la hoja y notifica al WeaponHitbox.

### Funciones clave

#### `void SetSaberColor(DamageType type)` ← PUNTO DE ENTRADA
```
Cambia currentDamageType, aplica el color y sincroniza con la hitbox.
```

#### `void ApplyCurrentColor()` (con switch encapsulado)
```
switch(currentDamageType):
  Red  → Color.red
  Blue → Color.cyan
  Gray → Color.gray
El alumno NO toca esto.
```

### ZONA EDITABLE (Escena 3)
```
El alumno tiene que entender que:
- Tecla 1 → SetSaberColor(DamageType.Red)   → daña a enemigos rojos
- Tecla 2 → SetSaberColor(DamageType.Blue)  → daña a enemigos azules
- Tecla 3 → SetSaberColor(DamageType.Gray)  → daña a enemigos grises
Y que el color del sable debe coincidir con la debilidad del enemigo.
No hay que modificar el script. Solo hay que usarlo correctamente.
```

---

## SCRIPT 6 — WeaponHitbox.cs (LightSaberHitbox-11.cs)
**Dónde está en Unity**: en el GameObject de la hoja del sable láser.
**Analogía C++**: callback de colisión genérico para armas simples.

### Qué hace
Detecta colisiones igual que RapierHitbox pero sin gestión de modos.
Es la versión más simple: detecta, pregunta, aplica.

### Diferencia con RapierHitbox
```
WeaponHitbox  → solo detecta y aplica. Sin lógica de modos.
RapierHitbox  → detecta, aplica, Y gestiona los modos Normal/Veneno/Sangrado.
```

### ZONA EDITABLE (Escena 2)
```
damage → el alumno debe asignar un valor mayor que 0 en el Inspector.
Si damage = 0, los enemigos no recibirán daño nunca.
```

---

## SCRIPT 7 — KickHitbox.cs (KickCollider-10.cs)
**Dónde está en Unity**: en los GameObjects de los colliders de patada.
**Analogía C++**: misma estructura que WeaponHitbox pero para las patadas.

### Qué hace
Exactamente igual que WeaponHitbox.
Detecta colisiones cuando el colisionador de patada está activo,
pregunta al Damageable, aplica el daño.

### ZONA EDITABLE (Escena 2) ← EJERCICIO PRINCIPAL
```
damage → DEBE ser mayor que 0. El alumno lo configura en el Inspector.

El ejercicio real está en SimpleWalk.Update():
  El alumno tiene que escribir los if/else que activan estas hitboxes
  al pulsar K y M. KickHitbox no se modifica directamente.

  Ejemplo de estructura que el alumno tiene que completar:

  // En SimpleWalk.Update():
  if (/* tecla K pulsada */)
  {
      // TODO: lanzar animación de chut
      // TODO: activar la hitbox correcta
  }
  else if (/* tecla M pulsada */)
  {
      // TODO: lanzar animación de martelo
      // TODO: activar la hitbox correcta
  }
```

---

## SCRIPT 8 — PlayerHealth.cs (PlayerHealth-12.cs)
**Dónde está en Unity**: en el GameObject del personaje ninja.

### Qué hace
Gestiona la vida del jugador.
Tiene `vida` y `vidaMax`.
Funciones: `RecibirDanio(float)` y `Curar(float)`.
El Mathf.Clamp en Update() garantiza que vida nunca salga de [0, vidaMax].

### ZONA EDITABLE (futura — UI)
```
El alumno puede leer vida y vidaMax para actualizar
la barra de vida de la interfaz gráfica.
La lógica de vida en sí NO se toca.
```

---

## SCRIPT 9 — DamageTypes.cs (DamageTypes-9.cs)
**Dónde está en Unity**: archivo independiente, no en ningún GameObject.

### Qué hace
Define el enum DamageType con todos los tipos posibles:
```
DamageType.Red     → sable láser rojo
DamageType.Blue    → sable láser azul
DamageType.Gray    → sable láser gris
DamageType.Normal  → rapier sin efecto
DamageType.Poison  → rapier veneno
DamageType.Bleed   → rapier sangrado
```

### ZONA EDITABLE
```
Si en una escena futura se necesita un nuevo tipo de daño,
se añade aquí y se trata en el switch de Damageable.
El alumno NO lo modifica hasta que se necesite.
```

---

## SCRIPT 10 — Potion.cs (potion-3.cs)
**Dónde está en Unity**: en los GameObjects de las pociones del escenario.

### Qué hace
Cuando el jugador toca la poción (OnTriggerEnter),
busca el PlayerHealth del jugador y llama a Curar().
Luego se destruye a sí misma.

### ZONA EDITABLE
```
cantidadCuracion → cuánta vida restaura cada poción.
El alumno puede ajustar este valor en el Inspector.
```

---

## SCRIPT 11 — CameraPlayer.cs (cameraplayer-5.cs)
**Dónde está en Unity**: en la cámara principal.

### Qué hace
La cámara sigue al jugador desde una posición fija relativa.
Siempre mira al jugador.
**ESTADO ACTUAL**: funcional básico (sigue al jugador).
**PENDIENTE**: sistema de tercera persona completo con rotación.

### ZONA EDITABLE (futura — cámara tercera persona)
```
Este es uno de los sistemas pendientes del proyecto.
El script actual hace seguimiento simple.
La cámara de tercera persona completa está en el TODO del proyecto.
```

---

## Resumen rápido: qué toca el alumno y en qué escena

| Script | Función editable | Escena |
|--------|-----------------|--------|
| SimpleWalk.cs | if/else patadas en Update() | 2 |
| KickHitbox.cs (Inspector) | damage > 0 | 2 |
| WeaponHitbox.cs (Inspector) | damage > 0 | 2 |
| Damageable.cs (Inspector) | maxHealth > 0 | 2 |
| LightsaberColorController (uso) | entender teclas 1/2/3 | 3 |
| Damageable → EsEtiquetaPermitida | lógica de tags | 3 |
| Damageable → EsColorVulnerable | lógica de colores | 4 |
| Damageable (Inspector) | vulnerableTypes por enemigo | 4 |
| Sistema XP (pendiente) | función DarXP() | 4 |
| Damageable → veneno (valores) | DURACION, INTERVALO, DANIO | 5 |
| Damageable → sangrado (if) | if golpesDeSangrado >= 5 | 5 |
| RapierColorController (colores) | valores RGB opcionales | 5 |
| Boss (pendiente) | switch teclas 7/8/9 | 6 |
| Boss (pendiente) | AplicarAturdimiento() | 6 |
| Boss (pendiente) | AplicarConfusion() | 6 |
| Boss (pendiente) | if && golpe final | 6 |

---

## Sistemas pendientes (aún no implementados)

### Sistema de XP
```
Pendiente. Deberá implementarse antes de la Escena 4.
El alumno crea una función DarXP(int cantidad) que acumula
puntos según el tipo de enemigo:
  Alien      → 1 XP
  Mousey     → 5 XP
  Fernando   → 10 XP
  Otros      → 15 XP
Se llama desde onDeath de cada enemigo.
```

### Sistema mente colmena / comportamiento enemigo
```
Pendiente. Los enemigos actuales no tienen IA.
Se necesita:
  - Navegación con NavMesh (Unity ya lo tiene)
  - Clase base EnemyBase con estados: IDLE, PURSUE, ATTACK
  - Sistema de turnos para que no todos ataquen a la vez
```

### Dodging del jugador
```
Pendiente. SimpleWalk no tiene esquiva.
Se añadirá como nuevo IEnumerator en SimpleWalk.
Tecla a definir.
```

### Cámara de tercera persona completa
```
CameraPlayer.cs actual: seguimiento básico.
Pendiente: rotación con ratón, distancia ajustable,
colisión de cámara con paredes.
```

### Pantallas de historia (texto negro)
```
Pendiente. Se implementará con un sistema de UI:
Canvas → Panel negro → Text → array de string[] con los párrafos
→ avanzar con ENTER.
```

### Sistema de locks de escena
```
Pendiente. Cada escena necesita verificar las condiciones
antes de cargar la siguiente.
Se implementará con un GameManager que comprueba
las variables del alumno antes de activar el trigger de salida.
```

### Sistema de boss (Escena 6)
```
Pendiente. Nuevo script BossEnemy.cs con:
  estadoBoss: 0=normal, 1=aturdido, 2=confundido, 3=derrotado
  contadorAturdimiento: int (objetivo: 8)
  contadorConfusion: int (objetivo: 8)
  funciones AplicarAturdimiento() y AplicarConfusion()
  lógica de IDLE 5s al aturdirse
  lógica de cambio de objetivo al confundirse
```

### Barras de UI (vida, XP, círculo boss)
```
Pendiente. Se implementará con:
  Image con fillAmount para la barra de vida (rojo)
  Image con fillAmount para la barra de XP (dorado)
  Image para el círculo del boss (color variable)
```
