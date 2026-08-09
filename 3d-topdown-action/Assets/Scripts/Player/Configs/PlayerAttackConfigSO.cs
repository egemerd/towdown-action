using UnityEngine;

/// <summary>
/// Bir attack'in tüm verisi. Ýleride farklý silahlar, farklý combo hit'leri
/// için birden fazla asset yaratýlabilir.
/// </summary>
[CreateAssetMenu(fileName = "AttackData", menuName = "Combat/Attack Data", order = 3)]
public class PlayerAttackConfigSO : ScriptableObject
{
    [Header("Damage")]
    public float damage = 10f;
    public float knockbackForce = 5f;

    [Header("Hit Detection")]
    public float range = 1.5f;
    public float radius = 1.2f;
    public Vector3 offset = Vector3.zero;
    public LayerMask targetLayer;

    [Header("Timing")]
    [Tooltip("Attack baþladýktan sonra hit aktif olana kadar geçen süre. Anticipation fazý.")]
    [Range(0f, 0.3f)]
    public float windupDuration = 0.06f;

    [Tooltip("Hit'in aktif olduðu süre. Bu pencerede rakip vurulur.")]
    [Range(0.02f, 0.3f)]
    public float activeDuration = 0.10f;

    [Tooltip("Attack bittikten sonra oyuncunun toparlanma süresi. Cancel window burada.")]
    [Range(0f, 0.6f)]
    public float recoveryDuration = 0.20f;

    [Header("Cancel Window")]
    [Tooltip("Recovery'nin baþýndan itibaren kaç saniye sonra cancel'a izin verilir. " +
             "0 ise recovery boyunca cancel edilebilir. Süreden yüksek olursa cancel yok demektir.")]
    [Range(0f, 0.6f)]
    public float cancelWindowStart = 0.05f;

    [Header("Feel")]
    [Tooltip("Hit landýðýnda Time.timeScale'in düþürüleceði süre. 0 = hit stop yok.")]
    public float hitStopDuration = 0.06f;
    public float shakeMagnitude = 0.3f;
    public float shakeDuration = 0.15f;

    // Toplam attack süresi — dýþ sistemler için kullanýþlý
    public float TotalDuration => windupDuration + activeDuration + recoveryDuration;
}