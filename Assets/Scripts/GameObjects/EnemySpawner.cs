using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class EnemySpawner : MonoBehaviour
{
    struct SpawnedData
    {
        public bool isSpawned;
        public EnemyCharacterController enemyCharacter;

        public SpawnedData(bool isSpawned, EnemyCharacterController enemyCharacter)
        {
            this.isSpawned = isSpawned;
            this.enemyCharacter = enemyCharacter;
        }
    }

    [Header("References")]
    public EnemyCharacterController[] EnemyCharacterPrefab;
    [Header("Data")]
    public float RadiusNearSpawner = 2f;
    [Header("Debug")]
    public bool DebugDoRespawn = false;
    public bool DebugRespawnExisting = false;

    SpawnedData[] spawnedDatas;


    private void Awake()
    {
        spawnedDatas = new SpawnedData[EnemyCharacterPrefab.Length];
        SpawnEnemies(true);
    }

    private void Update()
    {
        if (DebugDoRespawn)
        {
            DebugDoRespawn = false;
            SpawnEnemies(DebugRespawnExisting);
        }
    }


    public void SpawnEnemies(bool respawnExisting)
    {
        for (int i = 0; i < EnemyCharacterPrefab.Length; i++)
        {
            var spawnData = spawnedDatas[i];
            if (spawnData.isSpawned)
            {
                if (respawnExisting == false) { continue; }

                if (spawnData.enemyCharacter != null)
                    spawnData.enemyCharacter.Despawn(); // suboptimal, maybe copy from despawn to here so that array isnt searched multiple times
            }

            var charPrefab = EnemyCharacterPrefab[i];
            if (charPrefab != null)
            {
                EnemyCharacterController newChar = Instantiate(charPrefab, transform.position + 2.0f * i * Vector3.forward, transform.rotation);
                //newChar.GetCombatData().charName += i;
                newChar.Spawner = this;

                spawnData.isSpawned = true;
                spawnData.enemyCharacter = newChar;
                spawnedDatas[i] = spawnData;
            }
        }
    }


    public void OnDespawnCharacter(EnemyCharacterController despawnChar)
    {
        for (int i = 0; i < spawnedDatas.Count(); i++)
        {
            var spawnData = spawnedDatas[i];
            if (spawnedDatas[i].enemyCharacter == despawnChar)
            {
                spawnData.isSpawned = false;
                spawnData.enemyCharacter = null;
                spawnedDatas[i] = spawnData;
            }
        }
    }


    public Vector3 GetPointNearSpawner()
    {
        Vector3 offsetDir = new(Random.Range(-1, 1), 0f, Random.Range(-1, 1));
        return transform.position + offsetDir.normalized * RadiusNearSpawner;
    }
}
