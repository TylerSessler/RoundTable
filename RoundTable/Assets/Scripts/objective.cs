using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            gameManager.instance.interactPromptText.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            gameManager.instance.interactPromptText.SetActive(false);
        }
    }

    IEnumerator pickUp()
    {
        gameManager.instance.playerScript.hasObjective = true;
        gameManager.instance.interactPromptText.SetActive(false);
        Destroy(gameObject);
        // Play audio?
        yield return new WaitForEndOfFrame();
    }
}
