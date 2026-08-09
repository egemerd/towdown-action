using UnityEngine;

/// <summary>
/// Karakterin dönüş (facing) davranışını merkezi olarak yönetir.
///
/// İki mod var:
///   - Movement: karakter velocity yönüne smooth döner
///   - Override: dışarıdan verilen bir yöne anlık döner (attack, dash vs.)
///
/// Attack başlangıcında SnapToDirection çağrılır → karakter anında mouse yönüne döner.
/// Attack bitince ClearOverride ile movement facing'e geri dönülür.
///
/// Bu component olmadan Movement kendi rotation'unu kontrol ederdi.
/// Şimdi Movement ona "hareket yönü şu" der, Facing decide eder.
/// </summary>
public class PlayerFacing : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Movement yönüne dönme hızı (smooth lerp). Yüksek = anlık, düşük = smooth.")]
    [SerializeField] private float movementTurnSpeed = 15f;

    [Tooltip("Yön hesabında minimum vektör büyüklüğü. Bu değerin altında rotation güncellenmez.")]
    [SerializeField] private float minDirectionMagnitude = 0.05f;

    // Movement'ın verdiği "olması gereken" yön (velocity based)
    private Vector3 movementDirection;

    // Override — attack/dash gibi geçici zorunlu yönler için
    private bool isOverriding;
    private Vector3 overrideDirection;

    /// <summary>
    /// Movement her frame'de kendi hareket yönünü buraya push eder.
    /// Override aktif değilse bu yöne smooth dönülür.
    /// </summary>
    public void SetMovementDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude > minDirectionMagnitude * minDirectionMagnitude)
            movementDirection = direction.normalized;
    }

    /// <summary>
    /// Karakter'i anlık olarak belirtilen yöne çevir (snap).
    /// Attack, dash gibi eylemlerde çağrılır — smooth dönüş bekletmez.
    /// </summary>
    public void SnapToDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < minDirectionMagnitude * minDirectionMagnitude) return;

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    /// <summary>
    /// Karakter'i bir süre boyunca belirtilen yöne kilitle.
    /// Attack sırasında oyuncu strafe yaparsa karakter aim yönünde kalır.
    /// Bitince ClearOverride çağırılmalı.
    /// </summary>
    public void SetOverrideDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < minDirectionMagnitude * minDirectionMagnitude) return;

        overrideDirection = direction.normalized;
        isOverriding = true;
    }

    public void ClearOverride()
    {
        isOverriding = false;
    }

    private void LateUpdate()
    {
        // Update'te SetMovementDirection çağrıldıktan sonra LateUpdate'te
        // rotation uygulanır. Bu sırayla movement bir frame öne geçmiş olmaz.

        Vector3 targetDir;

        if (isOverriding)
        {
            targetDir = overrideDirection;
        }
        else
        {
            targetDir = movementDirection;
            if (targetDir.sqrMagnitude < 0.001f) return; // movement yoksa dönme
        }

        Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            1f - Mathf.Exp(-movementTurnSpeed * Time.deltaTime));
    }
}