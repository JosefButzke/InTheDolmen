using UnityEngine;

public class MovementExtension : MonoBehaviour
{
    [SerializeField] WheelCollider tireFLCollider;
    [SerializeField] WheelCollider tireFRCollider;
    [SerializeField] WheelCollider tireBLCollider;
    [SerializeField] WheelCollider tireBRCollider;

    [SerializeField] Transform tireFLTransform;
    [SerializeField] Transform tireFRTransform;
    [SerializeField] Transform tireBLTransform;
    [SerializeField] Transform tireBRTransform;

    private void FixedUpdate()
    {
        UpdateWheel(tireFLCollider, tireFLTransform);
        UpdateWheel(tireFRCollider, tireFRTransform);
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
