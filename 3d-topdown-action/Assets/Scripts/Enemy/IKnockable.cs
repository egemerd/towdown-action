using UnityEngine;

public interface IKnockable
{
    void ApplyForce(Vector3 force, EnemyKnockbackConfigSO config);
    bool IsKnockedBack { get; }
}
