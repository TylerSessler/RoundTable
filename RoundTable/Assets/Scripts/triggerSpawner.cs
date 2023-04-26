using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] prefab;
    [SerializeField] int intervalTime;
    [SerializeField] Transform[] spawnPos;
    [SerializeField] int prefabMaxNum;

    int prefabsSpawnCount;
    bool playerInRange;
    bool isSpawning;

    List<GameObject> prefabList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        gameManager.instance.updateGameGoal(prefabMaxNum);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && !isSpawning && prefabsSpawnCount < prefabMaxNum)
        {
            StartCoroutine(spawn());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    IEnumerator spawn()
    {
        isSpawning = true;
        int rand = Random.Range(0, prefab.Length);
        GameObject prefabClone = Instantiate(prefab[rand], spawnPos[Random.Range(0, spawnPos.Length)].position, prefab[rand].transform.rotation);

        prefabList.Add(prefabClone);

        prefabsSpawnCount++;
        yield return new WaitForSeconds(intervalTime);
        isSpawning = false;
    }
}
