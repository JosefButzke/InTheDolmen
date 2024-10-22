using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public ComputeShader verticesComputeShader;
    public ComputeShader marchCubeComputeShader;

    Dictionary<Vector3Int, GameObject> chunks = new Dictionary<Vector3Int, GameObject>();


    public GameObject parentContainer;

    private bool isCreatingChunck = false;

    // void Start()
    // void Start()
    // {
    //     foreach (Transform child in parentContainer.transform)
    //     {
    //         string[] parts = child.name.Split(',');
    //         Vector3Int key = new Vector3Int(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
    //         if (!chunks.ContainsKey(key))
    //         {
    //             chunks.Add(key, child.gameObject);
    //         }
    //         else
    //         {
    //             Debug.LogWarning("Duplicate child name found: " + child.name);
    //         }
    //     }
    // }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {
    //     int chunksPlayerDistanceToLoad = 10;

    //     Vector3Int playerPosition = Vector3Int.zero;
    //     playerPosition.y = 0;

    //     foreach (Transform child in parentContainer.transform)
    //     {
    //         Destroy(child.gameObject);
    //     }

    //     for (int x = -chunksPlayerDistanceToLoad; x < chunksPlayerDistanceToLoad; x++)
    //     {
    //         for (int z = -chunksPlayerDistanceToLoad; z < chunksPlayerDistanceToLoad; z++)
    //         {
    //             Vector3Int positionChecked = playerPosition + (Vector3Int.forward * x * (TerrainData.width - 8)) + (Vector3Int.right * z * (TerrainData.width - 8));
    //             if (!chunks.ContainsKey(positionChecked))
    //             {
    //                 StartCoroutine(CreateChunk(positionChecked));
    //             }
    //         }
    //     }
    // }

    private void FixedUpdate()
    {
        int chunksPlayerDistanceToLoad = 20;

        Vector3Int playerPosition = new Vector3Int(NearestMultipleOfChunkSize(Player.Instance.transform.position.x), NearestMultipleOfChunkSize(Player.Instance.transform.position.y), NearestMultipleOfChunkSize(Player.Instance.transform.position.z));
        playerPosition.y = 0;
        Vector3Int chunkOutsideRadius = playerPosition + new Vector3Int(chunksPlayerDistanceToLoad * TerrainData.width, 0, chunksPlayerDistanceToLoad * TerrainData.width);
        Vector3Int closestPositionChecked = chunkOutsideRadius;

        foreach (KeyValuePair<Vector3Int, GameObject> chunk in chunks.ToArray())
        {
            if (Vector3.Distance(chunk.Key, playerPosition) > chunksPlayerDistanceToLoad * 1.5f * TerrainData.width)
            {
                Destroy(chunk.Value);
                chunks.Remove(chunk.Key);
                Debug.Log("Destroy");
            }
            //chunks
        }

        for (int x = -chunksPlayerDistanceToLoad; x < chunksPlayerDistanceToLoad; x++)
        {
            for (int z = -chunksPlayerDistanceToLoad; z < chunksPlayerDistanceToLoad; z++)
            {
                Vector3Int positionChecked = playerPosition + (Vector3Int.forward * x * (TerrainData.width - 8)) + (Vector3Int.right * z * (TerrainData.width - 8));
                if (!chunks.ContainsKey(positionChecked) && !isCreatingChunck)
                {
                    float distanceNewChunkFromPlayer = Vector3Int.Distance(playerPosition, positionChecked);
                    float distanceClosestPositionFromPlayer = Vector3Int.Distance(playerPosition, closestPositionChecked);

                    if (distanceNewChunkFromPlayer < distanceClosestPositionFromPlayer)
                    {
                        closestPositionChecked = positionChecked;
                    }
                }
            }
        }

        if (closestPositionChecked != chunkOutsideRadius && !isCreatingChunck)
        {
            isCreatingChunck = true;

            if (Player.Instance.transform.position.y >= 0)
            {
                chunks.Add(closestPositionChecked, null);
                chunks.Add(closestPositionChecked + Vector3Int.up * TerrainData.height, null);
                chunks.Add(closestPositionChecked + Vector3Int.up * 2 * TerrainData.height, null);
            }
            else
            {
                chunks.Add(closestPositionChecked + Vector3Int.down * TerrainData.height, null);
                chunks.Add(closestPositionChecked + Vector3Int.down * TerrainData.height * 2, null);
                chunks.Add(closestPositionChecked + Vector3Int.down * TerrainData.height * 3, null);
            }

            StartCoroutine(CreateChunk(closestPositionChecked));
        }
    }

    IEnumerator CreateChunk(Vector3Int positionChecked)
    {
        chunks[positionChecked] = new ChunkGenerator(parentContainer, positionChecked, verticesComputeShader, marchCubeComputeShader).gameObject;
        chunks[positionChecked + (Vector3Int.up * (TerrainData.width - 8))] = new ChunkGenerator(parentContainer, positionChecked + (Vector3Int.up * (TerrainData.width - 8)), verticesComputeShader, marchCubeComputeShader).gameObject;
        chunks[positionChecked + (Vector3Int.up * 2 * (TerrainData.width - 8))] = new ChunkGenerator(parentContainer, positionChecked + (Vector3Int.up * 2 * (TerrainData.width - 8)), verticesComputeShader, marchCubeComputeShader).gameObject;
        chunks[positionChecked + (Vector3Int.down * (TerrainData.width - 8))] = new ChunkGenerator(parentContainer, positionChecked + (Vector3Int.down * (TerrainData.width - 8)), verticesComputeShader, marchCubeComputeShader).gameObject;
        chunks[positionChecked + (Vector3Int.down * 2 * (TerrainData.width - 8))] = new ChunkGenerator(parentContainer, positionChecked + (Vector3Int.down * 2 * (TerrainData.width - 8)), verticesComputeShader, marchCubeComputeShader).gameObject;
        chunks[positionChecked + (Vector3Int.down * 3 * (TerrainData.width - 8))] = new ChunkGenerator(parentContainer, positionChecked + (Vector3Int.down * 3 * (TerrainData.width - 8)), verticesComputeShader, marchCubeComputeShader).gameObject;

        yield return null;
        isCreatingChunck = false;
    }

    public int NearestMultipleOfChunkSize(float number)
    {
        return Mathf.FloorToInt(number) / (TerrainData.width - 8) * (TerrainData.width - 8);
    }
}
