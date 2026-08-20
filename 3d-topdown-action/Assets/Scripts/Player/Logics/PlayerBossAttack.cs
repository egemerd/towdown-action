using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBossAttack : MonoBehaviour
{
    [SerializeField] private Transform bossHolder;



    private Transform boss;
    PlayerInput playerInput;
    InputAction dashAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        dashAction = playerInput.actions["Dash"];
    }
    private void Update()
    {
        if(dashAction.triggered)
        {
            BossAttack();
        }
    }

    void BossAttack()
    {

    }
}
