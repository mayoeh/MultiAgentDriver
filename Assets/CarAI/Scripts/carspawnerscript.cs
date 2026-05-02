using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class carspawnerscript : MonoBehaviour
{
    [Tooltip("A list of car models that will be spawned randomly.")]
    public List<GameObject> cars = new List<GameObject>();

    [Tooltip("The number of cars that will be spawned.")]
    public int numberOfCarsToSpawn = 1;

    [Tooltip("If false the spawner won't spawn cars.")]
    public bool canSpawn = true;

    [Tooltip("The first checkpoint that the car(s) will be redirected to.")]
    public Transform startingCheckpoint;

    [Tooltip("Time interval between cars in seconds.")]
    public float timeIntervalBetweenCarsInSeconds = 0f;

    [Header("Distance cars keep from objects")]
    public float distanceKeptMin = 2f;
    public float distanceKeptMax = 2f;

    [Header("Driving recklessness threshold")]
    public int recklessnessMin = 0;
    public int recklessnessMax = 0;

    // Track spawned cars
    private List<GameObject> spawnedCars = new List<GameObject>();

    // Store coroutine reference
    private Coroutine spawnCoroutine;

    void Start()
    {
        StartSpawning();
    }

    public void StartSpawning()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnCycle());
        }
    }

    public void StopSpawning()
    {
        canSpawn = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnCycle()
    {
        int index = 0;

        while (index < numberOfCarsToSpawn)
        {
            if (canSpawn)
            {
                GameObject model = cars[Random.Range(0, cars.Count)];
                GameObject newCar = Instantiate(model);

                newCar.transform.position = transform.position;
                newCar.transform.rotation = transform.rotation;

                // Add to tracking list
                spawnedCars.Add(newCar);

                CarAIController controller = newCar.GetComponent<CarAIController>();

                controller.CheckPointSearch = true;
                controller.isCarControlledByAI = true;
                controller.distanceFromObjects =
                    Random.Range(distanceKeptMin, distanceKeptMax);
                controller.recklessnessThreshold =
                    Random.Range(recklessnessMin, recklessnessMax);
                controller.nextCheckpoint = startingCheckpoint;

                index++;

                yield return new WaitForSeconds(
                    timeIntervalBetweenCarsInSeconds
                );
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }

        spawnCoroutine = null;
    }

    // Call this from UI button
    public void ResetSpawner()
    {
        // Stop spawning
        StopSpawning();

        // Delete all spawned cars
        foreach (GameObject car in spawnedCars)
        {
            if (car != null)
            {
                Destroy(car);
            }
        }

        // Clear tracking list
        spawnedCars.Clear();

        // Reset and restart
        canSpawn = true;
        StartSpawning();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<CarAIController>())
        {
            canSpawn = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CarAIController>())
        {
            canSpawn = true;
        }
    }
}