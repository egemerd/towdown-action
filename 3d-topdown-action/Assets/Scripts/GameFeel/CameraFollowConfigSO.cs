using UnityEngine;

[CreateAssetMenu(fileName = "CameraFollowConfig", menuName = "Config/Camera Follow Config")]
public class CameraFollowConfigSO : ScriptableObject
{
    [Header("Follow Feel")]
    [Tooltip("Base drift amount toward the player. Keep low for a subtle, non-tracking feel.")]
    [Range(0f, 1f)] public float baseFollowAmount = 0.12f;

    [Tooltip("SmoothDamp time. Higher = lazier, more delayed catch-up.")]
    [Range(0.05f, 2f)] public float smoothTime = 0.45f;

    [Tooltip("Hard cap on how far the camera can drift from its anchor (world units, along camera-right and depth axes).")]
    public Vector2 maxDrift = new Vector2(2.5f, 2f);

    [Header("Deadzone")]
    [Tooltip("Player must exit this radius from the anchor before camera reacts.")]
    public float deadzoneRadius = 0.75f;

    [Header("Depth Influence")]
    [Tooltip("Extra follow strength when player is far from camera (top of screen). 0 = disabled.")]
    [Range(0f, 4f)] public float depthMultiplier = 1.8f;

    [Tooltip("Depth range (near, far) along the camera-forward axis used to normalize the depth factor.")]
    public Vector2 depthRange = new Vector2(-4f, 10f);

    [Tooltip("Curve for depth response. Default linear, but ease-in feels nicer for arcade action.")]
    public AnimationCurve depthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Zoom On Distance")]
    public bool enableZoom = true;

    [Tooltip("FOV/ortho size delta at max depth. Negative = zoom IN as player gets further.")]
    public float zoomAmount = -2.5f;

    [Tooltip("Zoom smoothing time.")]
    [Range(0.05f, 3f)] public float zoomSmoothTime = 0.6f;
}