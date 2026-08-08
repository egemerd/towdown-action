using UnityEngine;

public class MoveState : IState
{
    public void EnterState()
    {

    }

    public void ExitState()
    {
        
    }

    public void UpdateState(PlayerMovement player)
    {
        player.UpdateHorizontalVelocity();
    }
}
