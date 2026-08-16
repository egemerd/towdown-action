using DG.Tweening;
using System.Collections;
using UnityEngine;
public enum EnemyDashState
{
    Idle,
    Preparing,
    Dashing,
    Cooldown,
    KnockedBack, //bu knockback cancel knockbacki 
}
public class EnemyDashAttack : MonoBehaviour
{
    [Header("Dash Attack Config")]
    [SerializeField] private EnemyDashAttackConfigSO config;
    [SerializeField] private Renderer enemyRenderer; // rengi deðiþtireceðimiz renderer (Inspector'dan ata)

    [SerializeField] private Transform shakeRoot;
    EnemyHealth enemyHealth;
    bool isDashing = false;
    Coroutine dashCoroutine;
    Vector3 target;
    Vector3 enemyDirection;
    EnemyDashState currentState;

    // Reset için orijinal deðerleri saklýyoruz
    Vector3 originalScale;
    Color originalColor;
    MaterialPropertyBlock mpb;
    static readonly int MainColor = Shader.PropertyToID("_MainColor");
    EnemyKnockback enemyKnockback;
    public bool CanBounceAttackInDash => currentState == EnemyDashState.Dashing;
    public bool IsDashing => isDashing;


    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyKnockback = GetComponent<EnemyKnockback>();
    }
    private void Start()
    {
        currentState = EnemyDashState.Idle;
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        enemyHealth.OnDamageTaken += CancelDash;
        enemyKnockback.OnKnockbackEnded += HandleKnockbackEnd;
    }

    private void OnDisable()
    {
        enemyHealth.OnDamageTaken -= CancelDash;
        enemyKnockback.OnKnockbackEnded -= HandleKnockbackEnd;
    }
    private void Update()
    {
        if (currentState != EnemyDashState.Idle)
            return;
        if (DetectPlayer())
        {
            Debug.Log("Player detected, starting dash");
            StartDash();
        }
    }

    void StartDash()
    {
        if(isDashing)
        {
            return;
        }
        dashCoroutine = StartCoroutine(DashCoroutine());    
    }

    void HandleKnockbackEnd()
    {
        if (currentState == EnemyDashState.KnockedBack)
        {
            currentState = EnemyDashState.Idle;
        }
    }
    private IEnumerator DashCoroutine()
    {   
        Debug.Log("DashCoroutine started");
        isDashing = true;
        // === PREPARING: shake + scale down + grileþ ===
        currentState = EnemyDashState.Preparing;

        // Shake ve scale down ayný anda baþlasýn
        
        shakeRoot.DOShakeScale(config.prepareDuration, config.shakeStrength, config.shakeVibrato, config.shakeRandomness);
        transform.DOScale(config.prepareScale, config.prepareDuration).SetEase(Ease.OutQuad);

        // Renk grileþme
        //SetColor(config.prepareColor, config.prepareDuration);
        

        yield return new WaitForSeconds(config.prepareDuration);

        // === DASHING: hedef yönünü bir kere hesapla, smooth curve ile git ===
        currentState = EnemyDashState.Dashing;

        

        Vector3 dashDirection = (target - transform.position).normalized;
        enemyDirection = dashDirection; // diðer sistemler için yönü sakla
        Vector3 startPos = transform.position;
        float dashSlowSpeed = ((target - transform.position).magnitude + config.dashSlowOffset) / config.dashDuration;
        float dashPos = (target - transform.position).magnitude < config.dashDetectionRadius ? dashSlowSpeed : config.dashSpeed; ;
        Vector3 endPos = startPos + dashDirection * dashPos * config.dashDuration;

        

        transform.DOScale(originalScale, config.resetDuration).SetEase(Ease.OutBack);

        bool hasHitTarget = false;
        float elapsedTime = 0f;
        while (elapsedTime < config.dashDuration)
        {
            float t = elapsedTime / config.dashDuration;
            float curvedT = config.dashCurve.Evaluate(t); // AnimationCurve ile smooth kontrol
            transform.position = Vector3.Lerp(startPos, endPos, curvedT);
            if (!hasHitTarget && TryDashAttackHit())
                hasHitTarget = true;             
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos; // son pozisyonu garantile

        // === COOLDOWN: scale ve renk normale dönsün ===
        currentState = EnemyDashState.Cooldown;

        
        //SetColor(originalColor, config.resetDuration);

        yield return new WaitForSeconds(config.dashCooldown);

        currentState = EnemyDashState.Idle;
        isDashing = false;
    }

    // Renk deðiþimini MaterialPropertyBlock üzerinden DOTween ile lerp'liyoruz
    // (senin GPU instancing / MaterialPropertyBlock felsefene uygun)
    void SetColor(Color targetColor, float duration)
    {
        if (enemyRenderer == null) return;

        Color startColor = enemyRenderer.sharedMaterial.GetColor(MainColor);
        enemyRenderer.GetPropertyBlock(mpb);
        Color currentColor = mpb.GetColor(MainColor);
        if (currentColor == default) currentColor = startColor;

        DOTween.To(
            () => currentColor,
            c =>
            {
                currentColor = c;
                enemyRenderer.GetPropertyBlock(mpb);
                mpb.SetColor(MainColor, c);
                enemyRenderer.SetPropertyBlock(mpb);
            },
            targetColor,
            duration
        ).SetId(this); // CancelDash'te DOKill(this) ile durdurulabilsin
    }

    bool DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, config.dashDetectionRadius, config.playerLayerMask);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                target = hit.transform.position;
                return true;
            }
        }
        return false;
    }

    bool TryDashAttackHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + enemyDirection * config.dashRangeOffset, config.dashRange, config.playerLayerMask);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    HitInfo info = HitInfo.FromAttack(
                        source: transform.position,
                        target: hit.ClosestPoint(transform.position),
                        damage: config.dashDamage,
                        knockback: 0f,
                        knockbackConfig: null,
                        chainIndex: 0f,
                        isKnockedAttack: false
                    );
                    damageable.TakeDamage(info);
                    return true;
                }
            }
        }
        return false;
    }

    public void CancelDash(HitInfo hitInfo)
    {
        if (dashCoroutine != null)
        {
            
            StopCoroutine(dashCoroutine);
            transform.DOKill();
            DOTween.Kill(this); // renk tween'ini de kes

            // State'leri temizle, görsel olarak normale döndür
            transform.DOScale(originalScale, 0.4f).SetEase(Ease.OutBack);
            //if (enemyRenderer != null)
            //{
            //    enemyRenderer.GetPropertyBlock(mpb);
            //    mpb.SetColor(MainColor, originalColor);
            //    enemyRenderer.SetPropertyBlock(mpb);
            //}

            isDashing = false;

            if (hitInfo.isKnockedAttack)
            {
                currentState = EnemyDashState.KnockedBack;
                // Knockback bittiðinde OnKnockbackEnded event'i state'i Idle yapacak
            }
            else
            {
                // Kýsa bir cancel cooldown ver, yoksa Update anýnda yeni dash baþlatýr
                StartCoroutine(CancelCooldownCoroutine());
            }

            dashCoroutine = null;
        }
    }

    private IEnumerator CancelCooldownCoroutine()
    {
        currentState = EnemyDashState.Cooldown;
        yield return new WaitForSeconds(config.dashCooldown);
        currentState = EnemyDashState.Idle;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, config.dashDetectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + enemyDirection * config.dashRangeOffset, config.dashRange);
    }
}
