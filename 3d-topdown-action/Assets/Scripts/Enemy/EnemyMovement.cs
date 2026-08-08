using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    float moveSpeed = 2f;
    [SerializeField] Transform target;

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
    }
}
