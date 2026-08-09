using UnityEngine;

public interface IKnockable
{
    void ApplyForce(Vector3 force, float duration);
    bool IsKnockedBack { get; }
}
