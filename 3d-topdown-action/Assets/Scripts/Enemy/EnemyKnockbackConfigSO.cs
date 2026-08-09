using UnityEngine;

/// <summary>
/// Bir enemy türünün knockback karakterini tanýmlar.
/// Her enemy prefab kendi profile'ýný referans eder — light enemy hýzlý ve uzaða savrulur,
/// heavy enemy yavaþ ve kýsa. Profile asset'leri arasýnda hýzlýca swap edilebilir.
/// </summary>
[CreateAssetMenu(fileName = "KnockbackProfile", menuName = "Combat/Knockback Profile", order = 5)]
public class EnemyKnockbackConfigSO : ScriptableObject
{
    [Header("— Distance —")]
    [Tooltip("Attacker'dan gelen knockbackForce'un çarpaný. " +
             "1 = attacker'ýn verdiði mesafe aynen, 2 = iki katý, 0.5 = yarýsý. " +
             "Light enemy 1.5x, heavy enemy 0.3-0.5x tipik.")]
    public float distanceMultiplier = 1f;

    [Header("— Timing —")]
    [Tooltip("Knockback süresi (saniye). Uzun süre = daha uzun glide hissi. " +
             "Akýþkan glide için 0.4-0.6s önerilir.")]
    [Range(0.1f, 1.5f)]
    public float duration = 0.5f;

    [Header("— Curve —")]
    [Tooltip("Hareket eðrisi. Ease-out (dik baþla, yatay bit) glide hissi verir. " +
             "Ease-in-out daha 'aðýr' bir his verir.")]
    public AnimationCurve movementCurve = CreateDefaultGlideCurve();

    [Header("— Overshoot (opsiyonel) —")]
    [Tooltip("Enemy hedefi hafifçe geçip geri gelir mi? Cartoon overshoot için. " +
             "0 = yok, 0.1-0.2 = hafif geçme.")]
    [Range(0f, 0.3f)]
    public float overshootAmount = 0f;

    /// <summary>
    /// Cubic ease-out benzeri, akýþkan glide veren default curve.
    /// Baþta hýzlý, sonda yumuþakça durur — buz üstünde kayan taþ hissi.
    /// </summary>
    private static AnimationCurve CreateDefaultGlideCurve()
    {
        var curve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 4f),      // Baþlangýç: yatay giriþ, dik çýkýþ (ivmelenme)
            new Keyframe(0.3f, 0.75f, 1f, 1f), // Orta: hýzlý ilerleme
            new Keyframe(1f, 1f, 0f, 0f)       // Bitiþ: yatay giriþ ve çýkýþ (durak glide)
        );
        return curve;
    }
}