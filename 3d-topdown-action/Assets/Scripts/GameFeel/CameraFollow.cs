using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Deadzone'un merkezi. Kameranın 'rest' pozisyonu bunun etrafında sabitlenir.")]
    [SerializeField] private Transform target;

    [Tooltip("Takip edilecek karakter. Deadzone testi bunun pozisyonuna göre yapılır.")]
    [SerializeField] private Transform player;

    [SerializeField] private CameraFollowConfigSO config;

    // Runtime
    private Vector3 restOffset;   // Startta kamera target'a göre neredeyse orası "rest"
    private Vector3 velocity;     // SmoothDamp state

    private void Start()
    {
        if (target == null) return;
        // Kameranın sahnedeki başlangıç konumunu target'a göre kilitle.
        // Deadzone içindeyken kamera hep buraya döner.
        restOffset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null || player == null || config == null) return;

        // 1) Player'ın target'a göre yerel konumu
        Vector3 local = player.position - target.position;
        float halfW = config.deadzoneWidth * 0.5f;
        float halfD = config.deadzoneDepth * 0.5f;

        // 2) Deadzone DIŞINA taşan miktarı bul. İçindeyse 0.
        //    Örn: local.x = 5, halfW = 2 → excessX = +3
        //         local.x = 1, halfW = 2 → excessX =  0
        float excessX = Mathf.Max(0f, Mathf.Abs(local.x) - halfW) * Mathf.Sign(local.x);
        float excessZ = Mathf.Max(0f, Mathf.Abs(local.z) - halfD) * Mathf.Sign(local.z);
        Vector3 excess = new Vector3(excessX, 0f, excessZ);

        // 3) Drift = excess * multiplier, sonra max mesafeye clamp
        Vector3 drift = excess * config.driftAmount;
        drift = Vector3.ClampMagnitude(drift, config.maxDriftDistance);

        // 4) Hedef pozisyon = rest + drift (Y kilitli)
        Vector3 restPosition = target.position + restOffset;
        Vector3 desired = new Vector3(
            restPosition.x + drift.x,
            restPosition.y,   // Y sabit
            restPosition.z + drift.z
        );

        // 5) SmoothDamp — deadzone'a girince drift = 0 olur, kamera restPosition'a döner
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref velocity,
            config.smoothTime
        );
    }

    private void OnDrawGizmos()
    {
        if (target == null || config == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Vector3 center = target.position;
        Vector3 size = new Vector3(config.deadzoneWidth, 0.1f, config.deadzoneDepth);
        Gizmos.DrawCube(center, size);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }
}