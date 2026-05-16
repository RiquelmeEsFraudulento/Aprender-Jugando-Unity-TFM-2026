using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LightsaberColorController : MonoBehaviour
{
    [Header("Input")]
    public KeyCode redKey  = KeyCode.Alpha1;
    public KeyCode blueKey = KeyCode.Alpha2;
    public KeyCode grayKey = KeyCode.Alpha3;

    [Header("Current State (read‑only at runtime)")]
    public DamageType currentDamageType = DamageType.Red;

    Renderer rend;
    Material instancedMaterial;
    [SerializeField] private WeaponHitbox hitbox; // ← Arrastra aquí el GameObject con WeaponHitbox


    void Awake()
    {
        rend = GetComponent<Renderer>();
        // This creates a unique material instance for this saber,
        // so you do NOT change all objects using the same shared material.[web:17][web:19]
        instancedMaterial = rend.material;

        // Assuming the hitbox is a child of this blade object
        //hitbox = transform.root.GetComponentInChildren<WeaponHitbox>();

        ApplyCurrentColor();
    }

    void Update()
    {
        if (Input.GetKeyDown(redKey))
            SetSaberColor(DamageType.Red);
        if (Input.GetKeyDown(blueKey))
            SetSaberColor(DamageType.Blue);
        if (Input.GetKeyDown(grayKey))
            SetSaberColor(DamageType.Gray);
    }

    public void SetSaberColor(DamageType type)
    {
        currentDamageType = type;
        ApplyCurrentColor();
    }

    void ApplyCurrentColor()
    {
        if (instancedMaterial == null) return;

        Color c;
        switch (currentDamageType)
        {
            case DamageType.Red:
                c = Color.red;
                break;
            case DamageType.Blue:
                c = Color.cyan;   // tweak to the exact blue you want
                break;
            case DamageType.Gray:
                c = Color.gray;
                break;
            default:
                c = Color.white;
                break;
        }

        // Support Standard shader and URP/Lit (_Color vs _BaseColor).[web:16][web:18]
        if (instancedMaterial.HasProperty("_BaseColor"))
            instancedMaterial.SetColor("_BaseColor", c);
        else if (instancedMaterial.HasProperty("_Color"))
            instancedMaterial.SetColor("_Color", c);
        
        // Update hitbox damage type so logic matches visuals
        if (hitbox != null)
        {
            hitbox.damageType = currentDamageType;            
            Debug.Log("CAMBIE DE COLOR");
        }

    }
}