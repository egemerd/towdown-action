using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Player için squash deformasyonu. EnemySquash'ýn birebir aynýsý — 
/// hostname decoupling amaçlý ayrý component. Ýleride `SquashDeformer` 
/// olarak generalize edilebilir, ayrý bir migration.
/// 
/// Dash tetiklendiðinde çaðýrýlýr, dash yönüne göre hizalanýr.
/// </summary>
public class PlayerSquash : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Ölçeklenecek transform. Player prefab hierarchy'sinde 'SquashRoot' " +
             "olarak ayýr — visual mesh burada, collider ve logic YOK.")]
    [SerializeField] private Transform squashRoot;

    private Vector3 originalLocalScale;
    private Quaternion originalLocalRotation;
    private Coroutine squashRoutine;

    public bool IsSquashing => squashRoutine != null;

    public event Action OnSquashStarted;
    public event Action OnSquashEnded;

    private void Awake()
    {
        if (squashRoot == null)
        {
            Debug.LogError($"[PlayerSquash] squashRoot atanmamýþ! {name}", this);
            enabled = false;
            return;
        }

        originalLocalScale = squashRoot.localScale;
        originalLocalRotation = squashRoot.localRotation;
    }

    public void TriggerSquash(Vector3 direction, EnemySquashConfigSO config)
    {
        if (config == null || squashRoot == null) return;

        if (squashRoutine != null)
        {
            StopCoroutine(squashRoutine);
            squashRoot.localRotation = originalLocalRotation;
        }

        squashRoutine = StartCoroutine(SquashRoutine(direction, config));
    }

    private IEnumerator SquashRoutine(Vector3 direction, EnemySquashConfigSO cfg)
    {
        OnSquashStarted?.Invoke();

        Vector3 flatDir = new Vector3(direction.x, 0f, direction.z);
        if (flatDir.sqrMagnitude < 0.0001f) flatDir = squashRoot.forward;
        flatDir.Normalize();

        if (cfg.alignToHitDirection)
        {
            Transform parent = squashRoot.parent;
            Vector3 localDir = parent != null
                ? parent.InverseTransformDirection(flatDir)
                : flatDir;
            squashRoot.localRotation = Quaternion.LookRotation(localDir, Vector3.up);
        }

        Vector3 squashedScale = new Vector3(
            originalLocalScale.x * cfg.stretchAxisScale,
            originalLocalScale.y * cfg.verticalScale,
            originalLocalScale.z * cfg.squashAxisScale
        );

        float elapsed = 0f;
        while (elapsed < cfg.duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cfg.duration);
            float amount = cfg.squashCurve.Evaluate(t);

            squashRoot.localScale = Vector3.LerpUnclamped(originalLocalScale, squashedScale, amount);
            yield return null;
        }

        squashRoot.localScale = originalLocalScale;
        squashRoot.localRotation = originalLocalRotation;
        squashRoutine = null;

        OnSquashEnded?.Invoke();
    }

    public void StopSquash()
    {
        if (squashRoutine != null)
        {
            StopCoroutine(squashRoutine);
            squashRoutine = null;
        }

        if (squashRoot != null)
        {
            squashRoot.localScale = originalLocalScale;
            squashRoot.localRotation = originalLocalRotation;
        }

        OnSquashEnded?.Invoke();
    }

    private void OnDisable() => StopSquash();
}