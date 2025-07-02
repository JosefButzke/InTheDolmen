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
    public int chunksPlayerDistanceToLoad = 8;
    public int precision = 1;

    // private bool isCreatingChunck = false;

    public void RegenerateWorld()
    {
        if (parentContainer == null)
        {
            Debug.LogWarning("Parent container is not assigned.");
            return;
        }

        // Destroy all children of parentContainer
        for (int i = parentContainer.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = parentContainer.transform.GetChild(i);
            DestroyImmediate(child.gameObject); // Use DestroyImmediate in editor context
        }

        Debug.Log("Regenerated world (cleared children).");

        Vector3Int centerPosition = Vector3Int.zero;
        centerPosition.y = 0;

        for (int x = -chunksPlayerDistanceToLoad; x < chunksPlayerDistanceToLoad; x++)
        {
            for (int z = -chunksPlayerDistanceToLoad; z < chunksPlayerDistanceToLoad; z++)
            {
                Vector3Int positionChecked = centerPosition + (Vector3Int.forward * x * (TerrainData.width - precision)) + (Vector3Int.right * z * (TerrainData.width - precision));

                chunks[positionChecked] = new ChunkGenerator(parentContainer, positionChecked, verticesComputeShader, marchCubeComputeShader, precision).gameObject;
                chunks[positionChecked + Vector3Int.up * (TerrainData.height - precision)] = new ChunkGenerator(parentContainer, positionChecked + Vector3Int.up * (TerrainData.height - precision), verticesComputeShader, marchCubeComputeShader, precision).gameObject;
            }
        }
    }
}
