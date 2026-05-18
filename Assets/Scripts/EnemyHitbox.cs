using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public EnemyAI enemigo;
    public int dano = 8;
    public float retrocesoJugador = 2.25f;
    public float retrocesoEnemigo = 0.9f;
    public bool activo = false;

    void Awake()
    {
        if (enemigo == null) enemigo = GetComponentInParent<EnemyAI>();
        SetActivo(false);
    }

    public void Enlazar(EnemyAI e)
    {
        enemigo = e;
    }

    public void SetActivo(bool value)
    {
        activo = value;
        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = value;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Toma putiaso");
        if (!activo) return;
        if (enemigo == null) return;
        if (!other.CompareTag("Player")) return;
        PlayerHealth vidaJugador = other.GetComponent<PlayerHealth>();
        if (vidaJugador == null) return;
        Vector3 dir = (other.transform.position - transform.position).normalized;
        vidaJugador.TakeDamage(dano);
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null) rb.AddForce((dir + Vector3.up * 0.15f) * retrocesoJugador, ForceMode.Impulse);
        if (enemigo.controller != null)
        {
            Vector3 push = (transform.position - other.transform.position).normalized * retrocesoEnemigo;
            enemigo.controller.Move(push * Time.deltaTime);
        }
    }
}
