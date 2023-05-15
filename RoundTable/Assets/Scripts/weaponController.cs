using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponController : MonoBehaviour
{
    playerController controller;

    [Header("---References---")]
    public Animator weaponAnimator;

    [Header("---weapon---")]
    public gameManager.WeaponSettings weaponSettings;

    bool isInitialized;

    Vector3 weaponRotation;
    Vector3 weaponRotationVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    Vector3 weaponMovementRotation;
    Vector3 weaponMovementRotationVelocity;

    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementRotationVelocity;

    bool isGroundedTrigger;

    public float fallingDelay;

    [Header("Weapon Idle Sway")]
    public Transform weaponSway;
    public Transform weaponBreathing;
    public Transform cameraSwayADS;
    public float swayAmountA;
    public float swayAmountB;
    public float swayScale;
    public float swayScaleADS;
    public float swayLerpSpeed;
    public float swayTime;
    Vector3 swayPosition;
    Vector3 cameraOriginalPosition = new(0, 0, 0);
    public float lerpSpeed;

    [Header("Sights")]
    public bool isAiming;
    public Transform weaponSight;
    public float sightOffset;
    public float ADSSpeed;
    public float fovOrig;
    public float zoomMaxFov;
    public int zoomInFOVSpeed;
    public int zoomOutFOVSpeed;
    Vector3 weaponSwayPosition;
    Vector3 weaponSwayPositionVelocity;

    public void Start()
    {
        weaponRotation = transform.localRotation.eulerAngles;
        fovOrig = Camera.main.fieldOfView;
    }

    public void Initialize(playerController PlayerController)
    {
        controller = PlayerController;
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        WeaponRotation();
        SetWeaponAnimation();
        WeaponIdleSway();
        Aiming();
    }

    void Aiming()
    {
        Vector3 targetPosition = transform.position;

        if (isAiming)
        {
            targetPosition = controller.cam.position + (weaponSway.position - weaponSight.position) + (controller.cam.forward * sightOffset);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, zoomMaxFov, zoomInFOVSpeed * Time.deltaTime);
            //cameraSwayADS.localEulerAngles = swayPosition; Needs work. Trying to rotate camera with gun ADS sway.
        }
        else
        {
            cameraSwayADS.localPosition = cameraOriginalPosition;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fovOrig, zoomOutFOVSpeed * Time.deltaTime);
        }

        weaponSwayPosition = weaponSway.position;
        weaponSwayPosition = Vector3.SmoothDamp(weaponSwayPosition, targetPosition, ref weaponSwayPositionVelocity, ADSSpeed);
        weaponSway.position = weaponSwayPosition;
    }

    public void TriggerJump()
    {
        isGroundedTrigger = false;
        weaponAnimator.SetTrigger("Jump");
    }

    void WeaponRotation()
    {
        targetWeaponRotation.y += (isAiming ? weaponSettings.swayAmountADS : weaponSettings.swayAmount) * (weaponSettings.swayXInverted ? -controller.inputCamera.x : controller.inputCamera.x);// * Time.deltaTime; // Horizontal Rotation
        targetWeaponRotation.x += (isAiming ? weaponSettings.swayAmountADS : weaponSettings.swayAmount) * (weaponSettings.swayYInverted ? controller.inputCamera.y : -controller.inputCamera.y);// * Time.deltaTime; // Vertical Rotation

        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -weaponSettings.swayClampX, weaponSettings.swayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -weaponSettings.swayClampY, weaponSettings.swayClampY);
        targetWeaponRotation.z = isAiming ? 0 : targetWeaponRotation.y * weaponSettings.swayLeanAmount;

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, weaponSettings.swayResetSmoothing);
        weaponRotation = Vector3.SmoothDamp(weaponRotation, targetWeaponRotation, ref weaponRotationVelocity, weaponSettings.swaySmoothing);

        targetWeaponMovementRotation.z = (isAiming ? weaponSettings.swayMovementXADS : weaponSettings.swayMovementX) * (weaponSettings.swayMovementXInverted ? -controller.inputMovement.x : controller.inputMovement.x);
        targetWeaponMovementRotation.x = (isAiming ? weaponSettings.swayMovementYADS : weaponSettings.swayMovementY) * (weaponSettings.swayMovementYInverted ? -controller.inputMovement.y : controller.inputMovement.y);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, weaponSettings.swayMovementSmoothing);
        weaponMovementRotation = Vector3.SmoothDamp(weaponMovementRotation, targetWeaponMovementRotation, ref weaponMovementRotationVelocity, weaponSettings.swayMovementSmoothing);

        transform.localRotation = Quaternion.Euler(weaponRotation + weaponMovementRotation);
    }

    void SetWeaponAnimation()
    {
        if (isGroundedTrigger)
        {
            fallingDelay = 0;
        }
        else
        {
            fallingDelay += Time.deltaTime;
        }

        if (controller.isGrounded() && !isGroundedTrigger && fallingDelay > 0.1f)
        {
            weaponAnimator.SetTrigger("Land");
            isGroundedTrigger = true;
        }
        else if (!controller.isGrounded() && isGroundedTrigger)
        {
            weaponAnimator.SetTrigger("Falling");
            isGroundedTrigger = false;
        }

        weaponAnimator.SetBool("IsWalking", controller.isWalking);
        weaponAnimator.SetBool("IsSprinting", controller.isSprinting);
        weaponAnimator.SetFloat("WeaponAnimSpeed", controller.weaponAnimSpeed);
    }

    void WeaponIdleSway()
    {
        Vector3 targetPosition = Curve(swayTime, swayAmountA, swayAmountB) / (isAiming ? swayScaleADS : swayScale);

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * swayLerpSpeed);
        swayTime += Time.deltaTime;

        if (swayTime > 6.29475f)
        {
            swayTime = 0;
        }

        weaponBreathing.localPosition = swayPosition;
    }

    Vector3 Curve(float Time, float A, float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }
}
