using Unity.VisualScripting;
using UnityEngine;

public class ChunckRegenerate : MonoBehaviour
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

    public MeshFilter meshFilter;

    private int numberOfVertexBySide = TerrainData.width;

    public ComputeShader verticesComputeShader;
    public ComputeShader marchCubeComputeShader;
    public Vector3 terraformPoint = Vector3.one;
    public bool terraformType = true;

    private Vector4[] verticesData = new Vector4[TerrainData.width * TerrainData.width * TerrainData.width];

    private bool first = true;

    public void GenerateMesh()
    {
        int pointsNumber = numberOfVertexBySide * numberOfVertexBySide * numberOfVertexBySide;
        int threadGroups = Mathf.CeilToInt(numberOfVertexBySide / 8.0f);

        //vertices
        ComputeBuffer verticesBuffer = new ComputeBuffer(pointsNumber, sizeof(float) * 4);

        if (first)
        {
            verticesComputeShader.SetBuffer(0, "vertices", verticesBuffer);
            verticesComputeShader.SetVector("offset", transform.position);
            verticesComputeShader.SetBool("terraformType", terraformType);
            verticesComputeShader.SetVector("terraformPoint", terraformPoint);
            verticesComputeShader.SetVector("playerFeetPoint", Player.Instance.groundCheck.position);
            verticesComputeShader.SetBool("first", first);
            verticesComputeShader.SetInt("sideChunk", numberOfVertexBySide);
            verticesComputeShader.Dispatch(0, threadGroups, threadGroups, threadGroups);

            verticesBuffer.GetData(verticesData);

        }
        else
        {
            verticesBuffer.SetData(verticesData);

            verticesComputeShader.SetBuffer(0, "vertices", verticesBuffer);
            verticesComputeShader.SetBool("first", first);
            verticesComputeShader.SetVector("offset", transform.position);
            verticesComputeShader.SetVector("terraformPoint", terraformPoint);
            verticesComputeShader.SetVector("playerFeetPoint", Player.Instance.groundCheck.position);
            verticesComputeShader.SetBool("terraformType", terraformType);
            verticesComputeShader.SetInt("sideChunk", numberOfVertexBySide);
            verticesComputeShader.Dispatch(0, threadGroups, threadGroups, threadGroups);

            verticesBuffer.GetData(verticesData);
        }

        // tris
        ComputeBuffer triangleBuffer = new ComputeBuffer(pointsNumber * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        // Set the buffer in the compute shader
        triangleBuffer.SetCounterValue(0);

        marchCubeComputeShader.SetBuffer(0, "vertices", verticesBuffer);
        marchCubeComputeShader.SetBuffer(0, "triangles", triangleBuffer);
        marchCubeComputeShader.SetInt("sideChunk", numberOfVertexBySide);
        marchCubeComputeShader.SetVector("offset", transform.position);
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

        meshFilter = gameObject.GetComponent<MeshFilter>();


        meshFilter.sharedMesh = new Mesh();

        meshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        meshFilter.sharedMesh.vertices = vertices;
        meshFilter.sharedMesh.triangles = meshTriangles;
        meshFilter.sharedMesh.RecalculateNormals();

        MeshCollider meshCollider = GetComponent<MeshCollider>();

        meshCollider.sharedMesh = null; // Clear the current mesh
        meshCollider.sharedMesh = meshFilter.sharedMesh; // Assign the updated mesh

        verticesBuffer.GetData(verticesData);

        first = false;

        // Release the buffer
        verticesBuffer.Release();
        triangleBuffer.Release();
        triCountBuffer.Release();

    }
}