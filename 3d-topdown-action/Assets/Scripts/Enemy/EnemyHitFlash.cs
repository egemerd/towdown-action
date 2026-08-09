using System.Collections;
using UnityEngine;

/// <summary>
/// Enemy hit alýnca material rengini kýsa süre flash ettirir.
/// EnemyHealth.OnDamageTaken event'ini dinler — EnemyCore koordine eder.
///
/// Material'ýn shader'ýnda bir "_HitFlash" property'si olmasý best case, ama fallback:
/// mesh renderer'ýn material color'unu doðrudan manipulate eder.
/// </summary>
public class EnemyHitFlash : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Flash uygulanacak renderer. Boþsa child'daki ilk MeshRenderer aranýr.")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Config")]
    [Tooltip("Default flash profili. Attack profile verilmezse bu kullanýlýr.")]
    [SerializeField] private HitFeedbackProfileSO defaultProfile;

    // MaterialPropertyBlock — GPU instancing bozmaz, shared material'ý bozmaz
    private MaterialPropertyBlock mpb;
    private Color originalColor;
    private Coroutine flashRoutine;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer == null)
        {
            Debug.LogError("EnemyHitFlash: Renderer bulunamadý!", this);
            return;
        }

        mpb = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(mpb);

        // Orijinal rengi shader'dan al
        if (targetRenderer.sharedMaterial.HasProperty(BaseColorID))
            originalColor = targetRenderer.sharedMaterial.GetColor(BaseColorID);
        else
            originalColor = Color.white;
    }

    /// <summary>
    /// Flash tetikle. Profile verilmezse defaultProfile kullanýlýr.
    /// </summary>
    public void Flash(HitFeedbackProfileSO profile = null)
    {
        if (targetRenderer == null) return;
        HitFeedbackProfileSO p = profile != null ? profile : defaultProfile;
        if (p == null) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(p));
    }

    private IEnumerator FlashRoutine(HitFeedbackProfileSO profile)
    {
        // Peak — anlýk flash rengi
        SetColor(profile.flashColor);
        yield return new WaitForSecondsRealtime(profile.flashHoldDuration);

        // Fade back — smooth geri dönüþ
        float elapsed = 0f;
        while (elapsed < profile.flashFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / profile.flashFadeDuration;
            Color current = Color.Lerp(profile.flashColor, originalColor, t);
            SetColor(current);
            yield return null;
        }

        SetColor(originalColor);
        flashRoutine = null;
    }

    private void SetColor(Color color)
    {
        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(BaseColorID, color);
        targetRenderer.SetPropertyBlock(mpb);
    }
}