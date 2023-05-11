using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class playerController : MonoBehaviour, IDamage
{
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
    [SerializeField] GameObject activeModel;
    [SerializeField] AudioSource aud;

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
    [Range(0, 10)][SerializeField] float shootRate;
    [Range(0, 500)][SerializeField] int shootDist;
    [Range(0, 250)][SerializeField] int shootDmg;
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
    bool isReloading;
    bool isZoomed;
    bool isAiming;
    int zoomedFov;
    bool isMelee;
    bool isPlayingSteps;
    public bool hasObjective;

    [Header("--- Weapon Transformations---")]
    public Transform weaponHolderPos;
    public Transform weaponPos;
    public Transform weaponSightsPos;
    [SerializeField] Transform shootPos;

    [Header("Store")]
    [Header("----- Weapon ADS Stats -----")]
    public float zoomMax;
    public int zoomInSpeed;
    public int zoomOutSpeed;

    [Header("Leaning")]
    public Transform cameraLeanPivot;
    public float leanAngle;
    public float leanSmoothing;
    float currentLean;
    float targetLean;
    float leanVelocity;

    bool isLeaningLeft;
    bool isLeaningRight;

    [Header("---Settings---")]
    public LayerMask playerMask;
    public LayerMask groundMask;
    public gameManager.PlayerSettings playerSettings;
    [SerializeField] float gravity;
    [SerializeField] Vector3 jumpForce;
    public float playerGravity;
    Vector3 jumpSpeed;
    //public bool isGrounded;
    public bool isFalling;

    public float timeSinceLastJump;
    Vector3 previousJumpDirection;

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
    [SerializeField] GameObject pistol;
    [SerializeField] GameObject rifle;
    [SerializeField] GameObject sniper;

    [Header("Audio")]
    [SerializeField] AudioClip[] audSteps;
    [SerializeField][Range(0, 1)] float audStepsVol;
    [SerializeField] AudioClip[] audJump;
    [SerializeField][Range(0, 1)] float audJumpVol;
    [SerializeField] AudioClip audGrav;
    [SerializeField][Range(0, 1)] float audGravVol;
    [SerializeField] AudioClip audPickup;
    [SerializeField][Range(0, 1)] float audPickupVol;

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


        input.Weapon.Shoot.performed += e => shoot();
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
        canShoot = true;
        canRotate = false;
        camerHeightOrig = cameraHeight;
        playerRotationOffset = 0f;

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

    void Update()
    {
        if (Time.timeScale != 0)
        {
            RotatePlayer();
            movement();
            PlayerStance();
            inventory();
            zoom();
            Leaning();
            Aiming();
            //zoom();
            //selectGun();
            SetIsFalling();

            Jumping();
            JumpTimer();
            ShootMouseClick();

            if (Input.GetButtonDown("Flip Gravity") && !isRotating && canRotate)
            {
                flipGrav();
            }

            if (!isReloading && Input.GetButtonDown("Reload"))
            {
                StartCoroutine(reload());
                bulletCountUpdate();
            }
        }
    }

    void LateUpdate()
    {
        Cam();
    }

    public void setPlayerPos()
    {
        controller.enabled = false;
        transform.position = gameManager.instance.playerSpawnPos.transform.position;
        controller.enabled = true;
    }

    void resetStats()
    {
        health = originalHealth;
        movementSpeed = originalMovementSpeed;
        jumpHeight = originalJumpHeight;
        maxJumps = originalMaxJumps;
        gravity = originalGravity;
    }

    public void SavePlayerData()
    {
        PlayerPrefs.SetInt("Health", health);
        PlayerPrefs.SetFloat("SprintSpeed", sprintSpeed);
        PlayerPrefs.SetInt("maxJumps", maxJumps);

        for (int i = 1; i < inv.Count; i++)
        {
            PlayerPrefs.SetString(inv[i].label, inv[i].label);
            PlayerPrefs.SetInt(inv[i].label + "Ammo", inv[i].ammo);
        }

        PlayerPrefs.Save();
    }

    public void LoadPlayerData()
    {
        health = PlayerPrefs.GetInt("Health");
        sprintSpeed = PlayerPrefs.GetFloat("SprintSpeed");
        maxJumps = PlayerPrefs.GetInt("maxJumps");

        if (PlayerPrefs.HasKey("Pistol"))
        {
            //add pistol to inventory
        }

        if (PlayerPrefs.HasKey("Rifle"))
        {
            //add pistol to inventory
        }

        if (PlayerPrefs.HasKey("Sniper"))
        {
            //add pistol to inventory
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
            playerGravity -= gravity * Time.deltaTime;
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

        if (inputMovement.y <= 0.1f || isAiming)
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
            canShoot = true;
            verticalSpeed = playerSettings.forwardWalkSpeed;
            horizontalSpeed = playerSettings.strafeWalkSpeed;
        }
        else
        {
            verticalSpeed = playerSettings.forwardSprintSpeed;
            horizontalSpeed = playerSettings.strafeSprintSpeed;
        }

        if (canShoot && isShooting)
        {
            isSprinting = false;
        }

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

        playerVelocity.y -= gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
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
        newMovementSpeed += jumpForce * Time.deltaTime;

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
        if (!isGrounded() || playerPose == gameManager.PlayerPose.Prone)
        {
            return;
        }

        if (playerPose == gameManager.PlayerPose.Crouch)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
            {
                return;
            }

            playerPose = gameManager.PlayerPose.Stand;
            //return; --------------------------- This can be used to make the player stand up only and not jump at the same time.
        }

        if (!isGrounded() && jumpCount == 0)
        {
            jumpCount = maxJumps;
        }
        // Compare number of jumps preformed & max number of jumps to check if player can jump.

        if (jumpCount < maxJumps)
        {
            jumpCount++;
            // Check state of gravity to determine direction to jump the player
        }

        if (isGrounded())
        {
            playerGravity = 0;

            if (!gravityFlipped)
            {
                playerVelocity.y = jumpHeight;

                jumpForce = new Vector3(0, playerSettings.jumpingHeight, 0);
            }
            else if (gravityFlipped)
            {
                playerVelocity.y = -jumpHeight;
                jumpForce = new Vector3(0, -playerSettings.jumpingHeight, 0);
            }

            aud.PlayOneShot(audJump[UnityEngine.Random.Range(0, audJump.Length)], audJumpVol);
            currentWeapon.TriggerJump();
        }
    }

    void Jumping()
    {
        if (!isGrounded())
        {
            isSprinting = false; // Can comment this out if you want to always sprint
            playerSettings.isJumping = true;
            jumpForce = Vector3.SmoothDamp(jumpForce, Vector3.zero, ref jumpSpeed, playerSettings.jumpingFalloff);
            //StartCoroutine(ResetJump());
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
    void SetIsFalling()
    {
        isFalling = !isGrounded() && controller.velocity.magnitude >= playerSettings.isFallingSpeed;
    }

    void Leaning()
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

        playerVelocity.y = 0;
        //playerVelocity.y = 0;
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
            //playerStandStance.stanceCollider.transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, time);
            //playerCrouchStance.stanceCollider.transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, time);
            //playerProneStance.stanceCollider.transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, time);

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
                Debug.DrawRay(transform.position, Vector3.down, Color.red);
                return true;
            }
            return false;
        }
        else if (gravityFlipped)
        {
            if (Physics.Raycast(transform.position, Vector3.up, out floorCheck, 1.25f) || controller.isGrounded)
            {
                Debug.DrawRay(transform.position, Vector3.up, Color.red);
                return true;
            }
            return false;
        }
        return false;
    }
    void ShootMouseClick()
    {
        if (!isShooting && Input.GetButton("Shoot") && inv.Count > 0 && canShoot)
        {
            StartCoroutine(shoot());
            //Update UI for bullet count
            bulletCountUpdate();
        }
        else
        {
            StopCoroutine(shoot());
        }
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

    void AimPressed()
    {
        isAiming = true;
    }

    void AimReleased()
    {
        isAiming = false;
    }

    void Aiming()
    {
        if (!currentWeapon)
        {
            return;
        }

        currentWeapon.isAiming = isAiming;
    }

    IEnumerator melee()
    {
        isMelee = true;
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
            //movementSpeed = sprintSpeed;
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

        isSprinting = !isSprinting;
    }

    void StopSprint()
    {
        if (playerSettings.holdSprint)
        {
            isSprinting = false;
        }
    }
}

// Update is called once per frame
//void Update()
//{
//    if (!gameManager.instance.isPaused)
//    {
//        movement();
//        inventory();
//        //zoom();

//        if (Input.GetButtonDown("Flip Gravity") && !isRotating && canRotate)
//        {
//            flipGrav();
//        }

//        if (!isReloading && Input.GetButtonDown("Reload"))
//        {
//            StartCoroutine(reload());
//            bulletCountUpdate();
//        }

//        if (!isShooting && Input.GetButton("Shoot"))
//        {
//            StartCoroutine(shoot());
//            // Update UI for bullet count
//            bulletCountUpdate();
//        }
//    }
//}

//void jump()
//{
//    // If player is not grounded and didn't jump to enter the state effectively disable double/extra jumps
//    if (groundedPlayer == false && jumpCount == 0)
//    {
//        jumpCount = maxJumps;
//    }
//    // Compare number of jumps preformed & max number of jumps to check if player can jump.
//    if (jumpCount < maxJumps)
//    {
//        jumpCount++;
//        // Check state of gravity to determine direction to jump the player
//        if (!gravityFlipped)
//        {
//            playerVelocity.y = jumpHeight;
//            aud.PlayOneShot(audJump[UnityEngine.Random.Range(0, audJump.Length)], audJumpVol);
//        }
//        else if (gravityFlipped)
//        {
//            playerVelocity.y = -jumpHeight;
//            aud.PlayOneShot(audJump[UnityEngine.Random.Range(0, audJump.Length)], audJumpVol);
//        }
//    }
//}

//void zoom()
//{
//    if (activeWeapon != null)
//    {
//        isAiming = Input.GetButton("Zoom");
//        if (isAiming && activeWeapon.canZoom == true)
//        {
//            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 0, 10f * Time.deltaTime);
//        }
//        else
//        {
//            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 80, 10f * Time.deltaTime);
//        }
//    }


//    if (isSprinting)
//    {
//        movementSpeed = sprintSpeed;
//        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 100, 10f * Time.deltaTime);
//    }
//    // Reset function
//    else
//    {
//        movementSpeed = originalMovementSpeed;
//        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 80, 10f * Time.deltaTime);
//    }
//}

//void movement()
//{
//    isSprinting = Input.GetButton("Sprint");
//    groundedPlayer = isGrounded();
//    if (groundedPlayer)
//    {
//        if (!isPlayingSteps && move.normalized.magnitude > 0.5f)
//        {
//            StartCoroutine(playSteps());
//        }
//        if (!gravityFlipped && playerVelocity.y < 0)
//        {
//            playerVelocity.y = 0;
//            jumpCount = 0;
//        }
//        else if (gravityFlipped && playerVelocity.y > 0)
//        {
//            playerVelocity.y = 0;
//            jumpCount = 0;
//        }

//    }

//    // Might implement if-check to prevent/lower air-strafing.
//    move = (transform.right * Input.GetAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical"));


//    // If sprint is held
//    if (isSprinting)
//    {
//        movementSpeed = sprintSpeed;
//        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 100, 10f * Time.deltaTime);
//    }
//    // Reset function
//    else
//    {
//        movementSpeed = originalMovementSpeed;
//        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 80, 10f * Time.deltaTime);
//    }
//    controller.Move(movementSpeed * Time.deltaTime * move);

//    if (Input.GetButtonDown("Jump"))
//    {
//        jump();
//    }

//    playerVelocity.y -= gravity * Time.deltaTime;
//    controller.Move(playerVelocity * Time.deltaTime);

//}