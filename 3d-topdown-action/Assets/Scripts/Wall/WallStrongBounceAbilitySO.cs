using UnityEngine;

[CreateAssetMenu(fileName = "WallStrongBounceAbilitySO", menuName = "WallAbilities/StrongBounceAbility")]
public class WallStrongBounceAbilitySO : WallAbilitySO , IWallBounceConfig
{
    [SerializeField] private EnemyKnockbackConfigSO bounceConfig;
    public EnemyKnockbackConfigSO BounceConfig => bounceConfig;

    public override void Apply(WallHitContext ctx)
    {
        throw new System.NotImplementedException();
    }
}
