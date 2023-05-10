using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class mainMenu : MonoBehaviour
{
    [SerializeField] public AudioSource aud;
    [SerializeField] public AudioClip click;
    [SerializeField][Range(0, 1)] public float clickVol;
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
    public void QuitGame()
    {
        Application.Quit();
    }
    
}
