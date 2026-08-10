using System.Collections;
using UnityEngine;

/// <summary>
/// Combat feel efektlerinin merkezi dispatcher'ý.
/// Time stop ve screen shake gibi global efektleri yönetir.
///
/// Kullaným:
///   GameFeelController.Instance.TriggerHitFeedback(profile);
///
/// Herhangi bir attack/damage sistemi bu singleton'a çaðrý yapabilir.
/// Component'ler bunu bilmez — event üzerinden dolaylý tetikler.
/// </summary>
public class GameFeelController : MonoBehaviour
{
    public static GameFeelController Instance { get; private set; }

    // Aktif time stop coroutine — üst üste binmesin
    private Coroutine timeStopRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
       
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Profile'a göre tüm feel efektlerini tetikler.
    /// Time stop, shake — ekleyeceðin ekstra global efektler buraya gelir.
    /// </summary>
    public void TriggerHitFeedback(HitFeedbackProfileSO profile)
    {
        if (profile == null) return;

        // Time stop
        if (profile.timeStopDuration > 0f)
            TriggerTimeStop(profile.timeStopScale, profile.timeStopDuration);
    }

    /// <summary>
    /// Sadece time stop tetikle — baðýmsýz kullaným için.
    /// </summary>
    public void TriggerTimeStop(float scale, float duration)
    {
        if (timeStopRoutine != null) StopCoroutine(timeStopRoutine);
        timeStopRoutine = StartCoroutine(TimeStopRoutine(scale, duration));
    }

    private IEnumerator TimeStopRoutine(float scale, float duration)
    {
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        timeStopRoutine = null;
    }
}