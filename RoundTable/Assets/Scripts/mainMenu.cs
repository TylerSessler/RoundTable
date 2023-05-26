using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Search;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public mainMenu instance;
    [Header("First Selected")]
    [SerializeField] public GameObject mainMenuFirst;
    [SerializeField] public GameObject levelSelectFirst;
    [SerializeField] public GameObject optionsSelectFirst;
    [Header("Audio")]
    [SerializeField] public AudioSource aud;
    [SerializeField] public AudioClip click;
    [SerializeField][Range(0, 1)] public float clickVol;
    [Header("Bools")]
    public bool mainMenuOpen;
    public bool levelSelectOpen;
    public bool optionsOpen;

    [SerializeField] GameObject creditsMenu;
    [SerializeField] GameObject mainMenuUI;
    [SerializeField] GameObject title;

    public void Update()
    {
        
    }
    public void Start()
    {
        instance = this;
        EventSystem.current.SetSelectedGameObject(mainMenuFirst);

        gameManager.instance.reloadAmmoText.enabled = false;
        gameManager.instance.reloadingText.enabled = false;
        gameManager.instance.lowAmmoText.enabled = false;
        gameManager.instance.noAmmoText.enabled = false;
    }
    public void Play()
    {
        StartCoroutine(delay(scenesManager.Scene.TutorialLevel));
    }

    public void TutorialLoad()
    {
        StartCoroutine(delay(scenesManager.Scene.TutorialLevel));
    }
    public void Level1Load()
    {
        StartCoroutine(delay(scenesManager.Scene.Level01));
    }
    public void Level2Load()
    {
        StartCoroutine(delay(scenesManager.Scene.Level02));
    }
    public void Level3Load()
    {
        StartCoroutine(delay(scenesManager.Scene.Level03));
    }

    public void buttonAudio()
    {
        aud.PlayOneShot(click);
    }
    public IEnumerator delay(scenesManager.Scene s)
    {
        aud.PlayOneShot(click);
        yield return new WaitForSeconds(click.length);
        scenesManager.instance.LoadScene(s);
    }
    public void openMainMenu()
    {
        mainMenuOpen = true;
        levelSelectOpen = false;
        optionsOpen = false;
        newSelection();
    }

    public void credits()
    {
        StartCoroutine(turnOffCredits());
    }
    IEnumerator turnOffCredits()
    {
        yield return new WaitForSeconds(27.25f);
        creditsMenu.SetActive(false);
        mainMenuUI.SetActive(true);
        title.SetActive(true);
    }
    public void openLevelSelect()
    {
        mainMenuOpen = false;
        levelSelectOpen = true;
        optionsOpen = false;
        newSelection();

    }
    public void openOptions()
    {
        mainMenuOpen = false;
        optionsOpen = true;
        levelSelectOpen = false;
        
        newSelection();
    }

    public void newSelection()
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (mainMenuOpen)
        {
            EventSystem.current.SetSelectedGameObject(mainMenuFirst);
        }
        else if (levelSelectOpen)
        {
            EventSystem.current.SetSelectedGameObject(levelSelectFirst);
        }
        else if (optionsOpen)
        {
            EventSystem.current.SetSelectedGameObject(optionsSelectFirst);
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    
}
