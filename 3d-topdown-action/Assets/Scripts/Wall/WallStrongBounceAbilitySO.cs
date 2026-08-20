using UnityEngine;

[CreateAssetMenu(fileName = "WallStrongBounceAbilitySO", menuName = "WallAbilities/StrongBounceAbility")]
public class WallStrongBounceAbilitySO : WallAbilitySO , IWallBounceConfig
{
    [SerializeField] private EnemyBounceConfigSO bounceConfig;
    public EnemyBounceConfigSO BounceConfig => bounceConfig;

    public override void Apply(WallHitContext ctx)
    {
        throw new System.NotImplementedException();
    }
}
