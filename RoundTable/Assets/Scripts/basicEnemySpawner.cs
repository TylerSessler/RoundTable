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
        Vector3 point;
        if (RandomPoint(transform.position, basicrange, out point))
        {
            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
            Instantiate(basic_enemyPrefab, point, basic_enemyPrefab.transform.rotation);
        }
        yield return new WaitForSeconds(basicspawnTime);
        basicspawner = false;
    }
    IEnumerator midspawn()
    {
        midspawner = true;
        Vector3 point;
        if (RandomPoint(transform.position, midrange, out point))
        {
            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
            Instantiate(mid_enemyPrefab, point, mid_enemyPrefab.transform.rotation);
        }
        yield return new WaitForSeconds(midspawnTime);
        midspawner = false;
    }
    IEnumerator bossspawn()
    {
        bossspawner = true;
        Vector3 point;
        if (RandomPoint(transform.position, bossrange, out point))
        {
            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
            Instantiate(boss_enemyPrefab, point, boss_enemyPrefab.transform.rotation);
        }
        yield return new WaitForSeconds(bossspawnTime);
        bossspawner = false;
    }
}
