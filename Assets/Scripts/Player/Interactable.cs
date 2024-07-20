using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public float radius = 1.5f;
    public Item item;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void Interact()
    {
        Inventory.Instance.AddItem(item, Random.Range(2, 4));
        Destroy(gameObject);
    }
}
