using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackConfig", menuName = "PlayerAttackConfig", order = 1)]
public class PlayerAttackConfigSO : ScriptableObject
{
   public float attackRange = 5f;
   public float attackRadius = 5f;
   public float attackDamage = 10f;
   public float attackKnockback = 5f;
   public Vector3 attackOffset;
   public float attackCooldown = 0.5f;
   public LayerMask enemyLayer;
}
