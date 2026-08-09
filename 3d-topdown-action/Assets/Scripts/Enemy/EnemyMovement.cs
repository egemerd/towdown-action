using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform target;

    private EnemyKnockback knockback;

    private void Awake()
    {
        knockback = GetComponent<EnemyKnockback>();
    }

    private void Update()
    {
        // Knockback aktifse hareket etme — knockback pozisyonu kendi uygular
        if (knockback != null && knockback.IsKnockedBack) return;
        if (target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime);
    }
}