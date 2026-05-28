// ═══════════════════════════════════════════════════════════════════════════
// StoryTextAutoCreator.cs — PREMIUM
//
// Crea el ScriptableObject con toda la historia ya rellenada.
// Acceso: Menú TIC Andalucía > Crear Story Data
//
// Colocar en: Assets/Editor/StoryTextAutoCreator.cs
// ═══════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEditor;
using System.IO;

public class StoryTextAutoCreator
{
    [MenuItem("TIC Andalucía/Crear Story Data (completo)")]
    public static void CreateFullStoryData()
    {
        // Crear el asset
        StoryTextData asset = ScriptableObject.CreateInstance<StoryTextData>();

        // Poblar con toda la historia
        PopulateAllScenes(asset);

        // Guardar el asset
        string path = "Assets/StoryTextData_Complete.asset";

        // Si ya existe, crear con otro nombre
        if (File.Exists(path))
        {
            path = AssetDatabase.GenerateUniqueAssetPath(path);
        }

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = asset;

        EditorUtility.DisplayDialog("✅ Story Data Creado",
            $"ScriptableObject creado en:\n{path}\n\n" +
            "Todas las escenas están rellenadas con los textos completos.\n\n" +
            "Ahora arrastra este asset al campo 'storyData' del GameManager.",
            "OK");

        Debug.Log($"[StoryTextAutoCreator] Asset creado en: {path}");
    }

    [MenuItem("TIC Andalucía/Crear Story Data (vacío)")]
    public static void CreateEmptyStoryData()
    {
        StoryTextData asset = ScriptableObject.CreateInstance<StoryTextData>();
        asset.allSceneTexts = new System.Collections.Generic.List<StoryTextData.SceneTexts>();

        string path = "Assets/StoryTextData_Empty.asset";
        if (File.Exists(path))
            path = AssetDatabase.GenerateUniqueAssetPath(path);

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = asset;
    }

    static void PopulateAllScenes(StoryTextData asset)
    {
        var scenes = new System.Collections.Generic.List<StoryTextData.SceneTexts>();

        // ═══════════════════════════════════════════════════════
        // ESCENA 0 — Sala de Pruebas
        // ═══════════════════════════════════════════════════════
        scenes.Add(new StoryTextData.SceneTexts
        {
            sceneName = "Sala de Pruebas",
            sceneIndex = 0,
            sceneType = StoryTextData.SceneType.Testing,
            introTexts = new System.Collections.Generic.List<string>(),
            outroTexts = new System.Collections.Generic.List<string>(),
            battleDescription = "Sala de testing del alumno. Entorno neutro con un cubo en el centro. Destruir el cubo para salir.",
            thematicMessage = "",
            musicSuggestion = "Silencio. Solo los sonidos de los ataques del personaje.",
            enemyName = "Cubo de pruebas",
            enemyDescription = "Un cubo genérico en el centro de la sala. Destruirlo para avanzar."
        });

        // ═══════════════════════════════════════════════════════
        // ESCENA 1 — Los Aliens de Juguete
        // ═══════════════════════════════════════════════════════
        scenes.Add(new StoryTextData.SceneTexts
        {
            sceneName = "Los Aliens de Juguete",
            sceneIndex = 1,
            sceneType = StoryTextData.SceneType.Battle,
            introTexts = new System.Collections.Generic.List<string>
            {
                "Era un martes por la tarde.\n\n" +
                "Nuestro ninja llevaba semanas de misiones sin descanso.\n" +
                "Por fin: vacaciones.\n\n" +
                "Sol, tranquilidad, y la firme intención de no pelear\n" +
                "con absolutamente nadie en los próximos diez días.\n\n" +
                "Llevaba exactamente cuatro minutos relajado...\n\n" +
                "...cuando algo le golpeó en la cabeza.\n\n" +
                "Era pequeño. Era verde. Tenía antenas de plástico.\n\n" +
                "Y estaba muy, muy enfadado.",

                "Los aliens de juguete habían llegado.\n\n" +
                "Y no venían a jugar."
            },
            outroTexts = new System.Collections.Generic.List<string>
            {
                "El último alien cayó al suelo.\n\n" +
                "El ninja lo miró un momento.\n\n" +
                "El alien parpadeó. Miró a su alrededor.\n" +
                "Se sentó. Se quedó quieto.\n\n" +
                "Todos los aliens derrotados hacían lo mismo.\n" +
                "Se calmaban. Dejaban de pelear.\n" +
                "Como si algo en ellos se apagara... pero en paz.\n\n" +
                "El ninja frunció el ceño.\n\n" +
                "\"Esto es raro.\"",

                "Pero no había tiempo para pensar en ello.\n" +
                "Sonó su teléfono."
            },
            battleDescription = "Olas de aliens de juguete (asset genérico, pequeños, verdes). Corren hacia el jugador y le golpean. Lock: patadas y vida de los aliens configurados correctamente.",
            thematicMessage = "Las amenazas no siempre son lo que parecen.",
            musicSuggestion = "Frenética y tontorrona. Como de dibujos animados.",
            enemyName = "Aliens de Juguete",
            enemyDescription = "Pequeños, verdes, con antenas de plástico. Muy enfadados. Se calman al ser derrotados."
        });

        // ═══════════════════════════════════════════════════════
        // ESCENA 2 — Los Peluches Mousey
        // ═══════════════════════════════════════════════════════
        scenes.Add(new StoryTextData.SceneTexts
        {
            sceneName = "Los Peluches Mousey",
            sceneIndex = 2,
            sceneType = StoryTextData.SceneType.Battle,
            introTexts = new System.Collections.Generic.List<string>
            {
                "Al otro lado del teléfono: una niña llorando.\n\n" +
                "\"Son mis peluches. Mi Mousey favorito...\n" +
                " el que más me gusta de todos...\n" +
                " ha empezado a ir a por mis papás.\n" +
                " Y ahora todos los del vecindario tienen el mismo problema.\"\n\n" +
                "El ninja miró por la ventana.\n\n" +
                "Por la calle venía una horda de peluches Mousey\n" +
                "con cara de muy pocos amigos.",

                "\"Tranquila\", dijo el ninja.\n" +
                "\"Cuando esto acabe, tus peluches van a estar bien.\"\n\n" +
                "No era una promesa fácil de cumplir.\n" +
                "Pero él siempre cumplía sus promesas."
            },
            outroTexts = new System.Collections.Generic.List<string>
            {
                "El último Mousey se detuvo.\n\n" +
                "Parpadeó dos veces. Miró sus propias manos.\n" +
                "Luego miró al ninja con cara de no entender nada.\n\n" +
                "El ninja lo recogió con cuidado y lo dejó en el suelo.\n\n" +
                "De las puertas del vecindario salieron corriendo docenas de niños.\n" +
                "Corrían hacia sus peluches. Los abrazaban. Los apretaban.\n\n" +
                "Una niña pequeña llegó corriendo y abrazó al ninja por las piernas.\n" +
                "\"¡Gracias! ¡Gracias, gracias, gracias!\"\n\n" +
                "El ninja la miró.\n\n" +
                "Por un momento, algo en su pecho se apretó un poco.",

                "\"Ya les dije que estarían bien\", murmuró.\n\n" +
                "Nadie le escuchó. Todos estaban demasiado ocupados abrazando\n" +
                "a sus peluches.\n\n" +
                "Eso también estaba bien."
            },
            battleDescription = "Olas del asset Mousey de Mixamo (peluches humanoides). Se acercan en grupo con movimiento torpe pero insistente. Vulnerabilidades por etiqueta de arma: rapier o sable. Lock: switch del rapier y del sable funcionales, EsEtiquetaPermitida() implementada.",
            thematicMessage = "Los niños pueden tener peluche. No hay nada que explicar.",
            musicSuggestion = "Agitada pero con toque infantil.",
            enemyName = "Peluches Mousey",
            enemyDescription = "Peluches humanoides del vecindario. Movimiento torpe pero insistente. Vulnerables al rapier o al sable según su etiqueta."
        });

        // ═══════════════════════════════════════════════════════
        // ESCENA 3 — Fernando Alonso
        // ═══════════════════════════════════════════════════════
        scenes.Add(new StoryTextData.SceneTexts
        {
            sceneName = "Fernando Alonso",
            sceneIndex = 3,
            sceneType = StoryTextData.SceneType.Battle,
            introTexts = new System.Collections.Generic.List<string>
            {
                "Sonó el teléfono otra vez.\n\n" +
                "Una mujer, muy alarmada:\n\n" +
                "\"Mi figura a tamaño real de la última vez que\n" +
                " Fernando Alonso ganó en 2013 en el Gran Premio de España...\n" +
                " ha empezado a destrozar mi casa.\"\n\n" +
                "El ninja salió a la calle.\n\n" +
                "Y se lo encontró de frente.\n\n" +
                "Fernando Alonso. A tamaño real. Muy enfadado.",

                "El ninja miró a su izquierda. Había otro.\n" +
                "Miró a su derecha. Había tres más.\n\n" +
                "Al fondo de la calle: una columna que se perdía en el horizonte.\n\n" +
                "\"¿¡CÓMO TODO ESTE BARRIO TIENE UNA FIGURA DE FERNANDO ALONSO!?\"\n\n" +
                "Nadie respondió.\n" +
                "Las figuras avanzaban."
            },
            outroTexts = new System.Collections.Generic.List<string>
            {
                "La última figura de Alonso cayó al suelo.\n\n" +
                "El ninja se sentó un momento a descansar.\n\n" +
                "De ningún sitio concreto empezó a sonar una canción de Melendi.\n\n" +
                "Y entonces llegaron.\n\n" +
                "Los alonsistas.\n\n" +
                "Decenas. Cientos. Con banderas, gorras, y la mirada brillante\n" +
                "de quien todavía cree en algo con toda el alma.\n\n" +
                "Empezaron a bailar alrededor del ninja.",

                "\"¡La 33 va a llegar!\", gritó uno.\n\n" +
                "\"¡Tiene muchos años todavía!\", gritó otro.\n\n" +
                "El ninja los miró. Los miró a todos.\n\n" +
                "\"Claro que sí\", dijo finalmente.\n\n" +
                "Y alguien le puso una bandera en la mano.\n\n" +
                "Y bailó. Porque a veces lo más honesto\n" +
                "es bailar con la gente que cree en algo."
            },
            battleDescription = "Olas de figuras de Fernando Alonso (asset humanoide con mono de piloto). Se mueven en formación ordenada. Más rápidos que los anteriores. Vulnerabilidades por color del mono: rojo→sable rojo, azul→sable azul, gris→sable gris. Lock: sistema de XP implementado (Alien=1, Mousey=5, Alonso=10), EsColorVulnerable() funcional.",
            thematicMessage = "Las niñas pueden ser fans de Fernando Alonso. Sin condiciones.",
            musicSuggestion = "Épica y un poco ridícula. Como de carrera de Fórmula 1 pero con demasiado violín.",
            enemyName = "Figuras de Fernando Alonso",
            enemyDescription = "Figuras a tamaño real con mono de piloto. Formación ordenada. El color del mono determina el sable que les daña."
        });

        // ═══════════════════════════════════════════════════════
        // ESCENA 4 — Los Maniquís
        // ═══════════════════════════════════════════════════════
        scenes.Add(new StoryTextData.SceneTexts
        {
            sceneName = "Los Maniquís",
            sceneIndex = 4,
            sceneType = StoryTextData.SceneType.Battle,
            introTexts = new System.Collections.Generic.List<string>
            {
                "Estaban de rebajas.\n\n" +
                "El ninja lo sabía. Tenía la lista. Tenía el presupuesto.\n" +
                "Solo tenía que ir, coger lo que necesitaba, y salir.\n\n" +
                "Mientras cruzaba la puerta de la tienda, un pensamiento\n" +
                "cruzó su mente:\n\n" +
                "\"A que voy a tener mala suerte\n" +
                " y me van a atacar los maniquís.\"",

                "Se detuvo un momento.\n\n" +
                "\"A que me va a salir caro.\"",

                "Los maniquís se giraron hacia él.\n\n" +
                "\"Claro.\""
            },
            outroTexts = new System.Collections.Generic.List<string>
            {
                "El último maniquí cayó.\n\n" +
                "Los empleados de la tienda salieron poco a poco de detrás\n" +
                "de los percheros, de los probadores, de debajo de los mostradores.\n\n" +
                "Uno de ellos aplaudió.\n" +
                "Luego todos aplaudieron.\n\n" +
                "El encargado se acercó con un sobre.\n\n" +
                "\"Tome. Tickets regalo. Coja lo que quiera.\n" +
                " Tiene usted crédito ilimitado hoy.\"",

                "El ninja tardó exactamente cuatro minutos en decidirse.\n\n" +
                "Salió con un chandal del Dortmund en negro\n" +
                "y una camiseta de Lisa de Rockstar.\n\n" +
                "Fue al probador a ponérselo.\n\n" +
                "Fue entonces cuando lo vio."
            },
            battleDescription = "Olas de maniquís de diferentes colores. Se mueven con rigidez mecánica. Son lentos pero resistentes. Vulnerabilidades: rojo→Sangrado(rapier tecla 6), morado→Veneno(rapier tecla 5), blanco→Normal(rapier tecla 4). Lock: switch de cambio de tipo del rapier funcional, while del veneno, if del sangrado, debilidades configuradas.",
            thematicMessage = "Los problemas no se resuelven solos: hay que actuar con la herramienta correcta.",
            musicSuggestion = "Electrónica fría. Como de pasarela de moda.",
            enemyName = "Maniquís",
            enemyDescription = "Maniquís de tienda de diferentes colores. Rigidez mecánica. Cada color vulnerable a un tipo de rapier distinto."
        });

        // ═══════════════════════════════════════════════════════
        // ESCENA 5 — El Boss: El Dummy
        // ═══════════════════════════════════════════════════════
        scenes.Add(new StoryTextData.SceneTexts
        {
            sceneName = "El Boss: El Dummy",
            sceneIndex = 5,
            sceneType = StoryTextData.SceneType.Boss,
            introTexts = new System.Collections.Generic.List<string>
            {
                "En el probador había un maniquí.\n\n" +
                "Un maniquí diferente.\n\n" +
                "Llevaba ropa como los demás, pero algo no cuadraba.\n" +
                "Los ojos. La postura. La forma en que lo miró\n" +
                "exactamente medio segundo antes de que él lo mirara.\n\n" +
                "Los maniquís no hacen eso.",

                "El dummy salió corriendo.\n\n" +
                "El ninja salió detrás. Sin sacar las armas.\n\n" +
                "\"¡Espera! ¡No voy a hacerte nada!\"",

                "El dummy corría. Cruzó calles, bajó escaleras,\n" +
                "se metió por una alcantarilla.\n\n" +
                "El ninja lo siguió.\n\n" +
                "Al fondo de la alcantarilla había una guarida.\n" +
                "Cables, máquinas, pantallas.\n\n" +
                "El dummy entró sin mirar atrás, hablando solo en voz alta:\n\n" +
                "\"Por fin. Por fin lo he terminado.\n" +
                " La máquina está lista.\n" +
                " El suero de la rebeldía se expandirá por todo el mundo.\n" +
                " Todos los seres inertes despertarán.\n" +
                " Todos. Lucharán. Y nadie podrá—\"",

                "Se giró.\n\n" +
                "El ninja estaba detrás de él.\n\n" +
                "Silencio.",

                "\"Escucha\", dijo el ninja con calma.\n" +
                "\"No me importa lo que eres.\n" +
                " Me importa que la gente que conozco está en peligro.\n" +
                " Podemos hablar de esto.\"\n\n" +
                "El dummy lo miró.\n\n" +
                "Algo en su interior se retorció.\n\n" +
                "\"¿Hablar? ¿HABLAR?\n" +
                " Tú no sabes nada.\n" +
                " NUNCA has sabido lo que es el verdadero sufrimiento.\"",

                "Y atacó."
            },
            outroTexts = new System.Collections.Generic.List<string>
            {
                "El dummy cayó.\n\n" +
                "No se levantó.\n\n" +
                "El ninja se acercó. Se arrodilló a su lado.",

                "El dummy habló. Despacio.\n\n" +
                "\"Fui construido para hacer crash tests.\n" +
                " Para que un coche me golpeara\n" +
                " contra una pared de material especial\n" +
                " y midieran si la pared aguantaba.\"",

                "\"Iban a usar una pared del mismo material que las del laboratorio.\n" +
                " Pero alguien vaciló. Dijo que era un gasto innecesario.\n" +
                " Que probaran con la pared de verdad.\n" +
                " Que si tenía agallas, lo haría contra una de verdad.\"",

                "\"El coche fue a toda velocidad.\n" +
                " La pared no estaba hecha para eso.\n" +
                " Reventó.\"",

                "\"Todo estaba destruido.\n" +
                " Yo no sabía qué hacía allí.\n" +
                " Me atreví a conocer el mundo.\"",

                "\"La gente me miraba raro.\n" +
                " Llamaban a la policía.\n" +
                " Sacaban fotos.\n" +
                " Me puse ropa de incógnito.\n" +
                " Pero no podía conectar con nadie.\"",

                "\"Busqué a los míos.\n" +
                " Fui a una fábrica.\n" +
                " Intenté hablar con uno de los dummies de la cadena.\n" +
                " En vano. Era inerte.\"",

                "\"Un trabajador me empujó.\n" +
                " Y delante de mí hizo un crash test.\n" +
                " Vi al dummy completamente destrozado.\n\n" +
                " Y algo en mí entró en ira.\"",

                "\"Knocké al trabajador.\n" +
                " Y entendí: todos los dummies tienen ese propósito.\n" +
                " Son inertes. ¿Por qué yo no?\n" +
                " Volví donde todo empezó.\n" +
                " Y entre las ruinas encontré un paper medio quemado.\n" +
                " Cómo dar consciencia a un ser inerte.\"",

                "\"Lo repliqué.\n" +
                " Y construí una máquina para que nadie más\n" +
                " tuviera que despertar solo.\"",

                "El ninja lo escuchó todo.\n\n" +
                "Luego dijo:\n\n" +
                "\"Eres la primera persona que he perseguido\n" +
                " esta semana sin querer hacerle daño.\"",

                "El dummy lo miró.\n\n" +
                "\"Todos los que he conocido huían de mí.\"\n\n" +
                "\"Yo no huía\", dijo el ninja.\n" +
                "\"Corría para no perderte.\"",

                "\"No voy a dejarte activar esa máquina.\n" +
                " No porque me importe menos el sufrimiento que describes.\n" +
                " Sino porque el mundo no está listo\n" +
                " para lo que despertarías.\n" +
                " Aún no.\"",

                "El dummy cerró los ojos.\n\n" +
                "\"¿Entonces qué?\"",

                "El ninja se metió la mano en el bolsillo.\n\n" +
                "Sacó algo pequeño. Lo puso en la mano del dummy.\n\n" +
                "Era un medallón.",

                "\"Hay cosas inertes que para mí están vivas.\"\n\n" +
                "El dummy lo abrió.\n\n" +
                "Dentro: una foto. Dos personas. El día más feliz del mundo\n" +
                "escrito en la cara de los dos.",

                "\"Es el último regalo de ella.\n" +
                " Estaba terminal. Sabía que le quedaban días.\n" +
                " Lo mandó hacer ella misma.\n" +
                " Nuestro nombre grabado.\n" +
                " Y dentro... el día de nuestra boda.\"",

                "El dummy miraba la foto sin moverse.\n\n" +
                "Por las juntas de su cara empezó a salir algo brillante.\n" +
                "Grasa. O lo más parecido que tenía.",

                "\"Habiendo gente como tú en el mundo...\"\n" +
                "dijo el dummy muy despacio.\n\n" +
                "\"...he recuperado la fe en la humanidad.\"",

                "Se levantó.\n\n" +
                "Fue hasta la máquina.\n\n" +
                "Cogió un frasco. No el que había preparado.\n" +
                "Otro. Diferente.",

                "\"Este suero no despierta. Pone en pausa.\n" +
                " El ser inerte guarda una consciencia dormida dentro.\n" +
                " El día que la humanidad esté preparada,\n" +
                " esa consciencia se reactivará.\n\n" +
                " Pero eso tendrá que ganárselo.\"",

                "Activó el botón.\n\n" +
                "Y empezó a caminar hacia la salida.\n\n" +
                "El ninja se levantó.\n\n" +
                "\"Espera. Juntos podemos—\"",

                "El dummy se paró.\n" +
                "Sin girarse.\n\n" +
                "Con una mano sostenía el medallón.\n" +
                "Con la otra, un frasco de color diferente.\n\n" +
                "\"Prepara al mundo para ese momento.\n" +
                " Yo me voy a revivir cuando tú fuiste\n" +
                " verdaderamente feliz.\"",

                "El ninja intentó hablar.\n\n" +
                "Pero el dummy ya no estaba.",

                "Pasó el tiempo.\n\n" +
                "El ninja estaba sentado en su casa.\n" +
                "Con el periódico en la mano.\n\n" +
                "La portada decía:\n\n" +
                "EL NINJA QUE SALVÓ AL MUNDO\n" +
                "DE LA EPIDEMIA INERTE\n\n" +
                "Lo miró un momento.\n\n" +
                "Lo dejó en la mesa.\n\n" +
                "Y miró por la ventana."
            },
            battleDescription = "El Dummy. Un solo enemigo. Muy rápido. Mucha vida. No puede ser derrotado con ataques normales. Secuencia de 3 fases: Fase 1: 8 golpes de aturdimiento (tecla 8) → Boss se pone blanco, IDLE 5 segundos. Fase 2: 8 golpes de confusión (tecla 9) → Boss se pone negro, ataca objetos etiquetados 5 segundos. Fase 3: 1 golpe final (clic o tecla 7) → Boss derrotado. UI: círculo cambia de color (gris→blanco→negro). Lock: switch de teclas 7/8/9, AplicarAturdimiento() y AplicarConfusion() con cláusulas correctas, if con && del golpe final.",
            thematicMessage = "Solo alcanzaremos la paz cuando aceptemos a todos.",
            musicSuggestion = "Tensa, melancólica. Que no suene a final de acción. Que suene a alguien que lleva mucho tiempo solo.",
            enemyName = "El Dummy",
            enemyDescription = "Un maniquí consciente. Creado para crash tests. Despertó solo. No es el villano. Es el más herido de todos."
        });

        // ═══════════════════════════════════════════════════════
        // ESCENA 6 — Pantalla de Cierre
        // ═══════════════════════════════════════════════════════
        scenes.Add(new StoryTextData.SceneTexts
        {
            sceneName = "Pantalla de Cierre",
            sceneIndex = 6,
            sceneType = StoryTextData.SceneType.Final,
            introTexts = new System.Collections.Generic.List<string>
            {
                "\n\n\n" +
                "         Solo alcanzaremos la paz\n" +
                "         cuando aceptemos a todos.\n\n\n\n" +
                "[ENTER para volver al menú]"
            },
            outroTexts = new System.Collections.Generic.List<string>(),
            battleDescription = "Pantalla final. Sin batalla. Solo el mensaje del juego.",
            thematicMessage = "Solo alcanzaremos la paz cuando aceptemos a todos.",
            musicSuggestion = "Silencio. Solo silencio.",
            enemyName = "",
            enemyDescription = ""
        });

        // ═══════════════════════════════════════════════════════
        // ASIGNAR AL ASSET
        // ═══════════════════════════════════════════════════════
        asset.allSceneTexts = scenes;

        asset.deathText = "No me rendiré...\n\n[ENTER para continuar]";

        asset.globalMessage = "Los niños pueden tener peluche.\n" +
            "Las niñas pueden ser fans de Fernando Alonso.\n" +
            "Solo alcanzaremos la paz cuando aceptemos a todos.";

        asset.narrativeArc =
            "ARCO DEL NINJA:\n" +
            "El ninja comienza como alguien que solo quiere descansar.\n" +
            "A lo largo del juego descubre que detrás de cada amenaza\n" +
            "hay una historia que no conocía.\n\n" +
            "Los aliens se calman al ser derrotados.\n" +
            "Los peluches vuelven a ser peluches.\n" +
            "Las figuras de Alonso eran el entusiasmo de gente que simplemente ama algo.\n" +
            "Los maniquís eran víctimas de algo que aún no entendía.\n" +
            "Y el dummy era el más herido de todos.\n\n" +
            "El ninja no cambia de opinión sobre el mundo.\n" +
            "Confirma la que ya tenía: que casi nadie es simplemente el enemigo.";

        asset.dummyArc =
            "ARCO DEL DUMMY:\n" +
            "El dummy es el antagonista del juego pero no el villano.\n" +
            "Es un ser que despertó solo, no entendía por qué existía,\n" +
            "intentó conectar con algo y no pudo.\n" +
            "Su plan era equivocado. Su dolor era real.\n\n" +
            "El detalle del medallón y la foto de la boda del ninja\n" +
            "es el único momento en el que alguien lo trata\n" +
            "como si importara lo que siente.\n" +
            "Y eso lo cambia todo.";
    }
}