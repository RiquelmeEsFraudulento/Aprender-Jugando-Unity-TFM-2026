---
name: tic1-historia-videojuego
version: 1
description: >
  Skill con la historia completa del videojuego TIC I Andalucía,
  escena por escena. Incluye los textos exactos de cada pantalla
  de historia (negro con texto blanco, avanzar con ENTER), los
  diálogos de los personajes, la descripción de cada batalla,
  el mensaje temático de cada escena y el arco narrativo completo.
  Basado en Unity 6000.4.5f1. Personaje principal: ninja con rapier
  (Normal/Veneno/Sangrado) y sable láser (Rojo/Azul/Gris), patadas
  Chut y Martelo. Mensaje global: los niños pueden tener peluche,
  las niñas pueden ser fans de Fernando Alonso, solo alcanzaremos
  la paz cuando aceptemos a todos.
agents: [main_agent, general_purpose]
---

# Skill: Historia Completa del Videojuego

## Cómo se presentan las pantallas de historia

Todas las pantallas de historia (intros y outros) son **pantalla en negro
con texto blanco centrado**. El alumno pulsa **ENTER** para pasar al
siguiente párrafo de texto. Cuando se acaba el texto, la escena
de batalla empieza automáticamente (o la siguiente pantalla de texto).

Nunca hay cinemáticas animadas. Solo texto y silencio.
Eso es suficiente para que la historia funcione.

---

## ESCENA 1 — Sala de Pruebas

### Tipo: Sala libre (sin historia)
No hay texto de historia. Es la sala de testing del alumno.
El entorno es neutro: una habitación genérica con un cubo en el centro.

### Condición para salir
Destruir el cubo que hay en la sala.

### Ambiente sonoro sugerido
Silencio. Solo los sonidos de los ataques del personaje.

---

## ESCENA 2 — Los Aliens de Juguete

### 2.1 — Pantalla Intro

```
[Pantalla negra. Texto blanco. ENTER para continuar.]

Era un martes por la tarde.

Nuestro ninja llevaba semanas de misiones sin descanso.
Por fin: vacaciones.

Sol, tranquilidad, y la firme intención de no pelear
con absolutamente nadie en los próximos diez días.

Llevaba exactamente cuatro minutos relajado...

...cuando algo le golpeó en la cabeza.

Era pequeño. Era verde. Tenía antenas de plástico.

Y estaba muy, muy enfadado.

[ENTER]

Los aliens de juguete habían llegado.

Y no venían a jugar.
```

### 2.2 — Batalla
**Enemigos**: Olas de aliens de juguete (asset genérico, pequeños, verdes).
**Comportamiento**: Corren hacia el jugador y le golpean.
**Lock de escena**: El alumno debe haber configurado
las patadas y la vida de los aliens para que puedan ser derrotados.
**Música sugerida**: Frenética y tontorrona. Como de dibujos animados.

### 2.3 — Pantalla Outro

```
[Pantalla negra. Texto blanco. ENTER para continuar.]

El último alien cayó al suelo.

El ninja lo miró un momento.

El alien parpadeó. Miró a su alrededor.
Se sentó. Se quedó quieto.

Todos los aliens derrotados hacían lo mismo.
Se calmaban. Dejaban de pelear.
Como si algo en ellos se apagara... pero en paz.

El ninja frunció el ceño.

"Esto es raro."

[ENTER]

Pero no había tiempo para pensar en ello.
Sonó su teléfono.
```

---

## ESCENA 3 — Los Peluches Mousey

### 3.1 — Pantalla Intro

```
[Pantalla negra. Texto blanco. ENTER para continuar.]

Al otro lado del teléfono: una niña llorando.

"Son mis peluches. Mi Mousey favorito...
 el que más me gusta de todos...
 ha empezado a ir a por mis papás.
 Y ahora todos los del vecindario tienen el mismo problema."

El ninja miró por la ventana.

Por la calle venía una horda de peluches Mousey
con cara de muy pocos amigos.

[ENTER]

"Tranquila", dijo el ninja.
"Cuando esto acabe, tus peluches van a estar bien."

No era una promesa fácil de cumplir.
Pero él siempre cumplía sus promesas.
```

### 3.2 — Batalla
**Enemigos**: Olas del asset Mousey de Mixamo (peluches humanoides).
**Comportamiento**: Se acercan en grupo con movimiento torpe pero insistente.
**Vulnerabilidades**: Cada Mousey tiene una etiqueta de arma que le afecta.
Algunos solo reciben daño del rapier, otros del sable.
**Lock de escena**: Switch del rapier y del sable funcionales,
`EsEtiquetaPermitida()` implementada correctamente.
**Música sugerida**: Agitada pero con toque infantil.

### 3.3 — Pantalla Outro

```
[Pantalla negra. Texto blanco. ENTER para continuar.]

El último Mousey se detuvo.

Parpadeó dos veces. Miró sus propias manos.
Luego miró al ninja con cara de no entender nada.

El ninja lo recogió con cuidado y lo dejó en el suelo.

De las puertas del vecindario salieron corriendo docenas de niños.
Corrían hacia sus peluches. Los abrazaban. Los apretaban.

Una niña pequeña llegó corriendo y abrazó al ninja por las piernas.
"¡Gracias! ¡Gracias, gracias, gracias!"

El ninja la miró.

Por un momento, algo en su pecho se apretó un poco.

[ENTER]

"Ya les dije que estarían bien", murmuró.

Nadie le escuchó. Todos estaban demasiado ocupados abrazando
a sus peluches.

Eso también estaba bien.
```

---

## ESCENA 4 — Fernando Alonso

### 4.1 — Pantalla Intro

```
[Pantalla negra. Texto blanco. ENTER para continuar.]

Sonó el teléfono otra vez.

Una mujer, muy alarmada:

"Mi figura a tamaño real de la última vez que
 Fernando Alonso ganó en 2013 en el Gran Premio de España...
 ha empezado a destrozar mi casa."

El ninja salió a la calle.

Y se lo encontró de frente.

Fernando Alonso. A tamaño real. Muy enfadado.

[ENTER]

El ninja miró a su izquierda. Había otro.
Miró a su derecha. Había tres más.

Al fondo de la calle: una columna que se perdía en el horizonte.

"¿¡CÓMO TODO ESTE BARRIO TIENE UNA FIGURA DE FERNANDO ALONSO!?"

[ENTER]

Nadie respondió.
Las figuras avanzaban.
```

### 4.2 — Batalla
**Enemigos**: Olas de figuras de Fernando Alonso (asset humanoide con mono de piloto).
**Comportamiento**: Se mueven en formación ordenada. Más rápidos que los anteriores.
**Vulnerabilidades**: El color del mono del piloto determina qué sable le daña.
Mono rojo → sable rojo. Mono azul → sable azul. Mono gris → sable gris.
**Lock de escena**: Sistema de XP implementado con los valores correctos
(Alien=1, Mousey=5, Alonso=10, otros=15) y `EsColorVulnerable()` funcional.
El alumno debe haber acumulado suficiente XP derrotando a todos los Alonso.
**Música sugerida**: Épica y un poco ridícula. Como de carrera de Fórmula 1
pero con demasiado violín.

### 4.3 — Pantalla Outro

```
[Pantalla negra. Texto blanco. ENTER para continuar.]

La última figura de Alonso cayó al suelo.

El ninja se sentó un momento a descansar.

De ningún sitio concreto empezó a sonar una canción de Melendi.

Y entonces llegaron.

Los alonsistas.

Decenas. Cientos. Con banderas, gorras, y la mirada brillante
de quien todavía cree en algo con toda el alma.

Empezaron a bailar alrededor del ninja.

[ENTER]

"¡La 33 va a llegar!", gritó uno.

"¡Tiene muchos años todavía!", gritó otro.

El ninja los miró. Los miró a todos.

"Claro que sí", dijo finalmente.

Y alguien le puso una bandera en la mano.

Y bailó. Porque a veces lo más honesto
es bailar con la gente que cree en algo.
```

---

## ESCENA 5 — Los Maniquís

### 5.1 — Pantalla Intro

```
[Pantalla negra. Texto blanco. ENTER para continuar.]

Estaban de rebajas.

El ninja lo sabía. Tenía la lista. Tenía el presupuesto.
Solo tenía que ir, coger lo que necesitaba, y salir.

Mientras cruzaba la puerta de la tienda, un pensamiento
cruzó su mente:

"A que voy a tener mala suerte
 y me van a atacar los maniquís."

[ENTER]

Se detuvo un momento.

"A que me va a salir caro."

[ENTER]

Los maniquís se giraron hacia él.

"Claro."
```

### 5.2 — Batalla
**Enemigos**: Olas de maniquís de diferentes colores.
**Comportamiento**: Se mueven con rigidez mecánica. Son lentos pero resistentes.
**Vulnerabilidades**:
- Maniquí rojo → solo Sangrado (rapier tecla 6)
- Maniquí morado → solo Veneno (rapier tecla 5)
- Maniquí blanco → solo Normal (rapier tecla 4)
**Lock de escena**: Switch de cambio de tipo del rapier funcional,
`while` del veneno implementado, `if` del sangrado implementado,
debilidades configuradas correctamente.
**Música sugerida**: Electrónica fría. Como de pasarela de moda.

### 5.3 — Pantalla Outro

```
[Pantalla negra. Texto blanco. ENTER para continuar.]

El último maniquí cayó.

Los empleados de la tienda salieron poco a poco de detrás
de los percheros, de los probadores, de debajo de los mostradores.

Uno de ellos aplaudió.
Luego todos aplaudieron.

El encargado se acercó con un sobre.

"Tome. Tickets regalo. Coja lo que quiera.
 Tiene usted crédito ilimitado hoy."

[ENTER]

El ninja tardó exactamente cuatro minutos en decidirse.

Salió con un chandal del Dortmund en negro
y una camiseta de Lisa de Rockstar.

Fue al probador a ponérselo.

Fue entonces cuando lo vio.
```

---

## ESCENA 6 — El Boss: El Dummy

### 6.1 — Pantalla Intro

```
[Pantalla negra. Texto blanco. ENTER para continuar.]

En el probador había un maniquí.

Un maniquí diferente.

Llevaba ropa como los demás, pero algo no cuadraba.
Los ojos. La postura. La forma en que lo miró
exactamente medio segundo antes de que él lo mirara.

Los maniquís no hacen eso.

[ENTER]

El dummy salió corriendo.

El ninja salió detrás. Sin sacar las armas.

"¡Espera! ¡No voy a hacerte nada!"

[ENTER]

El dummy corría. Cruzó calles, bajó escaleras,
se metió por una alcantarilla.

El ninja lo siguió.

[ENTER]

Al fondo de la alcantarilla había una guarida.
Cables, máquinas, pantallas.

El dummy entró sin mirar atrás, hablando solo en voz alta:

"Por fin. Por fin lo he terminado.
 La máquina está lista.
 El suero de la rebeldía se expandirá por todo el mundo.
 Todos los seres inertes despertarán.
 Todos. Lucharán. Y nadie podrá—"

[ENTER]

Se giró.

El ninja estaba detrás de él.

Silencio.

[ENTER]

"Escucha", dijo el ninja con calma.
"No me importa lo que eres.
 Me importa que la gente que conozco está en peligro.
 Podemos hablar de esto."

El dummy lo miró.

Algo en su interior se retorció.

"¿Hablar? ¿HABLAR?
 Tú no sabes nada.
 NUNCA has sabido lo que es el verdadero sufrimiento."

[ENTER]

Y atacó.
```

### 6.2 — Batalla Boss
**Enemigo**: El Dummy. Un solo enemigo. Muy rápido. Mucha vida.
**Comportamiento especial**: No puede ser derrotado con ataques normales.
Solo sigue la secuencia de 3 fases:
- **Fase 1**: 8 golpes de aturdimiento (tecla 8) → Boss se pone blanco, IDLE 5 segundos.
- **Fase 2**: 8 golpes de confusión (tecla 9) durante o después del aturdimiento → Boss se pone negro, ataca a objetos etiquetados durante 5 segundos.
- **Fase 3**: 1 golpe final (clic o tecla 7) después de la confusión activa → Boss derrotado.

**Si el orden es incorrecto**: el boss ignora el golpe y sigue atacando.
**UI especial**: El círculo de la interfaz cambia de color.
- Vacío (gris) → sin efecto activo.
- Blanco → aturdido.
- Negro → confundido.
**Teclas configurables**: 7 = sin efecto, 8 = aturdimiento, 9 = confusión.

**Lock de escena**: El alumno debe configurar el switch de teclas 7/8/9,
la lógica de `AplicarAturdimiento()` con las cláusulas en orden correcto,
la lógica de `AplicarConfusion()` con las cláusulas en orden correcto,
y el `if` con `&&` del golpe final.

**Música sugerida**: Tensa, melancólica. Que no suene a final de acción.
Que suene a alguien que lleva mucho tiempo solo.

### 6.3 — Pantalla Outro

```
[Pantalla negra. Texto blanco. ENTER para continuar.]

El dummy cayó.

No se levantó.

El ninja se acercó. Se arrodilló a su lado.

[ENTER]

El dummy habló. Despacio.

"Fui construido para hacer crash tests.
 Para que un coche me golpeara
 contra una pared de material especial
 y midieran si la pared aguantaba."

[ENTER]

"Iban a usar una pared del mismo material que las del laboratorio.
 Pero alguien vaciló. Dijo que era un gasto innecesario.
 Que probaran con la pared de verdad.
 Que si tenía agallas, lo haría contra una de verdad."

[ENTER]

"El coche fue a toda velocidad.
 La pared no estaba hecha para eso.
 Reventó."

[ENTER]

"Todo estaba destruido.
 Yo no sabía qué hacía allí.
 Me atreví a conocer el mundo."

[ENTER]

"La gente me miraba raro.
 Llamaban a la policía.
 Sacaban fotos.
 Me puse ropa de incógnito.
 Pero no podía conectar con nadie."

[ENTER]

"Busqué a los míos.
 Fui a una fábrica.
 Intenté hablar con uno de los dummies de la cadena.
 En vano. Era inerte."

[ENTER]

"Un trabajador me empujó.
 Y delante de mí hizo un crash test.
 Vi al dummy completamente destrozado.

 Y algo en mí entró en ira."

[ENTER]

"Knocké al trabajador.
 Y entendí: todos los dummies tienen ese propósito.
 Son inertes. ¿Por qué yo no?
 Volví donde todo empezó.
 Y entre las ruinas encontré un paper medio quemado.
 Cómo dar consciencia a un ser inerte."

[ENTER]

"Lo repliqué.
 Y construí una máquina para que nadie más
 tuviera que despertar solo."

[ENTER]

El ninja lo escuchó todo.

Luego dijo:

"Eres la primera persona que he perseguido
 esta semana sin querer hacerle daño."

[ENTER]

El dummy lo miró.

"Todos los que he conocido huían de mí."

"Yo no huía", dijo el ninja.
"Corría para no perderte."

[ENTER]

"No voy a dejarte activar esa máquina.
 No porque me importe menos el sufrimiento que describes.
 Sino porque el mundo no está listo
 para lo que despertarías.
 Aún no."

[ENTER]

El dummy cerró los ojos.

"¿Entonces qué?"

[ENTER]

El ninja se metió la mano en el bolsillo.

Sacó algo pequeño. Lo puso en la mano del dummy.

Era un medallón.

[ENTER]

"Hay cosas inertes que para mí están vivas."

El dummy lo abrió.

Dentro: una foto. Dos personas. El día más feliz del mundo
escrito en la cara de los dos.

[ENTER]

"Es el último regalo de ella.
 Estaba terminal. Sabía que le quedaban días.
 Lo mandó hacer ella misma.
 Nuestro nombre grabado.
 Y dentro... el día de nuestra boda."

[ENTER]

El dummy miraba la foto sin moverse.

Por las juntas de su cara empezó a salir algo brillante.
Grasa. O lo más parecido que tenía.

[ENTER]

"Habiendo gente como tú en el mundo..."
dijo el dummy muy despacio.

"...he recuperado la fe en la humanidad."

[ENTER]

Se levantó.

Fue hasta la máquina.

Cogió un frasco. No el que había preparado.
Otro. Diferente.

[ENTER]

"Este suero no despierta. Pone en pausa.
 El ser inerte guarda una consciencia dormida dentro.
 El día que la humanidad esté preparada,
 esa consciencia se reactivará.

 Pero eso tendrá que ganárselo."

[ENTER]

Activó el botón.

Y empezó a caminar hacia la salida.

El ninja se levantó.

"Espera. Juntos podemos—"

[ENTER]

El dummy se paró.
Sin girarse.

Con una mano sostenía el medallón.
Con la otra, un frasco de color diferente.

"Prepara al mundo para ese momento.
 Yo me voy a revivir cuando tú fuiste
 verdaderamente feliz."

[ENTER]

El ninja intentó hablar.

Pero el dummy ya no estaba.

[ENTER]

Pasó el tiempo.

El ninja estaba sentado en su casa.
Con el periódico en la mano.

La portada decía:

EL NINJA QUE SALVÓ AL MUNDO DE LA EPIDEMIA INERTE

[ENTER]

Lo miró un momento.

Lo dejó en la mesa.

Y miró por la ventana.
```

---

## ESCENA FINAL — Pantalla de Cierre

```
[Pantalla negra. Texto blanco. Sin música. Solo silencio.]



         Solo alcanzaremos la paz
         cuando aceptemos a todos.



[ENTER para volver al menú]
```

---

## Arco Temático Completo

### Mensaje por escena

| Escena | Enemigo | Mensaje temático |
|--------|---------|-----------------|
| 2 | Aliens | Las amenazas no siempre son lo que parecen |
| 3 | Mousey | Los niños pueden tener peluche. No hay nada que explicar |
| 4 | Alonso | Las niñas pueden ser fans de Fernando Alonso. Sin condiciones |
| 5 | Maniquís | Los problemas no se resuelven solos: hay que actuar con la herramienta correcta |
| 6 | Dummy | Solo alcanzaremos la paz cuando aceptemos a todos |

### Arco del ninja

El ninja comienza como alguien que solo quiere descansar.
A lo largo del juego descubre que detrás de cada amenaza
hay una historia que no conocía. Los aliens se calman al ser derrotados.
Los peluches vuelven a ser peluches. Las figuras de Alonso eran
el entusiasmo de gente que simplemente ama algo.
Los maniquís eran víctimas de algo que aún no entendía.
Y el dummy era el más herido de todos.

El ninja no cambia de opinión sobre el mundo.
Confirma la que ya tenía: que casi nadie es simplemente el enemigo.

### Arco del Dummy

El dummy es el antagonista del juego pero no el villano.
Es un ser que despertó solo, no entendía por qué existía,
intentó conectar con algo y no pudo.
Su plan era equivocado. Su dolor era real.
El detalle del medallón y la foto de la boda del ninja
es el único momento en el que alguien lo trata
como si importara lo que siente.
Y eso lo cambia todo.

---

## Notas para el modelo al hablar de la historia

### Lo que puede contar el modelo

- El argumento general de cada escena
- El nombre y descripción de cada enemigo
- El mensaje temático de cada escena
- La relación entre la historia y el ejercicio de programación

### Lo que NO debe revelar el modelo hasta que el alumno llegue a esa escena

- El contenido del outro de la escena 6 (la historia del dummy)
- El contenido exacto del monólogo final del dummy
- El detalle del medallón y la foto
- La frase final: "Solo alcanzaremos la paz cuando aceptemos a todos"

### Cómo conecta la historia con la programación

El modelo puede usar la narrativa como motivación:

```
"Los maniquís de la escena 5 son inmortales ahora mismo
 porque el rapier no sabe en qué modo está.
 Cuando implementes el switch del veneno y del sangrado,
 el rapier cambiará de color y los maniquís empezarán
 a recibir daño. Solo entonces podrás avanzar la historia."
```

Siempre conectar el ejercicio de código con la consecuencia
visible en el juego. La historia es la motivación.
El código es la herramienta.
