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
    public GameObject glow1;
    public GameObject glow2;
    public GameObject glow3;
    public GameObject glow4;
    public GameObject glow5;

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
        StartCoroutine(timerUpdate(timeValue, extractValue));
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
            winCondition();
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

    IEnumerator timerUpdate(int time, int extract)
    {
        // Time until extraction starts
        for (int i = time; i >= 0; i--)
        {
            timerText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
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
