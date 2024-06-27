using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaneGrowth : MonoBehaviour
{
    private float scale = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(scale, scale, scale);
    }

    // Update is called once per frame
    void Update()
    {
        if (scale < 1)
        {
            scale = scale + (0.03f * Time.deltaTime);
            transform.localScale = new Vector3(scale, scale, scale);
        }

        if (scale >= 1)
        {
            scale = 1f;
            transform.localScale = new Vector3(scale, scale, scale);
            Destroy(GetComponent<CaneGrowth>());
        }
    }
}
