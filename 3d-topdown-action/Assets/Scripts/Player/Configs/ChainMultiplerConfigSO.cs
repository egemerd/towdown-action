using UnityEngine;

/// <summary>
/// Chain step'lerine göre attack modifier'larını tanımlayan profile.
/// Her array'in index'i chain step'ine karşılık gelir:
///   - Index 0 = 1. hit
///   - Index 1 = 2. hit
///   - Index 2 = 3. hit (finisher, eğer maxChainIndex = 3 ise)
///
/// Array uzunluğu maxChainIndex kadar olmalı. Kısa kalırsa son değer kullanılır.
/// Sadece knockback yeterli değilse ilerde damage, hitstop vs. eklenebilir.
/// </summary>
[CreateAssetMenu(fileName = "ChainMultiplierProfile", menuName = "Combat/Chain Multiplier Profile", order = 8)]
public class ChainMultiplierProfileSO : ScriptableObject
{
    [Header("— Knockback —")]
    [Tooltip("Her chain step için knockback çarpanı. " +
             "1.0 = normal, 2.0 = iki katı, vs. Son eleman finisher çarpanı olur.")]
    public float[] knockbackMultipliers = new float[] { 1f, 1f, 3f };

    [Header("— Knockback Config (ileride) —")]
    public EnemyKnockbackConfigSO[] knockbackConfigs;

    [Header("— Damage (ileride kullanılabilir) —")]
    [Tooltip("Chain step'e göre damage çarpanı. Şimdi kullanılmıyor, ileride eklenir.")]
    public float[] damageMultipliers = new float[] { 1f, 1f, 1.5f };

    [Header("— Hit Feel (ileride) —")]
    [Tooltip("Chain step'e göre hitstop süresi çarpanı.")]
    public float[] hitStopMultipliers = new float[] { 1f, 1f, 2f };

    [Tooltip("Chain step'e göre shake şiddeti çarpanı.")]
    public float[] shakeMagnitudeMultipliers = new float[] { 1f, 1f, 1.5f };

    /// <summary>
    /// Verilen chain index için knockback çarpanı döner.
    /// Chain index 1-tabanlı (ilk hit = 1).
    /// Array'in dışına çıkarsa son değer döner (safe fallback).
    /// </summary>
    public float GetKnockbackMultiplier(int chainIndex)
    {
        return GetMultiplierAt(knockbackMultipliers, chainIndex);
    }

 
    public EnemyKnockbackConfigSO GetKnockbackConfig(int chainIndex)
    {
        if (knockbackConfigs == null || knockbackConfigs.Length == 0) return null;
        int arrayIndex = Mathf.Clamp(chainIndex - 1, 0, knockbackConfigs.Length - 1);
        //Debug.Log($"GetKnockbackConfig: chainIndex={chainIndex}, arrayIndex={arrayIndex}, config={knockbackConfigs[arrayIndex]}");
        return knockbackConfigs[arrayIndex];
    }

    public EnemyKnockbackConfigSO GetHeavyKnockbackConfig()
    {
        float index = knockbackConfigs.Length - 1;
        return knockbackConfigs[(int)index];
    }

    public bool IsHeavyKnockback(int chainIndex)
    {
        int lastIndex = knockbackConfigs.Length - 1;
        return (chainIndex - 1) == lastIndex;
    }

    public float GetDamageMultiplier(int chainIndex)
    {
        return GetMultiplierAt(damageMultipliers, chainIndex);
    }

    public float GetHitStopMultiplier(int chainIndex)
    {
        return GetMultiplierAt(hitStopMultipliers, chainIndex);
    }

    public float GetShakeMagnitudeMultiplier(int chainIndex)
    {
        return GetMultiplierAt(shakeMagnitudeMultipliers, chainIndex);
    }

    private float GetMultiplierAt(float[] array, int chainIndex)
    {
        if (array == null || array.Length == 0) return 1f;

        // Chain 1-tabanlı → array index için -1
        int arrayIndex = Mathf.Clamp(chainIndex - 1, 0, array.Length - 1);
        return array[arrayIndex];
    }

    
}