using UnityEngine;

/// <summary>
/// PlayerAttack.OnHitLanded event'ini dinler ve chain index'e göre
/// uygun kamera shake'i tetikler. Chain-based feedback logic'ini merkezileştirir.
///
/// İleride VFX variation, ses variasyonu, screen flash gibi diğer
/// chain-based feedback'ler de buraya eklenir.
/// </summary>
public class AttackFeedbackDispatcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private AttackChainTracker chainTracker;
    [SerializeField] private CameraShaker cameraShaker;

    [Header("Shake Profile")]
    [Tooltip("Attack hit landığında kullanılacak shake profile.")]
    [SerializeField] private CameraShakeProfileSO hitShakeProfile;

    [Header("Direction Alternation")]
    [Tooltip("Chain index çift ise sağa, tek ise sola shake. " +
             "İstemiyorsan false yap — her hit aynı yön.")]
    [SerializeField] private bool alternateDirection = true;

    private void Awake()
    {
        if (playerAttack == null) playerAttack = GetComponent<PlayerAttack>();
        if (chainTracker == null) chainTracker = GetComponent<AttackChainTracker>();
        // CameraShaker sahnede farklı bir GameObject'te — inspector'dan bağla

        if (cameraShaker == null)
            Debug.LogError("AttackFeedbackDispatcher: CameraShaker bağlanmamış!", this);
    }

    private void OnEnable()
    {
        if (chainTracker != null)
            chainTracker.OnChainIndexChanged += HandleHitLanded;
    }

    private void OnDisable()
    {
        if (chainTracker != null)
            chainTracker.OnChainIndexChanged -= HandleHitLanded;
    }

    private void HandleHitLanded(int chainIndex)
    {
        if (hitShakeProfile == null || cameraShaker == null) return;

        // Chain index'e göre yön belirle
        // Not: HandleHitLanded, AttackChainTracker.HandleHitLanded'dan önce mi sonra mı çalışır?
        // Event subscription sırasına bağlı. Güvenli olmak için chain index +1 varsayabiliriz
        // ama en temizi: bu dispatcher event'i dinlerken chain zaten güncellenmiş olmalı.
        int chainIdx = chainIndex;

        float direction = alternateDirection
            ? ((chainIdx % 2 == 0) ? 1f : -1f) // çift → sağ, tek → sol
            : 1f;

        cameraShaker.Shake(hitShakeProfile, direction);
    }


}