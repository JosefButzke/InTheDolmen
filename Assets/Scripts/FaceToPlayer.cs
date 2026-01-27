using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(SphereCollider))]
public class FaceToPlayer : MonoBehaviour
{
    private bool playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<UIDocument>().enabled = true;
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<UIDocument>().enabled = false;
            playerInside = false;
        }
    }

    void OnValidate()
    {
        GetComponent<UIDocument>().enabled = false;
    }

    void Awake()
    {
        GetComponent<UIDocument>().enabled = false;
    }

    private void Update()
    {
        if (!playerInside)
        {
            transform.rotation = Quaternion.identity;
            return;
        }

        Vector3 direction = transform.position - Player.Instance.cameraPlayer.transform.position;

        if (direction.sqrMagnitude < 0.1f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            360f
        );
    }
}
