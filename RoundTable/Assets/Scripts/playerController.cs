using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEditor;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.XR;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class playerController : MonoBehaviour, IDamage
{
    #region variables
    public static playerController instance;

    [Header("Components")]
    [SerializeField] CharacterController controller;
    public Transform mainCamera;
    public Transform cam;
    public Transform playerTransform;
    DefaultInput input;
    public Vector2 inputMovement;
    public Vector2 inputCamera;
    public float mouseScroll;
    [SerializeField] float lockVerMin;
    [SerializeField] float lockVerMax;
    [SerializeField] GameObject playerBullet;
    [SerializeField] GameObject grenade;
    [SerializeField] GameObject activeModel;
    [SerializeField] public AudioSource aud;

    explodingEnemyAI explodingEnemy;

    Vector3 cameraRotation;
    Vector3 playerRotation;

    [Header("Stats")]

    public int health;
    public int originalHealth;
    //[SerializeField] public float movementSpeed;
    [SerializeField] Vector3 originalMovementSpeed;
    [SerializeField] int jumpHeight;
    [SerializeField] int originalJumpHeight;
    [SerializeField] public int maxJumps;
    [SerializeField] int originalMaxJumps;
    //[SerializeField] float gravity;
    [SerializeField] float originalGravity;
    [SerializeField] float tempVal;
    [SerializeField] float bulletSpeed;
    [SerializeField] float rotationDuration;

    bool isSprintButtonPressed;
    bool isRotating;
    public bool canRotate;

    public float sprintSpeed;
    private Vector3 playerVelocity;
    Vector3 move;
    // Used for jump & sprint logic
    public int jumpCount;
    public bool groundedPlayer;
    bool gravityFlipped;
    float playerRotationOffset;

    [Header("Weapon Stats")]
    public List<weapon> inv = new List<weapon>();
    [SerializeField] MeshFilter weaponMesh;
    [SerializeField] MeshRenderer weaponMaterial;
    [SerializeField] GameObject shootEffect;
    public weaponController currentWeapon;
    public float weaponAnimSpeed;
    int selectedGun;
    bool isShooting;
    bool canShoot;
    int activeSlot;
    public weapon activeWeapon;
    int lastWeapon;
    bool isReloading;
    bool isZoomed;
    bool isAiming;
    int zoomedFov;
    bool isMelee;
    bool isPlayingSteps;
    public bool hasObjective;
    [SerializeField] float shootRate;
    [SerializeField] float shootRange;
    [SerializeField] float maxRayDistance;  // Set this to whatever maximum distance you want
    [SerializeField] float throwPower;
    public int weaponDamage;
    Coroutine shootCoroutine;
    bool isShootButtonPressed;
    float remainingReloadTime;

    [Header("--- Weapon Transformations---")]
    public Transform weaponHolderPos;
    public Transform weaponPos;
    public Transform weaponSightsPos;
    [SerializeField] Transform shootPos;

    [Header("----- Weapon ADS Stats -----")]
    [SerializeField] float zoomMax;
    [SerializeField] int zoomInSpeed;
    [SerializeField] int zoomOutSpeed;
    public float fovOrig;

    [Header("Leaning")]
    public Transform cameraLeanPivot;
    [SerializeField] bool aimToLean;
    public float leanAngle;
    public float leanSmoothing;
    float currentLean;
    float targetLean;
    float leanVelocity;

    bool isLeaningLeft;
    bool isLeaningRight;

    [Header("---Settings---")]
    public LayerMask playerMask;
    public LayerMask ceilingLayer;
    [SerializeField] float ceilingCheckDistance;
    [SerializeField] gameManager.PlayerSettings playerSettings;
    [SerializeField] float gravity;
    [SerializeField] Vector3 jumpForce;
    public float playerGravity;
    Vector3 jumpSpeed;
    public bool isFalling;

    public float timeSinceLastJump;
    Vector3 previousJumpDirection;

    int originalMaxHealth;
    public bool isSprinting;
    public bool isWalking;
    Vector3 movementSpeed;
    Vector3 velocitySpeed;

    [Header("---Preferences---")]
    [SerializeField] gameManager.PlayerPose playerPose;
    [SerializeField] float playerPoseSmooth;
    [SerializeField] float cameraHeight;
    [SerializeField] float cameraHeightSpeed;
    [SerializeField] float stanceCheck;
    [SerializeField] gameManager.PlayerStance playerStandStance;
    [SerializeField] gameManager.PlayerStance playerCrouchStance;
    [SerializeField] gameManager.PlayerStance playerProneStance;
    float playerStanceHeightVelocity;
    Vector3 playerStanceCenterVelocity;
    float camerHeightOrig;

    [Header("Weapons")]
    [SerializeField] weapon meleeweapon;
    [SerializeField] weapon pistol;
    [SerializeField] weapon rifle;
    [SerializeField] weapon sniper;
    [SerializeField] weapon placeHolder;

    [Header("Audio")]
    [SerializeField] AudioClip[] audSteps;
    [SerializeField][Range(0, 1)] float audStepsVol;
    [SerializeField] AudioClip[] audJump;
    [SerializeField][Range(0, 1)] float audJumpVol;
    [SerializeField] AudioClip audGrav;
    [SerializeField][Range(0, 1)] float audGravVol;
    [SerializeField] AudioClip audPickup;
    [SerializeField][Range(0, 1)] float audPickupVol;
    [SerializeField] AudioClip weaponShootAud;
    [SerializeField] AudioClip weaponReloadAud;
    [SerializeField] float weaponShootVol;
    [SerializeField] float weaponReloadVol;
    AudioSource reloadAudioSource;

    #endregion variables

    private void SetupInputActions()
    {
        input.Player.Movement.performed += e => inputMovement = e.ReadValue<Vector2>();
        input.Player.Camera.performed += e => inputCamera = e.ReadValue<Vector2>();
        input.Player.Jump.performed += e => Jump();
        input.Player.Crouch.performed += e => Crouch();
        input.Player.Prone.performed += e => Prone();

        input.Player.Sprint.performed += e => ToggleSprint();
        input.Player.SprintReleased.performed += e => StopSprint();

        input.Player.MouseScrollWheel.performed += e => mouseScroll = e.ReadValue<float>();

        input.Weapon.AimPressed.performed += e => AimPressed();
        input.Weapon.AimReleased.performed += e => AimReleased();


        //input.Weapon.Shoot.performed += e => shoot();
        //input.Weapon.Shoot.performed += e => isShooting = true;
        //input.Weapon.Shoot.canceled += e => isShooting = false;

        input.Player.LeanLeftPressed.performed += e => isLeaningLeft = true;
        input.Player.LeanLeftReleased.performed += e => isLeaningLeft = false;

        input.Player.LeanRightPressed.performed += e => isLeaningRight = true;
        input.Player.LeanRightReleased.performed += e => isLeaningRight = false;
    }

    void Awake()
    {
        instance = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        input = new();

        SetupInputActions();

        cameraRotation = mainCamera.localRotation.eulerAngles;
        playerRotation = transform.localRotation.eulerAngles;

        //cameraHeight = mainCamera.localPosition.y;
        inv.Add(meleeweapon);

        if (currentWeapon)
        {
            currentWeapon.Initialize(this);
        }
    }

    void Start()
    {
        activeWeapon = null;
        activeSlot = 0;
        isRotating = false;
        isShooting = false;
        isSprinting = false;
        isReloading = false;
        canShoot = true;
        canRotate = false;
        camerHeightOrig = cameraHeight;
        playerRotationOffset = 0f;
        originalMaxHealth = originalHealth;
        isSprintButtonPressed = false;
        isShootButtonPressed = false;
        lastWeapon = 99;
        gameManager.instance.reloadAmmoText.enabled = false;
        gameManager.instance.lowAmmoText.enabled = false;
        gameManager.instance.noAmmoText.enabled = false;
        gameManager.instance.reloadingText.enabled = false;

        reloadAudioSource = aud;
        remainingReloadTime = 0f;

        inventoryUI(1);
        // Default to ranged reticle (automatic since player has ammo)
        reticleSwap();
        setPlayerPos();
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void FixedUpdate()
    {
        movement();
        RotatePlayer();
        Cam();
    }

    void Update()
    {
        if (Time.timeScale != 0)
        {
            //RotatePlayer();
            //Cam();
            //movement();
            PlayerStance();
            inventory();
            ammoState();
            reticleSwap();
            //zoom();
            Leaning();
            Aiming();
            //zoom();
            //selectGun();
            SetIsFalling();

            Jumping();
            JumpTimer();
            ShootMouseClick();

            //if (isSprintButtonPressed && !isShooting)
            //{
            //    ToggleSprint();
            //}

            if (Input.GetButtonDown("Flip Gravity") && !isRotating && canRotate)
            {
                flipGrav();
            }

            if (!isReloading && Input.GetButtonDown("Reload") && activeWeapon != null && activeWeapon.clipSize < activeWeapon.maxClip && activeWeapon.ammo > 0)
            {
                StartCoroutine(reload());
            }
        }
    }

    //void LateUpdate()
    //{
    //    Cam();
    //}

    public void setPlayerPos()
    {
        controller.enabled = false;
        transform.position = gameManager.instance.playerSpawnPos.transform.position;
        controller.enabled = true;
    }

    public void resetStats()
    {
        originalHealth = originalMaxHealth;
        health = originalHealth;
        movementSpeed = originalMovementSpeed;
        jumpHeight = originalJumpHeight;
        maxJumps = originalMaxJumps;
        gravity = originalGravity;
        gameManager.instance.credits = 0;
    }

    public void clearWeapons()
    {
        if (inv.Count > 0)
        {
            for (int i = inv.Count - 1; i >= 0; i--)
            {
                inv.RemoveAt(i);
            }
        }
        PlayerPrefs.SetInt("Pistol", 0);
        PlayerPrefs.SetInt("Rifle", 0);
        PlayerPrefs.SetInt("Sniper", 0);
        PlayerPrefs.SetInt("Grenade", 0);
    }

    public void SavePlayerData()
    {
        PlayerPrefs.SetInt("CurrentHealth", health);
        PlayerPrefs.SetInt("MaxHealth", originalHealth);
        PlayerPrefs.SetFloat("SprintSpeed", sprintSpeed);
        PlayerPrefs.SetFloat("ForwardWalkSpeed", playerSettings.forwardWalkSpeed);
        PlayerPrefs.SetFloat("StrafeWalkSpeed", playerSettings.strafeWalkSpeed);
        PlayerPrefs.SetFloat("ForwardSprintSpeed", playerSettings.forwardSprintSpeed);
        PlayerPrefs.SetFloat("StrafeSprintSpeed", playerSettings.strafeSprintSpeed);
        PlayerPrefs.SetInt("maxJumps", maxJumps);

        for (int i = 1; i < inv.Count; i++)
        {
            PlayerPrefs.SetInt(inv[i].label, 1);
            PlayerPrefs.SetInt(inv[i].label + "Ammo", inv[i].ammo);
        }
        PlayerPrefs.Save();
    }

    public void LoadPlayerData()
    {
        int counter = 1;
        health = PlayerPrefs.GetInt("CurrentHealth");
        originalHealth = PlayerPrefs.GetInt("MaxHealth");
        sprintSpeed = PlayerPrefs.GetFloat("SprintSpeed");
        playerSettings.forwardWalkSpeed = PlayerPrefs.GetFloat("ForwardWalkSpeed");
        playerSettings.strafeWalkSpeed = PlayerPrefs.GetFloat("StrafeWalkSpeed");
        playerSettings.forwardSprintSpeed = PlayerPrefs.GetFloat("ForwardSprintSpeed");
        playerSettings.strafeSprintSpeed = PlayerPrefs.GetFloat("StrafeSprintSpeed");
        maxJumps = PlayerPrefs.GetInt("maxJumps");

        if (PlayerPrefs.HasKey("Pistol") && PlayerPrefs.GetInt("Pistol") == 1)
        {
            //add pistol to inventory
            addWeapon(pistol);
            inv[counter].ammo = PlayerPrefs.GetInt("PistolAmmo");
            counter++;
        }

        if (PlayerPrefs.HasKey("Rifle") && PlayerPrefs.GetInt("Rifle") == 1)
        {
            //add rifle to inventory
            addWeapon(rifle);
            inv[counter].ammo = PlayerPrefs.GetInt("RifleAmmo");
            counter++;
        }

        if (PlayerPrefs.HasKey("Sniper") && PlayerPrefs.GetInt("Sniper") == 1)
        {
            //add sniper to inventory
            addWeapon(sniper);
            inv[counter].ammo = PlayerPrefs.GetInt("SniperAmmo");
            counter++;
        }
        if (PlayerPrefs.HasKey("Grenade") && PlayerPrefs.GetInt("Grenade") == 1)
        {
            //add Grenade to inventory

        }
    }

    void RotatePlayer()
    {
        // Player Rotation
        if (!isRotating)
        {
            if (!gravityFlipped)
            {
                playerRotation.y += (isAiming ? playerSettings.cameraSensHor * playerSettings.aimSpeedEffector : playerSettings.cameraSensHor) * (playerSettings.invertX ? -inputCamera.x : inputCamera.x) * Time.deltaTime;
            }
            else
            {
                playerRotation.y -= (isAiming ? playerSettings.cameraSensHor * playerSettings.aimSpeedEffector : playerSettings.cameraSensHor) * (playerSettings.invertX ? -inputCamera.x : inputCamera.x) * Time.deltaTime;
            }

            playerRotation.z = playerRotationOffset;
            transform.localRotation = Quaternion.Euler(playerRotation);
        }
    }

    void Cam()
    {
        // Camera Rotation
        cameraRotation.x += (isAiming ? playerSettings.cameraSensVer * playerSettings.aimSpeedEffector : playerSettings.cameraSensVer) * (playerSettings.invertY ? inputCamera.y : -inputCamera.y) * Time.deltaTime;
        cameraRotation.x = Mathf.Clamp(cameraRotation.x, lockVerMin, lockVerMax);
        mainCamera.localRotation = Quaternion.Euler(cameraRotation);
    }

    void movement()
    {
        if (!isGrounded())
        {
            playerGravity -= gravity * Time.fixedDeltaTime;
        }
        else
        {
            if (!isPlayingSteps && movementSpeed.normalized.magnitude > 0.5f)
            {
                StartCoroutine(playSteps());
            }

            if (!gravityFlipped && playerGravity < -0.1f)
            {
                playerGravity = -0.1f;
                jumpCount = 0;
            }
            else if (gravityFlipped && playerGravity > 0.1f)
            {
                playerGravity = 0.1f;
                jumpCount = 0;
            }
        }

        if (inputMovement.y > 0.1f && isSprintButtonPressed && !isAiming && !isReloading && !isShootButtonPressed)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        if (Mathf.Abs(inputMovement.y) <= 0.1f && Mathf.Abs(inputMovement.x) <= 0.1f || isSprinting)
        {
            isWalking = false;
        }
        else
        {
            isWalking = true;
        }

        float verticalSpeed;
        float horizontalSpeed;

        if (!isSprinting)
        {
            verticalSpeed = playerSettings.forwardWalkSpeed;
            horizontalSpeed = playerSettings.strafeWalkSpeed;
        }
        else
        {
            verticalSpeed = playerSettings.forwardSprintSpeed;
            horizontalSpeed = playerSettings.strafeSprintSpeed;
        }

        //if (isShooting)
        //{
        //    isSprinting = false;
        //}

        float speedEffector = GetSpeedEffector();
        verticalSpeed *= speedEffector;
        horizontalSpeed *= speedEffector;

        weaponAnimSpeed = controller.velocity.magnitude / (playerSettings.forwardWalkSpeed * speedEffector);
        weaponAnimSpeed = Mathf.Clamp(weaponAnimSpeed, 0, 1);

        Vector3 moveDirection = CalculateMoveDirection();
        Vector3 newMovementSpeed = CalculateNewMovement(moveDirection, verticalSpeed, horizontalSpeed);

        controller.Move(newMovementSpeed);
    }

    Vector3 CalculateMoveDirection()
    {
        Vector3 transformForward = transform.forward.normalized;
        Vector3 transformRight = transform.right.normalized;

        transformForward.y = 0f;
        transformRight.y = 0f;

        Vector3 moveDirection = (inputMovement.y * transformForward) + (inputMovement.x * transformRight);

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
        else if (Mathf.Abs(inputMovement.x) < 0.1f && Mathf.Abs(inputMovement.y) < 0.1f)
        {
            moveDirection = Vector3.zero;
        }

        return moveDirection;
    }

    Vector3 CalculateNewMovement(Vector3 moveDirection, float verticalSpeed, float horizontalSpeed)
    {
        if (isGrounded())
        {
            playerSettings.isJumping = false;

            float forwardSpeed = verticalSpeed * inputMovement.y * Time.deltaTime;
            float strafeSpeed = horizontalSpeed * inputMovement.x * Time.deltaTime;
            movementSpeed = Vector3.SmoothDamp(movementSpeed, new Vector3(strafeSpeed, 0, forwardSpeed), ref velocitySpeed, isGrounded() ? playerSettings.movementSmoothing : playerSettings.fallingSmoothing);

            float backwardJumpingFactor = inputMovement.y < 0 ? 0.75f : 1f;
            playerSettings.jumpDirection = backwardJumpingFactor * movementSpeed.magnitude * moveDirection;
        }
        else
        {
            if (playerSettings.isJumping)
            {
                moveDirection.x = 0;
                moveDirection.z = 0;
            }
        }

        Vector3 newMovementSpeed;

        if (isGrounded() || !playerSettings.isJumping)
        {
            newMovementSpeed = moveDirection * movementSpeed.magnitude;
        }
        else
        {
            float angleBetweenJumps = Vector3.Angle(previousJumpDirection, playerSettings.jumpDirection); // Calculate the angle between previous jump direction and current jump direction

            if (angleBetweenJumps >= playerSettings.angleThreshold && timeSinceLastJump <= playerSettings.jumpTimeWindow)
            {
                float speedReductionFactor = Mathf.Clamp(angleBetweenJumps / 180f, 0.25f, 0.5f);
                playerSettings.jumpDirection *= speedReductionFactor;
            }

            timeSinceLastJump = 0f;
            previousJumpDirection = playerSettings.jumpDirection; // Update previousJumpDirection

            newMovementSpeed = playerSettings.jumpDirection;
        }

        newMovementSpeed.y += playerGravity;
        newMovementSpeed += jumpForce * Time.fixedDeltaTime;

        return newMovementSpeed;
    }

    float GetSpeedEffector()
    {
        if (!isGrounded())
        {
            return playerSettings.fallingSpeedEffector;
        }
        else if (playerPose == gameManager.PlayerPose.Crouch)
        {
            return playerSettings.crouchSpeedEffector;
        }
        else if (playerPose == gameManager.PlayerPose.Prone)
        {
            return playerSettings.proneSpeedEffector;
        }
        else if (isAiming)
        {
            return playerSettings.aimSpeedEffector;
        }

        return 1;
    }

    IEnumerator playSteps()
    {
        yield return new WaitForSeconds(0.25f);
        isPlayingSteps = false;
    }

    void inventory()
    {
        // Scroll wheel functionality
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) // positive
        {
            activeSlot++;
            if (activeSlot > inv.Count)
            {
                // Prevent scrollling past inventory limit.
                activeSlot--;
            }
            activeWeapon = inv[activeSlot - 1];
            // Enable inventory Highlight
            inventoryUI(activeSlot);
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // negative
        {
            activeSlot--;
            if (activeSlot < 1)
            {
                // Prevent scrollling past inventory limit.
                activeSlot = 1;
            }
            activeWeapon = inv[activeSlot - 1];
            // Enable inventory Highlight
            inventoryUI(activeSlot);
        }

        if (Input.GetButtonDown("1"))
        {
            if (activeSlot != 1)
            {
                StopCoroutine(weaponDelay());
            }
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
                if (activeSlot != 2)
                {
                    StopCoroutine(weaponDelay());
                }
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
                if (activeSlot != 3)
                {
                    StopCoroutine(weaponDelay());

                }
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
                if (activeSlot != 4)
                {
                    StopCoroutine(weaponDelay());
                }
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
                if (activeSlot != 5)
                {
                    StopCoroutine(weaponDelay());
                }
                // Set active weapon for shoot()
                activeWeapon = inv[4];
                activeSlot = 5;
                // Enable inventory Highlight
                inventoryUI(5);
            }
        }
    }

    IEnumerator weaponDelay()
    {
        StopCoroutine(shoot());
        isShooting = false;
        yield return new WaitForSeconds(1f);
    }

    void inventoryUI(int num)
    {
        // Edge case fix
        if (lastWeapon == null)
        {
            lastWeapon = 99;
        }
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

        if (activeWeapon != null)
        {
            ChangeWeapon();
            // Update UI for ammo to match current weapon
            bulletCountUpdate();

            // Make sure proper reticle is active
            //reticleSwap();
        }

    }

    void PlayerStance()
    {
        gameManager.PlayerStance currentStance = playerStandStance;

        if (playerPose == gameManager.PlayerPose.Crouch)
        {
            currentStance = playerCrouchStance;
        }
        else if (playerPose == gameManager.PlayerPose.Prone)
        {
            currentStance = playerProneStance;
        }

        cameraHeight = Mathf.SmoothDamp(mainCamera.localPosition.y, currentStance.cameraHeight, ref cameraHeightSpeed, playerPoseSmooth);
        mainCamera.localPosition = new Vector3(mainCamera.localPosition.x, cameraHeight, mainCamera.localPosition.z);

        controller.height = Mathf.SmoothDamp(controller.height, currentStance.stanceCollider.height, ref playerStanceHeightVelocity, playerPoseSmooth);
        controller.center = Vector3.SmoothDamp(controller.center, currentStance.stanceCollider.center, ref playerStanceCenterVelocity, playerPoseSmooth);
    }

    void Jump()
    {
        if (!isGrounded() && jumpCount >= maxJumps - 1)
        {
            // Player is in the air and has performed the maximum number of jumps already.
            return;
        }

        if (playerPose == gameManager.PlayerPose.Crouch)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
            {
                return;
            }
            playerPose = gameManager.PlayerPose.Stand;
            //return; This can be used to make the player stand up only and not jump at the same time.
        }
        else if (playerPose == gameManager.PlayerPose.Prone)
        {
            return;
        }

        // Perform the jump...
        jumpCount++;

        playerGravity = 0;

        if (!gravityFlipped)
        {
            jumpForce = new Vector3(0, playerSettings.jumpingHeight, 0);
        }
        else if (gravityFlipped)
        {
            jumpForce = new Vector3(0, -playerSettings.jumpingHeight, 0);
        }

        aud.PlayOneShot(audJump[UnityEngine.Random.Range(0, audJump.Length)], audJumpVol);
        currentWeapon.TriggerJump();
    }

    void Jumping()
    {
        if (IsHittingCeiling())
        {
            // Player is hitting the ceiling. Apply downward force, set jumpForce to zero, or handle it however you want.
            jumpForce = Vector3.zero;
        }

        if (!isGrounded())
        {
            //isSprinting = false; // Can comment this out if you want to always sprint
            playerSettings.isJumping = true;
            jumpForce = Vector3.SmoothDamp(jumpForce, Vector3.zero, ref jumpSpeed, playerSettings.jumpingFalloff);
            //StartCoroutine(ResetJump());
        }
        else
        {
            jumpCount = 0;
        }
    }

    IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(0.1f);
        if (isGrounded())
        {
            playerSettings.isJumping = false;
        }
    }

    void JumpTimer()
    {
        if (!playerSettings.isJumping)
        {
            timeSinceLastJump += Time.deltaTime;

            if (timeSinceLastJump > playerSettings.jumpTimeWindow)
            {
                timeSinceLastJump = playerSettings.jumpTimeWindow + 1f;
            }
        }
    }

    bool IsHittingCeiling()
    {
        if (!gravityFlipped)
        {
            return Physics.Raycast(transform.position, Vector3.up, ceilingCheckDistance, ceilingLayer);
        }
        else
        {
            return Physics.Raycast(transform.position, Vector3.down, ceilingCheckDistance, ceilingLayer);
        }

    }

    void SetIsFalling()
    {
        isFalling = !isGrounded() && controller.velocity.magnitude >= playerSettings.isFallingSpeed;
    }

    void Leaning()
    {
        if (aimToLean)
        {
            if (isAiming)
            {
                if (isLeaningLeft)
                {
                    targetLean = leanAngle;
                }
                else if (isLeaningRight)
                {
                    targetLean = -leanAngle;
                }
                else
                {
                    targetLean = 0;
                }
            }
            else
            {
                targetLean = 0;
            }
        }
        else
        {
            if (isLeaningLeft)
            {
                targetLean = leanAngle;
            }
            else if (isLeaningRight)
            {
                targetLean = -leanAngle;
            }
            else
            {
                targetLean = 0;
            }
        }

        currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, leanSmoothing);
        cameraLeanPivot.localRotation = Quaternion.Euler(new Vector3(0, 0, currentLean));
    }

    void Crouch()
    {
        if (playerPose == gameManager.PlayerPose.Crouch)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
            {
                return;
            }

            playerPose = gameManager.PlayerPose.Stand;
            return;
        }

        if (StanceCheck(playerCrouchStance.stanceCollider.height))
        {
            return;
        }

        playerPose = gameManager.PlayerPose.Crouch;
    }

    void Prone()
    {
        if (playerPose == gameManager.PlayerPose.Prone)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
            {
                if (StanceCheck(playerCrouchStance.stanceCollider.height))
                {
                    return;
                }

                playerPose = gameManager.PlayerPose.Crouch;
                return;
            }

            playerPose = gameManager.PlayerPose.Stand;
            return;
        }

        playerPose = gameManager.PlayerPose.Prone;
    }

    bool StanceCheck(float stanceCheckHeight)
    {
        Vector3 start;
        Vector3 end;

        if (!gravityFlipped)
        {
            start = playerTransform.position + Vector3.up * (controller.radius + stanceCheck);
            end = playerTransform.position + Vector3.up * (stanceCheckHeight - controller.radius - stanceCheck);
        }
        else
        {
            start = playerTransform.position + Vector3.down * (controller.radius + stanceCheck);
            end = playerTransform.position + Vector3.down * (stanceCheckHeight - controller.radius - stanceCheck);
        }

        return Physics.CheckCapsule(start, end, controller.radius, playerMask);
    }

    void flipGrav()
    {
        isRotating = true;
        gravityFlipped = !gravityFlipped;
        gravity *= -1;

        playerGravity = 0;
        aud.PlayOneShot(audGrav, audGravVol);

        if (gravityFlipped)
        {
            playerRotationOffset = 180;
        }
        else
        {
            playerRotationOffset = 0;
        }

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
            float time = elapsedTime / rotationDuration;
            controller.transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, time);

            // Apply input-based rotation directly during the coroutine
            if (!gravityFlipped)
            {
                playerRotation.y += (isAiming ? playerSettings.cameraSensHor * playerSettings.aimSpeedEffector : playerSettings.cameraSensHor) * (playerSettings.invertX ? -inputCamera.x : inputCamera.x) * Time.deltaTime;
            }
            else
            {
                playerRotation.y -= (isAiming ? playerSettings.cameraSensHor * playerSettings.aimSpeedEffector : playerSettings.cameraSensHor) * (playerSettings.invertX ? -inputCamera.x : inputCamera.x) * Time.deltaTime;
            }

            // Apply Z rotation
            playerRotation.z = Mathf.Lerp(startRotation.z, endRotation.z, time);
            transform.localRotation = Quaternion.Euler(playerRotation);

            yield return null;
        }

        onComplete?.Invoke();
    }

    public bool isGrounded()
    {
        RaycastHit floorCheck;
        if (!gravityFlipped)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out floorCheck, 1.25f) || controller.isGrounded)
            {
                return true;
            }
            return false;
        }
        else if (gravityFlipped)
        {
            if (Physics.Raycast(transform.position, Vector3.up, out floorCheck, 1.25f) || controller.isGrounded)
            {
                return true;
            }
            return false;
        }
        return false;
    }

    void ShootMouseClick()
    {
        if (activeWeapon != null)
        {
            if (Input.GetButton("Shoot"))
            {
                isShootButtonPressed = true;

                if (!isShooting && inv.Count > 0 && canShoot && !isReloading)
                {
                    shootCoroutine = StartCoroutine(shoot()); // Assign the shoot coroutine to the variable

                    //StartCoroutine(shoot());
                    //Update UI for bullet count
                    bulletCountUpdate();
                }
            }
            else
            {
                isShootButtonPressed = false;
            }
        }
    }

    void ammoState()
    {
        if (activeWeapon != null)
        {
            if (!isReloading)
            {
                if (activeWeapon.clipSize == 0 && activeWeapon.ammo > 0)
                {
                    gameManager.instance.reloadAmmoText.enabled = true;
                    gameManager.instance.lowAmmoText.enabled = false;
                    gameManager.instance.noAmmoText.enabled = false;
                }
                else if (activeWeapon.clipSize > 0 && activeWeapon.ammo == 0)
                {
                    gameManager.instance.lowAmmoText.enabled = true;
                    gameManager.instance.reloadAmmoText.enabled = false;
                    gameManager.instance.reloadingText.enabled = false;
                    gameManager.instance.noAmmoText.enabled = false;
                }
                else if (activeWeapon.clipSize == 0 && activeWeapon.ammo == 0)
                {
                    gameManager.instance.noAmmoText.enabled = true;
                    gameManager.instance.reloadAmmoText.enabled = false;
                    gameManager.instance.reloadingText.enabled = false;
                    gameManager.instance.lowAmmoText.enabled = false;
                }
                else
                {
                    gameManager.instance.reloadAmmoText.enabled = false;
                    gameManager.instance.reloadingText.enabled = false;
                    gameManager.instance.lowAmmoText.enabled = false;
                    gameManager.instance.noAmmoText.enabled = false;
                }
            }
            else
            {
                if (scenesManager.instance.mainMenuCheck == null)
                {
                    gameManager.instance.reloadingText.enabled = true;
                    gameManager.instance.reloadAmmoText.enabled = false;
                    gameManager.instance.lowAmmoText.enabled = false;
                    gameManager.instance.noAmmoText.enabled = false;
                }
            }
        }
    }

    IEnumerator shoot()
    {
        // If player isn't melee
        if (activeWeapon.label != "Unarmed" && activeWeapon.label != "Grenade" && activeWeapon != null)
        {
            if (activeWeapon.clipSize > 0)
            {
                aud.PlayOneShot(weaponShootAud, weaponShootVol);

                // Set flags
                isShooting = true;

                // Reduce current ammo
                activeWeapon.clipSize--;

                // Start bullet coroutine
                StartCoroutine(Bullet());

                yield return new WaitForSeconds(shootRate);

                float remainingShootTime = shootRate;

                while (activeSlot != lastWeapon && remainingShootTime > 0f)
                {
                    yield return null;
                    remainingShootTime -= Time.deltaTime;
                }

                isShooting = false;
            }
        }
        // Player is melee
        else if (activeWeapon.label == "Unarmed")
        {
            if (!isMelee)
            {
                StartCoroutine(melee());
            }
        }
        // Player has grenade selected
        else if (activeWeapon.label == "Grenade")
        {
            if (activeWeapon.clipSize > 0)
            {
                StartCoroutine(throwGrenade());
            }
        }
    }

    IEnumerator Bullet()
    {
        // Calculate direction of shot
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        RaycastHit hit;
        Vector3 rayDirection = ray.direction.normalized;

        // Fire projectile
        GameObject bulletClone = Instantiate(playerBullet, shootPos.position, playerBullet.transform.rotation);

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 direction = hit.point - shootPos.position;
            bulletClone.GetComponent<Rigidbody>().velocity = direction.normalized * bulletSpeed;
        }

        float distanceCovered = 0f;
        while (distanceCovered < shootRange)
        {
            float distanceToTravel = bulletSpeed * Time.deltaTime;
            if (Physics.Raycast(ray.origin + rayDirection * distanceCovered, rayDirection, out hit, distanceToTravel))
            {
                if (hit.collider.GetComponent<IDamage>() != null && !hit.collider.CompareTag("Player"))
                {
                    hit.collider.GetComponent<IDamage>().takeDamage(weaponDamage);
                    Destroy(bulletClone);
                    break;  // stop the loop if we hit an enemy
                }
            }

            // Debug the raycast
            Debug.DrawRay(ray.origin + rayDirection * distanceCovered, rayDirection * distanceToTravel, Color.red, 2f);

            distanceCovered += distanceToTravel;
            yield return null;  // wait for the next frame
        }
    }

    IEnumerator throwGrenade()
    {
        isShooting = true;
        // Enter loop while player is **holding** left click, leave when player releases
        while (Input.GetButton("Shoot"))
        {
            yield return new WaitForEndOfFrame();
        }
        // Loop has cancelled, meaning player let go of left click.
        // Throw grenade
        GameObject grenadeClone = Instantiate(grenade, shootPos.position, playerBullet.transform.rotation);
        Rigidbody projectile = grenadeClone.GetComponent<Rigidbody>();
        projectile.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);

        // Reduce current ammo
        activeWeapon.clipSize--;
        bulletCountUpdate();

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    void AimPressed()
    {
        if (activeWeapon != null && activeWeapon.label != "Unarmed")
        {
            isAiming = true;
        }
    }

    void AimReleased()
    {
        isAiming = false;
    }

    void Aiming()
    {
        if (!currentWeapon && activeWeapon == null && activeWeapon.label == "Unarmed")
        {
            return;
        }

        currentWeapon.isAiming = isAiming;
    }

    IEnumerator melee()
    {
        isMelee = true;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootRange))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                IDamage damageable = hit.collider.GetComponent<IDamage>();
                if (damageable != null)
                {
                    damageable.takeDamage(weaponDamage);
                    aud.PlayOneShot(weaponShootAud, weaponShootVol);
                }
            }
        }

        yield return new WaitForSeconds(shootRate);
        isMelee = false;
    }

    void zoom()
    {
        //    if (activeWeapon != null)
        //    {
        //        isZoomed = Input.GetButton("Zoom");
        //        if (isZoomed && activeWeapon.canZoom == true)
        //        {
        //            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 0, 10f * Time.deltaTime);
        //        }
        //        else
        //        {
        //            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60, 10f * Time.deltaTime);
        //        }
        //    }


        if (isSprinting)
        {
            //movementSpeed = sprintSpeed;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 80, 10f * Time.deltaTime);
        }
        // Reset function
        else
        {
            movementSpeed = originalMovementSpeed;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60, 10f * Time.deltaTime);
        }
    }

    IEnumerator reload()
    {
        isReloading = true;
        activeWeapon.reloadState = true;
        // Audio conditional
        if (activeWeapon.clipSize < activeWeapon.maxClip)
        {
            //reloadAudioSource.clip = weaponReloadAud;
            //reloadAudioSource.volume = weaponReloadVol;
            //reloadAudioSource.Play();
            reloadAudioSource.PlayOneShot(weaponReloadAud, weaponReloadVol);
            //aud.PlayOneShot(weaponReloadAud, weaponReloadVol);
        }

        if (!activeWeapon.reloadState || activeWeapon.ammo == activeWeapon.clipSize)
        {
            yield break;
        }

        yield return new WaitForSeconds(activeWeapon.reloadTime);

        //if (!activeWeapon.reloadState || activeWeapon.ammo == activeWeapon.clipSize)
        //{
        //    yield break;
        //}

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
                }
            }
        }

        bulletCountUpdate();
        isReloading = false;
        activeWeapon.reloadState = false;
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
        if (activeSlot != 1 && activeWeapon != null)
        {
            gameManager.instance.gunReticle.SetActive(true);
            gameManager.instance.meleeReticle.SetActive(false);

            if (isAiming)
            {
                gameManager.instance.gunReticle.SetActive(false);
            }
            else
            {
                gameManager.instance.gunReticle.SetActive(true);
            }
        }
        else
        {
            gameManager.instance.meleeReticle.SetActive(true);
            gameManager.instance.gunReticle.SetActive(false);
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

    public void ResetWeapon()
    {
        weaponHolderPos.transform.localPosition = new Vector3(0, 0, 0);
        weaponHolderPos.transform.localScale = new Vector3(0, 0, 0);
        weaponPos.transform.localPosition = new Vector3(0, 0, 0);
        weaponPos.transform.eulerAngles = new Vector3(0, 0, 0);
        weaponPos.transform.localScale = new Vector3(1, 1, 1);
        weaponSightsPos.transform.localPosition = new Vector3(0, 0, 0);
        shootPos.transform.localPosition = new Vector3(0, 0, 0);

        //shootEffectPos.localPosition = new Vector3(0, 0, 0);

        bulletCountUpdate();

        // Stop the reload audio if it's currently playing
        if (reloadAudioSource != null && reloadAudioSource.isPlaying)
        {
            reloadAudioSource.Stop();
        }

        if (isReloading)
        {
            isReloading = false;
        }
    }

    public void ChangeWeapon()
    {
        if (activeSlot == lastWeapon)
        {
            return;
        }

        ResetWeapon();

        weaponHolderPos.transform.localPosition = activeWeapon.weaponHolderPos;
        weaponPos.transform.eulerAngles = activeWeapon.weaponRot;
        weaponHolderPos.transform.localScale = activeWeapon.weaponScale;
        weaponSightsPos.transform.localPosition = activeWeapon.weaponSightsPos;
        shootPos.transform.localPosition = activeWeapon.shootPos;

        currentWeapon.sightOffset = activeWeapon.sightOffset;
        currentWeapon.zoomMaxFov = activeWeapon.zoomMaxFov;
        currentWeapon.zoomInFOVSpeed = activeWeapon.zoomInFOVSpeed;
        currentWeapon.zoomOutFOVSpeed = activeWeapon.zoomOutFOVSpeed;
        currentWeapon.ADSSpeed = activeWeapon.ADSSpeed;

        weaponDamage = activeWeapon.damage;
        shootRate = activeWeapon.rate;
        shootRange = activeWeapon.range;
        bulletSpeed = activeWeapon.bulletSpeed;

        weaponShootAud = activeWeapon.weaponShootAud;
        weaponShootVol = activeWeapon.weaponShotVol;
        weaponReloadAud = activeWeapon.weaponReloadAud;
        weaponReloadVol = activeWeapon.weaponReloadVol;
        weaponMesh.sharedMesh = activeWeapon.model.GetComponent<MeshFilter>().sharedMesh;
        weaponMaterial.sharedMaterial = activeWeapon.model.GetComponent<MeshRenderer>().sharedMaterial;

        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            isShooting = false;
        }

        if (activeWeapon.reloadState && !isReloading)
        {
            StartCoroutine(reload());
        }
        // Set last weapon to current weapon for purpose of not overriding graphics
        lastWeapon = activeSlot;
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

    void ToggleSprint()
    {
        isSprintButtonPressed = true;

        if (isShootButtonPressed || isReloading)
        {
            return;
        }

        if (inputMovement.y <= 0.2f || isAiming || playerPose == gameManager.PlayerPose.Prone)
        {
            isSprinting = false;
            return;
        }

        if (playerPose == gameManager.PlayerPose.Crouch)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
            {
                return;
            }

            playerPose = gameManager.PlayerPose.Stand;
        }

        isSprinting = true;
    }

    void StopSprint()
    {
        isSprintButtonPressed = false;

        if (playerSettings.holdSprint)
        {
            isSprinting = false;
        }
    }
}