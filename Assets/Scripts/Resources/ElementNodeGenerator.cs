using UnityEngine;

public class ElementNodeGenerator : MonoBehaviour
{
    public GameObject molecule;

    public Element element;

    public LayerMask terrainMask;

    [Range(2, 32)]
    [SerializeField]
    public int size = 6;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        int start = -(size / 2);
        int end = size / 2;

        molecule.GetComponent<MeshRenderer>().sharedMaterial = element.nodeMaterial;

        for (int x = start; x <= end; x++)
        {
            for (int y = start; y <= end; y++)
            {
                for (int z = start; z <= end; z++)
                {
                    Vector3 localPosition = new Vector3(x, y, z);

                    if (Random.Range(start, end) >= 0 && localPosition.magnitude <= size * 0.6f)
                    {
                        Instantiate(molecule, localPosition + transform.position, Quaternion.identity, transform);
                    }
                }
            }
        }
    }
}
