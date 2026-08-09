using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    float moveSpeed = 2f;
    float distanceToStop = 1f;
    [SerializeField] Transform target;

    private void Update()
    {
        if (Vector3.Distance(transform.position, target.position) > distanceToStop)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }
}
