using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerController : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject playerBullet;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject activeModel;

    [Header("Stats")]
    [SerializeField] int health;
    [SerializeField] float movementSpeed;
    [SerializeField] int jumpHeight;
    [SerializeField] int maxJumps;
    [SerializeField] float gravity;
    [SerializeField] float tempVal;
    [SerializeField] float bulletSpeed;

    // used for resetting stats. For all call resetStats()
    int originalHealth;
    public float originalMovementSpeed;
    float sprintSpeed;
    float originalSprintSpeed;
    int originalJumpHeight;
    int originalMaxJumps;
    float originalGravity;
    Vector3 playerLook;

    private Vector3 playerVelocity;
    Vector3 move;
    // Used for jump & sprint logic
    public int jumpCount;
    public bool groundedPlayer;
    bool isSprinting;
    bool gravityFlipped;
    // Used for shooting & zoom logic
    bool isShooting;
    bool isReloading;
    bool isZoomed;
    int zoomedFov;
    bool isMelee;

    int activeSlot;
    weapon activeWeapon;
    public List<weapon> inv = new List<weapon>();


    void Start()
    {
        activeWeapon = null;
        activeSlot = 0;
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
            zoom();
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
        else if (groundedPlayer && gravityFlipped && playerVelocity.y > 0)
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
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 100, 10f * Time.deltaTime);
        }
        // Reset function
        else
        {
            movementSpeed = originalMovementSpeed;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 80, 10f * Time.deltaTime);
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
            // Make sure slot is filled
            if (inv.Count >= 2)
            {
                // Set active weapon for shoot()
                activeWeapon = inv[1];
                activeSlot = 2;
                // Enable inventory Highlight
                inventoryUI(2);
            }
            
        }
        else if (Input.GetButtonDown("3"))
        {
            // Make sure slot is filled
            if (inv.Count >= 3)
            {
                // Set active weapon for shoot()
                activeWeapon = inv[2];
                activeSlot = 3;
                // Enable inventory Highlight
                inventoryUI(3);
            }
        }
        else if (Input.GetButtonDown("4"))
        {
            // Make sure slot is filled
            if (inv.Count >= 4)
            {
                // Set active weapon for shoot()
                activeWeapon = inv[3];
                activeSlot = 4;
                // Enable inventory Highlight
                inventoryUI(4);
            }
        }
        else if (Input.GetButtonDown("5"))
        {
            // Make sure slot is filled
            if (inv.Count >= 5)
            {
                // Set active weapon for shoot()
                activeWeapon = inv[4];
                activeSlot = 5;
                // Enable inventory Highlight
                inventoryUI(5);
            }
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
        // Set gun visual to active gun's texture/model
        activeModel.GetComponent<MeshFilter>().mesh = activeWeapon.model.GetComponent<MeshFilter>().sharedMesh;
        activeModel.GetComponent<MeshRenderer>().material = activeWeapon.model.GetComponent<MeshRenderer>().sharedMaterial;

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
        if (activeSlot != 1 && activeWeapon != null)
        {
            if (activeWeapon.clipSize > 0)
            {
                // Set flag
                isShooting = true;
                // Reduce current ammo
                activeWeapon.clipSize--;
                // Fire projectile
                GameObject bulletClone = Instantiate(playerBullet, shootPos.position, playerBullet.transform.rotation);
                bulletClone.GetComponent<Rigidbody>().velocity = Camera.main.transform.forward * bulletSpeed;
                yield return new WaitForSeconds(activeWeapon.rate);
                isShooting = false;
            }
        }
        // Player is melee
        else if (activeSlot == 1)
        {
            StartCoroutine(melee());
        }
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

    void zoom()
    {
        if (activeWeapon != null)
        {
            isZoomed = Input.GetButton("Zoom");
            if (isZoomed && activeWeapon.canZoom == true)
            {
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 0, 10f * Time.deltaTime);
            }
            else
            {
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 80, 10f * Time.deltaTime);
            }
        }
        

        if (isSprinting)
        {
            movementSpeed = sprintSpeed;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 100, 10f * Time.deltaTime);
        }
        // Reset function
        else
        {
            movementSpeed = originalMovementSpeed;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 80, 10f * Time.deltaTime);
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
        else if (other.CompareTag("Extraction Zone"))
        {
            gameManager.instance.winCondition();
        }
    }
    void reticleSwap()
    {
        // Exclusively used to swap reticle without code bloat.
        if (activeSlot != 1)
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
        if (activeWeapon != null)
        {
            gameManager.instance.bulletCountText.text = activeWeapon.clipSize.ToString() + " / " + activeWeapon.ammo.ToString();
        }
    }
    public void addWeapon(weapon gun)
    {
        bool found = false;
        gun.ammo = gun.maxAmmo;
        gun.clipSize = gun.maxClip;
        
        for (int i = 1; i < inv.Count; i++)
        {
            if (!found)
            {
                if (gun.label == inv[i].label)
                {
                    found = true;
                    inv[i].ammo += inv[i].clipSize;
                    bulletCountUpdate();
                }
            }
        }
        if (!found)
        {
            inv.Add(gun);
            switch (inv.Count)
            {
                case 2:
                    gameManager.instance.item2.SetActive(true);
                    gameManager.instance.item2.GetComponent<RawImage>().texture = gun.sprite;
                    break;
                case 3:
                    gameManager.instance.item3.SetActive(true);
                    gameManager.instance.item3.GetComponent<RawImage>().texture = gun.sprite;
                    break;
                case 4:
                    gameManager.instance.item4.SetActive(true);
                    gameManager.instance.item4.GetComponent<RawImage>().texture = gun.sprite;
                    break;
                case 5:
                    gameManager.instance.item5.SetActive(true);
                    gameManager.instance.item5.GetComponent<RawImage>().texture = gun.sprite;
                    break;
            }
        }
    }

}
