using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("TutorialLevel");
    }

    public void TutorialLoad()
    {
        SceneManager.LoadScene("TutorialLevel");
    }
    public void Level1Load()
    {
        SceneManager.LoadScene("Level01");
    }
    public void Level2Load()
    {
        SceneManager.LoadScene("Level02");
    }
    public void Level3Load()
    {
        SceneManager.LoadScene("Level03");
    }



    public void QuitGame()
    {
        Application.Quit();
    }
    
}
