using System.Collections;
using UnityEngine;

public class MultitoolManager : MonoBehaviour
{
    public static MultitoolManager Instance
    {
        get; private set;
    }

    enum MultitoolType
    {
        COLLECT,
        TERRAFORM
    }

    public LayerMask terrainLayer;
    public LayerMask resourceLayer;

    public float interactionDistance = 30f;

    public ComputeShader verticesComputeShader;
    public ComputeShader marchCubeComputeShader;

    private bool isTerraforming = false;

    [SerializeField]
    private GameObject mesh;
    private MultitoolType type = MultitoolType.COLLECT;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one Player instance");
        }
        Instance = this;

        mesh.GetComponent<MeshRenderer>().material.color = type == MultitoolType.COLLECT ? Color.green : Color.red;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            type = type == MultitoolType.COLLECT ? MultitoolType.TERRAFORM : MultitoolType.COLLECT;

            if (type == MultitoolType.COLLECT)
            {
                mesh.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            else
            {
                mesh.GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }

        RaycastHit hit;

        if (Input.GetMouseButton(0) && type == MultitoolType.TERRAFORM)
        {
            if (isTerraforming)
            {
                return;
            }

            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.forward, out hit, interactionDistance, terrainLayer))
            {

                Collider[] hitColliders = Physics.OverlapSphere(hit.point, 4f);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                    {
                        isTerraforming = true;
                        StartCoroutine(Terraform(hitCollider.gameObject, hit.point, true));
                    }

                    if (hitCollider.gameObject.layer == LayerMask.NameToLayer("TerrainBox") && hit.collider.gameObject != hitCollider.gameObject)
                    {
                        isTerraforming = true;
                        StartCoroutine(Terraform(hitCollider.gameObject.transform.parent.gameObject, hit.point, true));
                    }
                }
            }
        }

        if (Input.GetMouseButton(1) && type == MultitoolType.TERRAFORM)
        {
            if (isTerraforming)
            {
                return;
            }
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.forward, out hit, interactionDistance, terrainLayer))
            {
                Collider[] hitColliders = Physics.OverlapSphere(hit.point, 8f);

                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                    {
                        isTerraforming = true;
                        StartCoroutine(Terraform(hitCollider.gameObject, hit.point, false));
                    }

                    if (hitCollider.gameObject.layer == LayerMask.NameToLayer("TerrainBox") && hit.collider.gameObject != hitCollider.gameObject)
                    {
                        isTerraforming = true;
                        StartCoroutine(Terraform(hitCollider.gameObject.transform.parent.gameObject, hit.point, false));
                    }
                }
            }
        }

        if (Input.GetMouseButton(0) && type == MultitoolType.COLLECT)
        {
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.forward, out hit, interactionDistance, resourceLayer))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Resource"))
                {
                    GameObject obj = hit.collider.gameObject;
                    obj.GetComponent<BoxCollider>().enabled = false;
                    obj.GetComponent<Resource>().enabled = true;
                }
            }
        }

    }

    IEnumerator Terraform(GameObject gameObject, Vector3 hitPoint, bool terraformType)
    {

        if (!gameObject.GetComponent<ChunckRegenerate>())
        {
            gameObject.AddComponent<ChunckRegenerate>();
        }

        ChunckRegenerate script = gameObject.GetComponent<ChunckRegenerate>();
        script.terraformPoint = hitPoint;
        script.verticesComputeShader = verticesComputeShader;
        script.marchCubeComputeShader = marchCubeComputeShader;
        script.terraformType = terraformType;
        script.GenerateMesh();

        yield return new WaitForSeconds(0.1f);
        isTerraforming = false;

        // if (hit.collider.gameObject.layer == resourceLayer)
        // {
        //     GameObject obj = hit.collider.gameObject;
        //     obj.GetComponent<BoxCollider>().enabled = false;
        //     obj.GetComponent<Resource>().enabled = true;
        // }
    }
}
