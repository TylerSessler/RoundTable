using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class scenesManager : MonoBehaviour
{
    public static scenesManager instance;
    [SerializeField] GameObject mainMenuCheck;
    // True while cutscene/loading is active
    private bool CS;
    // Artificial timer
    private float timer;

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
        CS = true;
        if (mainMenuCheck != null)
        {
            mainMenuCheck.SetActive(false);
        }
        StartCoroutine(LoadSceneAsync(s));
    }

    public IEnumerator LoadSceneAsync(Scene s)
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
        
        while (CS && timer < 3f)
        {
            timer += Time.deltaTime;
            if (Input.GetKey(KeyCode.Space))
            {
                CS = false;
                timer = 0;
                operation.allowSceneActivation = true;
            }
        }
        
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
