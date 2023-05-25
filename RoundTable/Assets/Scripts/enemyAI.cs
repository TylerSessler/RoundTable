using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("----- Components -----")]
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] Transform headPos;
    [SerializeField] Transform shootPos;
    [SerializeField] AudioSource aud;
    [SerializeField] Transform patrol1;
    [SerializeField] Transform patrol2;

    [Header("----- Enemy Stats -----")]
    [Range(1, 100)][SerializeField] int HP;
    [SerializeField] int playerFaceSpeed;
    [SerializeField] int sightAngle;
    [SerializeField] int roamRange;
    [SerializeField] float roamCooldown;
    [SerializeField] float animTransSpeed;
    [SerializeField] bool roamAllowed;
    [SerializeField] bool patrolAllowed;
    [SerializeField] bool chaseAllowed;

    [Header("----- Enemy Gun -----")]
    [Range(1, 10)][SerializeField] int shootDamage;
    [Range(.1f, 5)][SerializeField] float shootRate;
    [Range(1, 100)][SerializeField] int shootDist;
    [Range(1, 100)][SerializeField] int bulletSpeed;
    [Range(0, 2)][SerializeField] float vertSpread;
    [Range(0, 2)][SerializeField] float HoriSpread;
    [SerializeField] GameObject bullet;
    [Header("Drop Table")]
    [SerializeField] GameObject[] drops;
    [Range(1, 3)][SerializeField] int creditsDropped;
    // End item decided to be dropped by the enemy
    GameObject trueDrop;

    Vector3 playerDir;
    Vector3 shootDir;
    bool playerInRange;
    float angleToPlayer;
    bool isShooting;
    float stoppingDistOrig;
    bool isRoaming;
    bool isPatrol;
    float speed;
    public bool sawPlayer;


    [Header("Audio")]
    [SerializeField] AudioClip[] audShoot;
    [SerializeField][Range(0, 1)] float audShootVol;


    // Start is called before the first frame update
    void Start()
    {
        gameManager.instance.updateGameGoal(1);
        stoppingDistOrig = agent.stoppingDistance;
        int dropSelect = Random.Range(0, drops.Length);
        trueDrop = drops[dropSelect];
    }

    // Update is called once per frame
    void Update()
    {

        speed = Mathf.Lerp(speed, agent.velocity.normalized.magnitude, Time.deltaTime * animTransSpeed);
        anim.SetFloat("Speed", speed);

        if (playerInRange)
        {
            if (!canSeePlayer() && agent.remainingDistance <= agent.stoppingDistance && roamAllowed)
            {
                StartCoroutine(roam());
            }
            else
            {
                StartCoroutine(patrol());
            }

        }
        // Make sure enemy isn't pathing currently. Both for agro & for current roaming if it is past the cooldown timer while still moving
        else if (!playerInRange && roamAllowed && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(roam());
        }
        else
        {
            StartCoroutine(patrol());
        }
    }

    bool canSeePlayer()
    {
        playerDir = (gameManager.instance.player.transform.position - headPos.position);
        shootDir = (gameManager.instance.player.transform.position - shootPos.position);
        angleToPlayer = Vector3.Angle(new Vector3(playerDir.x, 0, playerDir.z), transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= sightAngle)
            {
                sawPlayer = true;
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



    IEnumerator shoot()
    {
        float vertOffset = Random.Range(vertSpread * -1, vertSpread);
        float horiOffset = Random.Range(HoriSpread * -1, HoriSpread);
        Vector3 offset = new Vector3(horiOffset, vertOffset, 0);

        aud.PlayOneShot(audShoot[UnityEngine.Random.Range(0, audShoot.Length)], audShootVol);
        isShooting = true;
        GameObject bulletClone = Instantiate(bullet, shootPos.position, bullet.transform.rotation);
        bulletClone.GetComponent<Rigidbody>().velocity = (shootDir + offset) * bulletSpeed;
        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    IEnumerator roam()
    {
        if (!isRoaming)
        {
            isRoaming = true;
            NavMeshHit hit;
            Vector3 roamDestination = headPos.position + Random.insideUnitSphere * roamRange;
            if (NavMesh.SamplePosition(roamDestination, out hit, 2.0f, NavMesh.AllAreas))
            {
                agent.stoppingDistance = 0;
                agent.SetDestination(hit.position);
                yield return new WaitForSeconds(roamCooldown + Random.Range(1, 3));
            }

            isRoaming = false;
            agent.stoppingDistance = stoppingDistOrig;
        }
        else
        {
            isRoaming = true;
        }

    }

    IEnumerator patrol()
    {
        if (patrolAllowed && !isPatrol)
        {

            isPatrol = true;
            if (gameObject.transform.position == patrol1.position)
            {
                NavMeshHit hit;
                Vector3 patrolDest = patrol2.position;
                NavMesh.SamplePosition(patrolDest, out hit, 2.0f, NavMesh.AllAreas);
                agent.stoppingDistance = 0;
                agent.SetDestination(patrolDest);
            }
            else
            {
                NavMeshHit hit;
                Vector3 patrolDest = patrol1.position;
                NavMesh.SamplePosition(patrolDest, out hit, 2.0f, NavMesh.AllAreas);
                agent.stoppingDistance = 0;
                agent.SetDestination(patrolDest);
            }
            yield return new WaitForSeconds(roamCooldown);
            isPatrol = false;
        }

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
            if (sawPlayer && chaseAllowed)
            {
                agent.SetDestination(gameManager.instance.player.transform.position);
                StopCoroutine(delayedAgro());
                StartCoroutine(delayedAgro());
            }
        }
    }
    IEnumerator delayedAgro()
    {
        yield return new WaitForSeconds(3f);
        agent.SetDestination(gameManager.instance.player.transform.position);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        agent.stoppingDistance = 0;
        if (chaseAllowed)
        {
            goToPlayer();
        }

        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, 5);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == 6)
            {
                hitCollider.GetComponent<enemyAI>().goToPlayer();
            }
        }

        StartCoroutine(flashColor());

        if (HP <= 0)
        {
            gameManager.instance.credits += creditsDropped;
            gameManager.instance.updateGameGoal(-1);
            Instantiate(trueDrop, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 1, gameObject.transform.position.z), trueDrop.transform.rotation);
            Destroy(gameObject);
        }
    }

    IEnumerator flashColor()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(.1f);
        model.material.color = Color.white;
    }
    void goToPlayer()
    {
        agent.stoppingDistance = 0;
        agent.SetDestination(gameManager.instance.player.transform.position);
    }
    void facePlayer()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * playerFaceSpeed);
    }
}