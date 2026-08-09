using UnityEngine;

/// <summary>
/// Enemy'nin VFX olaylarını yönetir. EnemyHealth ve EnemyKnockback event'lerine
/// subscribe olur, VFXProfile'daki prefab'ları spawn eder.
///
/// Profile enemy başına Inspector'dan atanır. Aynı profile birden fazla enemy
/// tarafından paylaşılabilir (SO instance shared).
/// </summary>
public class EnemyVFXHandler : MonoBehaviour
{
    [Header("config")]
    [Tooltip("Bu enemy'nin VFX preset'i. Boşsa VFX spawn edilmez.")]
    [SerializeField] private VFXConfigSO config;


    private EnemyHealth health;
    private EnemyKnockback knockback;

    [Header("Idle VFX (opsiyonel)")]
    [Tooltip("Idle VFX'in enemy'ye attach olacağı transform. Boşsa enemy transform'u kullanılır.")]
    [SerializeField] private Transform idleVFXAnchor;

    // Runtime state
    private GameObject idleVFXInstance;
    private GameObject knockbackTrailInstance;

    private void Awake()
    {
        if (health == null) health = GetComponent<EnemyHealth>();
        if (knockback == null) knockback = GetComponent<EnemyKnockback>();
        if (idleVFXAnchor == null) idleVFXAnchor = transform;
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamageTaken += HandleDamageTaken;
            health.OnDied += HandleDied;
        }

        if (knockback != null)
        {
            knockback.OnKnockbackApplied += HandleKnockbackStart;
            knockback.OnKnockbackEnded += HandleKnockbackEnd;
        }

        SpawnIdleVFX();
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamageTaken -= HandleDamageTaken;
            health.OnDied -= HandleDied;
        }

        if (knockback != null)
        {
            knockback.OnKnockbackApplied -= HandleKnockbackStart;
            knockback.OnKnockbackEnded -= HandleKnockbackEnd;
        }

        CleanupIdleVFX();
    }

    // ─── HIT VFX ───
    private void HandleDamageTaken(HitInfo info)
    {
        if (config == null || config.hitVFXPrefab == null) return;

        Quaternion rotation = config.alignHitVFXToDirection
            ? Quaternion.LookRotation(info.hitDirection, Vector3.up)
            : Quaternion.identity;

        SpawnVFX(config.hitVFXPrefab, info.hitPosition, rotation, config.hitVFXLifetime);
    }

    // ─── DEATH VFX ───
    private void HandleDied()
    {
        if (config == null || config.deathVFXPrefab == null) return;

        SpawnVFX(config.deathVFXPrefab, transform.position, Quaternion.identity, config.deathVFXLifetime);
        CleanupIdleVFX();
    }

    // ─── KNOCKBACK TRAIL ───
    private void HandleKnockbackStart(Vector3 force)
    {
        if (config == null || config.knockbackTrailPrefab == null) return;

        // Trail enemy'ye attach edilir — pozisyonu takip etsin
        knockbackTrailInstance = Instantiate(config.knockbackTrailPrefab, transform.position,
                                              Quaternion.identity, transform);
        knockbackTrailInstance.transform.localPosition = Vector3.zero;
    }

    private void HandleKnockbackEnd()
    {
        if (knockbackTrailInstance != null)
        {
            // Trail'in kendi fade'i olabilir — parent'tan detach edip sahnede bırak,
            // TrailRenderer kendi ömrünü tamamlar
            knockbackTrailInstance.transform.SetParent(null);
            Destroy(knockbackTrailInstance, 1f);
            knockbackTrailInstance = null;
        }
    }

    // ─── IDLE VFX ───
    private void SpawnIdleVFX()
    {
        if (config == null || config.idleVFXPrefab == null) return;

        idleVFXInstance = Instantiate(config.idleVFXPrefab, idleVFXAnchor.position,
                                       idleVFXAnchor.rotation, idleVFXAnchor);
    }

    private void CleanupIdleVFX()
    {
        if (idleVFXInstance != null)
        {
            Destroy(idleVFXInstance);
            idleVFXInstance = null;
        }
    }

    // ─── VFX SPAWN HELPER ───
    /// <summary>
    /// VFX prefab spawn eder ve süre sonra destroy eder.
    /// İleride pool'a geçmek istersen bu tek method'u güncellemen yeter.
    /// </summary>
    private void SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, float lifetime)
    {
        GameObject instance = Instantiate(prefab, position, rotation);
        Destroy(instance, lifetime);

        // İleride:
        // GameObject instance = VFXPool.Instance.Spawn(prefab, position, rotation);
        // VFXPool.Instance.Return(instance, lifetime);
    }
}