using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public static buttonFunctions instance;

    bool isDead;

    [Header("Audio")]
    [SerializeField] public AudioSource aud;
    [SerializeField] public AudioClip click;
    [SerializeField][Range(0, 1)] public float clickVol;

    private void Awake()
    {
        instance = this;
    }

    public void resume()
    {
        gameManager.instance.unpauseState();
        gameManager.instance.isPaused = !gameManager.instance.isPaused;
        
    }

    public void restart()
    {
        // If player died and presses restart...
        if (playerController.instance.health <= 0)
            isDead = true;
        gameManager.instance.unpauseState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Player respawns with full health. Bool is used as a conditional that doesn't auto-reset when LoadScene is called.
        if (isDead)
        {
            playerController.instance.health = playerController.instance.originalHealth;
            playerController.instance.SavePlayerData();
            isDead = false;
        }
    }
        public void mainMenu()
    {
        playerController.instance.resetStats();
        playerController.instance.clearWeapons();
        gameManager.instance.unpauseState();
        Cursor.visible = true;
        scenesManager.instance.LoadScene(scenesManager.Scene.MainMenu);
    }
    public void options()
    {
        gameManager.instance.pauseMenu.SetActive(false);
        gameManager.instance.optionsMenu.SetActive(true);
    }
    public void back()
    {
        gameManager.instance.optionsMenu.SetActive(false);
        gameManager.instance.pauseMenu.SetActive(true);
    }
    public void quit()
    {
        Application.Quit();
    }

    public void buttonAudio()
    {
        // Prevents spammed clicks on slider
        if (gameManager.instance.optionsMenu.active == true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                aud.PlayOneShot(click);
            }
        }
        else
            aud.PlayOneShot(click);
    }
}
