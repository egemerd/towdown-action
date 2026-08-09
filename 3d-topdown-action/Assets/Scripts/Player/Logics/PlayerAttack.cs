using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] PlayerAttackConfigSO attackConfig;

    bool isAttacking = false;
    PlayerInput playerInput;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

    }

    private void Update()
    {
        PlayerBasicAttack();
    }
    void PlayerBasicAttack()
    {
        if(playerInput.actions["BasicAttack"].triggered && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }   
    }


    IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        attackPrefab.SetActive(true);

        PerformHitDetection();

        yield return new WaitForSeconds(attackConfig.attackCooldown);

        isAttacking = false;
        attackPrefab.SetActive(false);
    }

    void PerformHitDetection()
    {
        Vector3 attackCenter = transform.position + transform.forward * attackConfig.attackRange;
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackConfig.attackRadius, attackConfig.enemyLayer);


        foreach (var h in hits)
        {
            Debug.Log($"Hit {h.name}");
            var hitInfo = HitInfo.FromAttack(transform.position, h.transform.position, attackConfig.attackDamage, attackConfig.attackKnockback);

            if (h.TryGetComponent<IDamageable>(out var d)) d.TakeDamage(hitInfo);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + transform.forward * attackConfig.attackRange, attackConfig.attackRadius);
    }
}

