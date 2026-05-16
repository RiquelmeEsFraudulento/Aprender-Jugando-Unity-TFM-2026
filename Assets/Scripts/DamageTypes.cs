// ============================================================
// DamageTypes.cs
// ============================================================
// Enum central de tipos de daño del proyecto.
// Todos los scripts leen de aquí.  Si necesitas un tipo nuevo,
// sólo tienes que añadirlo aquí y tratar el caso en cada switch.
// ============================================================
public enum DamageType
{
    Red,        // Sable láser rojo
    Blue,       // Sable láser azul
    Gray,       // Sable láser gris

    // ─ Tipos de daño especiales del Rapier ─────────────────
    Normal,
    Poison,     // Veneno: quita 0.5 vida/segundo durante 6 s
    Bleed       // Sangrado: al 5.º golpe inflige 3 de daño extra
}