using UnityEngine;

/// <summary>
/// Impulse + Drag tabanlı knockback profile.
/// Enemy'ye başlangıç velocity'si verilir, sonra exponential decay ile söner.
/// Curve-based sistemin aksine "distance"ı direkt kontrol etmezsin — 
/// initial speed + drag birlikte total mesafeyi belirler.
/// 
/// Rough hesaplama: totalDistance ≈ initialSpeed / drag
/// (minSpeed threshold nedeniyle bir miktar daha az)
/// </summary>
[CreateAssetMenu(fileName = "KnockbackProfile", menuName = "Combat/Knockback Profile", order = 5)]
public class EnemyKnockbackConfigSO : ScriptableObject
{
    [Header("— Speed —")]
    [Tooltip("Attacker'ın verdiği knockbackForce (initial speed) çarpanı. " +
             "Light enemy 1.5x (daha hızlı ve uzağa savrulur), " +
             "heavy enemy 0.3-0.5x (kısa mesafede durur).")]
    public float speedMultiplier = 1f;

    [Header("— Drag / Damping —")]
    [Tooltip("Sürtünme katsayısı. Yüksek = hızlı durur (snappy). Düşük = uzun kayar (icy glide). " +
             "Tipik değerler: 3 (uzun glide), 6 (dengeli), 12 (hızlı durur).")]
    [Range(0.5f, 20f)]
    public float drag = 6f;

    [Tooltip("Bu hızın (units/sec) altına düşünce knockback zorla biter. " +
             "Yoksa exponential decay sonsuza kadar mikroskobik hızla sürünür. " +
             "0.2-0.5 arası iyi başlangıç.")]
    [Range(0.05f, 2f)]
    public float minSpeed = 0.3f;

    [Header("— Burst (optional punch phase) —")]
    [Tooltip("İlk N saniye 'kick' hissi için drag azaltılır. " +
             "0 = burst yok (saf exponential decay). " +
             "0.05-0.15 arası punchy arcade hissi.")]
    [Range(0f, 0.3f)]
    public float burstDuration = 0.08f;

    [Tooltip("Burst süresince drag bu değerle çarpılır. " +
             "0 = burst boyunca hiç drag yok (max punch). " +
             "0.3 = drag'in %30'u (yumuşak punch). " +
             "1 = burst effect yok.")]
    [Range(0f, 1f)]
    public float burstDragMultiplier = 0.2f;

    [Header("— Safety —")]
    [Tooltip("Knockback bu sürede zorla sonlanır — extreme low drag veya bug ihtimaline karşı guard.")]
    [Range(0.5f, 5f)]
    public float maxDuration = 2f;
}