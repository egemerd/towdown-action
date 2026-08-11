using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Dash direction'ın nereden geleceğini belirler. 
/// Flag olarak inspector'da veya runtime'da değiştirilebilir.
/// </summary>
public enum DashDirectionSource
{
    /// <summary>Movement input (WASD) yönüne. Yön yoksa facing.</summary>
    MovementInput,
    /// <summary>Mouse/aim yönüne. Ranged/aimed dash hissi.</summary>
    AimDirection,
    /// <summary>Player'ın şu anki forward'u (facing). Yön hiçbir input istemez.</summary>
    Facing
}

public enum DashState
{
    Idle,
    Dashing,
    Cooldown
}

/// <summary>
/// Player dash — 5+ farklı sistemi orchestrate eder (movement, facing, 
/// invulnerability, squash, trail, attack cancel).
/// 
/// Kendi input'unu dinler ama diğer sistemlere direkt call yapar — event route etmez 
/// çünkü ordering önemli (attack cancel → movement disable → iframe push → dash start 
/// sıralaması deterministic olmalı).
/// </summary>
public class PlayerDash : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerDashConfigSO config;

    [Header("Direction")]
    [Tooltip("Dash yönü nereden alınacak. Runtime'da API ile de değiştirilebilir.")]
    [SerializeField] private DashDirectionSource directionSource = DashDirectionSource.MovementInput;

    [Header("Input")]
    [SerializeField] private InputBuffer dashBuffer = new InputBuffer();

    [Header("References")]
    [SerializeField] private MonoBehaviour aimSourceComponent;
    private IAimSource aimSource;

    // Cached components
    private PlayerInput playerInput;
    private InputAction dashAction;
    private CharacterController characterController;
    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private PlayerFacing playerFacing;
    private PlayerInvulnerability playerInvuln;
    private PlayerSquash playerSquash;

    // State
    public DashState CurrentState { get; private set; } = DashState.Idle;
    private Coroutine dashRoutine;
    private float cooldownEndTime;

    // Invulnerability source key
    private const string INVULN_SOURCE = "dash";

    // Events
    public event Action<Vector3> OnDashStarted;   // parametre: dash direction
    public event Action OnDashEnded;
    public event Action OnDashCancelledByWall;

    // Queryable state
    public bool IsDashing => CurrentState == DashState.Dashing;
    public bool CanDash => CurrentState == DashState.Idle && Time.time >= cooldownEndTime;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
        playerFacing = GetComponent<PlayerFacing>();
        playerInvuln = GetComponent<PlayerInvulnerability>();
        playerSquash = GetComponent<PlayerSquash>();

        aimSource = aimSourceComponent as IAimSource;
        if (aimSource == null && aimSourceComponent != null)
            Debug.LogError($"{aimSourceComponent.GetType().Name} IAimSource implement etmiyor!", this);

        if (playerInput != null)
            dashAction = playerInput.actions["Dash"];  // Input Action asset'inde "Dash" olmalı

        if (config == null)
            Debug.LogError("[PlayerDash] Config atanmamış!", this);
    }

    private void Update()
    {
        // Buffer'a input yaz
        if (dashAction != null && dashAction.triggered)
            dashBuffer.RegisterInput();

        // Buffer'da input var ve dash başlatılabilir mi?
        if (dashBuffer.HasBufferedInput && CanStartDash())
        {
            dashBuffer.ConsumeInput();
            StartDash();
        }
    }

    /// <summary>
    /// Dash başlatılabilir mi? Attack cancel policy'sini burada değerlendiriyoruz.
    /// </summary>
    private bool CanStartDash()
    {
        if (!CanDash) return false;

        // Attack aktifse cancel edilebilir mi bak
        if (playerAttack != null && playerAttack.IsAttacking)
        {
            // CanCancel property'si PlayerAttack tarafından belirleniyor —
            // Windup veya Recovery-after-cancelWindow durumunda true döner.
            if (!playerAttack.CanCancel) return false;
        }

        return true;
    }

    private void StartDash()
    {
        // Attack aktifse cancel et — dash öncelikli
        if (playerAttack != null && playerAttack.IsAttacking)
            playerAttack.TryCancelAttack();

        Vector3 direction = ResolveDashDirection();
        if (direction.sqrMagnitude < 0.0001f)
        {
            // Yön yoksa dash başlatma — sessizce iptal.
            // Alternatif: transform.forward'a fallback yap. Ama input yokken 
            // "yanlışlıkla dash" yaratmak istemiyoruz, sessiz iptal daha güvenli.
            return;
        }

        direction.y = 0f;
        direction.Normalize();

        if (dashRoutine != null) StopCoroutine(dashRoutine);
        dashRoutine = StartCoroutine(DashRoutine(direction));
    }

    private Vector3 ResolveDashDirection()
    {
        switch (directionSource)
        {
            case DashDirectionSource.MovementInput:
                {
                    // PlayerMovement'ın current input direction'ını okumamız lazım.
                    // Şu an PlayerMovement.CurrentVelocity'ı public var — ama velocity 
                    // input değil. Movement'a `CurrentInputDirection` gibi public property 
                    // eklemen lazım. Yoksa velocity'e fallback ederiz (biraz gecikmeli hisseder).
                    Vector3 moveDir = playerMovement != null
                        ? playerMovement.CurrentVelocity
                        : Vector3.zero;
                    moveDir.y = 0f;
                    return moveDir.sqrMagnitude > 0.01f ? moveDir : transform.forward;
                }

            case DashDirectionSource.AimDirection:
                {
                    if (aimSource == null) return transform.forward;
                    Vector3 aimDir = aimSource.GetAimDirection(transform.position);
                    aimDir.y = 0f;
                    return aimDir.sqrMagnitude > 0.01f ? aimDir : transform.forward;
                }

            case DashDirectionSource.Facing:
                return transform.forward;

            default:
                return transform.forward;
        }
    }

    /// <summary>
    /// Direction source'u runtime'da değiştir. Powerup, mode toggle vs. için.
    /// </summary>
    public void SetDirectionSource(DashDirectionSource source)
    {
        directionSource = source;
    }

    private IEnumerator DashRoutine(Vector3 direction)
    {
        CurrentState = DashState.Dashing;
        OnDashStarted?.Invoke(direction);

        if (playerMovement != null) playerMovement.enabled = false;

        if (playerFacing != null)
        {
            playerFacing.SnapToDirection(direction);
            playerFacing.SetOverrideDirection(direction);
        }

        if (playerInvuln != null)
            playerInvuln.AddSource(INVULN_SOURCE, config.iframeDuration);

        if (playerSquash != null && config.squashProfile != null)
            //playerSquash.TriggerSquash(direction, config.squashProfile);

        SpawnTrail();

        // === Wall detection (öncekiyle aynı) ===
        Vector3 startPosition = transform.position;
        float actualDistance = ResolveDashDistance(startPosition, direction);
        bool hitWall = actualDistance < config.distance - 0.01f;

        // === Velocity-driven integration ===
        float effectiveDuration = config.GetEffectiveDuration();
        float initialSpeed = config.GetInitialSpeed(effectiveDuration);
        float endSpeed = config.endSpeed;

        // Duvara çarpma durumunda süreyi ve initial speed'i orantılı azalt
        // (aksi halde kısa mesafeye tam initial speed ile fırlar, bu yanlış)
        if (hitWall)
        {
            float ratio = actualDistance / config.distance;
            effectiveDuration *= ratio;
            // initialSpeed olduğu gibi kalır — kısa mesafede hızlıca decay eder
        }

        float elapsed = 0f;
        while (elapsed < effectiveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / effectiveDuration);

            // Power ease-out ile speed decay: initialSpeed → endSpeed
            // weight: t=0'da 1 (full initial), t=1'de 0 (full end)
            float weight = Mathf.Pow(1f - t, config.easePower);
            float currentSpeed = Mathf.Lerp(endSpeed, initialSpeed, weight);

            // Her frame velocity * dt = position delta
            Vector3 delta = direction * currentSpeed * Time.deltaTime;

            if (characterController != null && characterController.enabled)
                characterController.Move(delta);
            else
                transform.position += delta;

            yield return null;
        }

        // === HANDOFF — kritik satır ===
        // PlayerMovement uyanmadan ÖNCE velocity'i set et.
        // Bu sayede PlayerMovement Update'e girdiğinde currentVelocity = dash yönünde endSpeed
        // ve input target'ına doğru smooth lerp başlar. Snap yok.
        if (playerMovement != null)
        {
            playerMovement.SetVelocity(direction * endSpeed);
            playerMovement.enabled = true;
        }

        if (playerFacing != null) playerFacing.ClearOverride();

        if (hitWall) OnDashCancelledByWall?.Invoke();

        CurrentState = DashState.Cooldown;
        cooldownEndTime = Time.time + config.cooldown;

        yield return new WaitForSeconds(config.cooldown);
        CurrentState = DashState.Idle;

        dashRoutine = null;
        OnDashEnded?.Invoke();
    }

    /// <summary>
    /// Ken Perlin'in "improved smoothstep" tekniği — normal SmoothStep'i kendi 
    /// üstüne tekrar uygulayarak ease bölgesini genişletir. 
    /// iterations=1: standart smoothstep (3t² - 2t³), zaten iki ucunda velocity=0.
    /// iterations=2-3: daha "kademeli" başlangıç/bitiş, ortada daha belirgin hız farkı.
    /// Curve asset'i gerekmez — tek int parametre ile tune edilir.
    /// </summary>


    private static float PowerEaseOut(float t, float power)
    {
        return 1f - Mathf.Pow(1f - t, power);
    }

    /// <summary>
    /// Dash target'ına doğru wall check yapıp gerçek erişilebilir mesafeyi döndürür.
    /// Duvar varsa erken sonlanır (hit noktasında dur).
    /// </summary>
    private float ResolveDashDistance(Vector3 origin, Vector3 direction)
    {
        if (config.wallLayer == 0) return config.distance;

        // Player yüksekliği için origin'i biraz yukarı offset et — 
        // ground kenarlarını duvar sanma ihtimaline karşı
        Vector3 castOrigin = origin + Vector3.up * 0.5f;

        if (Physics.SphereCast(castOrigin, config.wallCastRadius, direction,
            out RaycastHit hit, config.distance, config.wallLayer))
        {
            // Duvar önünde küçük bir buffer bırak — content içine girme
            return Mathf.Max(0f, hit.distance - config.wallCastRadius);
        }

        return config.distance;
    }

    private void SpawnTrail()
    {
        if (config.trailPrefab == null) return;

        Vector3 spawnPos = transform.position + transform.rotation * config.trailSpawnOffset;
        GameObject trail = Instantiate(config.trailPrefab, spawnPos, transform.rotation);
        Destroy(trail, config.trailLifetime);
    }

    /// <summary>
    /// Dash'i zorla durdur. Death, cutscene, pause vs. senaryoları.
    /// </summary>
    public void ForceStopDash()
    {
        if (dashRoutine != null)
        {
            StopCoroutine(dashRoutine);
            dashRoutine = null;
        }

        if (playerInvuln != null) playerInvuln.RemoveSource(INVULN_SOURCE);
        if (playerFacing != null) playerFacing.ClearOverride();
        if (playerMovement != null) playerMovement.enabled = true;

        CurrentState = DashState.Idle;
        cooldownEndTime = 0f;
    }
}