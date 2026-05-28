// ═══════════════════════════════════════════════════════════════════════════
// StoryTextDataEditor.cs — PREMIUM
//
// Editor personalizado para el ScriptableObject.
// Añade botones de preview y validación.
//
// Colocar en: Assets/Editor/StoryTextDataEditor.cs
// ═══════════════════════════════════════════════════════════════════════════

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(StoryTextData))]
public class StoryTextDataEditor : Editor
{
    private bool showSceneTexts = true;
    private bool showDeathText = true;
    private bool showGlobalMessage = true;
    private bool showNarrative = true;
    private int previewSceneIndex = 0;
    private int previewTextType = 0; // 0 = intro, 1 = outro

    public override void OnInspectorGUI()
    {
        StoryTextData data = (StoryTextData)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("═══ TIC I ANDALUCÍA — HISTORIA DEL VIDEOJUEGO ═══",
            EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // ── Botones rápidos ──────────────────────────────────
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Poblar con historia completa", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirmar",
                "¿Rellenar el ScriptableObject con todos los textos de historia del juego? " +
                "Esto sobrescribirá los datos actuales.", "Sí, rellenar", "Cancelar"))
            {
                PopulateWithFullHistory(data);
                EditorUtility.SetDirty(data);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Validar datos", GUILayout.Height(25)))
        {
            ValidateData(data);
        }

        EditorGUILayout.Space(10);

        // ── Secciones plegables ──────────────────────────────
        showSceneTexts = EditorGUILayout.Foldout(showSceneTexts,
            $"Textos de Escenas ({data.allSceneTexts.Count} escenas)", true);
        if (showSceneTexts)
        {
            EditorGUI.indentLevel++;
            DrawSceneTextsSection(data);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        showDeathText = EditorGUILayout.Foldout(showDeathText, "Texto de Muerte", true);
        if (showDeathText)
        {
            EditorGUI.indentLevel++;
            data.deathText = EditorGUILayout.TextArea(data.deathText,
                GUILayout.MinHeight(60));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        showGlobalMessage = EditorGUILayout.Foldout(showGlobalMessage,
            "Mensaje Global", true);
        if (showGlobalMessage)
        {
            EditorGUI.indentLevel++;
            data.globalMessage = EditorGUILayout.TextArea(data.globalMessage,
                GUILayout.MinHeight(80));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        showNarrative = EditorGUILayout.Foldout(showNarrative,
            "Arco Narrativo (referencia)", true);
        if (showNarrative)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "ARCO DEL NINJA:\n" +
                "Comienza queriendo descansar. Descubre que detrás de cada amenaza " +
                "hay una historia. Los aliens se calman. Los peluches vuelven a ser " +
                "peluches. Las figuras de Alonso son entusiasmo puro. Los maniquís " +
                "son víctimas. El dummy es el más herido de todos.\n\n" +
                "ARCO DEL DUMMY:\n" +
                "No es el villano. Es un ser que despertó solo, no entendía por qué " +
                "existía. Su plan era equivocado. Su dolor era real. El medallón " +
                "es el único momento en que alguien lo trata como si importara.\n\n" +
                "MENSAJE FINAL:\n" +
                "\"Solo alcanzaremos la paz cuando aceptemos a todos.\"",
                MessageType.Info);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // ── Preview ──────────────────────────────────────────
        DrawPreviewSection(data);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
        }
    }

    void DrawSceneTextsSection(StoryTextData data)
    {
        for (int i = 0; i < data.allSceneTexts.Count; i++)
        {
            StoryTextData.SceneTexts scene = data.allSceneTexts[i];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Escena {scene.sceneIndex}: {scene.sceneName}",
                EditorStyles.boldLabel);

            scene.sceneName = EditorGUILayout.TextField("Nombre", scene.sceneName);
            scene.sceneIndex = EditorGUILayout.IntField("Índice", scene.sceneIndex);
            scene.sceneType = (StoryTextData.SceneType)EditorGUILayout.EnumPopup(
                "Tipo", scene.sceneType);

            EditorGUILayout.LabelField("Intro texts:", EditorStyles.miniLabel);
            for (int j = 0; j < scene.introTexts.Count; j++)
            {
                scene.introTexts[j] = EditorGUILayout.TextArea(scene.introTexts[j],
                    GUILayout.MinHeight(40));
            }

            if (GUILayout.Button("Añadir párrafo intro"))
                scene.introTexts.Add("");

            EditorGUILayout.LabelField("Outro texts:", EditorStyles.miniLabel);
            for (int j = 0; j < scene.outroTexts.Count; j++)
            {
                scene.outroTexts[j] = EditorGUILayout.TextArea(scene.outroTexts[j],
                    GUILayout.MinHeight(40));
            }

            if (GUILayout.Button("Añadir párrafo outro"))
                scene.outroTexts.Add("");

            scene.thematicMessage = EditorGUILayout.TextField(
                "Mensaje temático", scene.thematicMessage);
            scene.musicSuggestion = EditorGUILayout.TextField(
                "Música sugerida", scene.musicSuggestion);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        if (GUILayout.Button("Añadir escena"))
        {
            StoryTextData.SceneTexts newScene = new StoryTextData.SceneTexts();
            newScene.sceneIndex = data.allSceneTexts.Count;
            newScene.sceneName = "Nueva Escena";
            data.allSceneTexts.Add(newScene);
        }
    }

    void DrawPreviewSection(StoryTextData data)
    {
        EditorGUILayout.LabelField("═══ PREVIEW ═══", EditorStyles.boldLabel);

        previewSceneIndex = EditorGUILayout.IntSlider("Escena", previewSceneIndex, 0,
            Mathf.Max(0, data.allSceneTexts.Count - 1));
        previewTextType = EditorGUILayout.Popup("Tipo texto", previewTextType,
            new string[] { "Intro", "Outro" });

        StoryTextData.SceneTexts scene = data.GetSceneTexts(previewSceneIndex);
        if (scene == null) return;

        string[] texts = previewTextType == 0 ?
            scene.introTexts.ToArray() : scene.outroTexts.ToArray();

        EditorGUILayout.BeginVertical("box");
        for (int i = 0; i < texts.Length; i++)
        {
            EditorGUILayout.LabelField($"[Párrafo {i + 1}]", EditorStyles.miniLabel);
            EditorGUILayout.HelpBox(texts[i], MessageType.None);
        }
        EditorGUILayout.EndVertical();
    }

    void ValidateData(StoryTextData data)
    {
        List<string> errors = new List<string>();

        if (data.allSceneTexts.Count == 0)
            errors.Add("No hay escenas configuradas.");

        for (int i = 0; i < data.allSceneTexts.Count; i++)
        {
            StoryTextData.SceneTexts scene = data.allSceneTexts[i];

            if (string.IsNullOrEmpty(scene.sceneName))
                errors.Add($"Escena {i}: nombre vacío.");

            if (scene.sceneType != StoryTextData.SceneType.Testing &&
                scene.introTexts.Count == 0)
                errors.Add($"Escena {i} ({scene.sceneName}): sin textos de intro.");

            if (scene.sceneType == StoryTextData.SceneType.Battle &&
                scene.outroTexts.Count == 0)
                errors.Add($"Escena {i} ({scene.sceneName}): sin textos de outro.");
        }

        if (errors.Count == 0)
        {
            EditorUtility.DisplayDialog("Validación OK",
                $"Todas las {data.allSceneTexts.Count} escenas están correctamente configuradas.",
                "OK");
        }
        else
        {
            string msg = "Errores encontrados:\n\n";
            for (int i = 0; i < errors.Count; i++)
                msg += $"• {errors[i]}\n";
            EditorUtility.DisplayDialog("Validación con errores", msg, "OK");
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // POBLAR CON HISTORIA COMPLETA
    // ═══════════════════════════════════════════════════════════════════

    void PopulateWithFullHistory(StoryTextData data)
    {
        data.allSceneTexts.Clear();

        // ── ESCENA 0: Sala de Pruebas ─────────────────────────
        StoryTextData.SceneTexts scene0 = new StoryTextData.SceneTexts();
        scene0.sceneName = "Sala de Pruebas";
        scene0.sceneIndex = 0;
        scene0.sceneType = StoryTextData.SceneType.Testing;
        scene0.introTexts = new List<string>(); // Sin intro
        scene0.outroTexts = new List<string>(); // Sin outro
        scene0.battleDescription = "Sala de testing. Destruir el cubo.";
        scene0.thematicMessage = "";
        scene0.musicSuggestion = "Silencio. Solo sonidos de ataques.";
        scene0.enemyName = "Cubo de pruebas";
        scene0.enemyDescription = "Un cubo genérico. Destruirlo para salir.";
        data.allSceneTexts.Add(scene0);

        // ── ESCENA 1: Los Aliens de Juguete ───────────────────
        StoryTextData.SceneTexts scene1 = new StoryTextData.SceneTexts();
        scene1.sceneName = "Los Aliens de Juguete";
        scene1.sceneIndex = 1;
        scene1.sceneType = StoryTextData.SceneType.Battle;
        scene1.introTexts = new List<string>()
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
        };
        scene1.outroTexts = new List<string>()
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
        };
        scene1.battleDescription = "Olas de aliens de juguete. Corren hacia el jugador y le golpean.";
        scene1.thematicMessage = "Las amenazas no siempre son lo que parecen.";
        scene1.musicSuggestion = "Frenética y tontorrona. Como de dibujos animados.";
        scene1.enemyName = "Aliens de Juguete";
        scene1.enemyDescription = "Pequeños, verdes, con antenas de plástico. Muy enfadados.";
        data.allSceneTexts.Add(scene1);

        // ── ESCENA 2: Los Peluches Mousey ─────────────────────
        StoryTextData.SceneTexts scene2 = new StoryTextData.SceneTexts();
        scene2.sceneName = "Los Peluches Mousey";
        scene2.sceneIndex = 2;
        scene2.sceneType = StoryTextData.SceneType.Battle;
        scene2.introTexts = new List<string>()
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
        };
        scene2.outroTexts = new List<string>()
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
        };
        scene2.battleDescription = "Olas de peluches Mousey. Se acercan en grupo con movimiento torpe pero insistente.";
        scene2.thematicMessage = "Los niños pueden tener peluche. No hay nada que explicar.";
        scene2.musicSuggestion = "Agitada pero con toque infantil.";
        scene2.enemyName = "Peluches Mousey";
        scene2.enemyDescription = "Peluches humanoides. Movimiento torpe pero insistente.";
        data.allSceneTexts.Add(scene2);

        // ── ESCENA 3: Fernando Alonso ─────────────────────────
        StoryTextData.SceneTexts scene3 = new StoryTextData.SceneTexts();
        scene3.sceneName = "Fernando Alonso";
        scene3.sceneIndex = 3;
        scene3.sceneType = StoryTextData.SceneType.Battle;
        scene3.introTexts = new List<string>()
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
        };
        scene3.outroTexts = new List<string>()
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
        };
        scene3.battleDescription = "Olas de figuras de Fernando Alonso. Se mueven en formación ordenada.";
        scene3.thematicMessage = "Las niñas pueden ser fans de Fernando Alonso. Sin condiciones.";
        scene3.musicSuggestion = "Épica y un poco ridícula. Como de carrera de Fórmula 1 pero con demasiado violín.";
        scene3.enemyName = "Figuras de Fernando Alonso";
        scene3.enemyDescription = "Figuras a tamaño real con mono de piloto. Formación ordenada.";
        data.allSceneTexts.Add(scene3);

        // ── ESCENA 4: Los Maniquís ────────────────────────────
        StoryTextData.SceneTexts scene4 = new StoryTextData.SceneTexts();
        scene4.sceneName = "Los Maniquís";
        scene4.sceneIndex = 4;
        scene4.sceneType = StoryTextData.SceneType.Battle;
        scene4.introTexts = new List<string>()
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
        };
        scene4.outroTexts = new List<string>()
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
        };
        scene4.battleDescription = "Olas de maniquís de diferentes colores. Rigidez mecánica. Lentos pero resistentes.";
        scene4.thematicMessage = "Los problemas no se resuelven solos: hay que actuar con la herramienta correcta.";
        scene4.musicSuggestion = "Electrónica fría. Como de pasarela de moda.";
        scene4.enemyName = "Maniquís";
        scene4.enemyDescription = "Maniquís de tienda. Colores: rojo (sangrado), morado (veneno), blanco (normal).";
        data.allSceneTexts.Add(scene4);

        // ── ESCENA 5: El Boss — El Dummy ──────────────────────
        StoryTextData.SceneTexts scene5 = new StoryTextData.SceneTexts();
        scene5.sceneName = "El Boss: El Dummy";
        scene5.sceneIndex = 5;
        scene5.sceneType = StoryTextData.SceneType.Boss;
        scene5.introTexts = new List<string>()
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
        };
        scene5.outroTexts = new List<string>()
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
            "EL NINJA QUE SALVÓ AL MUNDO DE LA EPIDEMIA INERTE\n\n" +
            "Lo miró un momento.\n\n" +
            "Lo dejó en la mesa.\n\n" +
            "Y miró por la ventana."
        };
        scene5.battleDescription = "El Dummy. Un solo enemigo. Muy rápido. Mucha vida. Secuencia de 3 fases: aturdimiento → confusión → golpe final.";
        scene5.thematicMessage = "Solo alcanzaremos la paz cuando aceptemos a todos.";
        scene5.musicSuggestion = "Tensa, melancólica. Que suene a alguien que lleva mucho tiempo solo.";
        scene5.enemyName = "El Dummy";
        scene5.enemyDescription = "Un maniquí consciente. Creado para crash tests. Despertó solo. No es el villano.";
        data.allSceneTexts.Add(scene5);

        // ── ESCENA 6: Pantalla de Cierre ──────────────────────
        StoryTextData.SceneTexts scene6 = new StoryTextData.SceneTexts();
        scene6.sceneName = "Pantalla de Cierre";
        scene6.sceneIndex = 6;
        scene6.sceneType = StoryTextData.SceneType.Final;
        scene6.introTexts = new List<string>()
        {
            "\n\n\n" +
            "         Solo alcanzaremos la paz\n" +
            "         cuando aceptemos a todos.\n\n\n\n" +
            "[ENTER para volver al menú]"
        };
        scene6.outroTexts = new List<string>(); // Sin outro, es la última
        scene6.battleDescription = "Pantalla final. Sin batalla.";
        scene6.thematicMessage = "Solo alcanzaremos la paz cuando aceptemos a todos.";
        scene6.musicSuggestion = "Silencio. Solo silencio.";
        scene6.enemyName = "";
        scene6.enemyDescription = "";
        data.allSceneTexts.Add(scene6);

        // ── Arco narrativo ────────────────────────────────────
        data.narrativeArc =
            "ARCO DEL NINJA:\n" +
            "Comienza queriendo descansar. Descubre que detrás de cada amenaza " +
            "hay una historia. Los aliens se calman. Los peluches vuelven a ser " +
            "peluches. Las figuras de Alonso son entusiasmo puro. Los maniquís " +
            "son víctimas. El dummy es el más herido de todos.\n\n" +
            "El ninja no cambia de opinión sobre el mundo. " +
            "Confirma la que ya tenía: que casi nadie es simplemente el enemigo.";

        data.dummyArc =
            "ARCO DEL DUMMY:\n" +
            "No es el villano. Es un ser que despertó solo, no entendía por qué " +
            "existía. Intentó conectar con algo y no pudo. Su plan era equivocado. " +
            "Su dolor era real. El medallón es el único momento en que alguien " +
            "lo trata como si importara lo que siente. Y eso lo cambia todo.";

        Debug.Log("[StoryTextData] ✅ Historia completa poblada: " +
            $"{data.allSceneTexts.Count} escenas.");
    }
}
#endif