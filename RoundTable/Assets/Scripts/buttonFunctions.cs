using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
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
        gameManager.instance.unpauseState();
        Cursor.visible = true;
        PlayerPrefs.DeleteAll();
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
}
