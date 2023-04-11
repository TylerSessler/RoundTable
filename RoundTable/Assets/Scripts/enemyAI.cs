using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("----- Components -----")]
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform headPos;
    [SerializeField] Transform shootPos;

    [Header("----- Enemy Stats -----")]
    [Range(1, 10)][SerializeField] int HP;
    [SerializeField] int playerFaceSpeed;
    [SerializeField] int sightAngle;
    [SerializeField] int roamRange;
    [SerializeField] float roamCooldown;

    [Header("----- Enemy Gun -----")]
    [Range(1, 10)][SerializeField] int shootDamage;
    [Range(.1f, 5)][SerializeField] float shootRate;
    [Range(1, 100)][SerializeField] int shootDist;
    [Range(1, 100)][SerializeField] int bulletSpeed;
    [Range(0, 2)][SerializeField] float vertSpread;
    [Range(0, 2)][SerializeField] float HoriSpread;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject drop;

    Vector3 playerDir;
    bool playerInRange;
    float angleToPlayer;
    bool isShooting;
    float stoppingDistOrig;
    bool isRoaming;

    // Start is called before the first frame update
    void Start()
    {
        gameManager.instance.updateGameGoal(1);
        stoppingDistOrig = agent.stoppingDistance;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange)
        {
            if (canSeePlayer())
            {

            }
        }
        // Make sure enemy isn't pathing currently. Both for agro & for current roaming if it is past the cooldown timer while still moving
        if (!strippedVision() && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(roam());
        }
    }

    bool canSeePlayer()
    {
        playerDir = (gameManager.instance.player.transform.position - headPos.position);
        angleToPlayer = Vector3.Angle(new Vector3(playerDir.x, 0, playerDir.z), transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= sightAngle)
            {
                agent.stoppingDistance = stoppingDistOrig;
                agent.SetDestination(gameManager.instance.player.transform.position);

                if (agent.remainingDistance <= agent.stoppingDistance)
                    facePlayer();

                if (!isShooting)
                    StartCoroutine(shoot());

                return true;
            }
        }

        return false;
    }

    bool strippedVision()
    {
        playerDir = (gameManager.instance.player.transform.position - headPos.position);
        angleToPlayer = Vector3.Angle(new Vector3(playerDir.x, 0, playerDir.z), transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= sightAngle)
            {
                return true;
            }
        }
        
        return false;
    }

    IEnumerator shoot()
    {
        float vertOffset = Random.Range(vertSpread * -1, vertSpread);
        float horiOffset = Random.Range(HoriSpread * -1, HoriSpread);

        isShooting = true;
        GameObject bulletClone = Instantiate(bullet, shootPos.position, bullet.transform.rotation);
        bulletClone.GetComponent<Rigidbody>().velocity = new Vector3(playerDir.x + horiOffset, playerDir.y + vertOffset, playerDir.z) * bulletSpeed;
        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    IEnumerator roam()
    {
        Debug.Log("roam called");
        isRoaming = true;
        NavMeshHit hit;
        Vector3 roamDestination = headPos.position + Random.insideUnitSphere * roamRange;
        if (NavMesh.SamplePosition(roamDestination, out hit, 1.0f, NavMesh.AllAreas))
        {
            Debug.Log("Set position");
            agent.stoppingDistance = 0;
            agent.SetDestination(hit.position);
        }
        yield return new WaitForSeconds(roamCooldown);
        isRoaming = false;
        agent.stoppingDistance = stoppingDistOrig;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        agent.stoppingDistance = 0;
        agent.SetDestination(gameManager.instance.player.transform.position);

        StartCoroutine(flashColor());

        if (HP <= 0)
        {
            gameManager.instance.updateGameGoal(-1);
            Instantiate(drop, new Vector3(gameObject.transform.position.x, 1, gameObject.transform.position.z), drop.transform.rotation);
            Destroy(gameObject);
        }
    }

    IEnumerator flashColor()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(.1f);
        model.material.color = Color.white;
    }

    void facePlayer()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * playerFaceSpeed);
    }

}