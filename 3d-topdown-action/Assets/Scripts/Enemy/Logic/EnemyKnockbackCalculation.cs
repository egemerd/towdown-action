using UnityEngine;

public enum GlobalState
{
    Idle,
    Knockback,
    Stun,
    Dead
}
public class EnemyKnockbackCalculation : MonoBehaviour
{
    public GlobalState CurrentState { get; private set; } = GlobalState.Idle;



}
