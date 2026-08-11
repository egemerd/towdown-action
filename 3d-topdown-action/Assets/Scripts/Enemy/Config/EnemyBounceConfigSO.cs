using UnityEngine;

[CreateAssetMenu(fileName = "BounceConfig", menuName = "Combat/Bounce Config", order = 7)]
public class EnemyBounceConfigSO : ScriptableObject
{
    [Header("— Speed —")]
    [Tooltip("Bounce sonrasý SABÝT hýz (unit/saniye). " +
             "Enemy'nin duvara ne hýzla geldiði önemli deðil — bu deðer uygulanýr. " +
             "energyRetention < 1 ise her sonraki bounce'ta kümülatif olarak azalýr.")]
    [Range(1f, 50f)]
    public float bounceSpeed = 15f;

    [Tooltip("Bounce'lar arasý enerji kaybý. Her bounce'ta bounceSpeed bununla çarpýlýr (kümülatif). " +
             "1.0 = enerji kaybý yok (saf bilardo), " +
             "0.85 = her sekmede %15 azalma (doðal arcade decay), " +
             "0.6 = hýzlý sönme.")]
    [Range(0.3f, 1f)]
    public float energyRetention = 0.85f;

    [Header("— Angle —")]
    [Tooltip("Gerçek reflection (1) ile yüzey-parallel slide (0) arasýnda blend. " +
             "0.7-1.0 arasýnda tut — düþük deðerlerde enemy duvara yapýþýp kayar.")]
    [Range(0f, 1f)]
    public float angleBlend = 0.9f;

    [Header("— Limits —")]
    [Tooltip("Bir knockback zincirinde maksimum kaç kere sekebilir.")]
    [Range(0, 10)]
    public int maxBounces = 3;

    [Tooltip("Bu incoming hýzýn altýnda bounce yerine direkt durur. " +
             "Yavaþ çarpýþmalarda tam bounceSpeed uygulamak awkward hisseder.")]
    [Range(0f, 5f)]
    public float minVelocityToBounce = 1.5f;

    [Header("— Timing —")]
    [Tooltip("Duvara çarpma ile bounce baþlangýcý arasýndaki pause. " +
             "0.05-0.1 = juicy impact hissi (squash bu sýrada oynar).")]
    [Range(0f, 0.3f)]
    public float impactPauseDuration = 0.06f;
}