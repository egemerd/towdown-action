using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask playerLayer;

    private Vector3 direction;

    public void SetTarget(Vector3 targetPosition)
    {
        direction = (targetPosition - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            HitInfo info = HitInfo.FromAttack(
                source: transform.position - direction,
                target: other.ClosestPoint(transform.position),
                damage: damage,
                knockback: 0f,
                knockbackConfig: null
            );
            damageable.TakeDamage(info);
        }

        Destroy(gameObject);
    }
}