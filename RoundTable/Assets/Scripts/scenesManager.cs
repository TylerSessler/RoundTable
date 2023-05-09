using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scenesManager : MonoBehaviour
{
    public static scenesManager instance;

    private void Awake()
    {
        instance = this;
    }
    public enum Scene
    {
        MainMenu,
        TutorialLevel,
        ItemShopLevel,
        Level01,
        Level02,
        Level03,
        Debug,
    }

    public void LoadScene(Scene s)
    {
        SceneManager.LoadScene(s.ToString());
    }
    public void LoadNewGame()
    {
        SceneManager.LoadScene(Scene.TutorialLevel.ToString());
    }
    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(Scene.MainMenu.ToString());
    }
}
