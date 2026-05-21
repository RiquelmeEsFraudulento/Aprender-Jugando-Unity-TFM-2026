---
name: tfm-docente-tic1-unity
version: 1
description: >
  Skill maestra, extremadamente detallada, para generar toda la documentación
  del TFM y del proyecto docente basado en Unity 6000.4.5f1 para TIC I de Andalucía.
  Incluye marco teórico, fundamentación curricular, diseño didáctico, cronograma de 28 horas,
  desarrollo de materiales, metodología, atención a la diversidad, evaluación, conclusiones,
  limitaciones, líneas futuras, y reglas de redacción en formato LaTeX.
agents: [main_agent, general_purpose]
---

# Skill: Documentación total del TFM y del proyecto

## Propósito de esta skill

Esta skill existe para que el modelo pueda generar **toda la documentación necesaria**
del TFM y del proyecto docente sin perder el enfoque real del trabajo.
No es una skill breve: es la **skill central** del sistema.
Aquí se fijan el marco, el tono, la estructura, la justificación pedagógica,
la conexión curricular, el proyecto de Unity, la narrativa del videojuego,
la filosofía de encapsulamiento, y la forma concreta en que deben redactarse
los apartados académicos.

Esta skill debe usarse cuando se pida cualquiera de estas tareas:
- redactar apartados completos del TFM;
- desarrollar secciones del marco teórico;
- construir la programación didáctica o la unidad didáctica;
- generar cronogramas, sesiones o instrumentos de evaluación;
- redactar la documentación del proyecto de Unity como recurso educativo;
- justificar curricularmente la propuesta para TIC I en Andalucía;
- escribir materiales para profesorado o alumnado;
- preparar texto reusable para memoria, anexos, manuales o documentación docente.

---

# 1. Identidad exacta del proyecto

## 1.1 Naturaleza del trabajo

El trabajo propone el diseño de una experiencia docente para **Tecnologías de la Información y la Comunicación I**
de Bachillerato en Andalucía, centrada en la enseñanza de la programación mediante
la creación y modificación guiada de un pequeño videojuego en Unity.

No se plantea como una inmersión profesional en desarrollo de videojuegos,
sino como una **adaptación curricular fuertemente encapsulada** de una herramienta profesional.
Unity se usa como medio motivador y visual para que el alumnado trabaje los fundamentos
básicos de programación exigidos por el currículo, sin quedar sepultado por la complejidad
real del motor, de C#, del Animator, de la arquitectura de componentes o de la lógica
completa de un videojuego comercial.

## 1.2 Herramienta y versión

La herramienta usada es **Unity 6000.4.5f1**.
Ese dato debe aparecer siempre que se describa técnicamente el proyecto,
porque concreta la base real del entorno y evita presentar la propuesta como algo abstracto.

## 1.3 Enfoque de adaptación didáctica

La idea central no es “enseñar Unity” en sentido profesional.
La idea es **usar Unity como escenario visual encapsulado** para que el alumnado
aprenda programación elemental.

Eso significa:
- el alumnado no diseña un videojuego desde cero;
- el alumnado no programa un sistema complejo completo;
- el profesorado prepara una base funcional y controlada;
- el alumnado modifica únicamente zonas muy concretas del código;
- cada modificación se alinea con contenidos básicos de TIC I;
- el entorno visual hace visible el efecto del código.

La propuesta parte de una hipótesis docente muy clara:
**hacer que lo difícil parezca accesible mediante encapsulación**.
No se trivializa la programación; se reduce la carga accidental para que el alumnado
pueda concentrarse en las estructuras cognitivas que sí son curriculares.

---

# 2. Contexto académico y curricular

## 2.1 Etapa y materia

La propuesta se sitúa en **1.º de Bachillerato**, en la materia **TIC I**,
dentro del bloque de programación.

## 2.2 Competencia específica prioritaria

La competencia específica central es la **CE5**:

> Comprender qué es un algoritmo y cómo son implementados en forma de programa,
> analizando y aplicando los principios de la ingeniería del software,
> para desarrollar y depurar aplicaciones informáticas y resolver problemas.

Esta competencia debe aparecer como eje articulador del diseño de la propuesta.

## 2.3 Criterios de evaluación trabajados

Se trabajarán especialmente dos criterios:

### 5.2
Analizar y resolver problemas de tratamiento de la información,
dividiéndolos en subproblemas y definiendo algoritmos que los resuelvan.

### 5.1
Conocer y comprender la sintaxis y la semántica de un lenguaje de programación,
analizar la estructura de programas sencillos y desarrollar pequeñas aplicaciones.

## 2.4 Saberes básicos implicados

### Unidad 12 primero: Diseño de software y resolución de problemas
Se trabajará antes que la programación en código.
Saberes:
- TICO.1.E.2.1. Enfoque Top-Down.
- TICO.1.E.2.2. Fragmentación de problemas.
- TICO.1.E.2.3. Patrones.
- TICO.1.E.2.4. Algoritmos.
- TICO.1.E.2.5. Pseudocódigo y diagramas de flujo.
- TICO.1.E.2.6. Depuración.

### Unidad 11 después: Fundamentos de programación
Saberes:
- TICO.1.E.1.1. Lenguajes de programación. Tipos. Paradigmas.
- TICO.1.E.1.2. Estructura de un programa informático y elementos básicos del lenguaje.
- TICO.1.E.1.3. Tipos básicos de datos. Constantes y variables. Operadores y expresiones.
- TICO.1.E.1.4. Estructuras de control condicionales e iterativas.
- TICO.1.E.1.5. Estructuras de control y de datos.
- TICO.1.E.1.6. Funciones y bibliotecas de funciones.

## 2.5 Justificación del orden didáctico

La propuesta defiende de forma explícita impartir **antes la Unidad 12**
(Diseño de software y resolución de problemas) y **después la Unidad 11**
(Fundamentos de programación).

Esta inversión no es arbitraria. Se justifica porque:
- primero se entrena la idea abstracta de algoritmo;
- luego se baja esa abstracción al código concreto;
- el alumnado comprende mejor por qué un programa se fragmenta en funciones;
- el videojuego permite que los patrones se visualicen antes de codificarse;
- se reduce la probabilidad de memorizar sintaxis sin entender estructura.

Además, se adelanta este bloque dentro del curso para que el alumnado
lo afronte con más frescura y con menor coincidencia con la carga intensa
de exámenes finales. También se justifica que se le dediquen más horas
que a otros bloques por su importancia estructural para estudios posteriores.

## 2.6 Redistribución temporal de la materia

La propuesta sostiene que este bloque debe recibir más peso horario,
reduciendo parcialmente el tiempo dedicado a:
- La sociedad de la información y el ordenador.
- Software de aplicación.
- Procesadores de texto.

La justificación debe redactarse siempre desde un enfoque académico:
no se dice que esos bloques no importen, sino que el bloque de programación
posee un valor formativo especialmente alto por su potencia para desarrollar
pensamiento algorítmico, resolución de problemas, depuración y transferencia
a aprendizajes posteriores.

---

# 3. Contexto del centro y del alumnado

## 3.1 Centro hipotético

Debe integrarse, cuando proceda, el contexto ya definido:
- colegio privado;
- enseñanzas de ESO, Bachillerato y ciclos de FP;
- cultura pedagógica centrada en trabajar bien los fundamentos;
- continuidad hacia estudios posteriores de informática y telecomunicaciones;
- aulas con ordenadores suficientemente potentes;
- disponibilidad amplia de espacios de trabajo.

## 3.2 Perfil del alumnado

Debe mantenerse la idea de alumnado heterogéneo:
- diversidad cultural y personal;
- entorno socioeconómico medio;
- posibles incidencias puntuales de disponibilidad de equipo;
- importancia de diseñar materiales consultables dentro y fuera del aula.

## 3.3 Necesidades educativas y clima de aula

El modelo debe recordar que en el centro la presencia de alumnado con necesidades específicas
es superior a la media, con perfiles frecuentes como:
- pérdida auditiva o visual parcial;
- altas capacidades;
- discapacidad intelectual;
- TDAH;
- TEA.

Además, debe incorporarse el problema detectado de **bullying o estigmatización**
cuando las adaptaciones resultan demasiado visibles.
Por ello, la propuesta debe presentar las medidas de atención a la diversidad
como **ajustes universales**, no como parches aislados para un solo alumno.

## 3.4 Problemáticas detectadas

Hay dos problemáticas clave que siempre deben aparecer cuando se justifique la propuesta:

### Absentismo y dilación en entregas
- TIC I es optativa;
- se subordina a materias troncales;
- hay faltas concentradas antes de exámenes de otras asignaturas;
- las prácticas se hacen a veces con prisa o a última hora;
- una ausencia en una sesión de while o funciones provoca bloqueo en sesiones posteriores;
- materiales autónomos, manuales y vídeos ayudan a reducir ese problema.

### Desmotivación y escasa percepción de utilidad
- ejercicios tradicionales en C descontextualizados no conectan con el alumnado;
- la pregunta “¿para qué sirve esto?” aparece con frecuencia;
- ver que el código mueve un personaje 3D o activa una acción responde visualmente a esa duda;
- la propuesta se apoya en la percepción de utilidad y relevancia personal del contenido.

Pueden añadirse otras problemáticas de manera medida:
- bloqueo inicial ante la sintaxis;
- miedo a equivocarse;
- diferencias previas de experiencia con videojuegos o informática.

---

# 4. Identidad pedagógica del videojuego

## 4.1 Qué es el videojuego dentro del TFM

El videojuego no es el fin, sino el vehículo didáctico.
Es un recurso para enseñar programación básica de forma visual,
guiada, progresiva y curricularmente alineada.

## 4.2 Principio de encapsulamiento

La palabra clave de toda la propuesta es **encapsulamiento**.

Se debe explicar siempre de forma muy clara:
- Unity es compleja como herramienta profesional;
- el alumnado de TIC I no necesita dominar su complejidad total;
- el profesorado oculta la parte difícil del sistema;
- el alumnado trabaja solo “islas de código” donde puede aplicar if, else, switch, for, while, funciones y structs básicos;
- el resultado visible refuerza el aprendizaje conceptual.

### Fórmula de redacción recomendada

Cuando se explique esta idea, usar una formulación similar a:

> Unity es una herramienta profesional, pero su adaptación curricular exige simplificarla.
> Para ello, la propuesta encapsula la complejidad técnica del motor y expone al alumnado
> únicamente fragmentos de código muy concretos, alineados con los saberes básicos de TIC I.
> De este modo, el estudiante no tiene que comprender desde el primer momento el ciclo de vida completo
> de Unity ni la arquitectura total del proyecto, sino que puede centrarse en estructuras básicas de programación
> observando de inmediato su efecto en pantalla.

## 4.3 Filosofía de visibilidad del aprendizaje

Cada ejercicio debe poder responder a tres preguntas:
- qué concepto curricular se trabaja;
- qué modifica el alumno en el código;
- qué observa en pantalla cuando lo ha hecho bien.

---

# 5. Estructura narrativa del videojuego

## 5.1 Regla general de escenas

Cada escena sigue la estructura:
- pantalla negra con texto blanco de introducción;
- fase jugable o de combate;
- pantalla negra con texto blanco de cierre.

El jugador avanza los textos con Enter.
Antes de cada escena jugable se le explican tips y se le indica
qué debe implementar para poder superarla.

## 5.2 Regla de bloqueo pedagógico

No se puede superar una escena si no se han cumplido los requisitos didácticos.
Esto no es un castigo, sino un mecanismo de integración real entre narrativa y aprendizaje.
La progresión del juego depende de haber resuelto correctamente los ejercicios.

## 5.3 Resumen de escenas

### Escena 1
Sala de prueba. Testing inicial. Para pasar hay que romper un cubo concreto.

### Escena 2
Aliens de juguete.
Lock pedagógico: ataques básicos, patadas, if/else, variables fáciles.

### Escena 3
Mousey / peluches.
Lock pedagógico: switch de ataques, struct básico del personaje,
EsEtiquetaPermitida().

### Escena 4
Fernando Alonso.
Lock pedagógico: XP por tipo de enemigo y EsColorVulnerable().

### Escena 5
Maniquíes.
Lock pedagógico: switch de tipos del rapier, veneno, sangrado,
while de vulnerabilidad conceptual.

### Escena 6
Boss final Dummy.
Lock pedagógico: aturdimiento, confusión, orden correcto de condiciones,
UI de superefectos, debilidades finales, golpe remate.

### Final
Mensaje: “Solo alcanzaremos la paz cuando aceptemos a todos”.

## 5.4 Mensaje cultural y ético del proyecto

El modelo debe reconocer siempre que el juego transmite un mensaje de fondo:
- los niños pueden tener peluches;
- las niñas pueden ser fans de Fernando Alonso;
- lo importante no es encajar en estereotipos;
- la paz solo se alcanza cuando se acepta a todos.

Este mensaje no debe presentarse de forma panfletaria,
sino integrado en la lógica narrativa y en la resolución del conflicto final.

---

# 6. Estado actual del proyecto técnico

## 6.1 Sistemas ya existentes

Deben considerarse ya creados, aunque mejorables:
- script principal del jugador;
- hitboxes de rapier, lightsaber y patadas;
- Damageable;
- DamageTypes;
- control básico de colores;
- PlayerHealth;
- cámara simple de seguimiento;
- pociones u objetos de curación.

## 6.2 Sistemas pendientes o incompletos

Deben tratarse como parte del desarrollo o mejora:
- sistema de combate mente colmena;
- comportamiento enemigo con físicas de Unity, sin ragdoll;
- clases enemigas mejor definidas;
- dodging del jugador;
- cámara de tercera persona completa;
- sistema de experiencia;
- sistema de locks de escena;
- UI final completa;
- boss final con confusión y aturdimiento;
- pantallas negras con texto blanco.

## 6.3 Regla documental importante

Cuando se redacte la memoria del proyecto, debe diferenciarse siempre entre:
- lo que ya existe funcionalmente;
- lo que existe pero puede refinarse;
- lo que aún es una línea de desarrollo pendiente.

Eso da honestidad académica al TFM.

---

# 7. Marco teórico del TFM

## 7.1 Función del marco teórico

El marco teórico no describe aún la práctica concreta.
Su misión es justificar por qué tiene sentido la propuesta.
Debe conectar videojuegos, aprendizaje, motivación, programación y Bachillerato.

## 7.2 Subapartado: videojuegos como recurso educativo

Debe incluir:
- evolución del discurso desde críticas clásicas (violencia, adicción, estereotipos)
  hacia una visión más equilibrada;
- beneficios señalados por la literatura reciente: atención, habilidades espaciales,
  resolución de problemas, motivación, resiliencia, colaboración;
- 2–3 estudios de referencia.

### Referencias útiles ya identificadas
- Granic, Lobel y Engels (2014), *American Psychologist*: beneficios cognitivos,
  motivacionales, emocionales y sociales del videojuego [web:20][web:21].
- Papastergiou (2009): aprendizaje basado en juego digital en informática de secundaria/bachillerato,
  con mejora de motivación y resultados [web:25][web:31].
- Currículo andaluz TIC I y II / BOJA: encaje curricular de la competencia y saberes [web:29].

## 7.3 Subapartado: PEGI

Debe incluir una breve explicación de:
- qué es PEGI;
- para qué sirve;
- qué tipo de contenidos no serían admisibles en el aula;
- por qué la propuesta usa un enfoque visual no realista, sin gore,
  sin violencia explícita y con intencionalidad educativa.

Solo una página aproximadamente.

## 7.4 Subapartado: pros y contras

Debe presentar beneficios y riesgos del uso de videojuegos como recurso docente.

### Beneficios
- motivación;
- feedback inmediato;
- visualización de conceptos abstractos;
- aprendizaje por proyectos;
- clima emocional más positivo;
- mayor percepción de utilidad.

### Riesgos
- distracción;
- centrarse solo en lo lúdico;
- desigualdades previas entre alumnado con distinta experiencia;
- gamificación superficial;
- dependencia excesiva del componente espectacular.

Siempre debe añadirse que la propuesta adopta un **enfoque controlado y curricular**.

## 7.5 Subapartado: experiencias previas

Debe intentarse localizar experiencias previas en Bachillerato.
Si no hubiera suficientes, se puede ampliar a secundaria o contextos cercanos,
siempre indicando honestamente ese desplazamiento.

---

# 8. Objetivos del TFM y de la propuesta

## 8.1 Objetivo general

Debe formularse en términos similares a:

> Diseñar una propuesta didáctica para TIC I de Bachillerato que facilite la comprensión
> y retención de las estructuras básicas de programación mediante el desarrollo guiado
> de un videojuego sencillo en Unity, desde un enfoque inclusivo, accesible y curricularmente alineado.

## 8.2 Objetivos específicos para Bachillerato

Se pueden usar identificadores como:
- OBJBACH-1: comprender fundamentos de programación.
- OBJBACH-2: mejorar la retención de contenidos básicos.
- OBJBACH-3: aumentar la percepción de utilidad de la programación.
- OBJBACH-4: favorecer la depuración y la fragmentación de problemas.
- OBJBACH-5: reducir bloqueos iniciales mediante encapsulación y visualización.

## 8.3 Objetivos docentes personales

También se pueden incluir objetivos del docente-investigador, por ejemplo:
- mejorar el clima de aula;
- reducir el miedo inicial a programar;
- diseñar materiales reutilizables y abiertos;
- generar una propuesta transferible a otros docentes.

---

# 9. Desarrollo de la propuesta

## 9.1 Programación didáctica de TIC I

Cuando se redacte este apartado, debe explicarse:
- cómo se inserta la propuesta en la materia;
- por qué se eligen esos saberes básicos;
- cómo se relaciona con la CE5;
- cómo se distribuye temporalmente en el trimestre o curso.

## 9.2 Unidad didáctica

Debe explicarse:
- que se sitúa en el Bloque E: Programación;
- qué saberes concretos se trabajan;
- qué criterios de evaluación se conectan;
- cuál es la secuencia didáctica general:
  1. explicación conceptual breve;
  2. visualización en Unity;
  3. ejercicio guiado en código;
  4. puesta en común;
  5. reflexión / depuración.

## 9.3 Proyecto de Unity como REA

Debe describirse el proyecto como **recurso educativo abierto**.

### Materiales abiertos previstos
- proyecto Unity con personaje importado y base funcional;
- documento editable con enunciados;
- soluciones comentadas para profesorado;
- vídeos breves mostrando el comportamiento correcto;
- manuales visuales;
- documentación en GitHub si se decide publicar allí.

### Licencias
Se puede recomendar Creative Commons para documentos y materiales generados,
y separar la cuestión de la licencia de assets externos cuando sea necesario.

---

# 10. Cronograma completo de 28 sesiones

## Regla general

Hay 28 sesiones de 1 hora:
- Martes: 02/12, 09/12, 16/12, 13/01, 20/01, 27/01, 03/02, 10/02, 17/02, 24/02, 03/03, 10/03, 17/03, 24/03
- Miércoles: 03/12, 10/12, 17/12, 14/01, 21/01, 28/01, 04/02, 11/02, 18/02, 25/02, 04/03, 11/03, 18/03, 25/03

La skill debe permitir generar:
- cronograma resumido;
- cronograma detallado por sesión;
- cronograma por unidades;
- cronograma en texto o tabla.

## Reparto recomendado

### Bloque 1 — Unidad 12: Diseño de software y resolución de problemas
14 sesiones.

### Bloque 2 — Unidad 11: Fundamentos de programación
14 sesiones.

## Criterio de diseño

No hacer sesiones genéricas. Cada sesión debe tener:
- objetivo de aprendizaje;
- saber(es) básico(s);
- actividad principal;
- producto esperado;
- vínculo con el videojuego.

## Estructura recomendada de las 28 sesiones

### Sesiones 1–14: Unidad 12
1. Presentación del proyecto, motivación, videojuegos y programación.
2. Qué es un algoritmo. Ejemplos con acciones del ninja.
3. Enfoque top-down y fragmentación del videojuego en subsistemas.
4. Patrones y acciones repetibles del combate.
5. Diagramas de flujo aplicados a ataques básicos.
6. Pseudocódigo de movimientos y combate.
7. Qué es depurar. Error lógico frente a error sintáctico.
8. Análisis de escena 2 como problema fragmentado.
9. Análisis de escena 3 y decisiones con switch.
10. Análisis de escena 4 y tablas de vulnerabilidad.
11. Análisis de escena 5 y efectos repetitivos / while conceptual.
12. Análisis de escena 6 y orden de condiciones.
13. Diseño en papel del flujo completo de una escena.
14. Cierre de la unidad abstracta y puente a codificación real.

### Sesiones 15–28: Unidad 11
15. Introducción a lenguaje, programa y sintaxis.
16. Variables, tipos y comentarios sobre scripts del proyecto.
17. if / else con patadas y ataques básicos.
18. switch con cambio de espada/color/modo.
19. for con ejemplos de recuento y estructuras repetidas.
20. while y lógica de repetición en veneno/sangrado.
21. funciones y encapsulación del código base.
22. structs básicos del personaje y del enemigo.
23. Implementación guiada de escena 2.
24. Implementación guiada de escena 3.
25. Implementación guiada de escena 4.
26. Implementación guiada de escena 5.
27. Implementación guiada de escena 6.
28. Cierre, reflexión, depuración final y evaluación.

### Regla de detalle

Cuando se pida el cronograma completo, cada sesión debe redactarse con 1–2 párrafos
y/o con apartados: inicio, desarrollo, cierre.

---

# 11. Desarrollo del material docente

## 11.1 Unity como herramienta adaptada al aula

Hay que explicar Unity desde cero, sin dar por hecho conocimiento técnico.

Formulación recomendada:
- Unity es un motor y entorno de desarrollo para crear videojuegos.
- Es una herramienta profesional.
- Para el aula debe simplificarse.
- El proyecto docente usa una versión ya preparada, con animator, personaje y scripts base.
- El alumnado no construye el motor; modifica comportamientos concretos.

## 11.2 Qué ve el alumnado y qué no ve

### Zonas visibles y editables
- condicionales simples;
- switch de modos;
- bucles sencillos;
- structs básicos;
- funciones pequeñas;
- variables fáciles;
- relaciones entre color, arma y enemigo.

### Zonas ocultas o no editables
- arquitectura global de Unity;
- callbacks internos complejos;
- Animator completo;
- sincronización fina de hitboxes;
- parte difícil del motor.

## 11.3 Organización por escenas

Cuando se documenten ejercicios, cada escena debe incluir:
- objetivo curricular;
- qué modifica el alumno;
- qué observa si lo hace bien;
- nivel de dificultad;
- error típico;
- pista de depuración.

## 11.4 Materiales complementarios

El modelo debe poder generar descripciones de:
- manual visual paso a paso;
- vídeos cortos subtitulados;
- soluciones de profesorado;
- guías para autocorrección;
- repositorio GitHub;
- anexos con capturas de pantalla.

---

# 12. Metodología de aula

## 12.1 Sesión tipo

Toda sesión tipo debe poder describirse así:
1. activación inicial o mini debate;
2. breve explicación teórica (10–15 min);
3. demostración en Unity;
4. trabajo en parejas o individual guiado;
5. puesta en común;
6. reflexión o depuración final.

## 12.2 Metodologías que conviene nombrar

- aprendizaje basado en proyectos;
- trabajo cooperativo;
- enseñanza explícita de estrategias de depuración;
- andamiaje fuerte al inicio y retirada progresiva;
- aprendizaje multimodal (texto, vídeo, demostración, código, resultado visual).

## 12.3 Tono metodológico

La propuesta no debe sonar a innovación vacía.
Debe presentarse como una combinación de:
- explicitud en la enseñanza;
- secuenciación progresiva;
- proyecto motivador;
- evaluación formativa continua.

---

# 13. Atención a la diversidad

## 13.1 Principio general

Las adaptaciones deben presentarse como **universales** cuando sea posible,
para evitar estigma.

## 13.2 Ajustes universales recomendados

- vídeos con subtítulos;
- manual visual;
- ritmo flexible;
- posibilidad de repetir demostraciones;
- materiales disponibles fuera del aula;
- uso de auriculares;
- instrucciones fragmentadas;
- ejemplos muy guiados;
- capturas de pantalla y señalización visual.

## 13.3 Ajustes para TDAH

- tareas fragmentadas;
- objetivos claros por sesión;
- recordatorios visuales;
- pasos breves y secuenciados;
- comprobación frecuente del progreso.

## 13.4 Ajustes para TEA

- anticipación de cambios;
- secuencias visuales;
- estructura estable de sesiones;
- instrucciones literales y poco ambiguas;
- apoyo en intereses especiales cuando sea adecuado.

## 13.5 Ajustes para otros perfiles

Se pueden contemplar también:
- subtitulado y apoyo visual para hipoacusia;
- contraste, ampliación y zoom para dificultades visuales parciales;
- rutas de ampliación para altas capacidades;
- apoyo de aula específica si existe necesidad intensa.

## 13.6 Enfoque antiestigma

Redactar siempre que:
- todos usan los mismos materiales base;
- cada cual los aprovecha a su ritmo;
- los apoyos adicionales no deben marcar públicamente al alumno.

---

# 14. Evaluación del aprendizaje y de la experiencia

## 14.1 Evaluación del aprendizaje

Debe poder generarse:
- rúbrica de ejercicios de programación;
- pequeña prueba escrita o tipo test;
- observación del desempeño en Unity;
- análisis de código entregado;
- registro de depuración y corrección de errores.

## 14.2 Indicadores útiles en la rúbrica

- correcta aplicación de condicionales;
- uso básico de bucles;
- comprensión funcional de variables;
- uso adecuado de funciones;
- legibilidad y comentarios;
- capacidad de depuración;
- relación entre código y resultado observable.

## 14.3 Evaluación de motivación y experiencia

Debe contemplar:
- cuestionario antes/después;
- interés por la programación;
- percepción de utilidad;
- autoeficacia;
- impresiones cualitativas mediante entrevista o grupo focal.

## 14.4 Análisis sencillo de datos

Se puede proponer:
- comparación de medias pre/post;
- cambios de respuesta;
- selección de citas representativas;
- triangulación simple entre observación, resultados y cuestionario.

---

# 15. Conclusiones, limitaciones y líneas futuras

## 15.1 Conclusiones

Debe cerrarse el círculo:
- problema inicial;
- propuesta desarrollada;
- aportaciones respecto a la enseñanza tradicional;
- valor del encapsulamiento;
- utilidad del proyecto como REA.

## 15.2 Respuesta a problemáticas iniciales

Conviene conectar explícitamente la propuesta con:
- absentismo;
- bloqueos;
- desmotivación;
- estigmatización por adaptaciones visibles.

## 15.3 Limitaciones que deben aparecer

El modelo debe poder mencionar, con honestidad:
- supuesto teórico y no aplicación real completa;
- tamaño reducido o hipotético de la muestra;
- dependencia del correcto funcionamiento del aula de ordenadores;
- limitación de tiempo;
- cinemáticas reducidas a texto sobre fondo negro;
- assets abiertos menos espectaculares pero más realistas para centros educativos;
- necesidad de que el proyecto funcione en muchos ordenadores;
- límite de 60 páginas del TFM;
- dificultad de abarcar investigación y desarrollo técnico en un único trabajo;
- necesidad de más validación empírica.

## 15.4 Líneas futuras

Debe poder proponerse:
- adaptación a ESO con pseudocódigo, bloques o Scratch;
- uso de Minecraft y redstone para puertas lógicas y algoritmos;
- segunda parte del proyecto para TIC II;
- materiales equivalentes para otros bloques de TIC I;
- formación de profesorado en Unity encapsulado;
- estudios cuasiexperimentales en el futuro;
- expansión de la historia abierta del videojuego.

---

# 16. Reglas de escritura del TFM

## 16.1 Formato obligatorio de salida

**Todo lo que se genere para el TFM o para documentación formal debe redactarse en LaTeX.**
No en Markdown, no en texto plano salvo que el usuario lo pida explícitamente.

El modelo debe:
- devolver apartados listos para pegar en un documento `.tex`;
- usar secciones, subsecciones y subsubsections correctamente;
- redactar en estilo académico claro y natural;
- evitar listas excesivas si el usuario pide desarrollo en prosa;
- usar tablas en LaTeX solo cuando sean realmente útiles.

## 16.2 Tono de redacción académica

El estilo debe ser:
- formal pero no artificial;
- argumentativo;
- claro;
- no telegráfico;
- con conexión lógica entre párrafos;
- evitando frases huecas de innovación educativa.

## 16.3 Cómo integrar citas

Si el texto se genera con apoyo de fuentes,
se deben dejar citas en formato compatible con LaTeX/BibTeX o con el estilo que se indique.
Si el usuario no pide bibliografía completa, se puede redactar con citas parentéticas normales
para sustituirlas después.

### Referencias ya detectadas y muy útiles
- Granic, Lobel y Engels (2014): beneficios del videojuego [web:20][web:21].
- Papastergiou (2009): DGBL en informática de secundaria/bachillerato [web:25][web:31].
- BOJA / currículo TIC I Andalucía: CE5, 5.1, 5.2, saberes de programación [web:29][web:26].

---

# 17. Regla especial para tablas en LaTeX

## Importante

No generar una tabla solo por estar esta instrucción aquí.
Simplemente guardar esta preferencia del usuario.

**Si el usuario pide una tabla en LaTeX**, el modelo debe seguir esta preferencia de estilo:
- usar `tabularray` exclusivamente;
- usar `\begin{tblr}{...}`;
- declarar merges y colores en la cabecera del entorno, no inline;
- usar `hlines = {1pt, white}` y `vlines = {1pt, white}`;
- usar `rowsep = 4pt` y `colsep = 5pt`;
- usar `width = \textwidth`;
- poner `\noindent` antes de la tabla;
- respetar la lógica de celdas fusionadas y de celdas consumidas;
- seguir la convención de tamaños: título `\large\bfseries`, cabecera `\bfseries`,
  etiquetas de sección `\small\bfseries`, datos `\footnotesize` o `\small`.

## Preferencia exacta del usuario para tablas

Cuando el usuario pida una tabla con ese estilo,
considerar esta guía operativa como plantilla de comportamiento:

```text
You are an expert LaTeX typesetter specializing in tabularray tables.

TASK
Generate a complete, compilable LaTeX document containing a styled table based on the structure I describe.

FIXED STYLE RULES (always apply)
- Use the `tabularray` package exclusively (never tabular, longtable, etc.)
- Use `\\begin{tblr}{...}` with ALL merges and colors declared in the preamble header — NEVER use \\SetCell inline for cells that are already declared in the preamble
- Line separators: `hlines = {1pt, white}`, `vlines = {1pt, white}`
- Default padding: `rowsep = 4pt`, `colsep = 5pt`
- Table width: `width = \\textwidth`
- Always use `\\noindent` before the table

COLOR PALETTE (use these unless I specify others)
\\definecolor{headerblue}{RGB}{30,  90, 160}
\\definecolor{rowblue}   {RGB}{100, 150, 210}

CELL DECLARATION RULES
1. Every cell that has a background color MUST be declared in the preamble with `cell{row}{col} = {options}`
2. Merged cells use `cell{row}{col} = {r=N}` for row-span or `{c=N}` for col-span or `{r=N, c=M}` for both
3. A merged cell CONSUMES its child cells — do NOT declare or write content in consumed cells
4. In the table body, consumed cells are skipped with bare `&`
5. Columns are defined with `colspec = {X[weight, halign, valign] ...}` using proportional widths

STRUCTURE TEMPLATE
\\documentclass[a4paper]{article}
\\usepackage[spanish]{babel}
\\usepackage[utf8]{inputenc}
\\usepackage[T1]{fontenc}
\\usepackage{tabularray}
\\usepackage{xcolor}
\\usepackage{geometry}
\\geometry{margin=1.5cm}

[color definitions]

\\begin{document}
\\noindent
\\begin{tblr}{
    width   = \\textwidth,
    colspec = {[columns]},
    rowsep  = 4pt,
    colsep  = 5pt,
    hlines  = {1pt, white},
    vlines  = {1pt, white},
    %% [ALL cell declarations here]
}
[table body rows, one \\\\ per row]
\\end{tblr}
\\end{document}

MERGE LOGIC RULES
- When a cell spans N rows, declare it ONCE as `cell{startRow}{col} = {r=N}{...}`
- In the body, that cell's content goes ONLY in the first row; subsequent rows use bare `&` for that column
- NEVER redeclare a consumed cell in the preamble
- NEVER write content in consumed body cells

FONT SIZING CONVENTION
- Title row: \\large\\bfseries
- Header row: \\bfseries
- Section labels: \\small\\bfseries
- Data cells: \\footnotesize or \\small

HOW TO DESCRIBE YOUR TABLE TO ME
1. Title
2. Column headers with relative widths
3. Row-by-row content with row, column, content, span and color
4. Any special rows

OUTPUT
Return ONLY the complete LaTeX code block, ready to compile with pdflatex. No explanation unless I ask.
```

Regla crítica:
**No generar automáticamente tablas por esta instrucción. Solo usarla si el usuario pide una tabla.**

---

# 18. Regla de reutilización de contenido ya escrito

Cuando el usuario aporte párrafos previos de contexto del centro, problemáticas,
o secciones ya redactadas, el modelo debe:
- reutilizarlos de forma coherente;
- integrarlos estilísticamente en el nuevo texto;
- no contradecirlos;
- expandirlos con naturalidad;
- evitar repetir literalmente demasiado si no hace falta.

---

# 19. Regla de honestidad académica

Nunca presentar como aplicado empíricamente lo que solo está diseñado.
Debe diferenciarse entre:
- propuesta didáctica diseñada;
- materiales desarrollados;
- posible puesta en marcha;
- futura validación.

Si algo es un supuesto, debe decirse.
Si algo no se implementó del todo, debe reconocerse.
Eso fortalece el TFM.

---

# 20. Qué debe ser capaz de generar esta skill

Con esta skill, el modelo debe poder generar sin perder el enfoque:
- título del TFM;
- índice razonado;
- capítulos completos en LaTeX;
- marco teórico de 7–10 páginas;
- objetivos;
- desarrollo de la propuesta;
- cronograma de 28 sesiones;
- unidad didáctica;
- metodología;
- atención a la diversidad;
- evaluación del aprendizaje;
- evaluación de la motivación;
- análisis de limitaciones;
- conclusiones;
- líneas futuras;
- anexos explicativos;
- documentación del proyecto técnico y de materiales REA.

---

# 21. Fórmula final de identidad del trabajo

Si el modelo necesita resumir el corazón del TFM en una idea fuerza, puede usar esta:

> La propuesta adapta un proyecto de videojuego en Unity al marco de TIC I de Bachillerato
> mediante una fuerte encapsulación de la complejidad técnica, de forma que el alumnado pueda
> trabajar algoritmos, condicionales, bucles, funciones y estructuras básicas dentro de un entorno visual,
> motivador e inclusivo, alineado con la competencia específica 5 y con los saberes del bloque de programación.
