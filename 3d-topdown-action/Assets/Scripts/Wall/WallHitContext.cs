using UnityEngine;

public readonly struct WallHitContext
{
    public readonly Vector3 hitPoint;
    public readonly Vector3 hitNormal;
    public readonly Vector3 incomingVelocity;
    public readonly GameObject hitter;

    public WallHitContext(Vector3 hitPoint, Vector3 hitNormal,
                          Vector3 incomingVelocity, GameObject hitter)
    {
        this.hitPoint = hitPoint;
        this.hitNormal = hitNormal;
        this.incomingVelocity = incomingVelocity;
        this.hitter = hitter;
    }
}