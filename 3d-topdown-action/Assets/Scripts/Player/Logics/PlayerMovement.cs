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
    
    private PlayerAttack playerAttack;
    private PlayerFacing playerFacing;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        playerAttack = GetComponent<PlayerAttack>();
        playerFacing = GetComponent<PlayerFacing>();
        

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


        wasMovingLastFrame = isMoving;

        if (playerAttack.CurrentState == AttackState.Windup || playerAttack.CurrentState == AttackState.Active)
        {
            // Attack sırasında hareket %30 azalt
            currentVelocity *= config.attackVelocityDecrease;
        }

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

    public void SetVelocity(Vector3 velocity)
    {
        // 'currentVelocity' senin internal field'ının adı ne ise onu set et.
        // Muhtemelen şu an private bir field, `SerializeField` veya direkt public API 
        // ile set edilebilir hale getir.
        currentVelocity = velocity;
        currentVelocity.y = 0f;
    }

    private void UpdateFacingAndLean()
    {
        Vector3 horiz = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        // Facing artık PlayerFacing'in işi — biz sadece hareket yönünü push ederiz
        if (playerFacing != null)
            playerFacing.SetMovementDirection(horiz);

        // Lean — bu hala Movement'ın işi çünkü visual efekt
        float speedRatio = horiz.magnitude / Mathf.Max(0.0001f, config.moveSpeed);
        float targetLean = Mathf.Clamp01(speedRatio) * config.maxLeanAngle;

        currentLean = Mathf.Lerp(currentLean, targetLean,
            1f - Mathf.Exp(-config.leanSmoothing * Time.deltaTime));

        visualRoot.localRotation = Quaternion.Euler(currentLean, 0f, 0f);
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