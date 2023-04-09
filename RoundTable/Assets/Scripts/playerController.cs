using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] CharacterController controller;

    [Header("Stats")]
    [SerializeField] int health;
    [SerializeField] float movementSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] int jumpHeight;
    [SerializeField] int maxJumps;
    [SerializeField] float gravity;

    [Header("Weapon")]
    [SerializeField] float shootRate;
    [SerializeField] float shootDistance;
    [SerializeField] int shootDamage;

    // used for resetting stats. For all call resetStats()
    int originalHealth;
    float originalMovementSpeed;
    float originalSprintSpeed;
    int originalJumpHeight;
    int originalMaxJumps;

    private Vector3 playerVelocity;
    Vector3 move;
    // Used for jump & sprint logic
    int jumpCount;
    public bool groundedPlayer;
    public bool isSprinting;
    private bool isShooting;





    void Start()
    {
        originalHealth = health;
        originalMovementSpeed = movementSpeed;
        originalJumpHeight = jumpHeight;
        originalMaxJumps = maxJumps;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.instance.isPaused)
        {
            movement();
            if (isShooting == false && Input.GetButton("Shoot"))
            {
                StartCoroutine(shoot());
            }
        }
    }

    void resetStats()
    {
        health = originalHealth;
        movementSpeed = originalMovementSpeed;
        jumpHeight = originalJumpHeight;
        maxJumps = originalMaxJumps;
        sprintSpeed = originalSprintSpeed;
    }
    void movement()
    {
        isSprinting = Input.GetButton("Sprint");
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
            jumpCount = 0;
        }
        
        // Might implement if-check to prevent/lower air-strafing.
        move = (transform.right * Input.GetAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical"));


        // If sprint is held
        if (isSprinting)
        {
            movementSpeed = sprintSpeed;
        }
        else
        {
            movementSpeed = originalMovementSpeed;
        }
        controller.Move(move * Time.deltaTime * movementSpeed);

        if (Input.GetButtonDown("Jump"))
        {
            // If player is in air and didn't jump to enter the state effectively disable double/extra jumps
            if (groundedPlayer == false && jumpCount == 0)
            {
                jumpCount = maxJumps;
            }
            // Compare number of jumps preformed & max number of jumps to check if player can jump.
            if (jumpCount < maxJumps)
            {
                jumpCount++;
                playerVelocity.y = jumpHeight;
            }
        }

        playerVelocity.y -= gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

    }

    IEnumerator shoot()
    {
        isShooting = true;
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDistance))
        {
            IDamage damageable = hit.collider.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.takeDamage(shootDamage);
            }
        }

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    

    public void takeDamage(int dmg)
    {
        health -= dmg;
        playerUIUpdate();
        if (health <= 0)
        {
            gameManager.instance.playerDead();
        }
    }

    void playerUIUpdate()
    {
        gameManager.instance.HPBar.fillAmount = ((float)health / (float)originalHealth);
    }
}
