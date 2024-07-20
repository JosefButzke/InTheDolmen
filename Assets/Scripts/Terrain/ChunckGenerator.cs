using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ChunkGenerator
{

    struct Triangle
    {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }

    private MeshFilter meshFilter;

    private int numberOfVertexBySide = TerrainData.width;

    public GameObject chunk;

    public ChunkGenerator(GameObject _parent, Vector3Int _position, ComputeShader verticesComputeShader, ComputeShader marchCubeComputeShader)
    {
        Initialize(_parent, _position);
        GenerateMesh(verticesComputeShader, marchCubeComputeShader, _position);
        //GenerateFaces();
        chunk.AddComponent<MeshCollider>();
    }

    private void Initialize(GameObject parent, Vector3Int position)
    {
        if (meshFilter == null)
        {
            GameObject chunkGameObject = new GameObject(position.x + "," + position.y + "," + position.z);
            chunkGameObject.layer = LayerMask.NameToLayer("Terrain");
            chunkGameObject.tag = "Chunk";

            chunkGameObject.transform.position = position;
            chunkGameObject.transform.parent = parent.transform;

            meshFilter = chunkGameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();

            meshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            chunkGameObject.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Materials/Soil");

            chunk = chunkGameObject;
        }
    }

    private void GenerateMesh(ComputeShader verticesComputeShader, ComputeShader marchCubeComputeShader, Vector3Int offset)
    {
        int pointsNumber = numberOfVertexBySide * numberOfVertexBySide * numberOfVertexBySide;
        int threadGroups = Mathf.CeilToInt(numberOfVertexBySide / 8.0f);

        //vertices
        ComputeBuffer verticesBuffer = new ComputeBuffer(pointsNumber, sizeof(float) * 4);

        verticesComputeShader.SetBuffer(0, "vertices", verticesBuffer);
        verticesComputeShader.SetVector("offset", (Vector3)offset);
        verticesComputeShader.SetInt("sideChunk", numberOfVertexBySide);
        verticesComputeShader.Dispatch(0, threadGroups, threadGroups, threadGroups);

        // tris
        ComputeBuffer triangleBuffer = new ComputeBuffer(pointsNumber * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        // Set the buffer in the compute shader
        triangleBuffer.SetCounterValue(0);

        marchCubeComputeShader.SetBuffer(0, "vertices", verticesBuffer);
        marchCubeComputeShader.SetBuffer(0, "triangles", triangleBuffer);
        marchCubeComputeShader.SetInt("sideChunk", numberOfVertexBySide);
        marchCubeComputeShader.SetVector("offset", (Vector3)offset);
        marchCubeComputeShader.SetFloat("isoLevel", TerrainData.surfaceLevel);

        // Dispatch the compute shader
        marchCubeComputeShader.Dispatch(0, threadGroups, threadGroups, threadGroups);

        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }
        meshFilter.sharedMesh.vertices = vertices;
        meshFilter.sharedMesh.triangles = meshTriangles;
        meshFilter.sharedMesh.RecalculateNormals();

        // Release the buffer
        verticesBuffer.Release();
        triangleBuffer.Release();
        triCountBuffer.Release();
    }
}