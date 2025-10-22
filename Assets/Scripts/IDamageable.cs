using UnityEngine;

public class IDamageable : MonoBehaviour
{
    private float life = 50f;

    public void TakeDamage(float amount)
    {
        life = life - amount;

        if (life <= 0)
        {
            Destroy(transform.gameObject);
        }
    }
}
