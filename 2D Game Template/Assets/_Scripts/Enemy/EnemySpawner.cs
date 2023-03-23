using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

/// <summary>
/// Spawns collections of enemies in waves
/// </summary>

[System.Serializable]
public class Wave
{
    [Tooltip("This is the prefabs of enemies to spawn in this wave. It must match the length of the wave enemies count")] public GameObject[] waveEnemies;
    [Tooltip("This is number of enemies to spawn. It must match the length of the wave enemies")] public int[] waveEnemiesCount;

}

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] private LayerMask enemyLayerMask;

    private AudioSource audioSource;

    [Header("Enemy Spawning Locations")]
    [SerializeField] private bool doRandomEnemySpawnLocations;
    [SerializeField] private float minEnemyXAxisSpawnBoundary;
    [SerializeField] private float maxEnemyXAxisSpawnBoundary;

    [SerializeField] private float minEnemyYAxisSpawnBoundary;
    [SerializeField] private float maxEnemyYAxisSpawnBoundary;

    [SerializeField] private bool doSetEnemyLocations;
    [SerializeField][Tooltip("The set location will be randomly picked from one of these transforms")] private Transform[] enemySpawnLocations;

    [Header("Waves")]
    [SerializeField] private Wave[] waves;

    [Header("Other")]
    [SerializeField] private bool switchMusicOnEnter;
    [SerializeField] private Collider2D fadeMusicPoint;
    [SerializeField] private Collider2D spawnerArea;
    [SerializeField] private float fadeMusicDuration;
    [SerializeField] private string nameOfMusicAfterCompletion;

    [SerializeField] private AudioClip completedSound;
    [SerializeField] private ParticleSystem completionEffect;
    [SerializeField] private GameObject[] gameObjectsToEnable;
    [SerializeField] private GameObject[] gameObjectsToDisable;


    private bool inBattle;

    [SerializeField] private Enemy baseEnemyScript;
    private int enemiesLeft;

    [SerializeField] private Collider2D[] enemiesInSpawnArea;
    private ContactFilter2D contactFilter;

    public UnityEvent onPlayerEnter;
    public UnityEvent onBattleCompleted;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();

        if (PlayerPrefs.HasKey("SpawnerCompleted"))
        {
            HandleGameObjects(false);
        }
        else if (PlayerPrefs.GetString("SpawnerCompleted") == "false")
        {
            HandleGameObjects(false);
        }
        else if (PlayerPrefs.GetString("SpawnerCompleted") == "true")
        {
            HandleGameObjects(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        enemiesInSpawnArea = Physics2D.OverlapAreaAll(new Vector2(minEnemyXAxisSpawnBoundary, minEnemyYAxisSpawnBoundary), new Vector2(maxEnemyXAxisSpawnBoundary, maxEnemyXAxisSpawnBoundary), enemyLayerMask);
        if (enemiesInSpawnArea.Length == 0) enemiesLeft = 0;



    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
        {
            PlayerPrefs.SetString("SpawnerCompleted", "false");
            onPlayerEnter.Invoke();
            if (switchMusicOnEnter)
            {
                //StartCoroutine(AudioManager.Instance.SetMixerGroupVolume("Music", 0.5f, 0.1f, true));
                AudioManager.Instance.Play("Suspense Music");
                inBattle = true;
                StartCoroutine(SpawnWaves());
                //to prevent the player from triggering the spawner again while doing it or to randomly fade the music again while doing it

            }

        }

        if (tag.ToLower() == "fade_sound_zone" && fadeMusicPoint)
        {
            StartCoroutine(AudioManager.Instance.FadeMixerGroupVolume("Music", fadeMusicDuration, 0.0f));
            AudioManager.Instance.StopPlayingAllSounds();
        }
    }

    private IEnumerator SpawnWaves()
    {
        inBattle = true;
        for (int i = 0; i < waves.Length; i++)
        //this is the waves loop
        {
            UnityEngine.Debug.Log($"Wave number {i}");
            //enemiesLeft = numberOfEnemiesPerWave[i];
            foreach (var enemyCount in waves[i].waveEnemiesCount)
            //iterate through the number of each enemy type to spawn in that wave and add it all up for the enemies in that wave
            {
                enemiesLeft += enemyCount;
            }
            // enemiesLeft = waves[i].waveEnemiesCount;
            SpawnEnemies(i);

            yield return null;
            while (enemiesLeft != 0)
            {
                yield return null;
            }
        }
        onBattleCompleted.Invoke();
        inBattle = false;

        PlayerPrefs.SetString("SpawnerCompleted", "true");


        AudioManager.Instance.StopPlaying("Suspense Music", fadeMusicDuration, 0.0f);
        AudioManager.Instance.Play(nameOfMusicAfterCompletion);

        audioSource.PlayOneShot(completedSound);
        Vector2 spawnLocation = new Vector2(PlayerCharacter.Instance.transform.position.x, PlayerCharacter.Instance.transform.position.y + 6.0f);
        Instantiate(completionEffect, spawnLocation, Quaternion.identity);

    }

    private void SpawnEnemies(int waveIterator)
    {
        for (int j = 0; j < waves[waveIterator].waveEnemiesCount.Length; j++)
        //loop through the different count of each enemy and spawn that many enemies of the corresponding enemy type
        {
            for (int k = 0; k < waves[waveIterator].waveEnemiesCount[j]; k++)
            {
                Instantiate(waves[waveIterator].waveEnemies[j], GenerateEnemySpawnLocation(), Quaternion.identity);
            }
        }
    }

    private Vector2 GenerateEnemySpawnLocation()
    {
        Vector2 spawnLocation;
        if (doRandomEnemySpawnLocations)
        {
            float randomXSpawn = UnityEngine.Random.Range(minEnemyXAxisSpawnBoundary, maxEnemyXAxisSpawnBoundary);
            float randomYSpawn = UnityEngine.Random.Range(minEnemyYAxisSpawnBoundary, maxEnemyYAxisSpawnBoundary);
            spawnLocation = new Vector2(randomXSpawn, randomYSpawn);
        }

        else if (doSetEnemyLocations)
        {
            int randomIndex = UnityEngine.Random.Range(0, enemySpawnLocations.Length);
            float xSpawn = enemySpawnLocations[randomIndex].position.x;
            float ySpawn = enemySpawnLocations[randomIndex].position.y;
            spawnLocation = new Vector2(xSpawn, ySpawn);
        }
        else
        {
            UnityEngine.Debug.Log("Neither the \'doRandomEnemySpawnLocations\' nor \'doSetEnemyLocations\' is set! (That is why enemies spawn at (0,0)!)");
            spawnLocation = new Vector2(0, 0);
        }

        return spawnLocation;
    }

    private void EnemyDestroyed()
    {
        if (inBattle) enemiesLeft -= 1;
        UnityEngine.Debug.Log($"Enemies left is {enemiesLeft}");
    }

    private void HandleGameObjects(bool spawnerCompleted)
    {
        if (spawnerCompleted)
        {
            foreach (var enableObject in gameObjectsToEnable)
            //the objects that get enabled after the win are enabled
            {
                enableObject.SetActive(true);
            }
            foreach (var disableObject in gameObjectsToDisable)
            //the objects that are disabled after the win are disabled
            {
                disableObject.SetActive(false);
            }
        }
        else
        {
            foreach (var enableObject in gameObjectsToEnable)
            //at the beginning, the ones that are enabled must be disabled
            {
                enableObject.SetActive(false);
            }
            foreach (var disableObject in gameObjectsToDisable)
            //at the beginning, the ones that will be disabled must be enabled
            {
                disableObject.SetActive(true);
            }
        }
    }




}
