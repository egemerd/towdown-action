using System.Collections;
using UnityEngine;

/// <summary>
/// Kameraya spring-based shake uygular. Position offset, tilt (Z rotation)
/// ve zoom (FOV veya orthographic size) parametrelerini smooth þekilde animate eder.
///
/// Direction parametresi ile shake'in yönü kontrol edilebilir (sað/sol).
/// </summary>
public class CameraShaker : MonoBehaviour
{
    [Header("Target Camera")]
    [SerializeField] private Camera targetCamera;
    [Tooltip("Shake uygulanacak transform. Boþsa targetCamera'nýn transform'u kullanýlýr.")]
    [SerializeField] private Transform shakeTransform;

    // Cached original values
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private float originalFOV;
    private float originalOrthoSize;
    private bool isOrthographic;

    // Runtime — spring state
    private Vector3 currentPositionOffset;
    private Vector3 positionVelocity;
    private float currentTiltAngle;
    private float tiltVelocity;
    private float currentZoomOffset;
    private float zoomVelocity;

    // Active shake
    private Coroutine activeShakeRoutine;
    private CameraShakeProfileSO activeProfile;
    private float directionMultiplier = 1f;
    private float shakeIntensity = 1f;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (shakeTransform == null && targetCamera != null) shakeTransform = targetCamera.transform;

        if (shakeTransform != null)
        {
            originalLocalPosition = shakeTransform.localPosition;
            originalLocalRotation = shakeTransform.localRotation;
        }

        if (targetCamera != null)
        {
            isOrthographic = targetCamera.orthographic;
            originalFOV = targetCamera.fieldOfView;
            originalOrthoSize = targetCamera.orthographicSize;
        }
    }

    private void LateUpdate()
    {
        if (activeProfile == null)
        {
            // No active shake — return to rest
            ApplyRestState();
            return;
        }

        // Spring physics — her frame update
        UpdateSpring(ref currentPositionOffset, ref positionVelocity, Vector3.zero,
                     activeProfile.springStiffness, activeProfile.springDamping);
        UpdateSpring(ref currentTiltAngle, ref tiltVelocity, 0f,
                     activeProfile.springStiffness, activeProfile.springDamping);
        UpdateSpring(ref currentZoomOffset, ref zoomVelocity, 0f,
                     activeProfile.springStiffness, activeProfile.springDamping);

        // Apply to camera
        ApplyShake();
    }

    /// <summary>
    /// Shake tetikle. Direction multiplier +1 (sað), -1 (sol), 0 (nötr).
    /// Intensity 0-1 arasý, profile deðerlerinin çarpaný.
    /// </summary>
    public void Shake(CameraShakeProfileSO profile, float directionMultiplier = 1f, float intensity = 1f)
    {
        if (profile == null) return;

        activeProfile = profile;
        this.directionMultiplier = directionMultiplier;
        this.shakeIntensity = intensity;

        // Spring'e "kick" ver — initial impulse
        Vector3 kickDirection = new Vector3(directionMultiplier, Random.Range(-0.3f, 0.3f), 0f).normalized;
        positionVelocity += kickDirection * profile.positionMagnitude * intensity * 20f;
        tiltVelocity += profile.tiltAngle * directionMultiplier * intensity * 20f;
        zoomVelocity -= profile.zoomAmount * intensity * 20f; // negative = zoom IN

        if (activeShakeRoutine != null) StopCoroutine(activeShakeRoutine);
        activeShakeRoutine = StartCoroutine(ShakeLifecycle(profile.duration));
    }

    private IEnumerator ShakeLifecycle(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        activeProfile = null;
        activeShakeRoutine = null;
    }

    /// <summary>
    /// Damped spring — mevcut deðer target'a spring physics ile döner.
    /// Bu yöntem oscillation ve smooth settle saðlar.
    /// </summary>
    private void UpdateSpring(ref float current, ref float velocity, float target,
                              float stiffness, float damping)
    {
        float displacement = current - target;
        float springForce = -stiffness * displacement;
        float dampingForce = -damping * velocity;
        float acceleration = springForce + dampingForce;

        velocity += acceleration * Time.deltaTime;
        current += velocity * Time.deltaTime;
    }

    private void UpdateSpring(ref Vector3 current, ref Vector3 velocity, Vector3 target,
                              float stiffness, float damping)
    {
        Vector3 displacement = current - target;
        Vector3 springForce = -stiffness * displacement;
        Vector3 dampingForce = -damping * velocity;
        Vector3 acceleration = springForce + dampingForce;

        velocity += acceleration * Time.deltaTime;
        current += velocity * Time.deltaTime;
    }

    private void ApplyShake()
    {
        // Position
        shakeTransform.localPosition = originalLocalPosition + currentPositionOffset;

        // Tilt (Z rotation)
        shakeTransform.localRotation = originalLocalRotation * Quaternion.Euler(0f, 0f, currentTiltAngle);

        // Zoom
        if (isOrthographic)
            targetCamera.orthographicSize = originalOrthoSize + currentZoomOffset;
        else
            targetCamera.fieldOfView = originalFOV + currentZoomOffset;
    }

    private void ApplyRestState()
    {
        // Rest state'e smoothly dön — spring hala kalaný absorb ediyorsa devam et
        if (currentPositionOffset.sqrMagnitude > 0.0001f ||
            Mathf.Abs(currentTiltAngle) > 0.01f ||
            Mathf.Abs(currentZoomOffset) > 0.01f)
        {
            // Küçük deðerlerde bile spring'i decay ettir
            currentPositionOffset = Vector3.Lerp(currentPositionOffset, Vector3.zero, 10f * Time.deltaTime);
            currentTiltAngle = Mathf.Lerp(currentTiltAngle, 0f, 10f * Time.deltaTime);
            currentZoomOffset = Mathf.Lerp(currentZoomOffset, 0f, 10f * Time.deltaTime);
            ApplyShake();
        }
    }

    /// <summary>
    /// Aktif shake'i durdur, kamerayý reset et.
    /// </summary>
    public void StopShake()
    {
        if (activeShakeRoutine != null) StopCoroutine(activeShakeRoutine);
        activeProfile = null;
        currentPositionOffset = Vector3.zero;
        positionVelocity = Vector3.zero;
        currentTiltAngle = 0f;
        tiltVelocity = 0f;
        currentZoomOffset = 0f;
        zoomVelocity = 0f;

        if (shakeTransform != null)
        {
            shakeTransform.localPosition = originalLocalPosition;
            shakeTransform.localRotation = originalLocalRotation;
        }
    }
}