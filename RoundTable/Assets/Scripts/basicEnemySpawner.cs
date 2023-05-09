using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class basicEnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public struct enemyType
    {
        public int EnemiesAtStart;
        public float range;
        public float spawnTime;
        public bool spawn;
        public GameObject _enemyPrefab;

        public enemyType(int start, float r, float t, bool s, GameObject enemy)
        {
            this.EnemiesAtStart = start;
            this.range = r;
            this.spawnTime = t;
            this.spawn = s;
            this._enemyPrefab = enemy;
        }
    }

    [SerializeField] private enemyType[] enemyList;

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
        for(int i = 0; i < enemyList.Length; i++)
        {
            for(int j = 0; j < enemyList[i].EnemiesAtStart; j++)
            {
                spawns(enemyList[i]._enemyPrefab, enemyList[i].range);
            }
            enemyType enemy = enemyList[i];
            enemy.spawn = false;
            enemyList[i] = enemy;
        }
    }

    void Update()
    {
        for(int i = 0; i < enemyList.Length; i++) 
        {
            if (!enemyList[i].spawn)
            {
                StartCoroutine(cspawn(enemyList[i]));
            }
            
        }
    }

    IEnumerator cspawn(enemyType e)
    {
        e.spawn = true;
        yield return new WaitForSeconds(e.spawnTime);
        spawns(e._enemyPrefab, e.range);
        e.spawn = false;
    }
    

    void spawns(GameObject g, float r)
    {
        Vector3 point;
        if (RandomPoint(transform.position, r, out point))
        {
            Instantiate(g, point, g.transform.rotation);
        }
        else
        {
            spawns(g, r);
        }
    }
}
