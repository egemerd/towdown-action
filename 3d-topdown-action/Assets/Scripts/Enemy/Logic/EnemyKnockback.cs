using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Impulse + Drag tabanlı knockback.
/// ApplyForce() anlık velocity impulse'u uygular; her frame velocity 
/// exponential decay ile söner. Curve yok — pure physics-inspired tuning.
/// 
/// CurrentVelocity public — VFX trail veya diğer sistemler bunu okuyup 
/// intensity scale edebilir (knockback trail'i hıza göre uzasın gibi).
/// </summary>
public class EnemyKnockback : MonoBehaviour, IKnockable
{
    [Header("Config")]
    [SerializeField] private EnemyKnockbackConfigSO config;
    [SerializeField] private EnemyKnockbackConfigSO resumeKnockbackConfig;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;
    [Tooltip("SphereCast radius — enemy collider'ından biraz küçük tut ki sıkışma olmasın.")]
    [SerializeField] private float wallCastRadius = 0.5f;

    [Header("Combo (bilardo hazırlığı)")]
    [SerializeField] private int knockbackThreshold = 3;

    // Runtime state
    private Coroutine knockbackRoutine;
    private Vector3 currentVelocity;

    // Public state
    public bool IsKnockedBack => knockbackRoutine != null;
    public Vector3 CurrentVelocity => currentVelocity;  // VFX/Trail sistemleri için
    public float CurrentSpeed => currentVelocity.magnitude;
    public int HitCount { get; private set; }
    public EnemyKnockbackConfigSO Config => config;
    private bool pendingResume;

    // Yeni public method
    public void MarkForResume()
    {
        pendingResume = true;
    }


    // Events
    public event Action<Vector3> OnKnockbackApplied;
    public event Action OnKnockbackEnded;
    public event Action OnThresholdReached;
    public event Action<Vector3,Vector3> OnWallHit;

    private void Awake()
    {
        if (config == null)
            Debug.LogError($"EnemyKnockback: Config atanmamış! {gameObject.name}", this);
    }

    /// <summary>
    /// IKnockable implementation.
    /// force.magnitude = INITIAL SPEED (units/sec), NOT distance.
    /// force direction = knockback yönü.
    /// configOverride null ise inspector'daki default config kullanılır.
    /// </summary>
    public void ApplyForce(Vector3 force, EnemyKnockbackConfigSO configOverride)
    {
        var activeConfig = configOverride != null ? configOverride : config;
        if (activeConfig == null) return;

        // Direction + speed'i ayrıştır, speed'i profile ile scale et
        Vector3 direction = force.sqrMagnitude > 0.0001f
            ? force.normalized
            : transform.forward;
        float initialSpeed = force.magnitude * activeConfig.speedMultiplier;

        // Anlık velocity impulse
        currentVelocity = direction * initialSpeed;

        // Ongoing knockback varsa iptal et — yeni hit tazeleyecek
        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(KnockbackRoutine(activeConfig));

        HitCount++;
        OnKnockbackApplied?.Invoke(currentVelocity);

        if (HitCount >= knockbackThreshold)
            OnThresholdReached?.Invoke();
    }

    private IEnumerator KnockbackRoutine(EnemyKnockbackConfigSO activeConfig)
    {
        float totalElapsed = 0f;
        float burstElapsed = 0f;
        Vector3 previousPosition = transform.position;
        float minSpeedSqr = activeConfig.minSpeed * activeConfig.minSpeed;

        while (true)
        {
            float dt = Time.deltaTime;
            totalElapsed += dt;

            // Safety cap
            if (totalElapsed >= activeConfig.maxDuration) break;

            // Burst phase'de drag azaltılır — punchy kick hissi
            float effectiveDrag;
            if (burstElapsed < activeConfig.burstDuration)
            {
                effectiveDrag = activeConfig.drag * activeConfig.burstDragMultiplier;
                burstElapsed += dt;
            }
            else
            {
                effectiveDrag = activeConfig.drag;
            }

            // Framerate-independent exponential decay: v(t+dt) = v(t) * exp(-drag * dt)
            // Bu senin klasik `1 - Exp(-speed * dt)` pattern'inin türev'i —
            // burada target=0 olduğu için doğrudan çarpım yeterli.
            float decayFactor = Mathf.Exp(-effectiveDrag * dt);
            currentVelocity *= decayFactor;

            // Position update
            Vector3 delta = currentVelocity * dt;

            // Wall check — hareket ETMEDEN önce doğrula
            if (CheckWallHit(previousPosition, delta, out Vector3 hitPoint, out Vector3 hitNormal))
            {
                Debug.Log($"EnemyKnockback: Wall hit at {hitPoint} with normal {hitNormal}");
                //transform.position = hitPoint + hitNormal * 0.05f;

                var bounceEffect = GetComponent<WallBounceEffect>();

                // NOT: velocity'i sıfırlamıyoruz henüz — Bounce component event içinde 
                //      CurrentVelocity'yi okuyup direction hesaplayacak.
                knockbackRoutine = null;
                pendingResume = false; // temiz slate — dinleyicilerden birinin claim etmesini bekliyoruz

                OnWallHit?.Invoke(hitPoint, hitNormal);

                if (!pendingResume)
                {
                    // Kimse bounce claim etmedi — gerçekten bittik
                    currentVelocity = Vector3.zero;
                    OnKnockbackEnded?.Invoke();
                }
                // else: Bounce component MarkForResume() çağırdı, ResumeKnockback pause sonrası 
                //       çağrılacak. Şimdilik enemy duruyor (velocity aynı ama coroutine yok) — 
                //       impact pause'un yarattığı doğal freeze.

                yield break;
            }

            Vector3 newPosition = previousPosition + delta;
            transform.position = newPosition;
            previousPosition = newPosition;

            // End condition: hız çok düşükse dur.
            // sqrMagnitude karşılaştırma — sqrt hesaplamasından kaçınır (mikro-opt ama free)
            if (currentVelocity.sqrMagnitude < minSpeedSqr) break;

            yield return null;
        }

        currentVelocity = Vector3.zero;
        knockbackRoutine = null;
        OnKnockbackEnded?.Invoke();
    }

    public void ResumeKnockback(Vector3 velocity, EnemyKnockbackConfigSO configOverride = null)
    {
        // Priority: parametre > resumeKnockbackConfig field > default config
        var activeConfig = configOverride != null
            ? configOverride
            : (resumeKnockbackConfig != null ? resumeKnockbackConfig : config);

        if (activeConfig == null) return;

        if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);

        currentVelocity = velocity;
        pendingResume = false;  // claim tamamlandı, flag temizle
        knockbackRoutine = StartCoroutine(KnockbackRoutine(activeConfig));
    }

    private bool CheckWallHit(Vector3 fromPosition, Vector3 delta,
                          out Vector3 hitPoint, out Vector3 hitNormal)
    {
        hitPoint = default;
        hitNormal = default;
        if (wallLayer == 0 || delta.sqrMagnitude < 0.0001f) return false;

        float castDistance = delta.magnitude;
        Vector3 castDirection = delta.normalized;


        if (Physics.SphereCast(fromPosition, wallCastRadius, castDirection,
            out RaycastHit hit, castDistance, wallLayer))
        {
            hit.collider.gameObject.GetComponent<WallBounceEffect>().BounceEffect();
            hitPoint = hit.point;
            hitNormal = hit.normal;   
            return true;
        }
        return false;
    }

    public void ResetHitCount() => HitCount = 0;

    public void StopKnockback()
    {
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
            currentVelocity = Vector3.zero;
            OnKnockbackEnded?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, wallCastRadius);
    }
}