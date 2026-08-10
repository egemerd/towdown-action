using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Enemy'ye profile-based knockback uygular.
/// Her enemy kendi EnemyKnockbackProfileSO'sunu taþýr — light/heavy/boss enemy
/// farklý karakterde savrulur, ama sistemi ayný.
///
/// HitInfo.knockbackForce = attacker'ýn verdiði base distance,
/// profile.distanceMultiplier ile ölçeklenir.
/// </summary>
public class EnemyKnockback : MonoBehaviour, IKnockable
{
    [Header("Config")]
    [Tooltip("Bu enemy'nin knockback karakterini belirleyen profile. " +
             "Farklý enemy türleri farklý profile referans eder.")]
    [SerializeField] private EnemyKnockbackConfigSO config;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;

    [Header("Combo (bilardo hazýrlýðý)")]
    [Tooltip("Kaç hit sonrasý threshold event fýrlar.")]
    [SerializeField] private int knockbackThreshold = 3;

    // Runtime state
    private Coroutine knockbackRoutine;

    // Public state
    public bool IsKnockedBack => knockbackRoutine != null;
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
    /// force: yön vektörü (magnitude = attacker'ýn base distance'ý)
    /// duration parametresi ignore edilir — profile.duration kullanýlýr.
    /// </summary>
    public void ApplyForce(Vector3 force, float duration)
    {
        if (config == null) return;

        // Attacker'ýn verdiði force'u config ile ölçekle
        Vector3 scaledForce = force * config.distanceMultiplier;

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(KnockbackRoutine(scaledForce));

        HitCount++;
        OnKnockbackApplied?.Invoke(scaledForce);

        if (HitCount >= knockbackThreshold)
            OnThresholdReached?.Invoke();
    }

    private IEnumerator KnockbackRoutine(Vector3 totalOffset)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + totalOffset;

        // Overshoot destekliyorsa hedefi biraz öteye taþý — curve %100'ü geçerse target'ý aþar
        Vector3 overshootTarget = targetPosition;
        if (config.overshootAmount > 0f)
            overshootTarget = startPosition + totalOffset * (1f + config.overshootAmount);

        float elapsed = 0f;
        float duration = config.duration;
        Vector3 previousPosition = startPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveT = config.movementCurve.Evaluate(t);

            // Overshoot varsa curve'ün 1'i geçtiði bölümde overshootTarget'a lerp
            Vector3 currentTarget = config.overshootAmount > 0f && curveT > 1f
                ? overshootTarget
                : targetPosition;

            Vector3 newPosition = Vector3.LerpUnclamped(startPosition, currentTarget, curveT);

            // Wall check
            Vector3 moveDelta = newPosition - previousPosition;
            if (CheckWallHit(previousPosition, moveDelta, out Vector3 hitPoint))
            {
                transform.position = hitPoint;
                OnWallHit?.Invoke(hitPoint);
                break;
            }

            transform.position = newPosition;
            previousPosition = newPosition;

            yield return null;
        }

        // Final position — duvar yoksa hedefe otur
        if (elapsed >= duration)
            transform.position = targetPosition;

        knockbackRoutine = null;
        OnKnockbackEnded?.Invoke();
    }

    private bool CheckWallHit(Vector3 fromPosition, Vector3 delta, out Vector3 hitPoint)
    {
        hitPoint = default;
        if (wallLayer == 0 || delta.sqrMagnitude < 0.0001f) return false;

        float castDistance = delta.magnitude;
        Vector3 castDirection = delta.normalized;

        if (Physics.SphereCast(fromPosition, 0.5f, castDirection,
            out RaycastHit hit, castDistance, wallLayer))
        {
            hitPoint = hit.point;
            return true;
        }

        return false;
    }

    public void ResetHitCount()
    {
        HitCount = 0;
    }

    public void StopKnockback()
    {
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
            OnKnockbackEnded?.Invoke();
        }
    }
}