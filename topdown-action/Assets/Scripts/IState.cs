using UnityEngine;

public interface IState 
{
    void EnterState();  
    void UpdateState(PlayerMovement player); 
    void ExitState();

}
