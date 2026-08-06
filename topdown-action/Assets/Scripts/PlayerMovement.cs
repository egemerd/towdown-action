using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerMoveConfigSO config;

    [Header("References")]
    [SerializeField] private Transform enemy;
    [Tooltip("Squash/tilt için ölçeklenecek/döndürülecek visual child. Boş bırakırsan bu transform kullanılır.")]
    [SerializeField] private Transform visualRoot;

    // Cached components
    private PlayerInput playerInput;
    private CharacterController characterController;

    // Input
    private Vector2 moveInput;

    // Movement state — MoveState bunlara erişebilsin diye public getter'lar
    private Vector3 currentVelocity;   // XZ düzleminde smoothed velocity
    private float verticalVelocity;    // gravity için
    private Vector3 baseScale;
    private float currentLean;         // derece cinsinden

    // Was moving last frame? (start/stop trigger'ları için)
    private bool wasMovingLastFrame;

    // Squash coroutine handle
    private Coroutine squashRoutine;

    // State machine
    private IState currentState;

    // Public accessors (state'lerin ihtiyacı olabilir)
    public Vector2 MoveInput => moveInput;
    public Vector3 CurrentVelocity => currentVelocity;
    public PlayerMoveConfigSO Config => config;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        if (visualRoot == null) visualRoot = transform;
        baseScale = visualRoot.localScale;
    }

    private void Start()
    {
        ChangeState(new IdleState());
    }

    private void Update()
    {
        moveInput = playerInput.actions["Movement"].ReadValue<Vector2>();
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        // Start/stop kenar tetikleri — squash pulse'ları burada tetiklenir
        if (isMoving && !wasMovingLastFrame)
            TriggerSquash(config.startStretch, config.startStretchDuration, config.startStretchCurve);
        else if (!isMoving && wasMovingLastFrame)
            TriggerSquash(config.stopSquash, config.stopSquashDuration, config.stopSquashCurve);

        wasMovingLastFrame = isMoving;

        // State transition
        if (isMoving) ChangeState(new MoveState());
        else ChangeState(new IdleState());

        currentState.UpdateState(this);

        // Bu sürekli çalışmalı — state'ten bağımsız
        ApplyGravityAndMove();
        UpdateFacingAndLean();
    }

    /// <summary>
    /// MoveState bunu çağırır. Sadece target velocity'yi acceleration ile smoothly hedefine yaklaştırır.
    /// Asıl CharacterController.Move çağrısı ApplyGravityAndMove'da tek noktadan yapılır.
    /// </summary>
    public void UpdateHorizontalVelocity()
    {
        Vector3 target = new Vector3(moveInput.x, 0f, moveInput.y);
        if (target.sqrMagnitude > 1f) target.Normalize(); // diagonal clamp
        target *= config.moveSpeed;

        // Hedef sıfır değilse hızlanıyor, sıfırsa yavaşlıyor
        float smoothTime = target.sqrMagnitude > 0.01f
            ? config.accelerationTime
            : config.decelerationTime;

        // SmoothDamp yerine basit lerp — daha predictable
        float t = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.0001f, smoothTime));
        currentVelocity = Vector3.Lerp(currentVelocity, target, t);
    }

    /// <summary>
    /// IdleState çağırır. Hedef sıfır, deceleration ile durur.
    /// </summary>
    public void DecelerateToStop()
    {
        float t = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.0001f, config.decelerationTime));
        currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, t);
    }

    private void ApplyGravityAndMove()
    {
        // Gravity
        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // grounded'ken hafif negatif → yerde kalır
        else
            verticalVelocity += config.gravity * Time.deltaTime;

        Vector3 delta = (currentVelocity + Vector3.up * verticalVelocity) * Time.deltaTime;
        characterController.Move(delta);
    }

    private void UpdateFacingAndLean()
    {
        Vector3 horiz = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        // Facing rotation — sadece hareket varken döner
        if (horiz.sqrMagnitude > 0.05f)
        {
            Quaternion targetRot = Quaternion.LookRotation(horiz.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot,
                1f - Mathf.Exp(-config.turnSpeed * Time.deltaTime));
        }

        // Lean — hızın oranına göre öne doğru eğilir
        float speedRatio = horiz.magnitude / Mathf.Max(0.0001f, config.moveSpeed);
        float targetLean = Mathf.Clamp01(speedRatio) * config.maxLeanAngle;

        currentLean = Mathf.Lerp(currentLean, targetLean,
            1f - Mathf.Exp(-config.leanSmoothing * Time.deltaTime));

        // Lean'i visual root'un LOCAL X rotation'ı olarak uygula.
        // (transform'un kendisi zaten hareket yönüne bakıyor, X ekseni öne doğru = forward tilt)
        visualRoot.localRotation = Quaternion.Euler(currentLean, 0f, 0f);
    }

    private void TriggerSquash(Vector3 squashScale, float duration, AnimationCurve curve)
    {
        if (squashRoutine != null) StopCoroutine(squashRoutine);
        squashRoutine = StartCoroutine(SquashRoutine(squashScale, duration, curve));
    }

    private System.Collections.IEnumerator SquashRoutine(Vector3 squashScale, float duration, AnimationCurve curve)
    {
        Vector3 targetScale = Vector3.Scale(baseScale, squashScale);
        float half = duration * 0.5f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / half;
            visualRoot.localScale = Vector3.Lerp(baseScale, targetScale, curve.Evaluate(Mathf.Clamp01(t)));
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / half;
            visualRoot.localScale = Vector3.Lerp(targetScale, baseScale, curve.Evaluate(Mathf.Clamp01(t)));
            yield return null;
        }
        visualRoot.localScale = baseScale;
        squashRoutine = null;
    }

    private void ChangeState(IState newState)
    {
        // Aynı state'e tekrar geçme — GC ve gereksiz Enter/Exit yaratır
        if (currentState != null && currentState.GetType() == newState.GetType()) return;

        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}