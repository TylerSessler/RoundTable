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
    [SerializeField] int jumpHeight;
    [SerializeField] int maxJumps;
    [SerializeField] float gravity;
    [SerializeField] float tempVal;

    [Header("Weapon")]
    [SerializeField] float shootRate;
    [SerializeField] float meleeSpeed;
    [SerializeField] float shootDistance;
    [SerializeField] float meleeRange;
    [SerializeField] int shootDamage;
    [SerializeField] int meleeDamage;
    [SerializeField] int bulletCount;

    // used for resetting stats. For all call resetStats()
    int originalHealth;
    float originalMovementSpeed;
    float sprintSpeed;
    float originalSprintSpeed;
    int originalJumpHeight;
    int originalMaxJumps;
    float originalGravity;
    int originalBulletCount;

    private Vector3 playerVelocity;
    Vector3 move;
    // Used for jump & sprint logic
    int jumpCount;
    public bool groundedPlayer;
    public bool isSprinting;
    private bool isShooting;
    private bool isMelee;
    public bool gravityFlipped;





    void Start()
    {
        originalHealth = health;
        originalMovementSpeed = movementSpeed;
        originalJumpHeight = jumpHeight;
        originalMaxJumps = maxJumps;
        sprintSpeed = 1.5f * movementSpeed;
        originalSprintSpeed = sprintSpeed;
        originalGravity = gravity;
        originalBulletCount = bulletCount;

        // Initial UI Update
        bulletCountUpdate();
        // Default to ranged reticle (automatic since player has ammo)
        reticleSwap();
    }

    // Update is called once per frame
    void Update()
    {

        if (!gameManager.instance.isPaused)
        {
            if (Input.GetButtonDown("Flip Gravity"))
            {
                gravityFlipped = !gravityFlipped;
                gravity = gravity * -1;

                controller.transform.Rotate(new Vector3(180,0,0));
                playerVelocity.y = 0;
            }
            movement();
            if (isShooting == false && Input.GetButton("Shoot"))
            {
                StartCoroutine(shoot());
                // Update UI for bullet count
                bulletCountUpdate();
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
        gravity = originalGravity;
        bulletCount = originalBulletCount;
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

   
   /*   Deal with later
    void checkFloor()
    {
        RaycastHit floorCheck;

        if (Physics.Raycast(transform.position, Vector3.down, out floorCheck, 3))
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.red);
            groundedPlayer = true;
        }

    }
   */

    IEnumerator shoot()
    {
        if (bulletCount > 0)
        {
            isShooting = true;
            bulletCount -= 1;
            bulletCountUpdate();
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDistance))
            {
                IDamage damageable = hit.collider.GetComponent<IDamage>();
                if (damageable != null)
                {
                    damageable.takeDamage(shootDamage);
                }
            }
            // If player runs out of ammo, swap to melee reticle
            if (bulletCount == 0)
            {
                reticleSwap();
            }
            yield return new WaitForSeconds(shootRate);
            isShooting = false;
        }

        // If player is out of ammo, default to melee
        if (bulletCount == 0) 
        {
            gameManager.instance.meleeReticle.SetActive(true);
            StartCoroutine(melee());
        }
    }

    IEnumerator melee()
    {
        isMelee = true;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, meleeRange))
        {
            IDamage damageable = hit.collider.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.takeDamage(meleeDamage);
            }
        }

        yield return new WaitForSeconds(meleeSpeed);
        isMelee = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Drop"))
        {
            // Using -3 to heal instead of creating heal function
            takeDamage(-3);

            // Update amount of bullets, change reticle to gun
            bulletCount += 5;
            bulletCountUpdate();
            // Make sure player has correct reticle after picking up ammo.
            reticleSwap();
            Destroy(other.gameObject);
        }

    }

    // Exclusively used to swap reticle without code bloat.
    void reticleSwap()
    {
        if (bulletCount > 0)
        {
            gameManager.instance.gunReticle.SetActive(true);
            gameManager.instance.meleeReticle.SetActive(false);
        }
        else
        {
            gameManager.instance.gunReticle.SetActive(false);
            gameManager.instance.meleeReticle.SetActive(true);
        }
    }

    public void takeDamage(int dmg)
    {
        health -= dmg;
        // Prevent overhealing
        if (health > originalHealth)
        {
            health = originalHealth;
        }
        playerUIUpdate();
        // Player dies
        if (health <= 0)
        {
            gameManager.instance.playerDead();
        }
    }

    void playerUIUpdate()
    {
        gameManager.instance.HPBar.fillAmount = ((float)health / (float)originalHealth);
    }

    void bulletCountUpdate()
    {
        gameManager.instance.bulletCountText.text = bulletCount.ToString();
    }

}
