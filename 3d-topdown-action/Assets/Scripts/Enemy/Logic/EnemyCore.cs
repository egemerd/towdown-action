using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    [Header("Feedback")]
    [Tooltip("Default hit feedback profile. Attack'in kendi profile'ý override edebilir.")]
    [SerializeField] private HitFeedbackProfileSO defaultHitFeedback;

    [Header("Knockback")]
    [Tooltip("Knockback süresi. HitInfo.knockbackForce ile birlikte uygulanýr.")]
    [SerializeField] private float knockbackDuration = 0.2f;

    EnemyHealth enemyHealth;
    EnemyMovement enemyMovement;
    EnemyKnockback enemyKnockback;
    EnemyHitFlash hitFlash;
    EnemySquash enemySquash;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyMovement = GetComponent<EnemyMovement>();
        enemyKnockback = GetComponent<EnemyKnockback>();
        hitFlash = GetComponent<EnemyHitFlash>();
        enemyKnockback = GetComponent<EnemyKnockback>();
        enemySquash = GetComponent<EnemySquash>();
    }

    private void OnEnable()
    {
        enemyHealth.OnDied += HandleDeath;
        enemyHealth.OnDamageTaken += HandleDamageTaken; 
    }

    private void OnDisable()
    {
        enemyHealth.OnDied -= HandleDeath;  
        enemyHealth.OnDamageTaken -= HandleDamageTaken;
    }

    private void HandleDamageTaken(HitInfo info)
    {
        // 1. Knockback uygula
        if (enemyKnockback != null && info.knockbackForce > 0f)
        {
            Vector3 force = info.hitDirection * info.knockbackForce;
            force.y = 0f; // top-down: sadece XZ
            enemyKnockback.ApplyForce(force, info.knockbackConfig);
            //Debug.Log($"Applied knockback with force {force} and config {info.knockbackConfig}");
        }

        // 2. Hit flash tetikle
        if (hitFlash != null)
            hitFlash.Flash(defaultHitFeedback);

        if(enemySquash != null)
            enemySquash.TriggerSquash(info.hitDirection);
        
        // 3. Global feel efektleri (timestop + shake)
        if (GameFeelController.Instance != null && defaultHitFeedback != null)
            GameFeelController.Instance.TriggerHitFeedback(defaultHitFeedback);
    }

    void HandleDeath()
    {
        Destroy(gameObject, 0.5f);
    }

}
