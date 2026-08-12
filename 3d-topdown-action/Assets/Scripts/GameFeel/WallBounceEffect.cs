using UnityEngine;
using DG.Tweening;  
using System;

public class WallBounceEffect : MonoBehaviour
{
    [SerializeField] WallBounceEffectConfigSO config;
    public void BounceEffect()
    {
        transform.DOPunchScale(config.bounceForce, config.bounceDuration, config.bounceVibrato, config.randomness);
        transform.DOShakePosition(config.positionDuration, config.positionForce, config.positionVibrato, config.positionRandomness);
    }
}
