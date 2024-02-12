using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    private int numberOfChunkOfset = 2;

    public ComputeShader shader;

    [SerializeField]
    Dictionary<Vector3Int, ChunkData> chuncks = new Dictionary<Vector3Int, ChunkData>();

    Vector3 cameraPosition;

    // Start is called before the first frame update

    private void Start()
    {
        cameraPosition = Camera.main.transform.position;

        for (int x = -numberOfChunkOfset/2; x <= numberOfChunkOfset/2; x++)
        {
            for (int z = -numberOfChunkOfset/2; z <= numberOfChunkOfset/2; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x * TerrainData.width, 0, z * TerrainData.width);
                chuncks.Add(chunkPos, new ChunkData(gameObject, chunkPos, shader));
            }
        }
    }
}
