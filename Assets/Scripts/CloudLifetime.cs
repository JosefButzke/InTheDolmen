using UnityEngine;

public class CloudLifetime : MonoBehaviour
{
    private float scaleIncreaseSpeed = 0.1f;
    private float lifetime = 60f; // 1 minute
    private float timer = 0f;

    void Start()
    {
        transform.localScale = Vector3.one * 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        timer = timer + Time.deltaTime;

        if (timer >= lifetime)
        {
            Destroy(transform.gameObject);
        }

        transform.position = transform.position + Vector3.forward * 2f * Time.deltaTime;

        if (timer <= lifetime * 0.3f && transform.localScale.x <= 8.0f) // one third of lifetime
        {
            transform.localScale = transform.localScale + Vector3.one * scaleIncreaseSpeed * Time.deltaTime;
        }

        if (timer >= lifetime * 0.7f) // one third of lifetime
        {
            if (transform.localScale.x <= 0.3f)
            {
                Destroy(transform.gameObject);
            }
            transform.localScale = transform.localScale - Vector3.one * scaleIncreaseSpeed * Time.deltaTime;
        }
    }
}
