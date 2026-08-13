using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDashAttackConfig", menuName = "Enemy/EnemyDashAttackConfig")]
public class EnemyDashAttackConfigSO : ScriptableObject
{
    [Header("Detection")]
    public float dashDetectionRadius = 5f;
    public LayerMask playerLayerMask;

    [Header("Dash Movement")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.4f;
    public float dashSlowOffset = 0.1f; // radius icindeyken playerdan ne kadar ileri gidecegi offset.
    public AnimationCurve dashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    // dash boyunca hýzýn nasýl daðýlacaðý (ease-out için: hýzlý baþla, yavaþla)

    [Header("Dash Attack")]
    public float dashDamage = 20f;
    public float dashRange = 1f;
    public float dashRangeOffset = 0f;  

    [Header("Wind-up (Preparing)")]
    public float prepareDuration = 0.35f;    // shake + charge süresi
    public Vector3 prepareScale = new Vector3(0.7f, 0.7f, 0.7f); // charge sýrasýnda hedef scale
    public Vector3 shakeStrength = new Vector3(0.15f, 0.15f, 0f);
    public int shakeVibrato = 20;
    public float shakeRandomness = 90f;
    public Color prepareColor = new Color(0.5f, 0.5f, 0.5f, 1f); // grileþme rengi

    [Header("Cooldown / Reset")]
    public float dashCooldown = 1f;
    public float resetDuration = 0.2f; // scale ve renk normale dönme süresi
}