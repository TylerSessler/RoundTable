using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class playerController : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject playerBullet;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject activeModel;
    [SerializeField] AudioSource aud;

    [Header("Stats")]
    [SerializeField] public int health;
    [SerializeField] public int originalHealth;
    [SerializeField] public float movementSpeed;
    [SerializeField] float originalMovementSpeed;
    [SerializeField] int jumpHeight;
    [SerializeField] int originalJumpHeight;
    [SerializeField] public int maxJumps;
    [SerializeField] int originalMaxJumps;
    [SerializeField] float gravity;
    [SerializeField] float originalGravity;
    [SerializeField] float tempVal;
    [SerializeField] float bulletSpeed;
    [SerializeField] float rotationDuration;

    bool isRotating;
    public bool canRotate;

    public float sprintSpeed;
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
    bool isPlayingSteps;
    public bool hasObjective;

    int activeSlot;
    public weapon activeWeapon;
    public List<weapon> inv = new List<weapon>();

    [Header("Store")]


    [Header("Audio")]
    [SerializeField] AudioClip[] audSteps;
    [SerializeField][Range(0, 1)] float audStepsVol;
    [SerializeField] AudioClip[] audJump;
    [SerializeField][Range(0, 1)] float audJumpVol;
    [SerializeField] AudioClip audGrav;
    [SerializeField][Range(0, 1)] float audGravVol;
    [SerializeField] AudioClip audPickup;
    [SerializeField][Range(0, 1)] float audPickupVol;

    void Awake()
    {
        
    }
    void Start()
    {
        activeWeapon = null;
        activeSlot = 0;
        isRotating = false;
        canRotate = false;
        inventoryUI(1);
        // Default to ranged reticle (automatic since player has ammo)
        reticleSwap();
        setPlayerPos();
        if(PlayerPrefs.HasKey("Health"))
        {
            LoadPlayerData();
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (!gameManager.instance.isPaused)
        {
            movement();
            inventory();
            zoom();

            if (Input.GetButtonDown("Flip Gravity") && !isRotating && canRotate)
            {
                flipGrav();
            }

            if (!isReloading && Input.GetButtonDown("Reload"))
            {
                StartCoroutine(reload());
                bulletCountUpdate();
            }

            if (!isShooting && Input.GetButton("Shoot"))
            {
                StartCoroutine(shoot());
                // Update UI for bullet count
                bulletCountUpdate();
            }
        }
    }
    public void setPlayerPos()
    {
        controller.enabled = false;
        transform.position = gameManager.instance.playerSpawnPos.transform.position;
        controller.enabled = true;
    }
    public void SavePlayerData()
    {
        PlayerPrefs.SetInt("Health", health);
        PlayerPrefs.SetFloat("SprintSpeed", sprintSpeed);
        PlayerPrefs.SetInt("maxJumps", maxJumps);
        for (int i = 1; i < inv.Count; i++)
        {
            PlayerPrefs.SetString("Gun" + i, inv[i].label);
            PlayerPrefs.SetInt("GunAmmo" + i, inv[i].ammo);
        }
        PlayerPrefs.Save();
    }
    public void LoadPlayerData()
    {
        health = PlayerPrefs.GetInt("Health");
        sprintSpeed = PlayerPrefs.GetFloat("SprintSpeed");
        maxJumps = PlayerPrefs.GetInt("maxJumps");
        for (int i = 0; i < inv.Count; i++)
        {
            inv[i] = JsonConvert.DeserializeObject<weapon>(PlayerPrefs.GetString("Gun" + i));
        }
    }
    void resetStats()
    {
        health = originalHealth;
        movementSpeed = originalMovementSpeed;
        jumpHeight = originalJumpHeight;
        maxJumps = originalMaxJumps;
        gravity = originalGravity;
    }
    void movement()
    {

        isSprinting = Input.GetButton("Sprint");
        groundedPlayer = isGrounded();
        if (groundedPlayer)
        {
            if (!isPlayingSteps && move.normalized.magnitude > 0.5f)
            {
                StartCoroutine(playSteps());
            }
            if (!gravityFlipped && playerVelocity.y < 0)
            {
                playerVelocity.y = 0;
                jumpCount = 0;
            }
            else if (gravityFlipped && playerVelocity.y > 0)
            {
                playerVelocity.y = 0;
                jumpCount = 0;
            }

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

    IEnumerator playSteps()
    {
        isPlayingSteps = true;
        aud.PlayOneShot(audSteps[UnityEngine.Random.Range(0, audSteps.Length)], audStepsVol);
        if (!isSprinting)
            yield return new WaitForSeconds(0.45f);
        else
            yield return new WaitForSeconds(0.25f);
        isPlayingSteps = false;
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

        if (activeWeapon != null)
        {
            activeModel.GetComponent<MeshFilter>().mesh = activeWeapon.model.GetComponent<MeshFilter>().sharedMesh;
            activeModel.GetComponent<MeshRenderer>().material = activeWeapon.model.GetComponent<MeshRenderer>().sharedMaterial;
        }
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
                aud.PlayOneShot(audJump[UnityEngine.Random.Range(0, audJump.Length)], audJumpVol);
            }
            else if (gravityFlipped)
            {
                playerVelocity.y = -jumpHeight;
                aud.PlayOneShot(audJump[UnityEngine.Random.Range(0, audJump.Length)], audJumpVol);
            }
        }
    }
    void flipGrav()
    {
        isRotating = true;
        canRotate = false;

        gravityFlipped = !gravityFlipped;
        gravity *= -1;

        playerVelocity.y = 0;
        aud.PlayOneShot(audGrav, audGravVol);

        StartCoroutine(RotatePlayerSmoothly(new Vector3(0, 0, 180), () =>
        {
            isRotating = false;
        }));
    }
    IEnumerator RotatePlayerSmoothly(Vector3 targetRotation, System.Action onComplete)
    {
        Vector3 startRotation = controller.transform.eulerAngles;
        Vector3 endRotation = controller.transform.eulerAngles + targetRotation;
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotationDuration;
            controller.transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, t);
            yield return null;
        }

        onComplete?.Invoke();
    }
    bool isGrounded()
    {
        RaycastHit floorCheck;
        if (!gravityFlipped)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out floorCheck, 1.25f))
            {
                Debug.DrawRay(transform.position, Vector3.down, Color.red);
                return true;
            }
            return false;
        }
        else if (gravityFlipped)
        {
            if (Physics.Raycast(transform.position, Vector3.up, out floorCheck, 1.25f))
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
                aud.PlayOneShot(activeWeapon.gunShotAud, activeWeapon.gunShotAudVol);
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
            if (!isMelee)
            {
                StartCoroutine(melee());
            }
           
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
                aud.PlayOneShot(activeWeapon.gunShotAud, activeWeapon.gunShotAudVol);
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
        isReloading = true;
        // Reload 1 bullet at a time (to prevent overloading/free-loading)
        for (int i = 0; i < activeWeapon.maxClip; i++)
        {
            // Prevent overloading
            if (activeWeapon.clipSize < activeWeapon.maxClip)
            {
                // Prevent free-loading
                if (activeWeapon.ammo > 0)
                {
                    // swap ammo from supply->clip
                    activeWeapon.ammo--;
                    activeWeapon.clipSize++;
                    aud.PlayOneShot(activeWeapon.audReload, activeWeapon.audReloadVol);
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
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("FlipControl"))
        {
            // Makes it so the player can flip gravity when they are inside the tagged area
            canRotate = true;
        }
        else if (other.CompareTag("Extraction Zone"))
        {
            gameManager.instance.winCondition();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("FlipControl"))
        {
            // Defaults the player to be unable to flip gravity
            canRotate = false;
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
    public void playerUIUpdate()
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
        aud.PlayOneShot(audPickup, audPickupVol);

        for (int i = 1; i < inv.Count; i++)
        {
            if (!found)
            {
                if (gun.label == inv[i].label)
                {
                    found = true;
                    inv[i].ammo += inv[i].maxClip;
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
