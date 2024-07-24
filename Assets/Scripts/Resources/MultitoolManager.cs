using System.Collections;
using UnityEngine;

public class MultitoolManager : MonoBehaviour
{
    public static MultitoolManager Instance
    {
        get; private set;
    }

    public LayerMask terrainLayer;
    public LayerMask resourceLayer;

    public float interactionDistance = 30f;

    public ComputeShader verticesComputeShader;
    public ComputeShader marchCubeComputeShader;

    private bool isTerraforming = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one Player instance");
        }
        Instance = this;
    }


    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (isTerraforming)
            {
                return;
            }
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.forward, out hit, interactionDistance, terrainLayer))
            {
                Collider[] hitColliders = Physics.OverlapSphere(hit.point, 4f);

                foreach (var hitCollider in hitColliders)
                {
                    Debug.Log(hitCollider.gameObject.name);
                    if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                    {
                        isTerraforming = true;
                        StartCoroutine(Terraform(hitCollider.gameObject, hit.point, 1));
                    }

                    if (hitCollider.gameObject.layer == LayerMask.NameToLayer("TerrainBox") && hit.collider.gameObject != hitCollider.gameObject)
                    {
                        isTerraforming = true;
                        StartCoroutine(Terraform(hitCollider.gameObject.transform.parent.gameObject, hit.point, 1));
                    }
                }

                // if (hit.collider.gameObject.layer == resourceLayer)
                // {
                //     GameObject obj = hit.collider.gameObject;
                //     obj.GetComponent<BoxCollider>().enabled = false;
                //     obj.GetComponent<Resource>().enabled = true;
                // }
            }
        }

        if (Input.GetMouseButton(1))
        {
            if (isTerraforming)
            {
                return;
            }
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.forward, out hit, interactionDistance, terrainLayer))
            {
                Collider[] hitColliders = Physics.OverlapSphere(hit.point, 4f);

                foreach (var hitCollider in hitColliders)
                {
                    Debug.Log(hitCollider.gameObject.name);
                    if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                    {
                        isTerraforming = true;
                        StartCoroutine(Terraform(hitCollider.gameObject, hit.point, 0));
                    }

                    if (hitCollider.gameObject.layer == LayerMask.NameToLayer("TerrainBox") && hit.collider.gameObject != hitCollider.gameObject)
                    {
                        isTerraforming = true;
                        StartCoroutine(Terraform(hitCollider.gameObject.transform.parent.gameObject, hit.point, 0));
                    }
                }

                // if (hit.collider.gameObject.layer == resourceLayer)
                // {
                //     GameObject obj = hit.collider.gameObject;
                //     obj.GetComponent<BoxCollider>().enabled = false;
                //     obj.GetComponent<Resource>().enabled = true;
                // }
            }
        }
    }

    IEnumerator Terraform(GameObject gameObject, Vector3 hitPoint, int style)
    {

        if (!gameObject.GetComponent<ChunckRegenerate>())
        {
            gameObject.AddComponent<ChunckRegenerate>();
        }

        ChunckRegenerate script = gameObject.GetComponent<ChunckRegenerate>();
        script.terraformPoint = hitPoint;
        script.verticesComputeShader = verticesComputeShader;
        script.marchCubeComputeShader = marchCubeComputeShader;
        script.style = style;
        script.GenerateMesh();

        yield return new WaitForSeconds(0.2f);
        isTerraforming = false;

        // if (hit.collider.gameObject.layer == resourceLayer)
        // {
        //     GameObject obj = hit.collider.gameObject;
        //     obj.GetComponent<BoxCollider>().enabled = false;
        //     obj.GetComponent<Resource>().enabled = true;
        // }
    }
}
