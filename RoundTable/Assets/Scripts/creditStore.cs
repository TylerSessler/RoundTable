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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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
            else if (creditStoreID == 2)
            {
                gameManager.instance.interactPromptText.SetActive(true);
                if (Input.GetButtonDown("E") && gameManager.instance.credits >= 2)
                {
                    Instantiate(pistol, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 1, gameObject.transform.position.z - 5), gameManager.instance.player.transform.rotation);
                    gameManager.instance.credits -= 2;
                    gameManager.instance.creditsAvailableUIUpdate();
                    Debug.Log("Pistol Given");
                }

            }
            // WEAPON SHOP SNIPER
            else if (creditStoreID == 3 && gameManager.instance.credits >= 3)
            {
                Instantiate(sniper, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 1, gameObject.transform.position.z - 5), gameManager.instance.player.transform.rotation);
                gameManager.instance.credits -= 3;
                gameManager.instance.creditsAvailableUIUpdate();
                Debug.Log("Sniper Given");

            }
            // WEAPON SHOP RIFLE
            else if (creditStoreID == 4 && gameManager.instance.credits >= 4)
            {
                Instantiate(rifle, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 1, gameObject.transform.position.z - 5), gameManager.instance.player.transform.rotation);
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
}
