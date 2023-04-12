using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class basicEnemySpawner : MonoBehaviour
{
    [Header("Basic Enemy Spawner Specs")]
    [SerializeField] float basicrange = 30.0f;
    [SerializeField] float basicspawnTime = 6.0f;
    [SerializeField] GameObject basic_enemyPrefab;
    [Header("Mid Enemy Spawner Specs")]
    [SerializeField] float midrange = 20.0f;
    [SerializeField] float midspawnTime = 10.0f;
    [SerializeField] GameObject mid_enemyPrefab;
    [Header("Boss Enemy Spawner Specs")]
    [SerializeField] float bossrange = 10.0f;
    [SerializeField] float bossspawnTime = 15.0f;
    [SerializeField] GameObject boss_enemyPrefab;

    private bool basicspawner;
    private bool midspawner;
    private bool bossspawner;
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
    private void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            spawns(basic_enemyPrefab, basicrange);
        }
        for (int i = 0; i < 5; i++)
        {
            spawns(mid_enemyPrefab, midrange);
        }
        for (int i = 0; i < 1; i++)
        {
            spawns(boss_enemyPrefab, bossrange);
        }
    }

    void Update()
    {
        if(!basicspawner)
            StartCoroutine(basicspawn());

        if (!midspawner)
            StartCoroutine(midspawn());

        if (!bossspawner)
            StartCoroutine(bossspawn());
    }

    IEnumerator basicspawn()
    {
        basicspawner = true;
        spawns(basic_enemyPrefab, basicrange);
        yield return new WaitForSeconds(basicspawnTime);
        basicspawner = false;
    }
    IEnumerator midspawn()
    {
        midspawner = true;
        spawns(mid_enemyPrefab, midrange);
        yield return new WaitForSeconds(midspawnTime);
        midspawner = false;
    }
    IEnumerator bossspawn()
    {
        bossspawner = true;
        spawns(boss_enemyPrefab, bossrange);
        yield return new WaitForSeconds(bossspawnTime);
        bossspawner = false;
    }

    void spawns(GameObject g, float r)
    {
        Vector3 point;
        if (RandomPoint(transform.position, r, out point))
        {
            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
            Instantiate(g, point, g.transform.rotation);
        }
    }
}
