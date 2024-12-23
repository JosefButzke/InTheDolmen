using Unity.VisualScripting;
using UnityEngine;

public class IKArmSolver : MonoBehaviour
{
    public GameObject body;

    public LayerMask terrainLayer;

    public bool inverse = false;

    private Vector3 currentPosition;

    public GameObject rayPoint;

    private float stepDistance = 0.75f;

    private void Start()
    {
        currentPosition = transform.position;
    }

    void Update()
    {
        transform.position = currentPosition;

        float distanceDown = 0f;
        float distanceForward = 0f;

        Ray rayDown = new(rayPoint.transform.position, transform.up * (inverse ? 1 : -1));
        Debug.DrawRay(rayPoint.transform.position, transform.up * (inverse ? 1 : -1), Color.red);
        if (Physics.Raycast(rayDown, out RaycastHit infoDown, 2f, terrainLayer.value))
        {
            distanceDown = Vector3.Distance(infoDown.point, currentPosition);
        }

        Ray rayForward = new(rayPoint.transform.position - (Vector3.up * 1), transform.forward);
        Debug.DrawRay(rayPoint.transform.position - (Vector3.up * 1), transform.forward, Color.red);
        if (Physics.Raycast(rayForward, out RaycastHit infoForward, 1f, terrainLayer.value))
        {
            distanceForward = Vector3.Distance(infoForward.point, currentPosition);
        }

        if (distanceDown >= stepDistance)
        {
            currentPosition = infoDown.point + (Vector3.up * 0.05f) + body.transform.forward * (stepDistance - 0.1f);
        }

        if (distanceForward >= stepDistance)
        {
            currentPosition = infoForward.point + (Vector3.up * 0.05f) + body.transform.forward * (stepDistance - 0.1f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(rayPoint.transform.position, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(currentPosition, 0.05f);
    }
}
