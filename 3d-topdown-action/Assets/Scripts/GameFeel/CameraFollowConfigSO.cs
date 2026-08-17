using UnityEngine;

[CreateAssetMenu(fileName = "CameraFollowConfig", menuName = "Config/Camera Follow Config")]
public class CameraFollowConfigSO : ScriptableObject
{
    [Header("Deadzone (Target etrafında oluşur)")]
    [Tooltip("Deadzone genişliği (X). Player bu kutunun içindeyken kamera oynamaz.")]
    public float deadzoneWidth = 4f;

    [Tooltip("Deadzone derinliği (Z).")]
    public float deadzoneDepth = 3f;

    [Header("Drift (Player deadzone DIŞINDAYKEN)")]
    [Tooltip("Player deadzone dışına çıktığı mesafenin ne kadarı kameraya aktarılsın. 0 = hiç, 1 = birebir. Minik his için 0.15-0.3 aralığı iyidir.")]
    [Range(0f, 1f)] public float driftAmount = 0.25f;

    [Tooltip("Kameranın rest pozisyonundan uzaklaşabileceği maksimum mesafe (units). Player ne kadar uzağa giderse gitsin kamera bunu aşmaz.")]
    [Range(0f, 10f)] public float maxDriftDistance = 2f;

    [Header("Smoothing")]
    [Tooltip("SmoothDamp süresi. Küçük = snappy, büyük = tembel/sinematik.")]
    [Range(0.05f, 2f)] public float smoothTime = 0.35f;
}