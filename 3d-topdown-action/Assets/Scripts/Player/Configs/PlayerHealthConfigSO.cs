using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHealthConfig", menuName = "Player/Health Config", order = 2)]
public class PlayerHealthConfigSO : ScriptableObject
{
    [Header("Health")]
    [Tooltip("Player'ýn maksimum caný")]
    public float maxHealth = 100f;

    [Header("Invulnerability")]
    [Tooltip("Hasar aldýktan sonra kaç saniye boyunca hasar almasýn")]
    public float invulnerabilityDuration = 1f;
}