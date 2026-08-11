using UnityEngine;

[CreateAssetMenu(fileName = "EnemyMoveConfigSO", menuName = "Enemy/EnemyMoveConfigSO", order = 1)]  
public class EnemyMoveConfigSO : ScriptableObject
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = 9.81f;   
}
