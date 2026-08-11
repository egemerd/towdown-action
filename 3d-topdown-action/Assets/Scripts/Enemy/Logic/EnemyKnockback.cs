using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Impulse + Drag tabanlý knockback.
/// ApplyForce() anlýk velocity impulse'u uygular; her frame velocity 
/// exponential decay ile söner. Curve yok — pure physics-inspired tuning.
/// 
/// CurrentVelocity public — VFX trail veya diðer sistemler bunu okuyup 
/// intensity scale edebilir (knockback trail'i hýza göre uzasýn gibi).
/// </summary>
public class EnemyKnockback : MonoBehaviour, IKnockable
{
    [Header("Config")]
    [SerializeField] private EnemyKnockbackConfigSO config;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;
    [Tooltip("SphereCast radius — enemy collider'ýndan biraz küçük tut ki sýkýþma olmasýn.")]
    [SerializeField] private float wallCastRadius = 0.5f;

    [Header("Combo (bilardo hazýrlýðý)")]
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

    // Events
    public event Action<Vector3> OnKnockbackApplied;
    public event Action OnKnockbackEnded;
    public event Action OnThresholdReached;
    public event Action<Vector3> OnWallHit;

    private void Awake()
    {
        if (config == null)
            Debug.LogError($"EnemyKnockback: Config atanmamýþ! {gameObject.name}", this);
    }

    /// <summary>
    /// IKnockable implementation.
    /// force.magnitude = INITIAL SPEED (units/sec), NOT distance.
    /// force direction = knockback yönü.
    /// configOverride null ise inspector'daki default config kullanýlýr.
    /// </summary>
    public void ApplyForce(Vector3 force, EnemyKnockbackConfigSO configOverride)
    {
        var activeConfig = configOverride != null ? configOverride : config;
        if (activeConfig == null) return;

        // Direction + speed'i ayrýþtýr, speed'i profile ile scale et
        Vector3 direction = force.sqrMagnitude > 0.0001f
            ? force.normalized
            : transform.forward;
        float initialSpeed = force.magnitude * activeConfig.speedMultiplier;

        // Anlýk velocity impulse
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

            // Burst phase'de drag azaltýlýr — punchy kick hissi
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
            // burada target=0 olduðu için doðrudan çarpým yeterli.
            float decayFactor = Mathf.Exp(-effectiveDrag * dt);
            currentVelocity *= decayFactor;

            // Position update
            Vector3 delta = currentVelocity * dt;

            // Wall check — hareket ETMEDEN önce doðrula
            if (CheckWallHit(previousPosition, delta, out Vector3 hitPoint))
            {
                transform.position = hitPoint;
                currentVelocity = Vector3.zero;
                OnWallHit?.Invoke(hitPoint);
                break;
            }

            Vector3 newPosition = previousPosition + delta;
            transform.position = newPosition;
            previousPosition = newPosition;

            // End condition: hýz çok düþükse dur.
            // sqrMagnitude karþýlaþtýrma — sqrt hesaplamasýndan kaçýnýr (mikro-opt ama free)
            if (currentVelocity.sqrMagnitude < minSpeedSqr) break;

            yield return null;
        }

        currentVelocity = Vector3.zero;
        knockbackRoutine = null;
        OnKnockbackEnded?.Invoke();
    }

    private bool CheckWallHit(Vector3 fromPosition, Vector3 delta, out Vector3 hitPoint)
    {
        hitPoint = default;
        if (wallLayer == 0 || delta.sqrMagnitude < 0.0001f) return false;

        float castDistance = delta.magnitude;
        Vector3 castDirection = delta.normalized;

        if (Physics.SphereCast(fromPosition, wallCastRadius, castDirection,
            out RaycastHit hit, castDistance, wallLayer))
        {
            hitPoint = hit.point;
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
}