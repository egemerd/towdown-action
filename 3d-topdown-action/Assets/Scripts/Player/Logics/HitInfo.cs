using UnityEngine;
public struct HitInfo
{
    public float damage;
    public EnemyKnockbackConfigSO knockbackConfig;
    public Vector3 sourcePosition;
    public Vector3 hitPosition;
    public Vector3 hitDirection;
    public float knockbackForce;
    public float chainIndex;
    public bool isKnockedAttack;

    public static HitInfo FromAttack(Vector3 source, Vector3 target, float damage, float knockback, EnemyKnockbackConfigSO knockbackConfig, float chainIndex ,bool isKnockedAttack)
    {
        Vector3 dir = target - source;
        dir.y = 0;
        dir = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector3.forward;

        return new HitInfo
        {
            damage = damage,
            sourcePosition = source,
            hitPosition = target,
            hitDirection = dir,
            knockbackForce = knockback,
            knockbackConfig = knockbackConfig,
            chainIndex = chainIndex,
            isKnockedAttack = isKnockedAttack
        };
    }
}