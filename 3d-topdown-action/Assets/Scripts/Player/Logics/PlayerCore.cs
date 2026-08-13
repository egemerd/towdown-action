using UnityEngine;

public class PlayerCore : MonoBehaviour
{
    PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        playerHealth.OnDamageTaken += HandleDamageTaken;
    }

    private void OnDisable()
    {
        playerHealth.OnDamageTaken -= HandleDamageTaken;
    }

    private void HandleDamageTaken(HitInfo info)
    {
        Debug.Log($"Player took {info.damage} damage from {info.sourcePosition}. Current HP: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
        GameFeelController.Instance.TriggerTimeStop(0.1f, 0.1f);
    }
}
