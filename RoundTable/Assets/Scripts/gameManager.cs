using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using JetBrains.Annotations;
using System;
using UnityEngine.SceneManagement;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioSource music;

    [Header("----- Player Stuff -----")]
    public GameObject player;
    public playerController playerScript;
    public GameObject playerSpawnPos;

    [Header("----- UI Stuff -----")]
    public GameObject activeMenu;
    public GameObject pauseMenu;
    public GameObject winMenu;
    public GameObject loseMenu;
    public GameObject optionsMenu;
    public GameObject creditsText;
    public TextMeshProUGUI creditsValueText;
    public GameObject gunReticle;
    public GameObject meleeReticle;
    public Image HPBar;
    public TextMeshProUGUI enemiesRemainingText;
    public TextMeshProUGUI bulletCountText;
    public GameObject loadMenu;
    public Image fillBar;
    public GameObject skipText;
    // Hotbar selected slot
    public GameObject glow1;
    public GameObject glow2;
    public GameObject glow3;
    public GameObject glow4;
    public GameObject glow5;
    // Dynamically add item to hotbar (graphic). Ignore slot 1 since it is always melee
    public GameObject item2;
    public GameObject item3;
    public GameObject item4;
    public GameObject item5;

    [Header("----- Timer Stuff -----")]
    [SerializeField] int timeValue;
    [SerializeField] int extractValue;
    public GameObject timeUntilText;
    public TextMeshProUGUI timerText;
    public GameObject extractionText;
    public TextMeshProUGUI extractionTimerText;
    public GameObject extractionZone;

    [Header("----- Conditional UI -----")]
    public GameObject shopPromptText;
    public GameObject artifactPromptText;

    [Header("----- Main Menu UI -----")]
    [SerializeField] public GameObject creditsUI;
    [SerializeField] public GameObject extractionUI;
    [SerializeField] public GameObject hotbarSquaresUI;
    [SerializeField] public GameObject enemiesRemainingUI;
    [SerializeField] public GameObject bulletCountUI;
    [SerializeField] public GameObject hpBarUI;
    [SerializeField] public GameObject reticleUI;

    public int enemiesRemaining;
    [SerializeField] public int credits;
    public bool isPaused;
    float timeScaleOrig;
    bool extractStarted;

    [Header("Audio")]
    [SerializeField] AudioClip audWin;
    [SerializeField][Range(0, 1)] float audWinVol;
    [SerializeField] AudioClip audLose;
    [SerializeField][Range(0, 1)] float audLoseVol;
    [SerializeField] AudioClip audMenuOpen;
    [SerializeField][Range(0, 1)] float audMenuOpenVol;
    [SerializeField] AudioClip audMenuClose;
    [SerializeField][Range(0, 1)] float audMenuCloseVol;
    [SerializeField] AudioClip audExtractionAppear;
    [SerializeField][Range(0, 1)] float audExtractionVol;

    public enum PlayerPose
    {
        Stand,
        Crouch,
        Prone
    }

    [Serializable]
    public class PlayerSettings
    {
        [Header("---Camera Settings---")]
        public float cameraSensHor;
        public float cameraSensVer;

        public float ADSSensEffector;

        public bool invertX;
        public bool invertY;

        [Header("---Movement---")]
        public bool holdSprint; //----- Used for Toggle to Sprint or Hold to Sprint - True = Hold, False = Toggle.
        public float movementSmoothing;

        [Header("---Walking---")]
        public float forwardWalkSpeed;
        public float backwardWalkSpeed;
        public float strafeWalkSpeed;

        [Header("---Sprinting---")]
        public float forwardSprintSpeed;
        public float strafeSprintSpeed;

        [Header("---Jumping---")]
        public bool isJumping;
        public Vector3 jumpDirection;
        public float jumpingHeight;
        public float jumpingFalloff;
        public float fallingSmoothing;
        public float backwardJumpFactor;
        public float angleThreshold;
        public float jumpTimeWindow;

        [Header("---Speed Effectors---")]
        public float speedEffector;
        public float crouchSpeedEffector;
        public float proneSpeedEffector;
        public float fallingSpeedEffector;
        public float aimSpeedEffector;

        [Header("---Is Grounded / Falling---")]
        public float isGroundedRadius;
        public float isFallingSpeed;
        public float groundDistance;
    }

    [Serializable]
    public class PlayerStance
    {
        public float cameraHeight;
        public CapsuleCollider stanceCollider;
    }

    [Serializable]
    public class WeaponSettings
    {
        [Header("---Weapon Sway---")]
        public float swayAmount;
        public float swayAmountADS;
        public float swaySmoothing;
        public float swayResetSmoothing;
        public float swayClampX;
        public float swayClampY;
        public float swayLeanAmount;
        public bool swayYInverted;
        public bool swayXInverted;

        [Header("---Weapon Movement---")]
        public float swayMovementX;
        public float swayMovementY;
        public float swayMovementXADS;
        public float swayMovementYADS;
        public bool swayMovementYInverted;
        public bool swayMovementXInverted;
        public float swayMovementSmoothing;
    }

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        playerSpawnPos = GameObject.FindGameObjectWithTag("Player Spawn Pos");
        if (player == null && scenesManager.instance.mainMenuCheck == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            playerScript = player.GetComponent<playerController>();
            playerScript.setPlayerPos();
            playerScript.playerUIUpdate();
        }
        extractionZone = GameObject.FindGameObjectWithTag("Extraction Zone");
        timeScaleOrig = Time.timeScale;

    }

    // Update is called once per frame
    void Update()
{
        if (Input.GetButtonDown("Cancel") && activeMenu == null && scenesManager.instance.mainMenuCheck == null)
        {
            isPaused = !isPaused;
            activeMenu = pauseMenu;
            activeMenu.SetActive(isPaused);

            if (isPaused)
            {
                pauseState();
            } 
            else
            {
                unpauseState();
            }
                
        }
        if (playerScript != null)
            playerScript.playerUIUpdate();
    }

    private void Start()
    {
        if (extractionZone)
        {
            extractionZone.SetActive(false);
        }

        if (gameManager.instance.extractionZone != null && playerScript.hasObjective) 
        {
            startTimer();
        }

        creditsValueText.text = credits.ToString();

        if(PlayerPrefs.HasKey("Credits") && scenesManager.instance.mainMenuCheck == null)
        {
            loadCredits();
            gameManager.instance.playerScript.LoadPlayerData();
            creditsAvailableUIUpdate();
        }

        if(PlayerPrefs.HasKey("MasterVolume"))
        {
            //Load audio settings here
            volumeController.instance.updateVolume();
        }
    }

    public void pauseState()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        aud.PlayOneShot(audMenuOpen, audMenuOpenVol);
    }
    public void unpauseState()
    {
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        activeMenu.SetActive(false);
        activeMenu = null;
        aud.PlayOneShot(audMenuClose, audMenuCloseVol);
    }

    public void updateGameGoal(int amount)
    {
        enemiesRemaining += amount;
        enemiesRemainingUIUpdate();
        creditsAvailableUIUpdate();
        if (gameManager.instance.playerScript.hasObjective && !extractStarted)
        {
            StartCoroutine(timerUpdate(timeValue));
        }
    }

    public void playerDead()
    {
        pauseState();
        activeMenu = loseMenu;
        activeMenu.SetActive(true);
        aud.PlayOneShot(audLose, audLoseVol);
    }

    public void startTimer()
    {
        StartCoroutine(timerUpdate(timeValue));
    }
    void enemiesRemainingUIUpdate()
    {
        enemiesRemainingText.text = enemiesRemaining.ToString();
    }

    public void creditsAvailableUIUpdate()
    {
        creditsValueText.text = credits.ToString();
    }

    public void winCondition()
    {
        aud.PlayOneShot(audWin, audWinVol);
        activeMenu = winMenu;
        activeMenu.SetActive(true);
        pauseState();
    }

    IEnumerator timerUpdate(int time)
    {
        extractStarted = true;
        extractionZone.SetActive(false);
        // Time until extraction starts
        for (int i = time; i >= 0; i--)
        {
            timerText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
        StartCoroutine(startExtraction(extractValue));
    }

    IEnumerator startExtraction(int extract)
    {
        if (timeUntilText.activeSelf == true) 
        {
            
            timeUntilText.SetActive(false);
            extractionZone.SetActive(true);
            aud.PlayOneShot(audExtractionAppear, audExtractionVol);
            /* Time until extraction ends
            extractionText.SetActive(true);
            for (int i = extract; i >= 0; i--)
            {
                extractionTimerText.text = i.ToString();
                yield return new WaitForSeconds(1);
            }
            playerDead();
            */
            yield return new WaitForEndOfFrame();
        }
    }
    public void saveCredits()
    {
        PlayerPrefs.SetInt("Credits", credits);
        PlayerPrefs.Save();
    }
    public void loadCredits()
    {
        credits = PlayerPrefs.GetInt("Credits");
    }
}
