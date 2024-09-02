using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public ComputeShader verticesComputeShader;
    public ComputeShader marchCubeComputeShader;

    Dictionary<Vector3Int, GameObject> chuncks = new Dictionary<Vector3Int, GameObject>();

    private bool isCreatingChunck = false;

    // void Start()
    // {
    //     int chunksPlayerDistanceToLoad = 10;

    //     Vector3Int playerPosition = Vector3Int.zero;

    //     for (int x = -chunksPlayerDistanceToLoad; x < chunksPlayerDistanceToLoad; x++)
    //     {
    //         for (int z = -chunksPlayerDistanceToLoad; z < chunksPlayerDistanceToLoad; z++)
    //         {
    //             Vector3Int positionChecked = playerPosition + (Vector3Int.forward * x * (TerrainData.width - 1)) + (Vector3Int.right * z * (TerrainData.width - 1));
    //             if (!chuncks.ContainsKey(positionChecked))
    //             {
    //                 StartCoroutine(CreateChunk(positionChecked));
    //             }
    //         }
    //     }
    // }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void OnValidate()
    // {
    //     int chunksPlayerDistanceToLoad = 10;

    //     Vector3Int playerPosition = new Vector3Int(NearestMultipleOfChunkSize(0), NearestMultipleOfChunkSize(0), NearestMultipleOfChunkSize(0));
    //     playerPosition.y = 0;

    //     for (int x = -chunksPlayerDistanceToLoad; x < chunksPlayerDistanceToLoad; x++)
    //     {
    //         for (int z = -chunksPlayerDistanceToLoad; z < chunksPlayerDistanceToLoad; z++)
    //         {
    //             Vector3Int positionChecked = playerPosition + (Vector3Int.forward * x * (TerrainData.width - 1)) + (Vector3Int.right * z * (TerrainData.width - 1));
    //             if (!chuncks.ContainsKey(positionChecked))
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

        for (int x = -chunksPlayerDistanceToLoad; x < chunksPlayerDistanceToLoad; x++)
        {
            for (int z = -chunksPlayerDistanceToLoad; z < chunksPlayerDistanceToLoad; z++)
            {
                Vector3Int positionChecked = playerPosition + (Vector3Int.forward * x * (TerrainData.width - 1)) + (Vector3Int.right * z * (TerrainData.width - 1));
                if (!chuncks.ContainsKey(positionChecked) && !isCreatingChunck)
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

        if (closestPositionChecked != chunkOutsideRadius)
        {
            isCreatingChunck = true;
            chuncks.Add(closestPositionChecked, null);
            chuncks.Add(closestPositionChecked + Vector3Int.up * TerrainData.height, null);
            chuncks.Add(closestPositionChecked + Vector3Int.up * 2 * TerrainData.height, null);
            chuncks.Add(closestPositionChecked + Vector3Int.up * 3 * TerrainData.height, null);
            chuncks.Add(closestPositionChecked + Vector3Int.down * TerrainData.height, null);
            chuncks.Add(closestPositionChecked + Vector3Int.down * TerrainData.height * 2, null);
            StartCoroutine(CreateChunk(closestPositionChecked));
        }
    }

    IEnumerator CreateChunk(Vector3Int positionChecked)
    {
        new ChunkGenerator(gameObject, positionChecked, verticesComputeShader, marchCubeComputeShader);
        new ChunkGenerator(gameObject, positionChecked + (Vector3Int.up * (TerrainData.width - 1)), verticesComputeShader, marchCubeComputeShader);
        new ChunkGenerator(gameObject, positionChecked + (Vector3Int.up * 2 * (TerrainData.width - 1)), verticesComputeShader, marchCubeComputeShader);
        new ChunkGenerator(gameObject, positionChecked + (Vector3Int.up * 3 * (TerrainData.width - 1)), verticesComputeShader, marchCubeComputeShader);
        new ChunkGenerator(gameObject, positionChecked + (Vector3Int.down * (TerrainData.width - 1)), verticesComputeShader, marchCubeComputeShader);
        new ChunkGenerator(gameObject, positionChecked + (Vector3Int.down * 2 * (TerrainData.width - 1)), verticesComputeShader, marchCubeComputeShader);

        yield return null;
        isCreatingChunck = false;
    }

    public int NearestMultipleOfChunkSize(float number)
    {
        return Mathf.FloorToInt(number) / (TerrainData.width - 1) * (TerrainData.width - 1);
    }
}
