using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header("----- Player Stuff -----")]
    public GameObject player;
    public playerController playerScript;

    [Header("----- UI Stuff -----")]
    public GameObject activeMenu;
    public GameObject pauseMenu;
    public GameObject winMenu;
    public GameObject loseMenu;
    public GameObject gunReticle;
    public GameObject meleeReticle;
    public Image HPBar;
    public TextMeshProUGUI enemiesRemainingText;
    public TextMeshProUGUI bulletCountText;
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


    public int enemiesRemaining;

    public bool isPaused;
    float timeScaleOrig;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");
        extractionZone = GameObject.FindGameObjectWithTag("Extraction Zone");
        playerScript = player.GetComponent<playerController>();
        timeScaleOrig = Time.timeScale;
    }

    // Update is called once per frame
    void Update()
{
        if (Input.GetButtonDown("Cancel") && activeMenu == null)
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
    }

    private void Start()
    {
        StartCoroutine(timerUpdate(timeValue));
    }

    public void pauseState()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void unpauseState()
    {
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        activeMenu.SetActive(false);
        activeMenu = null;
    }

    public void updateGameGoal(int amount)
    {
        enemiesRemaining += amount;
        enemiesRemainingUIUpdate();
        if (enemiesRemaining <= 0)
        {
            StartCoroutine(startExtraction(extractValue));
        }
    }

    public void playerDead()
    {
        pauseState();
        activeMenu = loseMenu;
        activeMenu.SetActive(true);

    }


    void enemiesRemainingUIUpdate()
    {
        enemiesRemainingText.text = enemiesRemaining.ToString();
    }

    public void winCondition()
    {
        activeMenu = winMenu;
        activeMenu.SetActive(true);
        pauseState();
    }

    IEnumerator timerUpdate(int time)
    {
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
        if (timeUntilText.active == true) // Since there are multiple win conditions, check if one is already active
        {
            timeUntilText.SetActive(false);
            extractionZone.SetActive(true);

            // Time until extraction ends
            extractionText.SetActive(true);
            for (int i = extract; i >= 0; i--)
            {
                extractionTimerText.text = i.ToString();
                yield return new WaitForSeconds(1);
            }
            playerDead();
        }
    }
}
