using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance
    {
        get; private set;
    }

    private InputActions inputActions;

    [Header("Player")]
    public CharacterController characterController;
    public float speedWalking = 12f;
    public float speedRunning = 24f;
    private float jumpHeight = 24f;
    private float speed = 12f;

    [Header("Camera")]
    public GameObject cameraPlayer;
    public float mouseSensitivity = 1f;
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

    [Header("Settings")]
    private bool playerDisabled = false;

    [Header("Pin")]
    public GameObject pinPrefab;
    public Transform releasePoint;

    [Header("Hovering")]
    public float maxDistance = 5f;
    public GameObject lastHoveredObject;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one Player instance");
        }
        Instance = this;
    }

    public void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        flashlight.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
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
        PlayerFocusingPlace();

        if (inputActions.Player.Sprint.triggered)
        {
            Sprint();
        }
        if (inputActions.Player.Sprint.WasReleasedThisFrame())
        {
            Walk();
        }

        if (inputActions.Player.Flashlight.triggered)
        {
            flashlight.SetActive(!flashlight.activeSelf);
        }

        if (inputActions.Player.Jump.triggered && isGrounded)
        {
            Jump();
        }

        if (inputActions.Player.Attack.triggered)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
                return;
            Instantiate(pinPrefab, releasePoint.position, transform.localRotation);
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
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        float verticalMovement = input.y;
        float horizontalMovement = input.x;

        Vector3 move = transform.right * horizontalMovement + transform.forward * verticalMovement;

        characterController.Move(speed * move * Time.deltaTime);

        // gravity effect
        velocity.y += gravity * 2f * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
    }

    public void PlayerFocusingPlace()
    {
        Ray ray = new Ray(cameraPlayer.transform.position, cameraPlayer.transform.forward);
        RaycastHit hit;
        // Debug.DrawRay(ray.origin, ray.direction * 5f, Color.green);
        // Cast a ray forward from the camera
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            GameObject hitObj = hit.collider.gameObject;


            // If new object hovered
            if (hitObj != lastHoveredObject)
            {
                Debug.Log("Hovering");
                // if previously hovering something, call OnHoverExit
                if (lastHoveredObject != null)
                {
                    Debug.Log("Hovering New");
                    Interactable prevHover = lastHoveredObject.GetComponent<Interactable>();
                    if (prevHover != null)
                        prevHover.OnHoverExit();
                }

                // call OnHoverEnter on the new one
                Interactable hover = hitObj.GetComponent<Interactable>();
                if (hover != null)
                    hover.OnHoverEnter();

                lastHoveredObject = hitObj;
            }
        }
        else
        {
            // if ray hits nothing, stop hovering
            if (lastHoveredObject != null)
            {
                Interactable prevHover = lastHoveredObject.GetComponent<Interactable>();
                if (prevHover != null)
                    prevHover.OnHoverExit();
                lastHoveredObject = null;
            }
        }
    }


    public void Sprint()
    {
        speed = speedRunning;
    }
    public void Walk()
    {
        speed = speedWalking;
    }

    public void MovePlayerCamera()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        // Frame-rate independent mouse movement
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // Vertical rotation (pitch)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        // Apply rotation
        transform.Rotate(0f, mouseX, 0f); // Horizontal (yaw)
        cameraPlayer.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f); // Vertical (pitch)
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
