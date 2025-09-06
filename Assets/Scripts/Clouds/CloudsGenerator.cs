using UnityEngine;

public class CloudsGenerator : MonoBehaviour
{
    public GameObject parentContainer;
    public float minSpawnRange = -512f;
    public float maxSpawnRange = 512f;
    private float baseSpawnRangeHeight = 128f;
    public float spawnRangeHeight = 32f;

    public GameObject cloudsPrefab;
    private float spawnCooldown = 0.2f;
    private float timer = 0f;

    void Update()
    {
        if (timer >= spawnCooldown)
        {
            timer = 0.0f; // RESET TIMER TO NEW SPAWN

            float randX = Random.Range(minSpawnRange, maxSpawnRange);
            float randZ = Random.Range(minSpawnRange, maxSpawnRange);

            Vector3 position = Vector3.right * randX + Vector3.forward * randZ;

            position.y = baseSpawnRangeHeight + Random.Range(-spawnRangeHeight, spawnRangeHeight);
            GameObject cloudInstantiated = Instantiate(cloudsPrefab, position, Quaternion.identity);
            cloudInstantiated.transform.parent = parentContainer.transform;
        }
        timer = timer + Time.deltaTime;
    }

    // public void RegenerateClouds()
    // {
    //     if (parentContainer == null)
    //     {
    //         Debug.LogWarning("Parent container is not assigned.");
    //         return;
    //     }

    //     // Destroy all children of parentContainer
    //     for (int i = parentContainer.transform.childCount - 1; i >= 0; i--)
    //     {
    //         Transform child = parentContainer.transform.GetChild(i);
    //         DestroyImmediate(child.gameObject); // Use DestroyImmediate in editor context
    //     }
    //     clouds.Clear();
    //     Debug.Log("Regenerated clouds (cleared children).");

    //     Vector3Int centerPosition = Vector3Int.zero;
    //     centerPosition.y = 0;

    //     for (int x = -chunksPlayerDistanceToLoad; x < chunksPlayerDistanceToLoad; x++)
    //     {
    //         for (int z = -chunksPlayerDistanceToLoad; z < chunksPlayerDistanceToLoad; z++)
    //         {
    //             Vector3Int position = new Vector3Int((int)(x * TerrainData.width * Random.Range(0f, 3f)), 128, (int)(z * TerrainData.width * Random.Range(0f, 3f)));
    //             GameObject cloudInstantiated = Instantiate(cloudsPrefab, position, Quaternion.identity);
    //             cloudInstantiated.transform.parent = parentContainer.transform;
    //             clouds.Add(position, cloudInstantiated);
    //         }
    //     }
    // }
}
