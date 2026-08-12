using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private CameraFollowConfigSO config;

    [Tooltip("Optional. If null, the camera's start position is used as the anchor.")]
    [SerializeField] private Transform anchor;

    private Camera cam;
    private Vector3 anchorPosition;
    private Vector3 currentVelocity;

    // Camera-space axes projected onto the ground plane (XZ).
    private Vector3 depthAxis;   // camera forward, flattened → "up on screen"
    private Vector3 rightAxis;   // camera right,   flattened → "right on screen"

    private float baseFOV;
    private float baseOrthoSize;
    private float currentZoom;
    private float zoomVelocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        anchorPosition = anchor ? anchor.position : transform.position;

        baseFOV = cam.fieldOfView;
        baseOrthoSize = cam.orthographicSize;

        depthAxis = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        rightAxis = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
    }

    private void LateUpdate()
    {
        if (target == null || config == null) return;

        // Player offset from anchor on the ground plane.
        Vector3 offset = target.position - anchorPosition;
        Vector3 flatOffset = Vector3.ProjectOnPlane(offset, Vector3.up);

        // Deadzone: subtract the radius so we don't get a snap when leaving it.
        float mag = flatOffset.magnitude;
        if (mag < config.deadzoneRadius)
            flatOffset = Vector3.zero;
        else
            flatOffset -= flatOffset.normalized * config.deadzoneRadius;

        // Depth factor: 0 near camera, 1 far from camera. Player position along depth axis.
        float depthDot = Vector3.Dot(offset, depthAxis);
        float depthT = Mathf.Clamp01(Mathf.InverseLerp(config.depthRange.x, config.depthRange.y, depthDot));
        float curvedT = config.depthCurve.Evaluate(depthT);
        float depthBoost = 1f + curvedT * config.depthMultiplier;

        // Desired drift in world space.
        Vector3 desiredDrift = flatOffset * config.baseFollowAmount * depthBoost;

        // Clamp along camera-local axes so limits stay intuitive regardless of world rotation.
        float rightComp = Vector3.Dot(desiredDrift, rightAxis);
        float depthComp = Vector3.Dot(desiredDrift, depthAxis);
        rightComp = Mathf.Clamp(rightComp, -config.maxDrift.x, config.maxDrift.x);
        depthComp = Mathf.Clamp(depthComp, -config.maxDrift.y, config.maxDrift.y);
        desiredDrift = rightAxis * rightComp + depthAxis * depthComp;

        Vector3 desiredPosition = anchorPosition + desiredDrift;
        desiredPosition.y = anchorPosition.y; // keep camera height locked to the anchor

        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPosition, ref currentVelocity, config.smoothTime);

        // Zoom based on depth factor (not on drift), so it responds to player's actual distance.
        if (config.enableZoom)
        {
            float targetZoom = curvedT * config.zoomAmount;
            currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, config.zoomSmoothTime);

            if (cam.orthographic)
                cam.orthographicSize = Mathf.Max(0.1f, baseOrthoSize + currentZoom);
            else
                cam.fieldOfView = Mathf.Clamp(baseFOV + currentZoom, 5f, 120f);
        }
    }

    // Optional runtime helpers
    public void SetAnchorToCurrent() => anchorPosition = transform.position;
    public void SetTarget(Transform t) => target = t;

    private void OnDrawGizmosSelected()
    {
        Vector3 a = Application.isPlaying
            ? anchorPosition
            : (anchor ? anchor.position : transform.position);

        if (config == null) return;

        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.9f);
        Gizmos.DrawWireSphere(a, config.deadzoneRadius);

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
        Vector3 r = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        Vector3 f = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3[] pts =
        {
            a + r * config.maxDrift.x + f * config.maxDrift.y,
            a - r * config.maxDrift.x + f * config.maxDrift.y,
            a - r * config.maxDrift.x - f * config.maxDrift.y,
            a + r * config.maxDrift.x - f * config.maxDrift.y
        };
        for (int i = 0; i < 4; i++) Gizmos.DrawLine(pts[i], pts[(i + 1) % 4]);
    }
}