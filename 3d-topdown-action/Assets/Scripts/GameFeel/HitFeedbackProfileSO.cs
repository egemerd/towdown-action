using UnityEngine;

/// <summary>
/// Bir hit landýðýnda tetiklenecek feel efektlerinin tümü.
/// Farklý attack türleri için farklý profile asset'leri yaratýlabilir:
/// LightHitProfile, HeavyHitProfile, CriticalHitProfile vs.
/// </summary>
[CreateAssetMenu(fileName = "HitFeedbackProfile", menuName = "Combat/Hit Feedback Profile", order = 4)]
public class HitFeedbackProfileSO : ScriptableObject
{
    [Header("Time Stop")]
    [Tooltip("Time.timeScale bu deðere düþer. 0 = tam duruþ, 0.05 = neredeyse duruþ.")]
    [Range(0f, 0.5f)]
    public float timeStopScale = 0.05f;

    [Tooltip("Time stop süresi (unscaled saniye).")]
    [Range(0f, 0.3f)]
    public float timeStopDuration = 0.06f;

    [Header("Screen Shake")]
    [Tooltip("Shake þiddeti. 0 = yok, 0.5 = orta, 1+ = güçlü.")]
    public float shakeMagnitude = 0.3f;
    [Range(0f, 0.5f)]
    public float shakeDuration = 0.15f;

    [Header("Enemy Hit Flash")]
    public Color flashColor = Color.white;
    [Tooltip("Flash zirve süresi (renk tam parlak).")]
    [Range(0f, 0.2f)]
    public float flashHoldDuration = 0.05f;
    [Tooltip("Flash geri sönme süresi.")]
    [Range(0f, 0.3f)]
    public float flashFadeDuration = 0.1f;
}