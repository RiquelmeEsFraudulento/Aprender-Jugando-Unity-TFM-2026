
using UnityEngine;
using System;
using System.Collections;
[RequireComponent(typeof(CharacterController))]
public class SimpleWalk : MonoBehaviour
{
    public float speed = 2f;
    public Animator animator;
    


    public GameObject ballPrefab; // Asignar desde el inspector
    private GameObject currentBall;
    public Transform ballSpawnPoint; // Donde aparece la pelota

    public Collider RapierCollider;
    public Collider LightSaberCollider;
    public Collider RightKickCollider;
    public Collider LeftKickCollider;
    public RapierHitbox Rapier;
    public WeaponHitbox LightSaber;
    public KickHitbox RightKick;
    public KickHitbox LeftKick;

    private CharacterController controller;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A / D
        float vertical = Input.GetAxisRaw("Vertical");     // W / S

        // Calculate movement vector
        moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        // Move the character (using world space)
        if (moveDirection.magnitude >= 0.1f)
        {
            // Rotate character toward movement direction
            transform.forward = moveDirection;

            // Move
            controller.Move(moveDirection * speed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(DanceSpin());
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(JumpCountdown());
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            StartCoroutine(Martelo());
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(Chut());
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(RaSwing());
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartCoroutine(SwSwing());
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(Ra360());
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(Sw360());
        }

        /*if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(CreateBall());
        }*/



        // Set animator parameter
        bool isWalking = moveDirection.magnitude > 0f;
        animator.SetBool("isWalking", isWalking);
    }
    
    
    IEnumerator DanceSpin(){
        for (int i = 0; i < 360; i += 30)
        {
            transform.Rotate(0, 30, 0);
            yield return new WaitForSeconds(0.05f);
        }
    }


    public void EnableRapierHitbox(){ 
        RapierCollider.enabled = true; 
        Rapier.Reactivar();
    }

    public void EnableLightSaberHitbox(){ 
        LightSaberCollider.enabled = true;
        LightSaber.Reactivar();
    }

    public void EnableLeftKickHitbox(){ 
        LeftKickCollider.enabled = true;
        Rapier.Reactivar();
    }

    public void EnableRightKickHitbox(){ 
        RightKickCollider.enabled = true;
        Rapier.Reactivar(); 
    }

    public void DisableRapierHitbox(){ 
        RapierCollider.enabled = false;
    }

    public void DisableLightSaberHitbox(){ 
        LightSaberCollider.enabled = false; 
    }

    public void DisableLeftKickHitbox(){ 
        LeftKickCollider.enabled = false; 
    }

    public void DisableRightKickHitbox(){ 
        RightKickCollider.enabled = false; 
    }


    IEnumerator JumpCountdown()
    {
        for (int i = 3; i > 0; i--)
        {
            Debug.Log(i);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("¡Jump!");
        controller.Move(Vector3.up * 2f); 
    }

    IEnumerator RaSwing(){
        animator.SetTrigger("TrRaSwing");
        yield return new WaitForSeconds(1.3f);
    }

    IEnumerator SwSwing(){
        animator.SetTrigger("TrSwSwing");
        yield return new WaitForSeconds(1.3f);
        //KickBall();
    }


    IEnumerator Ra360(){
        animator.SetTrigger("TrRa360");
        yield return new WaitForSeconds(1.3f);
    }

    IEnumerator Sw360(){
        animator.SetTrigger("TrSw360");
        yield return new WaitForSeconds(1.3f);
        //KickBall();
    }



    IEnumerator Martelo(){
        animator.SetTrigger("TrMartelo");
        yield return new WaitForSeconds(1.3f);
    }

    IEnumerator Chut(){
        animator.SetTrigger("TrChut");
        yield return new WaitForSeconds(1.3f);

        //KickBall();
    }

    /*IEnumerator CreateBall()
    {
        if (currentBall != null) yield return new WaitForSeconds(0.1f); // Solo una pelota a la vez

        Vector3 spawnPos = ballSpawnPoint != null ? ballSpawnPoint.position : transform.position + transform.forward + Vector3.up * 0.5f;

        currentBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

        yield return new WaitForSeconds(0.1f);

    }

    void KickBall()
{
    if (currentBall == null) return;

    float distance = Vector3.Distance(transform.position, currentBall.transform.position);
    if (distance < 2f)
    {
        Rigidbody rb = currentBall.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (currentBall.transform.position - transform.position).normalized + Vector3.up * 0.5f;
            rb.AddForce(direction * 8f, ForceMode.Impulse);
            currentBall = null; // Se libera para permitir otra creación
        }
    }
}
*/


    

    
}




