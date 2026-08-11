using UnityEngine;

/// <summary>
/// Bir enemy türünün VFX preset'i. Hit, death, knockback trail gibi
/// farklý olaylar için ayrý particle prefab'larý tutar.
///
/// Ayný profile birden fazla enemy türü tarafýndan paylaþýlabilir.
/// Örnek: 5 ateþ enemy'si tümü FireVFXConfig.asset kullanýr.
/// </summary>
[CreateAssetMenu(fileName = "VFXConfig", menuName = "Combat/VFX Config", order = 6)]
public class VFXConfigSO : ScriptableObject
{
    [Header("— Hit Effects —")]
    [Tooltip("Hit landýðýnda spawn olacak particle prefab. Hit position'da instantiate edilir.")]
    public GameObject hitVFXPrefab;

    [Tooltip("Hit VFX'in ömrü (saniye). Bu süre sonunda destroy edilir.")]
    [Range(0.1f, 5f)]
    public float hitVFXLifetime = 1f;

    [Tooltip("Hit VFX'i hit direction'a döndürsün mü? Kývýlcým gibi yön-önemli VFX'ler için.")]
    public bool alignHitVFXToDirection = true;

    [Header("— Death Effects —")]
    [Tooltip("Enemy öldüðünde spawn olacak particle. Genelde daha büyük ve dramatic.")]
    public GameObject deathVFXPrefab;

    [Range(0.1f, 5f)]
    public float deathVFXLifetime = 2f;

    [Header("— Knockback Trail (opsiyonel) —")]
    [Tooltip("Knockback sýrasýnda enemy'nin arkasýnda býrakýlan iz. TrailRenderer içermeli.")]
    public GameObject knockbackTrailPrefab;

    [Header("— Idle Effect (opsiyonel) —")]
    [Tooltip("Enemy sürekli aktifken görünen ambient particle (örn: ateþ enemy'nin alevi).")]
    public GameObject idleVFXPrefab;
}