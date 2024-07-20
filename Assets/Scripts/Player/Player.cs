using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance
    {
        get; private set;
    }

    [Header("Player")]
    public CharacterController characterController;
    public float speedWalking = 24f;
    public float speedRunning = 36f;
    public float jumpHeight = 3f;
    private float speed = 24f;

    [Header("Camera")]
    public GameObject cameraPlayer;
    public float mouseSensitivity = 3f;
    public float maxVerticalAngle = 80f; // Maximum vertical rotation angle
    public float minVerticalAngle = -80f; // Minimum vertical rotation angle
    private float verticalRotation = 0f;

    [Header("Ground")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;

    [Header("Gravity")]
    public float gravity = -9.7f;
    Vector3 velocity;

    [Header("Flashlight")]
    public GameObject flashlight;

    [Header("Animatior")]
    public Animator animator;

    [Header("Settings")]
    private bool playerDisabled = false;

    public GameObject canePrefab;

    public GameObject flagPrefab;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one Player instance");
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        flashlight.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerDisabled)
        {
            return;
        }

        GroundCheck();
        MovePlayer();
        MovePlayerCamera();

        if (Input.GetKeyDown(KeyCode.L))
        {
            flashlight.SetActive(!flashlight.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed = speedRunning;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed = speedWalking;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Ray ray = cameraPlayer.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 3))
            {
                Interactable interactableGameObject = hit.collider.GetComponent<Interactable>();
                if (interactableGameObject != null)
                {
                    interactableGameObject.Interact();
                }

                HoverController hover = hit.collider.GetComponent<HoverController>();
                if (hover != null)
                {
                    hover.Interact();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // if (Input.GetKeyDown(KeyCode.Z))
        // {

        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     if (Physics.Raycast(ray, out RaycastHit info, 100f, LayerMask.GetMask("Ground")))
        //     {
        //         BackpackAIA.Instance.MoveTo(info.point);
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit info, 55f, LayerMask.GetMask("Crop")))
            {
                Instantiate(canePrefab, info.point, Quaternion.identity);
            }
        }
    }

    public void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    public void MovePlayer()
    {
        float verticalMovement = Input.GetAxis("Vertical");
        float horizontalMovement = Input.GetAxis("Horizontal");

        bool isMoving = verticalMovement != 0 || horizontalMovement != 0;

        animator.SetBool("isRunning", isMoving);

        Vector3 move = transform.right * horizontalMovement + transform.forward * verticalMovement;

        characterController.Move(move * speed * Time.deltaTime);

        // gravity effect
        velocity.y += gravity * 2f * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
    }

    public void MovePlayerCamera()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Calculate rotation
        float horizontalRotation = mouseX * 3;
        verticalRotation -= mouseY * 3; // Invert vertical rotation for more intuitive control
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        // Apply rotation to the player
        transform.Rotate(0f, horizontalRotation, 0f);
        cameraPlayer.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public void EnablePlayer()
    {
        cameraPlayer.SetActive(true);
        playerDisabled = false;
    }

    public void DisablePlayer()
    {
        cameraPlayer.SetActive(false);
        playerDisabled = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(groundCheck.transform.position, groundDistance);
    }
}
