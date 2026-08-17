using UnityEngine;
using DG.Tweening;  
using System;

public class WallBounceEffect : MonoBehaviour
{
    [SerializeField] WallBounceEffectConfigSO config;

    private Vector3 originalScale;
    private Vector3 originalPosition;

    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
    }

    public void BounceEffect()
    {
        // Önceki tween'leri temizle ve reset
        transform.DOKill();
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;

        // Þimdi yeni tween'i baþlat
        transform.DOPunchScale(config.bounceForce, config.bounceDuration, config.bounceVibrato, config.randomness);
        transform.DOShakePosition(config.positionDuration, config.positionForce, config.positionVibrato, config.positionRandomness);
    }

    private void OnDisable()
    {
        // Obje disable olursa tween takýlý kalmasýn
        transform.DOKill();
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
    }
}
