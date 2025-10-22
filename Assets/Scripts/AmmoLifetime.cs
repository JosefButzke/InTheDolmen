using UnityEngine;

public class AmmoLifetime : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 30f;
    public float lifeTime = 5f;
    public float damage = 10f;
    public bool useGravity = false;
    private bool wasConllided = false;

    Rigidbody rb;
    float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        spawnTime = Time.time;
        rb.linearVelocity = transform.forward * speed;
        rb.useGravity = useGravity;
    }

    void Update()
    {
        if (Time.time - spawnTime > lifeTime)
            ReturnToPool();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Try to deal damage if target has IDamageable
        var dmg = collision.collider.GetComponent<IDamageable>();
        if (dmg != null)
        {

            if (!wasConllided)
            {
                wasConllided = true;
                Quaternion baseRot = transform.rotation;

                // Rotate 22 degrees left around Y axis
                Quaternion rot1 = baseRot * Quaternion.Euler(0f, -45f, 0f);
                Quaternion rot2 = baseRot * Quaternion.Euler(0f, 45f, 0f);
                Quaternion rot3 = baseRot * Quaternion.Euler(0f, 0f, 0f);
                dmg.TakeDamage(damage);

                Destroy(transform.gameObject);

                Instantiate(transform.gameObject, dmg.transform.position, rot1);
                Instantiate(transform.gameObject, dmg.transform.position, rot2);
                Instantiate(transform.gameObject, dmg.transform.position, rot3);
            }
        }
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false);
    }
}
