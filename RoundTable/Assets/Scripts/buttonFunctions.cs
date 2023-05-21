using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public static buttonFunctions instance;

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
        gameManager.instance.unpauseState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        if(Input.GetMouseButtonDown(0))
        {
            aud.PlayOneShot(click);

        }
    }
}
