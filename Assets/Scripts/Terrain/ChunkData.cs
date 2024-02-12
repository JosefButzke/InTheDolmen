using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ChunkData
{
    private Vector3Int chunkPosition;
    private MeshFilter meshFilter;

    private int width = TerrainData.width;
    private int height = TerrainData.height;

    private float[,,] terrainMap;

    private GameObject chunk;

    private ComputeShader shader;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    public ChunkData(GameObject _parent, Vector3Int _position, ComputeShader _shader)
    {
        shader = _shader;
        terrainMap = new float[width + 1, height + 1, width + 1];
        Initialize(_parent, _position);
        GenerateVertexWeight();
        GenerateFaces();
        //chunk.AddComponent<MeshCollider>();
    }

    private void Initialize(GameObject parent, Vector3Int position)
    {
        if (meshFilter == null)
        {
            GameObject chunkGameObject = new GameObject(position.x + "," + position.y + "," + position.z);
            chunkGameObject.layer = LayerMask.NameToLayer("Ground");
            chunkGameObject.tag = "Terrain";
            chunkPosition = position;
            chunkGameObject.transform.position = chunkPosition;
            chunkGameObject.transform.parent = parent.transform;

            meshFilter = chunkGameObject.AddComponent<MeshFilter>();
            //chunkGameObject.AddComponent<DrawGizmos>();
            chunkGameObject.AddComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PathSoil.mat");
            meshFilter.sharedMesh = new Mesh();

            chunk = chunkGameObject;
        }
    }

    private void GenerateVertexWeight()
    {
        for (int x = 0; x < width + 1; x++)
        {
            for (int z = 0; z < width + 1; z++)
            {
                for (int y = 0; y < height + 1; y++)
                {
                    terrainMap[x,y,z] = TerrainData.GetTerrainPointType(x + chunkPosition.x, y, z + chunkPosition.z);
                }
            }
        }
    }

    void GenerateFaces()
    {
        //int kernelIndex = shader.FindKernel("CSMain");
        //int numVertices = width * width;
        //ComputeBuffer verticesBuffer = new ComputeBuffer(numVertices, sizeof(float) * 3);
        //ComputeBuffer trianglesBuffer = new ComputeBuffer(numVertices * 6, sizeof(int));

        //shader.SetBuffer(kernelIndex, "vertices", verticesBuffer);
        //shader.SetFloat("Width", 10);

        //int[] triangles = new int[numVertices * 6];
        //trianglesBuffer.GetData(triangles);
        //trianglesBuffer.Release();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {

                    // Create an array of floats representing each corner of a cube and get the value from our terrainMap.
                    float[] cube = new float[8];
                    for (int i = 0; i < 8; i++)
                    {

                        Vector3Int corner = new Vector3Int(x, y, z) + TerrainData.CornerTable[i];
                        cube[i] = terrainMap[corner.x, corner.y, corner.z];

                    }

                    // Pass the value into our MarchCube function.
                    MarchCube(new Vector3Int(x, y, z));
                }
            }
        }


        meshFilter.sharedMesh.vertices = vertices.ToArray();
        meshFilter.sharedMesh.triangles = triangles.ToArray();
        meshFilter.sharedMesh.RecalculateNormals();
    }

    public void MarchCube(Vector3Int position)
    {
        float[] cube = new float[8];
        for (int i = 0; i < 8; i++)
        {
            Vector3Int corner = position + TerrainData.CornerTable[i];
            cube[i] = terrainMap[corner.x, corner.y, corner.z];
        }

        int configIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cube[i] > 0.05f)
                configIndex |= 1 << i;
        }


        if (configIndex == 0 || configIndex == 255)
        {
            return;
        }

        int edgeIndex = 0;

        for (int i = 0; i < 5; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                int indice = TerrainData.TriangleTable[configIndex, edgeIndex];

                if (indice == -1)
                {
                    return;
                }

                Vector3 vert1 = position + TerrainData.CornerTable[TerrainData.EdgeIndexes[indice, 0]];
                Vector3 vert2 = position + TerrainData.CornerTable[TerrainData.EdgeIndexes[indice, 1]];

                Vector3 vertPosition = (vert1 + vert2) / 2f;

                vertices.Add(vertPosition);

                triangles.Add(vertices.Count - 1);

                edgeIndex++;
            }
        }
    }
}