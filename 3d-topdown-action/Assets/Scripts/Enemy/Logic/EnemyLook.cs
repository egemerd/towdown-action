using UnityEngine;

public class EnemyLook : MonoBehaviour
{
    [SerializeField] private Transform target;
    
    
    void Update()
    {
        transform.LookAt(target);
    }
}
