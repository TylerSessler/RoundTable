using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] CharacterController controller;
    Weapon[] inv;
   

    [Header("Stats")]
    [SerializeField] int health;
    [SerializeField] float movementSpeed;
    [SerializeField] int jumpHeight;
    [SerializeField] int maxJumps;
    [SerializeField] float gravity;
    [SerializeField] float tempVal;

    // used for resetting stats. For all call resetStats()
    int originalHealth;
    float originalMovementSpeed;
    float sprintSpeed;
    float originalSprintSpeed;
    int originalJumpHeight;
    int originalMaxJumps;
    float originalGravity;

    private Vector3 playerVelocity;
    Vector3 move;
    // Used for jump & sprint logic
    public int jumpCount;
    public bool groundedPlayer;
    bool isSprinting;
    bool isShooting;
    bool isReloading;
    bool isMelee;
    bool gravityFlipped;
    int activeSlot;
    Weapon activeWeapon;




    void Start()
    {
        Weapon weaponController = new Weapon();
        inv = weaponController.generateInventory();

        activeWeapon = inv[0];
        inventoryUI(1);

        originalHealth = health;
        originalMovementSpeed = movementSpeed;
        originalJumpHeight = jumpHeight;
        originalMaxJumps = maxJumps;
        sprintSpeed = 1.5f * movementSpeed;
        originalSprintSpeed = sprintSpeed;
        originalGravity = gravity;

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
                flipGrav();
            }
            if (!isReloading && Input.GetButtonDown("Reload"))
            {
                StartCoroutine(reload());
                bulletCountUpdate();
            }
            movement();
            inventory();
            if (!isShooting && Input.GetButton("Shoot"))
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
    }
    void movement()
    {

        isSprinting = Input.GetButton("Sprint");
        groundedPlayer = isGrounded();
        if (groundedPlayer && !gravityFlipped && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
            jumpCount = 0;
        }
        if (groundedPlayer && gravityFlipped && playerVelocity.y > 0)
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
        // Reset function
        else
        {
            movementSpeed = originalMovementSpeed;
        }
        controller.Move(move * Time.deltaTime * movementSpeed);

        if (Input.GetButtonDown("Jump"))
        {
            jump();
        }

        playerVelocity.y -= gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

    }

    void inventory()
    {
        if (Input.GetButtonDown("1"))
        {
            // Set active weapon & slot for shoot()
            activeWeapon = inv[0];
            activeSlot = 1;
            // Enable inventory Highlight
            inventoryUI(1);
        }
        else if (Input.GetButtonDown("2"))
        {
            // Set active weapon for shoot()
            activeWeapon = inv[1];
            activeSlot = 2;
            // Enable inventory Highlight
            inventoryUI(2);
        }
        else if (Input.GetButtonDown("3"))
        {
            // Set active weapon for shoot()
            activeWeapon = inv[2];
            activeSlot = 3;
            // Enable inventory Highlight
            inventoryUI(3);
        }
        else if (Input.GetButtonDown("4"))
        {
            // Set active weapon for shoot()
            activeWeapon = inv[3];
            activeSlot = 4;
            // Enable inventory Highlight
            inventoryUI(4);
        }
        else if (Input.GetButtonDown("5"))
        {
            // Set active weapon for shoot()
            activeWeapon = inv[4];
            activeSlot = 5;
            // Enable inventory Highlight
            inventoryUI(5);
        }
    }
    
    void inventoryUI(int num)
    {
        // Disable all hightlights
        gameManager.instance.glow1.SetActive(false);
        gameManager.instance.glow2.SetActive(false);
        gameManager.instance.glow3.SetActive(false);
        gameManager.instance.glow4.SetActive(false);
        gameManager.instance.glow5.SetActive(false);
        // Enable specific hightlight
        switch (num)
        {
            case 1:
                gameManager.instance.glow1.SetActive(true);
                break;
            case 2:
                gameManager.instance.glow2.SetActive(true);
                break; 
            case 3:
                gameManager.instance.glow3.SetActive(true);
                break;
            case 4:
                gameManager.instance.glow4.SetActive(true);
                break;
            case 5:
                gameManager.instance.glow5.SetActive(true);
                break;
        }
        // Update UI for ammo to match current weapon
        bulletCountUpdate();
        // Make sure proper reticle is active
        reticleSwap();
    }

    void jump()
    {
        // If player is not grounded and didn't jump to enter the state effectively disable double/extra jumps
        if (groundedPlayer == false && jumpCount == 0)
        {
            jumpCount = maxJumps;
        }
        // Compare number of jumps preformed & max number of jumps to check if player can jump.
        if (jumpCount < maxJumps)
        {
            jumpCount++;
            // Check state of gravity to determine direction to jump the player
            if (!gravityFlipped)
            {
                playerVelocity.y = jumpHeight;
            }
            else if (gravityFlipped)
            {
                playerVelocity.y = -jumpHeight;
            }
        }
    }

    void flipGrav()
    {
        gravityFlipped = !gravityFlipped;
        gravity = gravity * -1;

        controller.transform.Rotate(new Vector3(180, 0, 0));
        playerVelocity.y = 0;
    }

    bool isGrounded()
    {
        RaycastHit floorCheck;
        if (!gravityFlipped)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out floorCheck, 1.09f))
            {
                Debug.DrawRay(transform.position, Vector3.down, Color.red);
                return true;
            }
            return false;
        }
        else if (gravityFlipped)
        {
            if (Physics.Raycast(transform.position, Vector3.up, out floorCheck, 1.09f))
            {
                Debug.DrawRay(transform.position, Vector3.up, Color.red);
                return true;
            }
            return false;
        }
        return false;
    }
   

    IEnumerator shoot()
    {
        // If player isn't melee
        if (activeSlot != 5)
        {
            if (activeWeapon.clipSize > 0)
            {
                // Set flag
                isShooting = true;
                // Reduce current ammo
                activeWeapon.clipSize--;
                // Fire projectile.



                yield return new WaitForSeconds(activeWeapon.rate);
                isShooting = false;
            }
        }
        // Player is melee
        else if (activeSlot == 5)
        {
            StartCoroutine(melee());
        }
        

        
    }

    IEnumerator reload()
    {
        Debug.Log("Reload Called");
        isReloading = true;
        // Reload 1 bullet at a time (to prevent overloading/free-loading)
        for (int i = 0; i < activeWeapon.maxClip; i++)
        {
            Debug.Log("Need Bullet");
            // Prevent overloading
            if (activeWeapon.clipSize < activeWeapon.maxClip)
            {
                Debug.Log("Not overloading");
                // Prevent free-loading
                if (activeWeapon.ammo > 0)
                {
                    Debug.Log("Not free-loading");
                    // swap ammo from supply->clip
                    activeWeapon.ammo--;
                    activeWeapon.clipSize++;
                }
            }
        }
        yield return new WaitForSeconds(activeWeapon.reloadTime);
        
        isReloading = false;
    }

    IEnumerator melee()
    {
        isMelee = true;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, activeWeapon.range))
        {
            IDamage damageable = hit.collider.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.takeDamage(activeWeapon.damage);
            }
        }

        yield return new WaitForSeconds(activeWeapon.rate);
        isMelee = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Drop"))
        {
            // Using -3 to heal instead of creating heal function
            takeDamage(-3);

            // Add ammo to all guns. Could use inv[1], inv[2], inv[3], inv[4] to give different ammo to each gun instead of for loop.
            // Skip inv[5] since melee doesn't use ammo
            for (int i = 0; i < 4; i++)
            {
                // Make sure player doesn't over-cap ammo.
                if (activeWeapon.ammo < activeWeapon.maxAmmo)
                {
                    // Only adding ammo to backup, not to active clip.
                    inv[i].ammo += 3;
                }
               
            }
            bulletCountUpdate();

            Destroy(other.gameObject);
        }

    }

    // Exclusively used to swap reticle without code bloat.
    void reticleSwap()
    {
        if (activeSlot != 5)
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
        gameManager.instance.bulletCountText.text = activeWeapon.clipSize.ToString() + " / " + activeWeapon.ammo.ToString();
    }

}
