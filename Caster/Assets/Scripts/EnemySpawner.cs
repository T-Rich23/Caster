using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnState {SPAWNING, WAITING, COUNTING };
    [System.Serializable]
    public class Spawner
    {
        public string name;
        public Transform enemy;
        public int count;
        public float rate;
    }

    public Spawner[] spawners;
    private int nextspawn = 0;
    public Transform[] points;

    public float timeBetweenWaves = 5f;
    public float spawnCountdown;
    private float searchCountDown = 1f;

    public SpawnState state = SpawnState.COUNTING;

    public void Start()
    {
        if (points.Length == 0)
        {
            Debug.Log("No SPAWN LOCATED");
        }
        spawnCountdown = timeBetweenWaves;
    }

    private void Update()
    {
        if (state == SpawnState.WAITING)
        {
            if (!enemyIsAlive())
            {
                WaveCompleted();
            }
            else
            {
                return;
            }
        }

        if (spawnCountdown <= 0)
        {
            if(state != SpawnState.SPAWNING)
            {
                StartCoroutine(Spawnwave(spawners[nextspawn]));
            }
        }
        else
        {
            spawnCountdown -= Time.deltaTime;
        }
    }
    void WaveCompleted()
    {
        state = SpawnState.COUNTING;
        spawnCountdown = timeBetweenWaves;

        if (nextspawn + 1 > spawners.Length - 1)
        {
            nextspawn = 0;
            Debug.Log("All Wave Complete Looping...");
        }
        else
        {
            nextspawn++;
        }

        
    }
    bool enemyIsAlive()
    {
        searchCountDown -= Time.deltaTime;
        if (searchCountDown <= 0f)
        {
            searchCountDown = 1f;
            if (GameObject.FindGameObjectWithTag("Enemy") == null)
            {
                return false;
            }
        }
        return true;
    }

    IEnumerator Spawnwave(Spawner _spawner)
    {
        state = SpawnState.SPAWNING;
        for(int i = 0;i<_spawner.count; i++)
        {
            spawnEnemy(_spawner.enemy);
            yield return new WaitForSeconds(1f / _spawner.rate);
        }
        state = SpawnState.WAITING;

        yield break;
    }
    void spawnEnemy(Transform _enemy)
    {
        
        Transform _sp = points[Random.Range(0, points.Length)];
        Instantiate(_enemy, _sp.position, _sp.rotation);
    }
}
