using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class scenesManager : MonoBehaviour
{
    public static scenesManager instance;
    bool spacePressed;

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
        DebugLevel,
    }

    public void LoadScene(Scene s)
    {
        gameManager.instance.loadMenu.SetActive(true);
        
        StartCoroutine(LoadSceneAsync(s));
    }

    IEnumerator LoadSceneAsync(Scene s)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(s.ToString());
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            gameManager.instance.fillBar.fillAmount = progressValue;
            yield return new WaitForEndOfFrame();
        }
        gameManager.instance.fillBar.fillAmount = 1;
        gameManager.instance.skipText.SetActive(true);
        yield return new WaitForSeconds(3);
        operation.allowSceneActivation = true;
    }

    public void LoadNewGame()
    {
        SceneManager.LoadSceneAsync(Scene.TutorialLevel.ToString());
    }
    public void LoadNextScene()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync(Scene.MainMenu.ToString());
    }

}
