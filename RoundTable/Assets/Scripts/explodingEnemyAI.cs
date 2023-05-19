using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

public class explodingEnemyAI : MonoBehaviour, IDamage
{
    [Header("----- Components -----")]
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] Transform headPos;
    [SerializeField] AudioSource aud;
    [SerializeField] Transform patrol1;
    [SerializeField] Transform patrol2;
    [SerializeField] SphereCollider explosionTrigger;

    [Header("----- Enemy Stats -----")]
    [Range(1, 100)][SerializeField] int HP;
    [SerializeField] int playerFaceSpeed;
    [SerializeField] int sightAngle;
    [SerializeField] float sightRange;
    [SerializeField] int roamRange;
    [SerializeField] float roamCooldown;
    [SerializeField] float animTransSpeed;
    [SerializeField] bool roamAllowed;
    [SerializeField] bool patrolAllowed;

    [Header("----- Explosion Stats -----")]
    [SerializeField] float explosionTimer = 3f;
    [SerializeField] float explosionRadius = 5f;
    [SerializeField] float explosionRange;
    [SerializeField] int explosionDamage = 5;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField][Range(0, 2)] float flashFrequency;

    bool isExploding = false;
    bool isDying = false;
    Color originalColor;

    [Header("Drop Table")]
    [SerializeField] GameObject[] drops;
    [Range(1, 3)][SerializeField] int creditsDropped;
    // End item decided to be dropped by the enemy
    GameObject trueDrop;

    Vector3 playerDir;
    bool playerInRange;
    float angleToPlayer;
    float stoppingDistOrig;
    bool isRoaming;
    bool isPatrol;
    float speed;
    bool sawPlayer;

    [Header("Audio")]
    [SerializeField] AudioClip audExplosion;
    [SerializeField][Range(0, 1)] float audExplosionVol;


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

            if (isExploding)
            {
                facePlayer();
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
        float distanceToPlayer = playerDir.magnitude;
        angleToPlayer = Vector3.Angle(new Vector3(playerDir.x, 0, playerDir.z), transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= sightAngle)
            {
                if (distanceToPlayer <= explosionRange)
                {
                    sawPlayer = false;
                    agent.isStopped = true;
                    if (!isExploding)
                    {
                        StartCoroutine(StartExplosion());
                    }
                }
                else
                {
                    sawPlayer = true;
                    agent.isStopped = false;
                    agent.SetDestination(gameManager.instance.player.transform.position);
                }

                if (agent.remainingDistance <= agent.stoppingDistance)
                    facePlayer();

                return true;
            }
        }

        return false;
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
            if (gameObject.CompareTag("Sight Range"))
            {
                playerInRange = false;
                if (sawPlayer)
                {
                    agent.SetDestination(gameManager.instance.player.transform.position);
                    StopCoroutine(delayedAgro());
                    StartCoroutine(delayedAgro());
                }
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
        if (roamAllowed)
        {
            agent.SetDestination(gameManager.instance.player.transform.position);
        }

        StartCoroutine(flashColor());

        if (HP <= 0 && !isExploding && !isDying)
        {
            isDying = true;
            StopExplosion();
            gameManager.instance.credits += creditsDropped;
            gameManager.instance.updateGameGoal(-1);
            Instantiate(trueDrop, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 1, gameObject.transform.position.z), trueDrop.transform.rotation);
            Destroy(gameObject);
        }
    }

    void Explode()
    {    
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionTrigger.radius);
        foreach (var collider in colliders)
        {
            IDamage damageable = collider.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.takeDamage(explosionDamage);
            }
        }
        gameManager.instance.playerScript.aud.PlayOneShot(audExplosion, audExplosionVol);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    IEnumerator StartExplosion()
    {
        isExploding = true;
        float countdown = 1f; // For example, let's say the countdown is 5 seconds

        while (countdown > 0f)
        {
            // Flash the enemy
            StartCoroutine(FlashExplosionColor());

            //aud.PlayOneShot(audExplosion, audExplosionVol);

            // Check if player is still within range only if the enemy is not dying
            if (!isDying)
            {
                playerDir = (gameManager.instance.player.transform.position - headPos.position);
                if (playerDir.magnitude > explosionRange)
                {
                    StopExplosion();
                    yield break;
                }
            }

            // Decrement the countdown and wait for the next frame
            countdown -= Time.deltaTime;
            yield return null;
        }

        // If the countdown reaches 0, explode
        Explode();
    }

    void StopExplosion()
    {
        isExploding = false;
        // Reset color to white, just in case
        model.material.color = Color.white;
    }

    IEnumerator flashColor()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(.1f);
        model.material.color = Color.white;
    }

    IEnumerator FlashExplosionColor()
    {
        while (isExploding)
        {
            model.material.color = Color.red;
            yield return new WaitForSeconds(flashFrequency);
            model.material.color = Color.white;
            yield return new WaitForSeconds(flashFrequency);
        }
    }

    void facePlayer()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * playerFaceSpeed);
    }
}