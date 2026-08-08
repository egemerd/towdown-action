using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    EnemyHealth enemyHealth;
    EnemyMovement enemyMovement;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void OnEnable()
    {
        enemyHealth.OnDied += HandleDeath;
        enemyHealth.OnDamageTaken += HandleDamageTaken; 
    }

    private void OnDisable()
    {
        enemyHealth.OnDied -= HandleDeath;  
        enemyHealth.OnDamageTaken -= HandleDamageTaken;
    }

    private void HandleDamageTaken(HitInfo info)
    {
        Debug.Log($"Enemy took {info.damage} damage from {info.sourcePosition}. Remaining health: {enemyHealth.CurrentHealth}");
    }

    void HandleDeath()
    {
        Destroy(gameObject, 0.5f);
    }

}
