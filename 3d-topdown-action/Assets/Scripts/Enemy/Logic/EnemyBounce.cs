using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyKnockback))]
public class EnemyBounce : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private EnemyBounceConfigSO config;

    private EnemyKnockback knockback;
    private int bounceCount;
    private Coroutine bounceRoutine;

    public event Action<Vector3, Vector3> OnBounced;
    public event Action OnBounceLimitReached;

    private void Awake()
    {
        knockback = GetComponent<EnemyKnockback>();
        if (config == null)
            Debug.LogError($"[EnemyBounce] Config atanmamış! {name}", this);
    }

    private void OnEnable()
    {
        knockback.OnWallHit += HandleWallHit;
        knockback.OnKnockbackApplied += HandleNewKnockback;
    }

    private void OnDisable()
    {
        knockback.OnWallHit -= HandleWallHit;
        knockback.OnKnockbackApplied -= HandleNewKnockback;

        // Enemy disable olurken pending bounce varsa iptal et
        if (bounceRoutine != null)
        {
            StopCoroutine(bounceRoutine);
            bounceRoutine = null;
        }
    }

    private void HandleNewKnockback(Vector3 _)
    {
        bounceCount = 0;
    }

    private void HandleWallHit(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (config == null) return;

        Vector3 incomingVelocity = knockback.CurrentVelocity;

        // Limit checks — bunlar için CLAIM YAPMIYORUZ, knockback normal şekilde bitmesine izin veriyoruz
        if (bounceCount >= config.maxBounces)
        {
            OnBounceLimitReached?.Invoke();
            return;
        }

        if (incomingVelocity.magnitude < config.minVelocityToBounce)
        {
            OnBounceLimitReached?.Invoke();
            return;
        }

        // === Bu noktadan sonra bounce olacak — CLAIM ET ===
        // Knockback'e "ben devralıyorum, OnKnockbackEnded fire etme" sinyali ver.
        // Bu SENKRON olmak zorunda — event dönüşünden önce set edilmeli.
        knockback.MarkForResume();

        // === Reflection matematiği ===
        // Yalnızca yön için incoming'i kullanıyoruz, magnitude bilinçli olarak atılıyor.
        Vector3 pureReflection = Vector3.Reflect(incomingVelocity, hitNormal);
        Vector3 slideDirection = Vector3.ProjectOnPlane(incomingVelocity, hitNormal);
        Vector3 blendedDirection = Vector3.Lerp(slideDirection, pureReflection, config.angleBlend);
        blendedDirection.y = 0f;

        // === Sabit bounce speed + kümülatif decay ===
        // Örnek: bounceSpeed=15, energyRetention=0.85
        // Bounce 1 (count 0 → 1): 15 * 0.85^0 = 15
        // Bounce 2 (count 1 → 2): 15 * 0.85^1 = 12.75
        // Bounce 3 (count 2 → 3): 15 * 0.85^2 = 10.83
        float decayFactor = Mathf.Pow(config.energyRetention, bounceCount);
        float finalSpeed = config.bounceSpeed * decayFactor;

        Vector3 finalVelocity = blendedDirection.normalized * finalSpeed;

        bounceCount++;

        // Impact pause + resume
        if (bounceRoutine != null) StopCoroutine(bounceRoutine);
        bounceRoutine = StartCoroutine(BounceWithPause(finalVelocity, hitPoint, hitNormal));
    }

    private IEnumerator BounceWithPause(Vector3 finalVelocity, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (config.impactPauseDuration > 0f)
            yield return new WaitForSeconds(config.impactPauseDuration);

        OnBounced?.Invoke(hitPoint, finalVelocity);
        knockback.ResumeKnockback(finalVelocity);
        bounceRoutine = null;
    }
}