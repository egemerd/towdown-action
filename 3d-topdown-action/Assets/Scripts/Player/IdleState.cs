using UnityEngine;

public class IdleState : IState
{
    public void EnterState()
    {
        
    }

    public void ExitState()
    {

    }

    public void UpdateState(PlayerMovement player)
    {
        player.DecelerateToStop();   
    }

    

    
}
