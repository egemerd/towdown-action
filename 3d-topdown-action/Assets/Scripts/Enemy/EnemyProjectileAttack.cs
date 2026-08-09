using System.Collections;
using UnityEngine;

public class EnemyProjectileAttack : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private Transform playerTransform;

    IDamageable damageable;

    private Coroutine attackRoutine;

    private void Awake()
    {
        damageable = GetComponent<IDamageable>();
    }
    private void OnEnable()
    {
        attackRoutine = StartCoroutine(ProjectileAttackLoop());
        damageable.OnDamageTaken += OnDamageTaken;
    }

    private void OnDisable()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        damageable.OnDamageTaken -= OnDamageTaken;
    }

    private void OnDamageTaken(HitInfo info)
    {
        // Hasar alýndýðýnda saldýrý döngüsünü sýfýrla
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(ProjectileAttackLoop());
        }
    }
    private IEnumerator ProjectileAttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);

            if (projectilePrefab == null || playerTransform == null) continue;

            FireProjectile();
        }
    }

    private void FireProjectile()
    {
        GameObject projectileObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        if (projectileObj.TryGetComponent<EnemyProjectile>(out var projectile))
        {
            projectile.SetTarget(playerTransform.position);
        }
        else
        {
            Debug.LogError("Projectile prefab'ýnda EnemyProjectile component'i yok!", projectileObj);
        }
    }
}