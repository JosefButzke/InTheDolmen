using UnityEngine;

public class Resource : MonoBehaviour
{
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = MultitoolManager.Instance.transform.position;
        gameObject.transform.parent = MultitoolManager.Instance.transform;
    }

    void Update()
    {
        targetPosition = MultitoolManager.Instance.transform.position - (MultitoolManager.Instance.transform.forward * 0.2f);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 8f);

        float distance = Vector3.Distance(transform.position, targetPosition);

        Vector3 scale = transform.localScale - (transform.localScale * Time.deltaTime * 8f);

        transform.localScale = scale;

        if (distance <= 0.2f)
        {
            Destroy(gameObject);
        }
    }
}
