using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum AttackState
{
    Idle,
    Windup,
    Active,
    Recovery
}

public class PlayerAttack : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerAttackConfigSO attackData;
    [SerializeField] private GameObject attackVisual;

    [Header("Input")]
    [SerializeField] private InputBuffer attackBuffer = new InputBuffer();

    [Header("Facing (Aim)")]
    [SerializeField] private PlayerFacing playerFacing;
    [SerializeField] private MonoBehaviour aimSourceComponent;   // MonoBehaviour ki inspector'da atansın
    private IAimSource aimSource;

    

    // Cached references
    private PlayerInput playerInput;
    private InputAction attackAction;
    private PlayerMovement playerMovement;
    private AttackChainTracker chainTracker;
    // State
    public AttackState CurrentState { get; private set; } = AttackState.Idle;
    private Coroutine attackRoutine;

    // Bir attack içinde aynı düşmana birden fazla vurmamak için — active fazın başında
    // clear edilir, active fazında bulunan düşmanlar buraya eklenir.
    private readonly HashSet<IDamageable> alreadyHitThisAttack = new HashSet<IDamageable>();

    // Cancel window kontrolü
    private float recoveryStartTime;
    private bool canCancelRecovery;

    // Events — dış sistemler subscribe olur
    public event Action<PlayerAttackConfigSO> OnAttackStarted;
    public event Action<PlayerAttackConfigSO> OnWindupStarted;
    public event Action<PlayerAttackConfigSO> OnActiveStarted;
    public event Action<PlayerAttackConfigSO> OnRecoveryStarted;
    public event Action<IDamageable, HitInfo> OnHitLanded; // her hit için ayrı
    public event Action OnAttackCompleted;
    public event Action OnAttackCancelled;

    // Public queryable state — Movement, Dash bakar
    public bool IsAttacking => CurrentState != AttackState.Idle;
    public bool CanCancel => CurrentState == AttackState.Recovery && canCancelRecovery;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        chainTracker = GetComponent<AttackChainTracker>();
        aimSource = aimSourceComponent as IAimSource;
        if (aimSource == null && aimSourceComponent != null)
            Debug.LogError($"{aimSourceComponent.GetType().Name} IAimSource implement etmiyor!", this);

        if (playerFacing == null) playerFacing = GetComponent<PlayerFacing>();
        if (playerInput != null)
            attackAction = playerInput.actions["BasicAttack"];

        if (attackData == null)
            Debug.LogError("AttackDataSO atanmamış!", this);

        if (attackVisual != null)
            attackVisual.SetActive(false);
    }

    private void Update()
    {
        // Input kaydı — oyuncu bastıysa buffer'a yaz
        if (attackAction != null && attackAction.triggered)
            attackBuffer.RegisterInput();

        // Buffer'da input var ve şu an başlayabilir miyiz? — başlat
        if (attackBuffer.HasBufferedInput && CanStartNewAttack())
        {
            attackBuffer.ConsumeInput();
            StartAttack();
        }
    }

    private void StartAttack()
    {
        if (attackData == null) return;
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackSequence(attackData));
    }

    private bool CanStartNewAttack()
    {
        if (CurrentState == AttackState.Idle) return true;
        // Combo için hazır: if (CurrentState == AttackState.Recovery && canCancelRecovery) return true;
        return false;
    }

    private IEnumerator AttackSequence(PlayerAttackConfigSO data)
    {
        OnAttackStarted?.Invoke(data);

        Vector3 aimDir = GetAimDirection();
        if (aimDir.sqrMagnitude > 0.01f)
        {
            playerFacing.SnapToDirection(aimDir);
            playerFacing.SetOverrideDirection(aimDir);
        }

        // ─── WINDUP ───
        CurrentState = AttackState.Windup;
        OnWindupStarted?.Invoke(data);
        if (attackVisual != null) attackVisual.SetActive(true);
        yield return new WaitForSeconds(data.windupDuration);

        // ─── ACTIVE ───
        CurrentState = AttackState.Active;
        OnActiveStarted?.Invoke(data);
        alreadyHitThisAttack.Clear();

        // Active fazı boyunca her frame hit detection yap.
        // Bu sayede fast-moving enemy active pencereye girerse yakalanır.
        float activeElapsed = 0f;
        while (activeElapsed < data.activeDuration)
        {
            PerformHitDetection(data);
            activeElapsed += Time.deltaTime;
            yield return null;
        }

        // ─── RECOVERY ───
        CurrentState = AttackState.Recovery;
        OnRecoveryStarted?.Invoke(data);
        if (attackVisual != null) attackVisual.SetActive(false);

        recoveryStartTime = Time.time;
        canCancelRecovery = false;

        float recoveryElapsed = 0f;
        while (recoveryElapsed < data.recoveryDuration)
        {
            // Cancel window kontrolü — süre geçtiyse cancel'a izin ver
            if (!canCancelRecovery && recoveryElapsed >= data.cancelWindowStart)
                canCancelRecovery = true;

            recoveryElapsed += Time.deltaTime;
            yield return null;
        }

        // ─── DONE ───
        CurrentState = AttackState.Idle;
        canCancelRecovery = false;

        Vector3 moveDir = playerMovement != null ? playerMovement.CurrentVelocity : Vector3.zero;
        moveDir.y = 0f;
        if (moveDir.sqrMagnitude > 0.01f)
            playerFacing.SnapToDirection(moveDir);

        playerFacing.ClearOverride();

        playerFacing.ClearOverride();
        OnAttackCompleted?.Invoke();
        attackRoutine = null;
    }

    private Vector3 GetAimDirection()
    {
        if (aimSource == null) return transform.forward;
        Vector3 dir = aimSource.GetAimDirection(transform.position);
        return dir.sqrMagnitude > 0.01f ? dir : transform.forward;
    }

    private void PerformHitDetection(PlayerAttackConfigSO data)
    {
        // Attack merkezini hesapla — player'ın forward'una göre offset uygula
        Vector3 attackCenter = transform.position
                              + transform.forward * data.range
                              + transform.rotation * data.offset;

        Collider[] hits = Physics.OverlapSphere(attackCenter, data.radius, data.targetLayer);

        float knockbackMultiplier = chainTracker != null
        ? chainTracker.GetCurrentKnockbackMultiplier()
        : 1f;

        foreach (var col in hits)
        {
            if (!col.TryGetComponent<IDamageable>(out var damageable)) continue;
            //if (!damageable.IsAlive) continue;
            if (alreadyHitThisAttack.Contains(damageable)) continue;

            // Bu hit için özel HitInfo yarat — direction ve position her enemy için farklı
            Vector3 hitPos = col.ClosestPoint(transform.position);

            HitInfo info = HitInfo.FromAttack(
                source: transform.position,
                target: hitPos,
                damage: data.damage,
                knockback: data.knockbackForce * knockbackMultiplier
            );

            damageable.TakeDamage(info);
            alreadyHitThisAttack.Add(damageable);

            OnHitLanded?.Invoke(damageable, info);
        }
    }

    public bool TryCancelAttack()
    {
        if (!CanCancel) return false;

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        CurrentState = AttackState.Idle;
        canCancelRecovery = false;
        if (attackVisual != null) attackVisual.SetActive(false);

        OnAttackCancelled?.Invoke();
        return true;
    }

    public void ForceStopAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        CurrentState = AttackState.Idle;
        canCancelRecovery = false;
        alreadyHitThisAttack.Clear();
        attackBuffer.Clear();

        if (attackVisual != null) attackVisual.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (attackData == null) return;

        Gizmos.color = CurrentState == AttackState.Active ? Color.red : Color.yellow;
        Vector3 center = transform.position
                        + transform.forward * attackData.range
                        + transform.rotation * attackData.offset;
        Gizmos.DrawWireSphere(center, attackData.radius);
    }

}

