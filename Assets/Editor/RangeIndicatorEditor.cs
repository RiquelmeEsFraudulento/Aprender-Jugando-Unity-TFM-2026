// ============================================================
// RangeIndicatorEditor.cs — Place in Assets/Editor/
// ============================================================
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RangeIndicator))]
public class RangeIndicatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RangeIndicator script = (RangeIndicator)target;

        EditorGUILayout.Space(10);
        DrawSeparator();
        EditorGUILayout.LabelField("  Sprite Generator", EditorStyles.boldLabel);
        DrawSeparator();
        EditorGUILayout.Space(4);

        EditorGUILayout.HelpBox(
            "Creates 3 child sprite objects (200x200 px each):\n" +
            "  ● Circle   — lightsaber only  (1 range)\n" +
            "  ■ Square   — rapier + lightsaber (2 ranges)\n" +
            "  ▲ Triangle — kick + rapier + lightsaber (3 ranges)\n\n" +
            "White outlined shapes on transparent background.",
            MessageType.Info
        );

        EditorGUILayout.Space(6);

        GUI.backgroundColor = new Color(0.3f, 0.75f, 0.3f);

        if (GUILayout.Button("▶  Generate Range Sprites", GUILayout.Height(40)))
        {
            GenerateSprites(script);
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);
        DrawStatus(script);
    }

    void DrawSeparator()
    {
        Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1));
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
    }

    void DrawStatus(RangeIndicator script)
    {
        EditorGUILayout.LabelField("Assignments:", EditorStyles.centeredGreyMiniLabel);

        if (script.rangeSprites == null || script.rangeSprites.Length < 3)
        {
            EditorGUILayout.HelpBox("Array not initialized. Click Generate.", MessageType.Warning);
            return;
        }

        string[] names = { "Circle  ", "Square  ", "Triangle" };
        string[] descs = { "lightsaber", "rapier+LS", "kick+rapier+LS" };

        bool allOK = true;
        for (int i = 0; i < 3; i++)
        {
            if (script.rangeSprites[i] == null) allOK = false;

            string status = script.rangeSprites[i] != null
                ? "✓ " + script.rangeSprites[i].name
                : "✗ NULL — click Generate";

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("[" + i + "] " + names[i], GUILayout.Width(80));
            EditorGUILayout.LabelField(descs[i], GUILayout.Width(100));
            EditorGUILayout.LabelField(status);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(4);

        if (allOK)
            EditorGUILayout.HelpBox("All 3 sprites assigned — ready to use.", MessageType.None);
        else
            EditorGUILayout.HelpBox("Missing sprites. Click Generate to create them.", MessageType.Warning);
    }

    // ══════════════════════════════════════════════════════════
    // GENERATE
    // ══════════════════════════════════════════════════════════

    void GenerateSprites(RangeIndicator script)
    {
        // Ensure array is initialized
        if (script.rangeSprites == null)
            script.rangeSprites = new GameObject[3];

        if (script.rangeSprites.Length != 3)
            System.Array.Resize(ref script.rangeSprites, 3);

        // Remove stale children
        RemoveStaleChildren(script);

        string[] childNames = { "Range_Circle", "Range_Square", "Range_Triangle" };
        Color[] tintColors = {
            new Color(0.3f,  0.85f, 1.0f, 1.0f),   // cyan
            new Color(1.0f,  0.85f, 0.2f, 1.0f),   // gold
            new Color(1.0f,  0.35f, 0.35f, 1.0f),  // red
        };

        Undo.RecordObject(script, "Generate Range Sprites");

        for (int i = 0; i < 3; i++)
        {
            // Create child
            GameObject go = new GameObject(childNames[i]);
            Undo.RegisterCreatedObjectUndo(go, "Create Range Sprite");
            go.transform.SetParent(script.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            // SpriteRenderer
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 100;

            // Create texture + sprite
            Texture2D tex;
            if (i == 0)      tex = CreateCircleTexture(200);
            else if (i == 1) tex = CreateSquareTexture(200);
            else             tex = CreateTriangleTexture(200);

            sr.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
            sr.color = tintColors[i];

            // Start disabled
            go.SetActive(false);

            // Assign
            script.rangeSprites[i] = go;

            Debug.Log("[RangeIndicatorEditor] Created '" + childNames[i] + "' — 200x200");
        }

        EditorUtility.SetDirty(script);
        AssetDatabase.SaveAssets();

        Debug.Log("[RangeIndicatorEditor] ✓ All 3 sprites generated successfully.");
    }

    void RemoveStaleChildren(RangeIndicator script)
    {
        string[] names = { "Range_Circle", "Range_Square", "Range_Triangle" };
        foreach (string n in names)
        {
            Transform t = script.transform.Find(n);
            if (t != null)
            {
                Undo.DestroyObjectImmediate(t.gameObject);
                Debug.Log("[RangeIndicatorEditor] Removed stale '" + n + "'");
            }
        }
    }

    // ══════════════════════════════════════════════════════════
    // TEXTURE BUILDERS — 200x200 outline shapes
    // ══════════════════════════════════════════════════════════

    Color _fill = Color.white;
    Color _clear = new Color(0f, 0f, 0f, 0f);

    // ────────────────────────────────────────────────────────
    // CIRCLE  — centered, outline thickness 8px
    // ────────────────────────────────────────────────────────
    Texture2D CreateCircleTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float cx = size / 2f, cy = size / 2f;
        float outerR = size / 2f - 4f;
        float innerR = outerR - 8f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                tex.SetPixel(x, y, dist <= outerR && dist >= innerR ? _fill : _clear);
            }
        }
        tex.Apply();
        return tex;
    }

    // ────────────────────────────────────────────────────────
    // SQUARE — centered, outline thickness 8px
    // ────────────────────────────────────────────────────────
    Texture2D CreateSquareTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float half = size / 2f - 4f;       // outer half-extent
        float innerHalf = half - 8f;       // inner hollow

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = Mathf.Abs(x - size / 2f);
                float dy = Mathf.Abs(y - size / 2f);

                bool inOuter = dx <= half && dy <= half;
                bool inInner = dx <= innerHalf && dy <= innerHalf;

                tex.SetPixel(x, y, inOuter && !inInner ? _fill : _clear);
            }
        }
        tex.Apply();
        return tex;
    }

    // ────────────────────────────────────────────────────────
    // TRIANGLE — equilateral, pointing up, outline thickness 8px
    // ────────────────────────────────────────────────────────
    Texture2D CreateTriangleTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };

        // Vertices (in pixel space, y-up)
        float margin = 8f;
        float s = size - 2f * margin;
        Vector2 top    = new Vector2(size / 2f,               size - margin);
        Vector2 bLeft  = new Vector2(size / 2f - s * 0.433f,  margin);
        Vector2 bRight = new Vector2(size / 2f + s * 0.433f,  margin);

        // Scale factors to shrink inward for inner triangle
        float shrink = 8f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 p = new Vector2(x, y);

                bool inOuter = PointInTriangle(p, top, bLeft, bRight);
                bool inInner = PointInTriangle(p, top, bLeft, bRight, shrink);

                tex.SetPixel(x, y, inOuter && !inInner ? _fill : _clear);
            }
        }
        tex.Apply();
        return tex;
    }

    // ══════════════════════════════════════════════════════════
    // GEOMETRY HELPERS
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Barycentric inside test.
    /// </summary>
    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 v0 = c - a, v1 = b - a, v2 = p - a;
        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);
        float inv = 1f / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * inv;
        float v = (dot00 * dot12 - dot01 * dot02) * inv;
        return u >= 0f && v >= 0f && u + v <= 1f;
    }

    /// <summary>
    /// Shrunk triangle inside test — offset each edge inward by 'offset' along its normal.
    /// </summary>
    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c, float offset)
    {
        // Inward normals for each edge
        Vector2 ab = b - a, bc = c - b, ca = a - c;
        Vector2 nAB = new Vector2(-(ab.y), ab.x).normalized;   // inward (winding a→b→c)
        Vector2 nBC = new Vector2(-(bc.y), bc.x).normalized;
        float caLen = ca.magnitude;
        Vector2 nCA = new Vector2(ca.y, -(ca.x)).normalized * (caLen > 0.001f ? 1f / caLen : 0f);

        // Half-plane checks — p must be on the inner side of all 3 offset edges
        bool abSide = (p.x - (a.x + nAB.x * offset)) * nAB.x +
                       (p.y - (a.y + nAB.y * offset)) * nAB.y >= 0f;
        bool bcSide = (p.x - (b.x + nBC.x * offset)) * nBC.x +
                       (p.y - (b.y + nBC.y * offset)) * nBC.y >= 0f;
        bool caSide = (p.x - (c.x + (new Vector2(-(a.y - c.y), a.x - c.x).normalized * offset).x)) *
                       (-(a.y - c.y)) +
                       (p.y - (c.y + (new Vector2(-(a.y - c.y), a.x - c.x).normalized * offset).y)) *
                       (a.x - c.x) >= 0f;

        return abSide && bcSide && caSide;
    }
}