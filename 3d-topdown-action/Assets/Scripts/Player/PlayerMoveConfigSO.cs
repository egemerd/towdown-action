using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMoveConfig", menuName = "Player/Move Config")]
public class PlayerMoveConfigSO : ScriptableObject
{
    [Header("— Speed —")]
    public float moveSpeed = 6f;

    [Tooltip("Sýfýrdan max speed'e ulaþma süresi (saniye). Küçük = snappy start.")]
    public float accelerationTime = 0.08f;

    [Tooltip("Max speed'den sýfýra düþme süresi. Küçük = anýnda dur, biraz büyük = arcade slide.")]
    public float decelerationTime = 0.12f;

    [Header("— Facing —")]
    [Tooltip("Karakterin hareket yönüne dönme hýzý. Yüksek = anlýk, düþük = smooth.")]
    public float turnSpeed = 18f;

    [Header("— Lean / Tilt —")]
    [Tooltip("Karakter hareket yönüne doðru kaç derece eðilsin.")]
    public float maxLeanAngle = 10f;
    [Tooltip("Lean'in hedef deðere ulaþma smoothness'ý.")]
    public float leanSmoothing = 12f;

    //[Header("— Squash & Stretch —")]
    //[Tooltip("Hareket baþlarken uygulanacak stretch scale (x/y/z). Kýsa süreli pulse.")]
    //public Vector3 startStretch = new Vector3(0.9f, 1.15f, 0.9f);
    //public float startStretchDuration = 0.18f;
    //public AnimationCurve startStretchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    //[Tooltip("Durunca uygulanacak squash scale.")]
    //public Vector3 stopSquash = new Vector3(1.15f, 0.85f, 1.15f);
    //public float stopSquashDuration = 0.15f;
    //public AnimationCurve stopSquashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("— Gravity —")]
    public float gravity = -20f;
}