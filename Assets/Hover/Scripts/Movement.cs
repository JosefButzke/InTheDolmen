using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] WheelCollider tireFLCollider;
    [SerializeField] WheelCollider tireFRCollider;
    [SerializeField] WheelCollider tireMLCollider;
    [SerializeField] WheelCollider tireMRCollider;
    [SerializeField] WheelCollider tireBLCollider;
    [SerializeField] WheelCollider tireBRCollider;

    [SerializeField] Transform tireFLTransform;
    [SerializeField] Transform tireFRTransform;
    [SerializeField] Transform tireMLTransform;
    [SerializeField] Transform tireMRTransform;
    [SerializeField] Transform tireBLTransform;
    [SerializeField] Transform tireBRTransform;

    public float acceleration = 12000f;
    public float breakingForce = 16000f;
    public float maxTurnAngle = 15f;
    public float maxSpeed = 80f;

    private float currentAcceleration = 0f;
    private float currentBreakingForce = 0f;
    private float currentTurnAngle = 0f;
    public float currentSpeed = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {

        currentAcceleration = acceleration * Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.Space))
        {
            currentBreakingForce = breakingForce;
        }
        else
        {
            currentBreakingForce = 0f;
        }

        currentSpeed = rb.linearVelocity.magnitude * 3.6f;

        //Only add motorTorque if your speed is less then max speed
        if (currentSpeed < maxSpeed)
        {
            tireFLCollider.motorTorque = currentAcceleration;
            tireFRCollider.motorTorque = currentAcceleration;
        }
        else
        {
            currentAcceleration = 0;
            tireFLCollider.motorTorque = currentAcceleration;
            tireFRCollider.motorTorque = currentAcceleration;
        }

        tireFLCollider.brakeTorque = currentBreakingForce;
        tireFRCollider.brakeTorque = currentBreakingForce;
        tireMLCollider.brakeTorque = currentBreakingForce;
        tireMRCollider.brakeTorque = currentBreakingForce;
        tireBLCollider.brakeTorque = currentBreakingForce;
        tireBRCollider.brakeTorque = currentBreakingForce;

        currentTurnAngle = maxTurnAngle * Input.GetAxis("Horizontal");
        tireFLCollider.steerAngle = currentTurnAngle;
        tireFRCollider.steerAngle = currentTurnAngle;

        UpdateWheel(tireFLCollider, tireFLTransform);
        UpdateWheel(tireFRCollider, tireFRTransform);
        UpdateWheel(tireMLCollider, tireMLTransform);
        UpdateWheel(tireMRCollider, tireMRTransform);
        UpdateWheel(tireBLCollider, tireBLTransform);
        UpdateWheel(tireBRCollider, tireBRTransform);
    }

    void UpdateWheel(WheelCollider col, Transform trans)
    {
        Vector3 position;
        Quaternion rotation;

        col.GetWorldPose(out position, out rotation);

        trans.position = position;
        trans.rotation = rotation;
    }
}
