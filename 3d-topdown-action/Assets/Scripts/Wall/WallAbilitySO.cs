using UnityEngine;

public abstract class WallAbilitySO : ScriptableObject
{
    public abstract void Apply(WallHitContext ctx);
}
