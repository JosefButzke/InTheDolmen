using UnityEngine;

public class PinAttach : MonoBehaviour
{
    private float speed = 40f;
    private Rigidbody rb;
    public LayerMask attachableLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & attachableLayer) != 0)
        {
            // Stop movement
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;

            SphereCollider collider = transform.GetComponent<SphereCollider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            PinAttach script = transform.GetComponent<PinAttach>();
            script.enabled = false;
        }
    }
}
