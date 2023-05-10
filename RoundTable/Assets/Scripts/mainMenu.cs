using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public void Play()
    {
        scenesManager.instance.LoadScene(scenesManager.Scene.TutorialLevel);
    }

    public void TutorialLoad()
    {
        scenesManager.instance.LoadScene(scenesManager.Scene.TutorialLevel);
    }
    public void Level1Load()
    {
        scenesManager.instance.LoadScene(scenesManager.Scene.Level01);
    }
    public void Level2Load()
    {
        scenesManager.instance.LoadScene(scenesManager.Scene.Level02);
    }
    public void Level3Load()
    {
        scenesManager.instance.LoadScene(scenesManager.Scene.Level03);
    }



    public void QuitGame()
    {
        Application.Quit();
    }
    
}
