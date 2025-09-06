using UnityEngine;
using UnityEngine.Rendering;


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
    public GameObject gameObject;

    public ChunkGenerator(GameObject _parent, Vector3Int _position, ComputeShader verticesComputeShader, ComputeShader marchCubeComputeShader, int precision)
    {
        Initialize(_parent, _position);
        GenerateMesh(verticesComputeShader, marchCubeComputeShader, _position, precision);

        gameObject.AddComponent<MeshCollider>();
    }

    private void Initialize(GameObject parent, Vector3Int position)
    {
        if (meshFilter == null)
        {
            GameObject chunkGameObject = new GameObject(position.x + "," + position.y + "," + position.z);
            chunkGameObject.layer = LayerMask.NameToLayer("Terrain");
            chunkGameObject.tag = "Chunk";
            chunkGameObject.isStatic = true;

            // GameObject childObject = new GameObject("TerrainBoxCollider");
            // childObject.transform.parent = chunkGameObject.transform;
            // BoxCollider boxCollider = childObject.AddComponent<BoxCollider>();
            // boxCollider.isTrigger = false;         
            // boxCollider.size = new Vector3Int(1,0,1) * TerrainData.width + new Vector3Int(0,1,0) * TerrainData.height;
            // childObject.layer = LayerMask.NameToLayer("TerrainBox");

            chunkGameObject.transform.position = position;
            // boxCollider.center = new Vector3Int(TerrainData.width/2,TerrainData.height/2,TerrainData.width/2);
            chunkGameObject.transform.parent = parent.transform;

            meshFilter = chunkGameObject.AddComponent<MeshFilter>();

            meshFilter.sharedMesh = new Mesh();

            meshFilter.sharedMesh.indexFormat = IndexFormat.UInt32;

            // UNDERGROUND
            chunkGameObject.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Materials/Soil");

            MeshRenderer meshRenderer = chunkGameObject.GetComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
            gameObject = chunkGameObject;
        }
    }

    private void GenerateMesh(ComputeShader verticesComputeShader, ComputeShader marchCubeComputeShader, Vector3Int offset, int precision)
    {
        int numberOfVertexWidth = TerrainData.width;
        int numberOfVertexHeight = TerrainData.height;
        int pointsNumber = numberOfVertexWidth / precision * numberOfVertexWidth / precision * numberOfVertexHeight / precision;
        int threadGroupX = Mathf.CeilToInt(numberOfVertexWidth / 8.0f);
        int threadGroupY = Mathf.CeilToInt(numberOfVertexHeight / 8.0f);

        //vertices
        ComputeBuffer verticesBuffer = new ComputeBuffer(pointsNumber, sizeof(float) * 4);
        verticesComputeShader.SetBuffer(0, "vertices", verticesBuffer);
        verticesComputeShader.SetVector("offset", (Vector3)offset);
        verticesComputeShader.SetInt("chunkWidth", numberOfVertexWidth);
        verticesComputeShader.SetInt("chunkHeight", numberOfVertexHeight);
        verticesComputeShader.SetInt("precision", precision);
        verticesComputeShader.Dispatch(0, threadGroupX, threadGroupY, threadGroupX);

        // tris
        ComputeBuffer triangleBuffer = new ComputeBuffer(pointsNumber * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        // Set the buffer in the compute shader
        triangleBuffer.SetCounterValue(0);

        marchCubeComputeShader.SetBuffer(0, "vertices", verticesBuffer);
        marchCubeComputeShader.SetBuffer(0, "triangles", triangleBuffer);
        marchCubeComputeShader.SetInt("chunkWidth", numberOfVertexWidth / precision);
        marchCubeComputeShader.SetInt("chunkHeight", numberOfVertexHeight / precision);
        marchCubeComputeShader.SetVector("offset", (Vector3)offset);
        marchCubeComputeShader.SetFloat("isoLevel", TerrainData.surfaceLevel);

        // Dispatch the compute shader
        marchCubeComputeShader.Dispatch(0, threadGroupX, threadGroupY, threadGroupX);

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

        Vector2[] uvs = new Vector2[vertices.Length];
        float uvScale = 0.1f; // Smaller = more tiling

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            uvs[i] = new Vector2(vertex.x * uvScale, vertex.z * uvScale);
        }

        meshFilter.sharedMesh.uv = uvs;

        meshFilter.sharedMesh.RecalculateNormals();
        meshFilter.sharedMesh.RecalculateBounds();
        meshFilter.sharedMesh.RecalculateTangents();

        // Release the buffer
        verticesBuffer.Release();
        triangleBuffer.Release();
        triCountBuffer.Release();
    }
}