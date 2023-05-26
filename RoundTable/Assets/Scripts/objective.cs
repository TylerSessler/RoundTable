using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class objective : MonoBehaviour
{
    [SerializeField] GameObject model;
    bool inRange;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.instance.isPaused)
        {
            if (Input.GetButtonDown("E") && inRange)
            {
                StartCoroutine(pickUp());
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            gameManager.instance.artifactPromptText.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            gameManager.instance.artifactPromptText.SetActive(false);
        }
    }

    IEnumerator pickUp()
    {
        gameManager.instance.playerScript.hasObjective = true;
        gameManager.instance.artifactPromptText.SetActive(false);
        gameManager.instance.startTimer();
        Destroy(gameObject);
        
        // Audio controlled by exit spawning
        yield return new WaitForEndOfFrame();
    }
}
