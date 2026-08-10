using UnityEngine;

/// <summary>
/// Bir kamera shake'in tüm parametrelerini içeren preset.
/// Farklý shake türleri için farklý asset'ler yaratýlýr:
/// AttackHitShake, HeavyImpactShake, DeathShake vs.
/// </summary>
[CreateAssetMenu(fileName = "CameraShakeProfile", menuName = "Combat/Camera Shake Profile", order = 7)]
public class CameraShakeProfileSO : ScriptableObject
{
    [Header("Base Shake")]
    [Tooltip("Random position offset þiddeti.")]
    [Range(0f, 2f)]
    public float positionMagnitude = 0.3f;

    [Tooltip("Shake'in toplam süresi (saniye).")]
    [Range(0.05f, 1f)]
    public float duration = 0.25f;

    [Header("Tilt (rotation shake)")]
    [Tooltip("Kameranýn Z ekseninde tilt açýsý (derece). Pozitif = saða, negatif = sola. " +
             "Direction multiplier ile ters çevirebilirsin.")]
    [Range(-15f, 15f)]
    public float tiltAngle = 4f;

    [Header("Zoom (FOV / orthographic size)")]
    [Tooltip("Zoom in miktarý. Perspective camera'da FOV azalýr, ortho'da size azalýr.")]
    [Range(0f, 10f)]
    public float zoomAmount = 2f;

    [Header("Spring")]
    [Tooltip("Spring stiffness. Yüksek = daha hýzlý hedefe döner, düþük = yavaþ salýným.")]
    [Range(1f, 200f)]
    public float springStiffness = 80f;

    [Tooltip("Spring damping. Yüksek = az salýným (kritik damping), düþük = daha fazla salýným.")]
    [Range(0.1f, 20f)]
    public float springDamping = 8f;

    [Header("Falloff")]
    [Tooltip("Shake þiddeti zamanla nasýl azalýr. 1'den 0'a inen bir curve olmalý.")]
    public AnimationCurve intensityFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
}