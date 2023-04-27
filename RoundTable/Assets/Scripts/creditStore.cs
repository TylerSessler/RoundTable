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
            if (creditStoreID == 0 && gameManager.instance.credits >= 5)
            {
                gameManager.instance.playerScript.sprintSpeed *= 1.2f;
                gameManager.instance.credits -= 5;
                gameManager.instance.creditsAvailableUIUpdate();
                Debug.Log("Speed Given");
            }
            else if (creditStoreID == 1 && gameManager.instance.credits >= 3)
            {
                gameManager.instance.playerScript.maxJumps += 1;
                gameManager.instance.credits -= 3;
                gameManager.instance.creditsAvailableUIUpdate();
                Debug.Log("Jump Given");
            }
            else if (creditStoreID == 2 && gameManager.instance.credits >= 2)
            {
                Instantiate(pistol, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 1, gameObject.transform.position.z - 5), gameManager.instance.player.transform.rotation);
                gameManager.instance.credits -= 2;
                gameManager.instance.creditsAvailableUIUpdate();
                Debug.Log("Pistol Given");

            }
            else if (creditStoreID == 3 && gameManager.instance.credits >= 3)
            {
                Instantiate(sniper, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 1, gameObject.transform.position.z - 5), gameManager.instance.player.transform.rotation);
                gameManager.instance.credits -= 3;
                gameManager.instance.creditsAvailableUIUpdate();
                Debug.Log("Sniper Given");

            }
            else if (creditStoreID == 4 && gameManager.instance.credits >= 4)
            {
                Instantiate(rifle, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 1, gameObject.transform.position.z - 5), gameManager.instance.player.transform.rotation);
                gameManager.instance.credits -= 4;
                gameManager.instance.creditsAvailableUIUpdate();
                Debug.Log("Rifle Given");

            }
        }
    }
}
