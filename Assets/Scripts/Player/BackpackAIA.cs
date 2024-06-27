using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BackpackStatus
{
    IDLE,
    WORKING,
    WALKING,
    REST
}

public class BackpackAIA : MonoBehaviour
{
    public GameObject handFR;
    public GameObject handFL;
    public GameObject handBR;
    public GameObject handBL;

    private Vector3 targetPosition;
    public GameObject targetPlaceholder;
    public GameObject targetPlaceholderInstance;

    public BackpackStatus status = BackpackStatus.IDLE;

    [SerializeField]
    private float movementSpeed = 10f;

    public CharacterController characterController;

    public static BackpackAIA Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one Player instance");
        }
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        float verticalMovement = Input.GetAxis("Vertical");
        float horizontalMovement = Input.GetAxis("Horizontal");

        bool isMoving = verticalMovement != 0 || horizontalMovement != 0;

        Vector3 move = transform.forward * verticalMovement;

        characterController.Move(move * movementSpeed * Time.deltaTime);

        transform.Rotate(Vector3.up, horizontalMovement * 90f * Time.deltaTime);

        // ALIGN WITH FLOOR
        Vector3 rayPosition = transform.position;

        Ray ray = new(rayPosition, -transform.up);
        Debug.DrawRay(rayPosition, -transform.up, Color.red);
        if (Physics.Raycast(ray, out RaycastHit info, 20f, LayerMask.GetMask("Ground")))
        {
            // Vector3 newPosition = info.point;
            // newPosition.y = newPosition.y + 0.4f;
            // transform.position = newPosition;

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, info.normal) * transform.rotation;
            // Apply the rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    public void MoveTo(Vector3 point)
    {
        if (targetPlaceholderInstance)
        {
            Destroy(targetPlaceholderInstance);
        }

        targetPlaceholderInstance = Instantiate(targetPlaceholder, point, Quaternion.identity);

        targetPosition = point;

        status = BackpackStatus.WALKING;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}