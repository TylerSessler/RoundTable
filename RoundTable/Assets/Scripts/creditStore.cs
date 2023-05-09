using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class creditStore : MonoBehaviour
{
    [SerializeField] int creditStoreID;
    [SerializeField] GameObject pistol;
    [SerializeField] GameObject rifle;
    [SerializeField] GameObject sniper;

    bool isInShop;

    [Header("Audio")]
    [SerializeField] AudioClip[] audPurchase;
    [SerializeField][Range(0, 1)] float audPurchaseVol;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.instance.isPaused)
        {
            if (Input.GetButtonDown("E") && isInShop)
            {
                Purchase();
            }
        }
        if (gameManager.instance.isPaused)
        {
            gameManager.instance.interactPromptText.SetActive(false);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInShop = true;
            gameManager.instance.interactPromptText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInShop = false;
            gameManager.instance.interactPromptText.SetActive(false);
        }
    }

    private void Purchase()
    {
        // SPRINT SPEED UPGRADE
        if (creditStoreID == 0 && gameManager.instance.credits >= 5)
        {
            gameManager.instance.playerScript.sprintSpeed *= 1.3f;
            gameManager.instance.credits -= 5;
            gameManager.instance.creditsAvailableUIUpdate();
            Debug.Log("Speed Given");
        }
        // MAX JUMPS UPGRADE
        else if (creditStoreID == 1 && gameManager.instance.credits >= 3)
        {
            gameManager.instance.playerScript.maxJumps += 1;
            gameManager.instance.credits -= 3;
            gameManager.instance.creditsAvailableUIUpdate();
            Debug.Log("Jump Given");
        }
        //WEAPON SHOP PISTOL
        else if (creditStoreID == 2 && gameManager.instance.credits >= 2)
        {
            Instantiate(pistol, new Vector3(gameManager.instance.player.transform.position.x, gameManager.instance.player.transform.position.y + 1, gameManager.instance.player.transform.position.z), gameManager.instance.player.transform.rotation);
            gameManager.instance.credits -= 2;
            gameManager.instance.creditsAvailableUIUpdate();
            Debug.Log("Pistol Given");
        }
        // WEAPON SHOP SNIPER
        else if (creditStoreID == 3 && gameManager.instance.credits >= 3)
        {
            Instantiate(sniper, new Vector3(gameManager.instance.player.transform.position.x, gameManager.instance.player.transform.position.y + 1, gameManager.instance.player.transform.position.z), gameManager.instance.player.transform.rotation);
            gameManager.instance.credits -= 3;
            gameManager.instance.creditsAvailableUIUpdate();
            Debug.Log("Sniper Given");

        }
        // WEAPON SHOP RIFLE
        else if (creditStoreID == 4 && gameManager.instance.credits >= 4)
        {
            Instantiate(rifle, new Vector3(gameManager.instance.player.transform.position.x, gameManager.instance.player.transform.position.y + 1, gameManager.instance.player.transform.position.z), gameManager.instance.player.transform.rotation);
            gameManager.instance.credits -= 4;
            gameManager.instance.creditsAvailableUIUpdate();
            Debug.Log("Rifle Given");

        }
        // MAX HEALTH UPGRADE
        else if (creditStoreID == 5 && gameManager.instance.credits >= 5)
        {
            gameManager.instance.playerScript.originalHealth += 2;
            gameManager.instance.credits -= 5;
            gameManager.instance.playerScript.playerUIUpdate();
            gameManager.instance.creditsAvailableUIUpdate();
            Debug.Log("Max Health Upgrade");

        }
        // HEALTH REFILL UPGRADE
        else if (creditStoreID == 6 && gameManager.instance.credits >= 4)
        {
            gameManager.instance.playerScript.health = gameManager.instance.playerScript.originalHealth;
            gameManager.instance.credits -= 4;
            gameManager.instance.playerScript.playerUIUpdate();
            gameManager.instance.creditsAvailableUIUpdate();
            Debug.Log("Player Health Refilled");

        }
    }
}
