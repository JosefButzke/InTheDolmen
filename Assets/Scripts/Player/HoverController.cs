using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverController : MonoBehaviour
{
    public float radius = 4f;
    public GameObject cameraHover;

    private bool hoverDisabled = true;
    private float horizontalInput;
    private float verticalInput;
    private bool isBreaking;
    private float currentBreakTorque;
    private float currentSteerAngle;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteerAngle;

    [SerializeField] private WheelCollider wheelL1Collider;
    [SerializeField] private WheelCollider wheelL2Collider;
    [SerializeField] private WheelCollider wheelL3Collider;
    [SerializeField] private WheelCollider wheelL4Collider;
    [SerializeField] private WheelCollider wheelL5Collider;
    [SerializeField] private WheelCollider wheelR1Collider;
    [SerializeField] private WheelCollider wheelR2Collider;
    [SerializeField] private WheelCollider wheelR3Collider;
    [SerializeField] private WheelCollider wheelR4Collider;
    [SerializeField] private WheelCollider wheelR5Collider;

    [SerializeField] private Transform wheelL1Mesh;
    [SerializeField] private Transform wheelL2Mesh;
    [SerializeField] private Transform wheelL3Mesh;
    [SerializeField] private Transform wheelL4Mesh;
    [SerializeField] private Transform wheelL5Mesh;
    [SerializeField] private Transform wheelR1Mesh;
    [SerializeField] private Transform wheelR2Mesh;
    [SerializeField] private Transform wheelR3Mesh;
    [SerializeField] private Transform wheelR4Mesh;
    [SerializeField] private Transform wheelR5Mesh;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);       
    }

    private void Start()
    {
        cameraHover.SetActive(false);
        hoverDisabled = true;
    }

    private void Update()
    {
        if(hoverDisabled)
        {
            return;
        }

        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            motorForce = motorForce * 3;
            breakForce = breakForce * 3;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            motorForce = motorForce / 3;
            breakForce = breakForce / 3;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            hoverDisabled = true;
            cameraHover.SetActive(false);
            Player.Instance.EnablePlayer();
        }
    }

    public void Interact()
    {
        Player.Instance.DisablePlayer();
        cameraHover.SetActive(true);
        hoverDisabled = false;
    }
    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }
    private void HandleMotor()
    {
        wheelL1Collider.motorTorque = verticalInput * motorForce;
        wheelL2Collider.motorTorque = verticalInput * motorForce;
        wheelR1Collider.motorTorque = verticalInput * motorForce;
        wheelR2Collider.motorTorque = verticalInput * motorForce;

        currentBreakTorque = isBreaking ? breakForce : 0f;
        if (isBreaking)
        {
            ApplyBreaking();
        }
    }

    private void ApplyBreaking()
    {
        wheelL1Collider.brakeTorque = currentBreakTorque;
        wheelL2Collider.brakeTorque = currentBreakTorque;
        wheelL3Collider.brakeTorque = currentBreakTorque;
        wheelL4Collider.brakeTorque = currentBreakTorque;
        wheelL5Collider.brakeTorque = currentBreakTorque;
        wheelR1Collider.brakeTorque = currentBreakTorque;     
        wheelR2Collider.brakeTorque = currentBreakTorque;
        wheelR3Collider.brakeTorque = currentBreakTorque;
        wheelR4Collider.brakeTorque = currentBreakTorque;
        wheelR5Collider.brakeTorque = currentBreakTorque;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        wheelL1Collider.steerAngle = currentSteerAngle;
        wheelL2Collider.steerAngle = currentSteerAngle;
        wheelR1Collider.steerAngle = currentSteerAngle;
        wheelR2Collider.steerAngle = currentSteerAngle;
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelMesh)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelMesh.position = pos;
        wheelMesh.rotation = rot;

    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(wheelL1Collider, wheelL1Mesh);
        UpdateSingleWheel(wheelL2Collider, wheelL2Mesh);
        UpdateSingleWheel(wheelL3Collider, wheelL3Mesh);
        UpdateSingleWheel(wheelL4Collider, wheelL4Mesh);
        UpdateSingleWheel(wheelL5Collider, wheelL5Mesh);

        UpdateSingleWheel(wheelR1Collider, wheelR1Mesh);
        UpdateSingleWheel(wheelR2Collider, wheelR2Mesh);
        UpdateSingleWheel(wheelR3Collider, wheelR3Mesh);
        UpdateSingleWheel(wheelR4Collider, wheelR4Mesh);
        UpdateSingleWheel(wheelR5Collider, wheelR5Mesh);
    }
}
